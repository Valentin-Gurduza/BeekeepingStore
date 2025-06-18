using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Blog;
using eUseControl.BeekeepingStore.Models;
using eUseControl.BeekeepingStore.Filters;
using System.IO;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlog _blogBL;
        private readonly ISession _sessionBL;

        public BlogController()
        {
            // Use the main BusinessLogic class to get properly configured instances
            var businessLogic = new BusinessLogic.BusinessLogic();
            _blogBL = businessLogic.GetBlogBL;
            _sessionBL = businessLogic.GetSessionBL;
        }

        // Alternative constructor for dependency injection (if DI container is used)
        public BlogController(IBlog blogBL, ISession sessionBL)
        {
            _blogBL = blogBL ?? throw new ArgumentNullException(nameof(blogBL));
            _sessionBL = sessionBL ?? throw new ArgumentNullException(nameof(sessionBL));
        }

        // GET: Blog
        public ActionResult Index(string category = null, string tag = null, string search = null)
        {
            try
            {
                List<BlogPost> blogPosts;

                if (!string.IsNullOrEmpty(category))
                {
                    blogPosts = _blogBL.GetBlogPostsByCategory(category);
                    ViewBag.Title = $"Blog - {category}";
                    ViewBag.FilterType = "category";
                    ViewBag.FilterValue = category;
                }
                else if (!string.IsNullOrEmpty(tag))
                {
                    blogPosts = _blogBL.GetBlogPostsByTag(tag);
                    ViewBag.Title = $"Blog - Posts tagged with '{tag}'";
                    ViewBag.FilterType = "tag";
                    ViewBag.FilterValue = tag;
                }
                else if (!string.IsNullOrEmpty(search))
                {
                    blogPosts = _blogBL.SearchBlogPosts(search);
                    ViewBag.Title = $"Blog - Search results for '{search}'";
                    ViewBag.FilterType = "search";
                    ViewBag.FilterValue = search;
                }
                else
                {
                    blogPosts = _blogBL.GetAllBlogPosts();
                    ViewBag.Title = "Blog";
                }

                // Get categories for sidebar
                ViewBag.Categories = _blogBL.GetCategoryCounts();

                // Get recent posts for sidebar
                ViewBag.RecentPosts = _blogBL.GetRecentBlogPosts(5);

                return View(blogPosts);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Blog Index: " + ex.ToString());
                TempData["ErrorMessage"] = "Error loading blog posts: " + ex.Message;
                return View(new List<BlogPost>());
            }
        }

        // GET: Blog/Post/my-blog-post-slug
        public ActionResult Post(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var blogPost = _blogBL.GetBlogPostBySlug(slug);

                if (blogPost == null)
                {
                    return HttpNotFound();
                }

                // Increment view count
                _blogBL.IncrementViewCount(blogPost.BlogPostId);

                // Get categories for sidebar
                ViewBag.Categories = _blogBL.GetCategoryCounts();

                // Get recent posts for sidebar
                ViewBag.RecentPosts = _blogBL.GetRecentBlogPosts(5);

                // Create a view model for the comment form
                ViewBag.CommentForm = new BlogCommentModel
                {
                    BlogPostId = blogPost.BlogPostId
                };

                return View(blogPost);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Blog Post: " + ex.ToString());
                TempData["ErrorMessage"] = "Error loading blog post: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Blog/AddComment
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(BlogCommentModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var comment = new BlogComment
                    {
                        BlogPostId = model.BlogPostId,
                        Name = model.Name,
                        Email = model.Email,
                        Content = model.Content,
                        ParentCommentId = model.ParentCommentId
                    };

                    _blogBL.AddComment(comment);

                    TempData["SuccessMessage"] = "Your comment has been submitted and is awaiting approval.";

                    // Get blog post slug for redirection
                    var post = _blogBL.GetBlogPostById(model.BlogPostId);
                    if (post != null)
                    {
                        return RedirectToAction("Post", new { slug = post.Slug });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error in AddComment: " + ex.ToString());
                    ModelState.AddModelError("", "Error submitting comment: " + ex.Message);
                }
            }

            // If we get here, something went wrong
            // Redirect back to the post
            return RedirectToAction("Post", new { id = model.BlogPostId });
        }

        #region User Blog Management

        // GET: Blog/MyPosts - Lista postărilor utilizatorului
        [UserMod]
        public ActionResult MyPosts()
        {
            try
            {
                string userEmail = Session["UserEmail"] as string;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                var userProfile = _sessionBL.GetUserProfile(userEmail);
                if (userProfile == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Get all posts by this user (including unpublished)
                var allPosts = _blogBL.GetAllBlogPosts(includeUnpublished: true);
                var userPosts = allPosts.Where(p => p.Author == userProfile.FullName || p.Author == userProfile.UserName).ToList();

                ViewBag.UserName = userProfile.FullName ?? userProfile.UserName;
                return View(userPosts);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading your posts: " + ex.Message;
                return View(new List<BlogPost>());
            }
        }

        // GET: Blog/Create - Creare postare nouă
        [UserMod]
        public ActionResult Create()
        {
            var model = new BlogPost
            {
                PublishDate = DateTime.Now,
                IsPublished = false // User posts need approval
            };
            return View(model);
        }

        // POST: Blog/Create
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [UserMod]
        public ActionResult Create(BlogPost blogPost, HttpPostedFileBase featuredImage)
        {
            // Allow HTML content only for Content field from authenticated users
            ModelState.Remove("Content");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle featured image upload
                    if (featuredImage != null && featuredImage.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(featuredImage.FileName);
                        var fileExtension = Path.GetExtension(fileName).ToLower();

                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        if (allowedExtensions.Contains(fileExtension))
                        {
                            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                            var uploadPath = Path.Combine(Server.MapPath("~/Content/Images/blog/"), uniqueFileName);

                            // Create directory if it doesn't exist
                            Directory.CreateDirectory(Path.GetDirectoryName(uploadPath));

                            featuredImage.SaveAs(uploadPath);
                            blogPost.FeaturedImage = "~/Content/Images/blog/" + uniqueFileName;
                        }
                        else
                        {
                            ModelState.AddModelError("", "Please upload a valid image file (jpg, jpeg, png, gif).");
                            return View(blogPost);
                        }
                    }

                    // Set author from session
                    string userEmail = Session["UserEmail"] as string;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var userProfile = _sessionBL.GetUserProfile(userEmail);
                        blogPost.Author = userProfile?.FullName ?? userProfile?.UserName ?? "User";
                    }
                    else
                    {
                        blogPost.Author = "User";
                    }

                    // User posts need approval
                    blogPost.IsPublished = false;

                    // Generate slug if not provided
                    if (string.IsNullOrEmpty(blogPost.Slug))
                    {
                        blogPost.Slug = GenerateSlug(blogPost.Title);
                    }

                    int blogPostId = _blogBL.AddBlogPost(blogPost);
                    TempData["SuccessMessage"] = "Your blog post has been submitted and is awaiting approval!";
                    return RedirectToAction("MyPosts");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating blog post: " + ex.Message);
                }
            }
            return View(blogPost);
        }

        // GET: Blog/Edit/5 - Editare postare utilizator
        [UserMod]
        public ActionResult Edit(int id)
        {
            try
            {
                var blogPost = _blogBL.GetBlogPostById(id);
                if (blogPost == null)
                {
                    return HttpNotFound();
                }

                // Check if user owns this post
                string userEmail = Session["UserEmail"] as string;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var userProfile = _sessionBL.GetUserProfile(userEmail);
                    if (userProfile != null &&
                        (blogPost.Author == userProfile.FullName || blogPost.Author == userProfile.UserName))
                    {
                        return View(blogPost);
                    }
                }

                TempData["ErrorMessage"] = "You can only edit your own posts.";
                return RedirectToAction("MyPosts");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading blog post: " + ex.Message;
                return RedirectToAction("MyPosts");
            }
        }

        // POST: Blog/Edit/5
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [UserMod]
        public ActionResult Edit(BlogPost blogPost, HttpPostedFileBase featuredImage)
        {
            // Allow HTML content only for Content field from authenticated users
            ModelState.Remove("Content");

            if (ModelState.IsValid)
            {
                try
                {
                    // Verify ownership
                    var existingPost = _blogBL.GetBlogPostById(blogPost.BlogPostId);
                    if (existingPost == null)
                    {
                        return HttpNotFound();
                    }

                    string userEmail = Session["UserEmail"] as string;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var userProfile = _sessionBL.GetUserProfile(userEmail);
                        if (userProfile == null ||
                            (existingPost.Author != userProfile.FullName && existingPost.Author != userProfile.UserName))
                        {
                            TempData["ErrorMessage"] = "You can only edit your own posts.";
                            return RedirectToAction("MyPosts");
                        }
                    }

                    // Handle featured image upload
                    if (featuredImage != null && featuredImage.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(featuredImage.FileName);
                        var fileExtension = Path.GetExtension(fileName).ToLower();

                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        if (allowedExtensions.Contains(fileExtension))
                        {
                            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                            var uploadPath = Path.Combine(Server.MapPath("~/Content/Images/blog/"), uniqueFileName);

                            // Create directory if it doesn't exist
                            Directory.CreateDirectory(Path.GetDirectoryName(uploadPath));

                            featuredImage.SaveAs(uploadPath);

                            // Delete old image if exists
                            if (!string.IsNullOrEmpty(existingPost.FeaturedImage))
                            {
                                var oldImagePath = Server.MapPath(existingPost.FeaturedImage);
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }

                            blogPost.FeaturedImage = "~/Content/Images/blog/" + uniqueFileName;
                        }
                        else
                        {
                            ModelState.AddModelError("", "Please upload a valid image file (jpg, jpeg, png, gif).");
                            return View(blogPost);
                        }
                    }
                    else
                    {
                        // Keep existing image
                        blogPost.FeaturedImage = existingPost.FeaturedImage;
                    }

                    // Keep original author
                    blogPost.Author = existingPost.Author;

                    // User edits need re-approval
                    blogPost.IsPublished = false;

                    // Generate slug if not provided
                    if (string.IsNullOrEmpty(blogPost.Slug))
                    {
                        blogPost.Slug = GenerateSlug(blogPost.Title);
                    }

                    bool result = _blogBL.UpdateBlogPost(blogPost);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Your blog post has been updated and is awaiting approval!";
                        return RedirectToAction("MyPosts");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update blog post.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating blog post: " + ex.Message);
                }
            }
            return View(blogPost);
        }

        // GET: Blog/Delete/5 - Ștergere postare utilizator
        [UserMod]
        public ActionResult Delete(int id)
        {
            try
            {
                var blogPost = _blogBL.GetBlogPostById(id);
                if (blogPost == null)
                {
                    return HttpNotFound();
                }

                // Check if user owns this post
                string userEmail = Session["UserEmail"] as string;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var userProfile = _sessionBL.GetUserProfile(userEmail);
                    if (userProfile != null &&
                        (blogPost.Author == userProfile.FullName || blogPost.Author == userProfile.UserName))
                    {
                        return View(blogPost);
                    }
                }

                TempData["ErrorMessage"] = "You can only delete your own posts.";
                return RedirectToAction("MyPosts");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading blog post: " + ex.Message;
                return RedirectToAction("MyPosts");
            }
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [UserMod]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var blogPost = _blogBL.GetBlogPostById(id);
                if (blogPost == null)
                {
                    return HttpNotFound();
                }

                // Verify ownership
                string userEmail = Session["UserEmail"] as string;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var userProfile = _sessionBL.GetUserProfile(userEmail);
                    if (userProfile == null ||
                        (blogPost.Author != userProfile.FullName && blogPost.Author != userProfile.UserName))
                    {
                        TempData["ErrorMessage"] = "You can only delete your own posts.";
                        return RedirectToAction("MyPosts");
                    }
                }

                // Delete featured image if exists
                if (!string.IsNullOrEmpty(blogPost.FeaturedImage))
                {
                    var imagePath = Server.MapPath(blogPost.FeaturedImage);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                bool result = _blogBL.DeleteBlogPost(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Blog post deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete blog post.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting blog post: " + ex.Message;
            }
            return RedirectToAction("MyPosts");
        }

        #endregion

        #region Helper Methods

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return "";

            // Convert to lowercase and replace spaces with hyphens
            string slug = title.ToLower()
                .Replace(" ", "-")
                .Replace("ă", "a")
                .Replace("â", "a")
                .Replace("î", "i")
                .Replace("ș", "s")
                .Replace("ț", "t");

            // Remove special characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // Remove multiple consecutive hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

            // Remove leading and trailing hyphens
            slug = slug.Trim('-');

            // Ensure uniqueness by checking existing slugs
            var existingSlugs = _blogBL.GetAllBlogPosts(includeUnpublished: true)
                .Select(p => p.Slug)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            string originalSlug = slug;
            int counter = 1;
            while (existingSlugs.Contains(slug))
            {
                slug = $"{originalSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        #endregion
    }
}