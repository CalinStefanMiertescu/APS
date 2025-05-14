using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using APS.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace APS.Policys
{
    public class IsAdminRequirement : IAuthorizationRequirement { }

    public class IsAdminHandler : AuthorizationHandler<IsAdminRequirement>
    {
        private readonly APSContext _context;

        public IsAdminHandler(APSContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAdminRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return;
            }

            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null && user.IsAdmin)
            {
                context.Succeed(requirement);
            }
        }
    }
}