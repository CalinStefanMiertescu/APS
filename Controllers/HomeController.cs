using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APS.Models;
using APS.Models.ViewModels;
using APS.Data;
using System.Collections.Generic;

namespace APS.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(APSContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var latestArticles = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.PublishedAt)
                .Take(5)
                .ToListAsync();

            var latestArticleViewModels = latestArticles.Select(a => new ArticleViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                CoverImageUrl = a.CoverImageUrl,
                PublishedAt = a.PublishedAt ?? DateTime.MinValue,
                UpdatedAt = a.UpdatedAt,
                IsPublished = a.IsPublished,
                AuthorId = a.AuthorId,
                AuthorName = a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : "Unknown",
                Images = new List<ArticleImageViewModel>(), // Add if you want images
                Comments = new List<ArticleCommentViewModel>(), // Add if you want comments
                IsAdmin = currentUser.IsAdmin,
                CategoryId = a.CategoryId,
                CategoryName = a.Category?.Name ?? ""
            }).ToList();

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
                ProfilePictureUrl = currentUser.ProfilePictureUrl,
                Biography = currentUser.Biography,
                ActiveMembersCount = await _context.Users.CountAsync(u => u.IsActive && u.IsPayingMember),
                PendingApprovalsCount = await _context.Users.CountAsync(u => !u.IsActive && !u.IsRejected),
                Announcements = await _context.Announcements
                    .Where(a => a.IsActive && (!a.ExpiresAt.HasValue || a.ExpiresAt > DateTime.UtcNow))
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                LatestArticles = latestArticleViewModels,
                IsAdmin = currentUser.IsAdmin,
                Role = currentUser.IsAdmin ? "Admin" : (currentUser.IsModerator ? "Moderator" : "User")
            };

            return View(viewModel);
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
            // For now, just update the membership
            user.IsPayingMember = true;
            user.IsActive = true;
            user.MembershipExpiresAt = DateTime.UtcNow.AddYears(duration);
            user.LastMembershipPayment = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
