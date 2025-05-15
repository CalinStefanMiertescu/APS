using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace APS.Models.ViewModels
{
    public class ArticleListViewModel
    {
        public List<ArticleViewModel> Articles { get; set; } = new List<ArticleViewModel>();
        public bool IsAdmin { get; set; }
        public bool IsModerator { get; set; }
    }

    public class ArticleViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public List<ArticleImageViewModel> Images { get; set; } = new List<ArticleImageViewModel>();
        public List<ArticleCommentViewModel> Comments { get; set; } = new List<ArticleCommentViewModel>();
        public bool IsAdmin { get; set; }
        public bool IsModerator { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Lead { get; set; }
    }

    public class ArticleImageViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ArticleCommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        public bool IsApproved { get; set; }
    }

    public class CreateArticleViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public IFormFile CoverImage { get; set; }

        [Required]
        public int CategoryId { get; set; } // Selected category

        public List<Category> Categories { get; set; } = new();
    }

    public class EditArticleViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public int CategoryId { get; set; } // Selected category

        public List<Category> Categories { get; set; } = new();

        public string? CurrentCoverImageUrl { get; set; }
        public IFormFile? NewCoverImage { get; set; }
        public List<ArticleImageViewModel> CurrentImages { get; set; } = new List<ArticleImageViewModel>();
    }

    public class AddCommentViewModel
    {
        public int ArticleId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }
    }
} 