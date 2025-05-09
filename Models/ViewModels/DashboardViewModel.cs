using System;
using System.Collections.Generic;
using APS.Models;

namespace APS.Models.ViewModels
{
    public class DashboardViewModel
    {
        public bool IsAdmin { get; set; }
        public bool IsPayingMember { get; set; }
        public DateTime? MembershipExpiresAt { get; set; }
        public int ActiveMembersCount { get; set; }
        public int PendingApprovalsCount { get; set; }
        public List<Announcement> Announcements { get; set; } = new List<Announcement>();
        public string AccountName { get; set; } // For sidebar display
    }
} 