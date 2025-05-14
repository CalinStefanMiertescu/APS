using Microsoft.AspNetCore.Authorization;
using APS.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace APS.Policys
{
    public class IsAdminOrModeratorRequirement : IAuthorizationRequirement { }

    public class IsAdminOrModeratorHandler : AuthorizationHandler<IsAdminOrModeratorRequirement>
    {
        private readonly APSContext _context;

        public IsAdminOrModeratorHandler(APSContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdminOrModeratorRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
                return;

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null && (user.IsAdmin || user.IsModerator))
            {
                context.Succeed(requirement);
            }

            // Debug logging for troubleshooting
            System.Diagnostics.Debug.WriteLine($"[DEBUG] UserId: {userId}, IsAdmin: {user?.IsAdmin}, IsModerator: {user?.IsModerator}, Roles: {string.Join(", ", context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");
        }
    }
} 