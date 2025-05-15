using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using APS.Models;
using APS.Models.ViewModels;
using APS.Data;
using System.Security.Claims;

namespace APS.Controllers
{
    public class ArticleController : BaseController
    {
        private readonly IWebHostEnvironment _environment;

        public ArticleController(APSContext context, IWebHostEnvironment environment) : base(context)
        {
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            bool isAdmin = currentUser?.IsAdmin ?? false;
            bool isModerator = currentUser?.IsModerator ?? false;

            var articles = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Images)
                .Include(a => a.Comments)
                .Include(a => a.Category)
                .Where(a => a.IsPublished || (User.Identity.IsAuthenticated && (isAdmin || isModerator)))
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();

            var viewModel = new ArticleListViewModel
            {
                Articles = articles.Select(a => new ArticleViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    CoverImageUrl = a.CoverImageUrl,
                    PublishedAt = a.PublishedAt ?? DateTime.MinValue,
                    UpdatedAt = a.UpdatedAt ?? DateTime.MinValue,
                    IsPublished = a.IsPublished,
                    AuthorId = a.AuthorId,
                    AuthorName = a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : "Unknown",
                    CategoryName = a.Category != null ? a.Category.Name : "",
                    Images = a.Images?.Select(i => new ArticleImageViewModel
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl ?? string.Empty,
                        Caption = i.Caption ?? string.Empty,
                        DisplayOrder = i.DisplayOrder
                    }).ToList() ?? new List<ArticleImageViewModel>(),
                    Comments = a.Comments
                        .Where(c => c.IsApproved || (User.Identity.IsAuthenticated && (isAdmin || isModerator)))
                        .Select(c => new ArticleCommentViewModel
                        {
                            Id = c.Id,
                            Content = c.Content ?? string.Empty,
                            CreatedAt = c.CreatedAt,
                            UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown",
                            IsApproved = c.IsApproved
                        }).ToList(),
                    CategoryId = a.CategoryId
                }).ToList(),
                IsAdmin = isAdmin,
                IsModerator = isModerator
            };

            return View(viewModel);
        }

        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            var model = new CreateArticleViewModel
            {
                Categories = categories
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> Create(CreateArticleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.ToListAsync(); // reload for redisplay
                return View(model);
            }

            var article = new Article
            {
                Title = model.Title,
                Content = model.Content,
                AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CategoryId = model.CategoryId,
                PublishedAt = DateTime.UtcNow,
                IsPublished = true // Always publish on creation
            };

            if (model.CoverImage != null)
            {
                article.CoverImageUrl = await SaveImage(model.CoverImage);
            }

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();

            var viewModel = new EditArticleViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                CategoryId = article.CategoryId, // Set selected category
                Categories = categories ?? new List<Category>(),         // Populate categories
                CurrentCoverImageUrl = article.CoverImageUrl,
                CurrentImages = (article.Images ?? new List<ArticleImage>()).Select(i => new ArticleImageViewModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Caption = i.Caption,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> Edit(EditArticleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.ToListAsync();
                var dbArticle = await _context.Articles
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.Id == model.Id);
                model.CurrentImages = dbArticle?.Images?.Select(i => new ArticleImageViewModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Caption = i.Caption,
                    DisplayOrder = i.DisplayOrder
                }).ToList() ?? new List<ArticleImageViewModel>();
                model.CurrentCoverImageUrl = dbArticle?.CoverImageUrl;
                TempData["EditError"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(model);
            }

            var article = await _context.Articles
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == model.Id);

            if (article == null)
            {
                TempData["EditError"] = "Anunțul nu a fost găsit.";
                return RedirectToAction(nameof(Index));
            }

            article.Title = model.Title;
            article.Content = model.Content;
            article.CategoryId = model.CategoryId;
            article.UpdatedAt = DateTime.UtcNow;

            if (model.NewCoverImage != null)
            {
                if (!string.IsNullOrEmpty(article.CoverImageUrl))
                {
                    DeleteImage(article.CoverImageUrl);
                }
                article.CoverImageUrl = await SaveImage(model.NewCoverImage);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> Publish(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            article.IsPublished = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            // Delete images
            if (!string.IsNullOrEmpty(article.CoverImageUrl))
            {
                DeleteImage(article.CoverImageUrl);
            }

            foreach (var image in article.Images ?? new List<ArticleImage>())
            {
                DeleteImage(image.ImageUrl);
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(AddCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Forbid();
            }

            var article = await _context.Articles
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Id == model.ArticleId);

            if (article == null)
            {
                return NotFound();
            }

            var comment = new ArticleComment
            {
                ArticleId = model.ArticleId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                Content = model.Content ?? string.Empty,
                IsApproved = User.IsInRole("Admin") || User.IsInRole("Moderator")
            };

            article.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> ApproveComment(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Comments.Any(c => c.Id == id));
            if (article == null)
                return NotFound();

            var comment = article.Comments.FirstOrDefault(c => c.Id == id);
            if (comment == null)
                return NotFound();

            comment.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminOrModerator")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Comments)
                .FirstOrDefaultAsync(a => a.Comments.Any(c => c.Id == id));
            if (article == null)
                return NotFound();

            var comment = article.Comments.FirstOrDefault(c => c.Id == id);
            if (comment == null)
                return NotFound();

            article.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Images)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsPublished);

            if (article == null)
            {
                return NotFound();
            }

            var viewModel = new ArticleViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                CoverImageUrl = article.CoverImageUrl,
                PublishedAt = article.PublishedAt ?? DateTime.MinValue,
                UpdatedAt = article.UpdatedAt,
                IsPublished = article.IsPublished,
                AuthorId = article.AuthorId,
                AuthorName = article.Author != null ? $"{article.Author.FirstName} {article.Author.LastName}" : "Unknown",
                CategoryName = article.Category != null ? article.Category.Name : "",
                Images = article.Images?.Select(i => new ArticleImageViewModel
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Caption = i.Caption,
                    DisplayOrder = i.DisplayOrder
                }).ToList() ?? new List<ArticleImageViewModel>(),
                Comments = new List<ArticleCommentViewModel>(),
                IsAdmin = User.IsInRole("Admin"),
                IsModerator = User.IsInRole("Moderator"),
                CategoryId = article.CategoryId
            };

            return View(viewModel);
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{uniqueFileName}";
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
} 