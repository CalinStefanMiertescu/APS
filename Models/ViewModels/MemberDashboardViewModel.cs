using System;
using System.Collections.Generic;

namespace APS.Models.ViewModels
{
    public class MemberDashboardViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string JournalistType { get; set; }
        public string Publication { get; set; }
        public bool IsPayingMember { get; set; }
        public DateTime? MembershipExpiresAt { get; set; }
        public bool HasPendingChanges { get; set; }
        public List<string> AvailableJournalistTypes { get; set; } = new List<string>();
        public List<string> AvailablePublications { get; set; } = new List<string>();

        public string ProfilePictureUrl { get; set; } = null;
        public string Biography { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }

        public string Role { get; set; }

        public int MembershipProgress
        {
            get
            {
                if (!IsPayingMember || !MembershipExpiresAt.HasValue)
                {
                    return 0;
                }

                var totalDays = (MembershipExpiresAt.Value - DateTime.UtcNow).TotalDays;
                var remainingDays = Math.Max(0, totalDays);
                return (int)((remainingDays / 365) * 100);
            }
        }

        public int ActiveMembersCount { get; set; }
        public int PendingApprovalsCount { get; set; }
        public List<APS.Models.Announcement> Announcements { get; set; } = new List<APS.Models.Announcement>();
        public List<ArticleViewModel> LatestArticles { get; set; } = new List<ArticleViewModel>();
    }
} 