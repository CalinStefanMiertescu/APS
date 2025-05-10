
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

            var userEmail = context.User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user != null && user.IsAdmin)
            {
                context.Succeed(requirement);
            }
        }
    }
}