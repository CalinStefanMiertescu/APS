using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using APS.Models;
using APS.Models.ViewModels;
using APS.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace APS.Controllers
{
    [Authorize(Policy = "RequireAdmin")]
    public class AdminController : BaseController
    {
        private readonly UserManager<User> _userManager;

        public AdminController(APSContext context, UserManager<User> userManager) : base(context)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string filterType = null, string filterPublication = null, string filterLocation = null)
        {
            var usersQuery = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filterType))
                usersQuery = usersQuery.Where(u => u.JournalistType == filterType);
            if (!string.IsNullOrEmpty(filterPublication))
                usersQuery = usersQuery.Where(u => u.Publication == filterPublication);
            if (!string.IsNullOrEmpty(filterLocation))
                usersQuery = usersQuery.Where(u => u.City == filterLocation);

            var viewModel = new AdminDashboardViewModel
            {
                TotalMembersCount = await _context.Users.CountAsync(),
                ActiveMembersCount = await _context.Users.CountAsync(u => u.IsActive && u.IsPayingMember),
                PendingApprovalsCount = await _context.Users.CountAsync(u => !u.IsActive && !u.IsRejected),
                NonPayingMembersCount = await _context.Users.CountAsync(u => !u.IsPayingMember),
                PendingUsers = await _context.Users
                    .Where(u => !u.IsActive && !u.IsRejected)
                    .ToListAsync(),
                UsersWithChanges = await _context.Users
                    .Where(u => u.HasPendingChanges)
                    .ToListAsync(),
                ExpiringMemberships = await _context.Users
                    .Where(u => u.IsPayingMember && u.MembershipExpiresAt.HasValue && 
                           u.MembershipExpiresAt.Value <= DateTime.UtcNow.AddDays(30))
                    .ToListAsync(),
                JournalistTypes = await _context.Users
                    .Select(u => u.JournalistType)
                    .Distinct()
                    .ToListAsync(),
                Publications = await _context.Users
                    .Select(u => u.Publication)
                    .Distinct()
                    .ToListAsync(),
                ActiveMembers = await usersQuery.Where(u => u.IsActive && u.IsPayingMember).ToListAsync(),
                NonPayingMembers = await usersQuery.Where(u => u.IsActive && !u.IsPayingMember).ToListAsync(),
                 // Add this to include categories
                Categories = await _context.Categories
                    .Include(c => c.Articles)
                    .ToListAsync(),
                AuditLogs = await _context.Audit_Logs
                    .Include(a => a.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(20)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            user.IsActive = true;
            user.IsPayingMember = false;
            await _context.SaveChangesAsync();
            // Audit log
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _context.Audit_Logs.Add(new Audit_Log {
                UserId = adminId,
                Action = $"Approved user {user.Email} ({user.Id})",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectUser(string userId, string reason)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsRejected = true;
            user.RejectedAt = DateTime.UtcNow;
            user.RejectionReason = reason;
            await _context.SaveChangesAsync();
            // Audit log
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _context.Audit_Logs.Add(new Audit_Log {
                UserId = adminId,
                Action = $"Rejected user {user.Email} ({user.Id}) Reason: {reason}",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApproveChanges(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.PendingChangesJson))
            {
                TempData["AdminMessage"] = "User not found or no pending changes.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "User not found or no pending changes." });
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var changes = JsonSerializer.Deserialize<Dictionary<string, object>>(user.PendingChangesJson);
                foreach (var change in changes)
                {
                    var property = typeof(User).GetProperty(change.Key);
                    if (property != null)
                    {
                        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        var valueStr = change.Value?.ToString();
                        if (targetType == typeof(string))
                        {
                            property.SetValue(user, valueStr);
                        }
                        else if (targetType == typeof(bool))
                        {
                            if (bool.TryParse(valueStr, out var boolVal))
                                property.SetValue(user, boolVal);
                        }
                        else if (targetType == typeof(int))
                        {
                            if (int.TryParse(valueStr, out var intVal))
                                property.SetValue(user, intVal);
                        }
                        else if (targetType == typeof(DateTime))
                        {
                            if (DateTime.TryParse(valueStr, out var dtVal))
                                property.SetValue(user, dtVal);
                        }
                        else if (targetType == typeof(Guid))
                        {
                            if (Guid.TryParse(valueStr, out var guidVal))
                                property.SetValue(user, guidVal);
                        }
                        // else skip unsupported/complex types
                    }
                }

                user.HasPendingChanges = false;
                user.PendingChangesJson = null;
                await _context.SaveChangesAsync();
                TempData["AdminMessage"] = "User changes approved.";
                // Audit log
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _context.Audit_Logs.Add(new Audit_Log {
                    UserId = adminId,
                    Action = $"Approved changes for user {user.Email} ({user.Id})",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["AdminMessage"] = "Error approving changes: " + ex.Message;
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Error approving changes: " + ex.Message });
                throw;
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectChanges(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["AdminMessage"] = "User not found.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "User not found." });
                return RedirectToAction(nameof(Index));
            }

            user.HasPendingChanges = false;
            user.PendingChangesJson = null;
            await _context.SaveChangesAsync();
            TempData["AdminMessage"] = "User changes rejected.";
            // Audit log
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _context.Audit_Logs.Add(new Audit_Log {
                UserId = adminId,
                Action = $"Rejected changes for user {user.Email} ({user.Id})",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddType(string type, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return BadRequest();
            }

            // For simplicity, we'll just add the type to the first user that doesn't have it
            // In a real application, you'd want to store these in a separate table
            var user = await _context.Users.FirstOrDefaultAsync();
            if (user != null)
            {
                if (type == "journalist")
                {
                    user.JournalistType = value;
                }
                else if (type == "publication")
                {
                    user.Publication = value;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteType(string type, string value)
        {
            // In a real application, you'd want to handle this differently
            // For now, we'll just remove it from all users
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                if (type == "journalist" && user.JournalistType == value)
                {
                    user.JournalistType = null;
                }
                else if (type == "publication" && user.Publication == value)
                {
                    user.Publication = null;
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> InactivateMember(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            user.IsActive = false;
            await _context.SaveChangesAsync();
            // Audit log
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _context.Audit_Logs.Add(new Audit_Log {
                UserId = adminId,
                Action = $"Inactivated member {user.Email} ({user.Id})",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ActivateMember(string userId, DateTime? until)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            user.IsActive = true;
            user.IsPayingMember = true;
            if (until.HasValue)
                user.MembershipExpiresAt = until;
            await _context.SaveChangesAsync();
            // Audit log
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _context.Audit_Logs.Add(new Audit_Log {
                UserId = adminId,
                Action = $"Activated member {user.Email} ({user.Id}) until {until}",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingChanges(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.PendingChangesJson))
            {
                return NotFound();
            }
            return Json(user.PendingChangesJson);
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate name (case-insensitive)
                bool exists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
                if (exists)
                {
                    TempData["CategoryError"] = "A category with this name already exists.";
                    return RedirectToAction(nameof(Index));
                }

                category.CreatedAt = DateTime.UtcNow;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            TempData["CategoryError"] = "Invalid category data.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = await _context.Categories.FindAsync(category.Id);
                if (existingCategory != null)
                {
                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;
                    existingCategory.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category != null)
            {
                // Check if there are associated articles
                var hasArticles = await _context.Articles.AnyAsync(a => a.CategoryId == categoryId);
                if (!hasArticles)
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }
            return Json(category);
        }

        [HttpPost]
        public async Task<IActionResult> SetUserRole(string userId, string role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["AdminMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Remove from all roles first
            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);

            if (role == "Admin")
            {
                user.IsAdmin = true;
                user.IsModerator = false;
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            else if (role == "Moderator")
            {
                user.IsAdmin = false;
                user.IsModerator = true;
                await _userManager.AddToRoleAsync(user, "Moderator");
            }
            else
            {
                user.IsAdmin = false;
                user.IsModerator = false;
            }

            await _context.SaveChangesAsync();
            TempData["AdminMessage"] = $"Role updated to {role} for {user.FirstName} {user.LastName}.";
            // Audit log
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _context.Audit_Logs.Add(new Audit_Log {
                UserId = adminId,
                Action = $"Set role to {role} for user {user.Email} ({user.Id})",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
} 