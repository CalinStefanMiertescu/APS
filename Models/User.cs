using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace APS.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = null!;

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = null!;

        [Required]
        [EmailAddress]
        public override string? Email { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string JournalistType { get; set; } = null!;

        // Stores the original type for role switching
        public string? OriginalJournalistType { get; set; }

        [Required]
        [StringLength(100)]
        public string Publication { get; set; } = null!;

        public bool IsActive { get; set; } = false;
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Membership related fields
        public bool IsAdmin { get; set; } = false;
        public bool IsPayingMember { get; set; } = false;
        public DateTime? MembershipExpiresAt { get; set; }
        public DateTime? LastMembershipPayment { get; set; }
        public bool AutoRenew { get; set; } = false;
        public bool HasPendingChanges { get; set; } = false;
        public string? PendingChangesJson { get; set; }
        public bool IsRejected { get; set; } = false;
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        public bool IsModerator { get; set; } = false;

        // Profile fields
        public string? ProfilePictureUrl { get; set; }
        public string? Biography { get; set; }
    }
}
