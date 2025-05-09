using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APS.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        [Required]
        [ForeignKey("Author")]
        public string AuthorId { get; set; } = null!;

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public string? CoverImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        public bool IsPublished { get; set; } = false;
        public bool IsHighlighted { get; set; } = false;
        public int ViewCount { get; set; } = 0;

        // Navigation properties
        public virtual User Author { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;

        public List<ArticleImage>? Images { get; set; } = new List<ArticleImage>();
        public List<ArticleComment>? Comments { get; set; } = new List<ArticleComment>();
    }

    public class ArticleImage
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ArticleComment
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
    }
} 