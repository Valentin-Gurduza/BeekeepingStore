using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Blog;
using eUseControl.BeekeepingStore.Models;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlog _blogBL;

        public BlogController()
        {
            // Use the main BusinessLogic class to get properly configured instances
            var businessLogic = new BusinessLogic.BusinessLogic();
            _blogBL = businessLogic.GetBlogBL;
        }

        // Alternative constructor for dependency injection (if DI container is used)
        public BlogController(IBlog blogBL)
        {
            _blogBL = blogBL ?? throw new ArgumentNullException(nameof(blogBL));
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
    }
}