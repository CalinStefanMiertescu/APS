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
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace APS.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly APSContext _context;
        private readonly IWebHostEnvironment _environment;

        public MemberController(APSContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new MemberDashboardViewModel
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Email = currentUser.Email,
                Phone = currentUser.Phone,
                City = currentUser.City,
                JournalistType = currentUser.JournalistType,
                Publication = currentUser.Publication,
                IsPayingMember = currentUser.IsPayingMember,
                MembershipExpiresAt = currentUser.MembershipExpiresAt,
                HasPendingChanges = currentUser.HasPendingChanges,
                AvailableJournalistTypes = await _context.Users
                    .Select(u => u.JournalistType)
                    .Distinct()
                    .ToListAsync(),
                AvailablePublications = await _context.Users
                    .Select(u => u.Publication)
                    .Distinct()
                    .ToListAsync(),
                ProfilePictureUrl = currentUser.ProfilePictureUrl,
                Biography = currentUser.Biography
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded." });
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Only image files (jpg, jpeg, png, gif) are allowed." });
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            var fileName = $"{user.Id}_{DateTime.UtcNow.Ticks}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }
            user.ProfilePictureUrl = $"/uploads/profile-pictures/{fileName}";
            await _context.SaveChangesAsync();
            return Json(new {
                success = true,
                newProfilePictureUrl = user.ProfilePictureUrl,
                firstName = user.FirstName,
                lastName = user.LastName
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBiography(string biography)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user == null)
            {
                TempData["BiographyMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            user.Biography = biography;
            await _context.SaveChangesAsync();

            TempData["BiographyMessage"] = "Biography updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(MemberDashboardViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            // Store current values for comparison
            var changes = new Dictionary<string, object>();
            if (user.FirstName != model.FirstName) changes["FirstName"] = model.FirstName;
            if (user.LastName != model.LastName) changes["LastName"] = model.LastName;
            if (user.Phone != model.Phone) changes["Phone"] = model.Phone;
            if (user.City != model.City) changes["City"] = model.City;
            if (user.JournalistType != model.JournalistType) changes["JournalistType"] = model.JournalistType;
            if (user.Publication != model.Publication) changes["Publication"] = model.Publication;

            if (changes.Any())
            {
                user.HasPendingChanges = true;
                user.PendingChangesJson = JsonSerializer.Serialize(changes);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ActivateMembership()
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            // TODO: Implement payment processing
            user.IsPayingMember = true;
            user.MembershipExpiresAt = DateTime.UtcNow.AddYears(1);
            user.LastMembershipPayment = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ExtendMembership(int duration, bool autoRenew)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            // TODO: Implement payment processing
            user.MembershipExpiresAt = user.MembershipExpiresAt?.AddYears(duration) ?? DateTime.UtcNow.AddYears(duration);
            user.LastMembershipPayment = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CancelMembership()
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            user.IsPayingMember = false;
            user.MembershipExpiresAt = null;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ActivateAutoRenewal()
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            // TODO: Implement payment processing for auto-renewal
            // For now, just update the membership
            user.IsPayingMember = true;
            user.MembershipExpiresAt = DateTime.UtcNow.AddYears(1);
            user.LastMembershipPayment = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PayPalPaymentModel payment)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Update user's membership status
                user.IsPayingMember = true;
                user.MembershipExpiresAt = DateTime.UtcNow.AddYears(1);
                user.LastMembershipPayment = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Profile()
        {
            // Reuse the logic from Index
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new MemberDashboardViewModel
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Email = currentUser.Email,
                Phone = currentUser.Phone,
                City = currentUser.City,
                JournalistType = currentUser.JournalistType,
                Publication = currentUser.Publication,
                IsPayingMember = currentUser.IsPayingMember,
                MembershipExpiresAt = currentUser.MembershipExpiresAt,
                HasPendingChanges = currentUser.HasPendingChanges,
                AvailableJournalistTypes = await _context.Users
                    .Select(u => u.JournalistType)
                    .Distinct()
                    .ToListAsync(),
                AvailablePublications = await _context.Users
                    .Select(u => u.Publication)
                    .Distinct()
                    .ToListAsync(),
                ProfilePictureUrl = currentUser.ProfilePictureUrl,
                Biography = currentUser.Biography
            };

            return View("Index", viewModel);
        }
    }

    public class PayPalPaymentModel
    {
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public string Status { get; set; }
    }
} 