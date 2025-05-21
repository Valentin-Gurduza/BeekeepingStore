using eUseControl.BeekeepingStore.Domain.Entities.Blog;
using System;
using System.Collections.Generic;

namespace eUseControl.BeekeepingStore.BusinessLogic.Interfaces
{
    public interface IBlog
    {
        // Blog post operations
        int AddBlogPost(BlogPost blogPost);
        bool UpdateBlogPost(BlogPost blogPost);
        bool DeleteBlogPost(int blogPostId);
        BlogPost GetBlogPostById(int blogPostId);
        BlogPost GetBlogPostBySlug(string slug);
        List<BlogPost> GetAllBlogPosts(bool includeUnpublished = false);
        List<BlogPost> GetRecentBlogPosts(int count = 5);
        List<BlogPost> GetBlogPostsByCategory(string category);
        List<BlogPost> GetBlogPostsByTag(string tag);
        List<BlogPost> SearchBlogPosts(string searchTerm);

        // Blog comment operations
        int AddComment(BlogComment comment);
        bool UpdateComment(BlogComment comment);
        bool DeleteComment(int commentId);
        bool ApproveComment(int commentId);
        List<BlogComment> GetCommentsByBlogPostId(int blogPostId, bool includeUnapproved = false);
        List<BlogComment> GetRecentComments(int count = 5);

        // Statistics
        int IncrementViewCount(int blogPostId);
        int GetTotalPostCount();
        Dictionary<string, int> GetCategoryCounts();
    }
}