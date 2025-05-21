using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Blog;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
    public class BlogBL : IBlog
    {
        private void LogError(Exception ex)
        {
            using (var context = new DataContext())
            {
                var errorLog = new ErrorLog
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    CreatedAt = DateTime.UtcNow
                };
                context.ErrorLogs.Add(errorLog);
                context.SaveChanges();
            }
        }

        #region Blog Post Operations

        public int AddBlogPost(BlogPost blogPost)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Set default values if not provided
                    if (blogPost.PublishDate == default)
                    {
                        blogPost.PublishDate = DateTime.Now;
                    }

                    // Generate slug if not provided
                    if (string.IsNullOrEmpty(blogPost.Slug))
                    {
                        blogPost.Slug = GenerateSlug(blogPost.Title);
                    }

                    context.BlogPosts.Add(blogPost);
                    context.SaveChanges();

                    return blogPost.BlogPostId;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool UpdateBlogPost(BlogPost blogPost)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var existingPost = context.BlogPosts.Find(blogPost.BlogPostId);

                    if (existingPost == null)
                        return false;

                    // Update the existing post with new values
                    context.Entry(existingPost).CurrentValues.SetValues(blogPost);

                    // Ensure slug is updated if title changed
                    if (string.IsNullOrEmpty(blogPost.Slug) || existingPost.Title != blogPost.Title)
                    {
                        existingPost.Slug = GenerateSlug(blogPost.Title);
                    }

                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool DeleteBlogPost(int blogPostId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var blogPost = context.BlogPosts.Find(blogPostId);

                    if (blogPost == null)
                        return false;

                    context.BlogPosts.Remove(blogPost);
                    context.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public BlogPost GetBlogPostById(int blogPostId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Include comments in the result
                    var blogPost = context.BlogPosts
                        .Include(p => p.Comments)
                        .FirstOrDefault(p => p.BlogPostId == blogPostId);

                    // Filter approved comments after loading
                    if (blogPost != null && blogPost.Comments != null)
                    {
                        blogPost.Comments = blogPost.Comments.Where(c => c.IsApproved).ToList();
                    }

                    return blogPost;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public BlogPost GetBlogPostBySlug(string slug)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Include comments in the result
                    var blogPost = context.BlogPosts
                        .Include(p => p.Comments)
                        .FirstOrDefault(p => p.Slug == slug);

                    // Filter approved comments after loading
                    if (blogPost != null && blogPost.Comments != null)
                    {
                        blogPost.Comments = blogPost.Comments.Where(c => c.IsApproved).ToList();
                    }

                    return blogPost;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<BlogPost> GetAllBlogPosts(bool includeUnpublished = false)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var query = context.BlogPosts.AsQueryable();

                    if (!includeUnpublished)
                    {
                        query = query.Where(p => p.IsPublished);
                    }

                    return query
                        .OrderByDescending(p => p.PublishDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<BlogPost> GetRecentBlogPosts(int count = 5)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.BlogPosts
                        .Where(p => p.IsPublished)
                        .OrderByDescending(p => p.PublishDate)
                        .Take(count)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<BlogPost> GetBlogPostsByCategory(string category)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.BlogPosts
                        .Where(p => p.IsPublished && p.Category == category)
                        .OrderByDescending(p => p.PublishDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<BlogPost> GetBlogPostsByTag(string tag)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Simple approach: look for tag in the Tags field
                    return context.BlogPosts
                        .Where(p => p.IsPublished && p.Tags.Contains(tag))
                        .OrderByDescending(p => p.PublishDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<BlogPost> SearchBlogPosts(string searchTerm)
        {
            try
            {
                using (var context = new DataContext())
                {
                    if (string.IsNullOrWhiteSpace(searchTerm))
                        return new List<BlogPost>();

                    searchTerm = searchTerm.ToLower();

                    return context.BlogPosts
                        .Where(p => p.IsPublished &&
                            (p.Title.ToLower().Contains(searchTerm) ||
                             p.Content.ToLower().Contains(searchTerm) ||
                             p.Summary.ToLower().Contains(searchTerm) ||
                             p.Tags.ToLower().Contains(searchTerm)))
                        .OrderByDescending(p => p.PublishDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        #endregion

        #region Blog Comment Operations

        public int AddComment(BlogComment comment)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Set default values
                    comment.CommentDate = DateTime.Now;
                    comment.IsApproved = false; // Require approval by default

                    context.BlogComments.Add(comment);
                    context.SaveChanges();

                    return comment.CommentId;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool UpdateComment(BlogComment comment)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var existingComment = context.BlogComments.Find(comment.CommentId);

                    if (existingComment == null)
                        return false;

                    // Update properties
                    existingComment.Content = comment.Content;
                    existingComment.IsApproved = comment.IsApproved;

                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool DeleteComment(int commentId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var comment = context.BlogComments.Find(commentId);

                    if (comment == null)
                        return false;

                    context.BlogComments.Remove(comment);
                    context.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool ApproveComment(int commentId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var comment = context.BlogComments.Find(commentId);

                    if (comment == null)
                        return false;

                    comment.IsApproved = true;
                    context.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<BlogComment> GetCommentsByBlogPostId(int blogPostId, bool includeUnapproved = false)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var query = context.BlogComments
                        .Where(c => c.BlogPostId == blogPostId);

                    if (!includeUnapproved)
                    {
                        query = query.Where(c => c.IsApproved);
                    }

                    return query
                        .OrderBy(c => c.CommentDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<BlogComment> GetRecentComments(int count = 5)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.BlogComments
                        .Where(c => c.IsApproved)
                        .OrderByDescending(c => c.CommentDate)
                        .Take(count)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        #endregion

        #region Statistics

        public int IncrementViewCount(int blogPostId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var blogPost = context.BlogPosts.Find(blogPostId);

                    if (blogPost == null)
                        return 0;

                    blogPost.ViewCount++;
                    context.SaveChanges();

                    return blogPost.ViewCount;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public int GetTotalPostCount()
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.BlogPosts.Count(p => p.IsPublished);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public Dictionary<string, int> GetCategoryCounts()
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.BlogPosts
                        .Where(p => p.IsPublished && !string.IsNullOrEmpty(p.Category))
                        .GroupBy(p => p.Category)
                        .ToDictionary(g => g.Key, g => g.Count());
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return "";

            // Remove special characters
            string slug = title.ToLower();
            slug = slug.Replace(" ", "-");

            // Remove accents and normalize
            slug = System.Text.Encoding.ASCII.GetString(
                System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(slug)
            );

            // Remove any other non-alphanumeric characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // Remove multiple hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            return slug;
        }

        #endregion
    }
}