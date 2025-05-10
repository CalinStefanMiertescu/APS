
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

                var userEmail = context.User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user != null && user.IsModerator)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
