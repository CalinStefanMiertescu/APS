using System.Collections.Generic;
using APS.Models;

namespace APS.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalMembersCount { get; set; }
        public int ActiveMembersCount { get; set; }
        public int PendingApprovalsCount { get; set; }
        public int NonPayingMembersCount { get; set; }
        public List<User> PendingUsers { get; set; } = new List<User>();
        public List<User> UsersWithChanges { get; set; } = new List<User>();
        public List<User> ExpiringMemberships { get; set; } = new List<User>();
        public List<string> JournalistTypes { get; set; } = new List<string>();
        public List<string> Publications { get; set; } = new List<string>();
        public List<User> ActiveMembers { get; set; } = new List<User>();
        public List<User> NonPayingMembers { get; set; } = new List<User>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Audit_Log> AuditLogs { get; set; } = new List<Audit_Log>();
    }
} 