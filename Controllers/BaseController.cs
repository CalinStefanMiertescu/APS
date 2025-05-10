using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using APS.Data;
using APS.Models.ViewModels;
using System.Linq;

public class BaseController : Controller
{
    protected readonly APSContext _context;

    public BaseController(APSContext context)
    {
        _context = context;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user != null)
            {
                ViewBag.SidebarModel = new MemberDashboardViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.IsAdmin ? "Admin" : user.IsModerator ? "Moderator" : "User",
                    IsAdmin = user.IsAdmin,
                    IsPayingMember = user.IsPayingMember,
                    MembershipExpiresAt = user.MembershipExpiresAt,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Biography = user.Biography,
                    City = user.City,
                    Publication = user.Publication,
                    JournalistType = user.JournalistType
                };
            }
        }
        base.OnActionExecuting(context);
    }
} 