using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Optimization;
using System.Web.Http;
using eUseControl.BeekeepingStore.Domain.Entities.Blog;

namespace eUseControl.BeekeepingStore
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();

            // Register Web API routes before MVC routes
            GlobalConfiguration.Configure(WebApiConfig.Register);

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Seed blog data for initial setup
            SeedBlogData();
        }

        private void SeedBlogData()
        {
            try
            {
                // Use BusinessLogic class to get access to the database instead of directly instantiating DataContext
                var businessLogic = new BusinessLogic.BusinessLogic();
                var blogPosts = businessLogic.GetBlogBL.GetAllBlogPosts();

                // If blog posts already exist, skip seeding
                if (blogPosts != null && blogPosts.Any())
                {
                    return;
                }

                // Create blog posts and comments using business logic layer instead
                // This avoids directly accessing the internal DataContext
                var blogBL = businessLogic.GetBlogBL;

                // Create blog posts
                var post1 = new Domain.Entities.Blog.BlogPost
                {
                    Title = "Getting Started with Beekeeping: A Beginner's Guide",
                    Slug = "getting-started-with-beekeeping",
                    Content = @"<p>Beekeeping is a rewarding hobby that offers numerous benefits, from pollinating plants to producing delicious honey. But how do you get started? Here's a comprehensive guide for beginners.</p>
                        <h2>Essential Equipment</h2>
                        <p>Before diving into beekeeping, you'll need some essential equipment:</p>
                        <ul>
                            <li><strong>Beehive</strong> - The standard Langstroth hive is recommended for beginners</li>
                            <li><strong>Protective Gear</strong> - A suit, veil, gloves, and boots</li>
                            <li><strong>Smoker</strong> - Helps calm the bees during inspections</li>
                            <li><strong>Hive Tool</strong> - For prying apart hive components</li>
                            <li><strong>Bee Brush</strong> - Gently removes bees from frames</li>
                        </ul>
                        <p>Investing in quality equipment will make your beekeeping journey much more enjoyable and safe.</p>",
                    Summary = "Learn everything you need to know to start your beekeeping journey, from essential equipment to seasonal hive management.",
                    FeaturedImage = "~/Content/Images/blog/beekeeping-beginner.jpg",
                    PublishDate = DateTime.Now.AddDays(-30),
                    Author = "John Beekeeper",
                    IsPublished = true,
                    Category = "Beginner Guides",
                    Tags = "beekeeping,beginners,equipment,getting started",
                    ViewCount = 42
                };

                blogBL.AddBlogPost(post1);

                // Add more blog posts as needed

                // Add comments using business logic layer
                var comment1 = new Domain.Entities.Blog.BlogComment
                {
                    BlogPostId = post1.BlogPostId,
                    Name = "Michael Brown",
                    Email = "michael@example.com",
                    Content = "Great article! I've been thinking about getting into beekeeping for a while. This gives me a good starting point.",
                    CommentDate = DateTime.Now.AddDays(-28),
                    IsApproved = true
                };

                blogBL.AddComment(comment1);

                // Add more comments as needed
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine("Error seeding blog data: " + ex.Message);
            }
        }
    }
}
