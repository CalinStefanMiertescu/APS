using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APS.Models;
using APS.Models.ViewModels;
using APS.Data;

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
            user.MembershipExpiresAt = DateTime.UtcNow.AddYears(duration);
            user.LastMembershipPayment = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
