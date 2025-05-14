using Microsoft.AspNetCore.Authorization;

using APS.Data;

using Microsoft.EntityFrameworkCore;
namespace APS.Policys
{
    public class IsModeratorRequirment
    {
        public class IsModeratorRequirement : IAuthorizationRequirement { }

        public class IsModeratorHandler : AuthorizationHandler<IsModeratorRequirement>
        {
            private readonly APSContext _context;

            public IsModeratorHandler(APSContext context)
            {
                _context = context;
            }

            protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsModeratorRequirement requirement)
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    return;
                }

                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user != null && user.IsModerator)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
