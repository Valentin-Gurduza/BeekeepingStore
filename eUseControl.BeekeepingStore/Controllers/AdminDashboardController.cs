using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Order;
using eUseControl.BeekeepingStore.Domain.Entities.User;
using eUseControl.BeekeepingStore.Domain.Entities.Product;
using eUseControl.BeekeepingStore.Domain.Entities.Blog;
using eUseControl.BeekeepingStore.Domain.Entities.Payment;
using eUseControl.BeekeepingStore.Filters;
using System.Data.Entity;
using System.IO;

namespace eUseControl.BeekeepingStore.Controllers
{
    [AdminMod]
    public class AdminDashboardController : Controller
    {
        private readonly IOrder _orderBL;
        private readonly IProduct _productBL;
        private readonly SessionBL _sessionBL;
        private readonly IPayment _paymentBL;
        private readonly IBlog _blogBL;
        private readonly IPromotion _promotionBL;

        public AdminDashboardController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _orderBL = bl.GetOrderBL;
            _productBL = new ProductBL();
            _sessionBL = new SessionBL();
            _paymentBL = new PaymentBL();
            _blogBL = new BlogBL();
            _promotionBL = bl.GetPromotionBL;
        }

        // GET: AdminDashboard - Pagina principală cu date statistice
        public ActionResult Index()
        {
            // Statistici generale pentru dashboard
            ViewBag.TotalUsers = _sessionBL.GetUserCount();
            ViewBag.TotalProducts = _productBL.GetAllProducts().Count;
            ViewBag.TotalOrders = _orderBL.GetAllOrders().Count;
            ViewBag.TotalRevenue = _orderBL.GetTotalRevenue();

            // Comenzi recente
            ViewBag.RecentOrders = _orderBL.GetRecentOrders(5);

            // Produse cu stoc scăzut
            ViewBag.LowStockProducts = _productBL.GetLowStockProducts(10, 5);

            // Vânzări pe ultimele 7 zile
            ViewBag.SalesData = _orderBL.GetSalesDataByDateRange(DateTime.Now.AddDays(-7), DateTime.Now);

            // Top 5 produse vândute
            ViewBag.TopProducts = _orderBL.GetTopSellingProducts(5);

            // Ultimii utilizatori înregistrați
            ViewBag.NewUsers = _sessionBL.GetRecentUsers(5);

            return View();
        }

        // GET: AdminDashboard/Users - Gestionare utilizatori
        public ActionResult Users(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            // Get users with paging and filtering
            var users = _sessionBL.GetFilteredUsers(searchTerm, page, pageSize, out int totalCount);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;

            return View(users);
        }

        // GET: AdminDashboard/UserDetails/5 - Detalii utilizator
        public ActionResult UserDetails(int id)
        {
            var user = _sessionBL.GetUserById(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Obținem comenzile utilizatorului
            var userOrders = _orderBL.GetOrdersByUserId(id);

            ViewBag.UserOrders = userOrders;

            // Total cheltuieli utilizator
            ViewBag.TotalSpent = userOrders.Where(o => o.OrderStatus != "Cancelled").Sum(o => o.TotalAmount);

            return View(user);
        }

        // GET: AdminDashboard/Payments - Gestionare plăți
        public ActionResult Payments(string searchTerm = "", string status = "", int page = 1, int pageSize = 10)
        {
            // Get payments with paging and filtering
            var payments = _paymentBL.GetFilteredPayments(searchTerm, status, page, pageSize, out int totalCount);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentStatus = status;

            // Lista de statusuri pentru filtrare
            ViewBag.StatusList = new List<string> { "Pending", "Completed", "Failed", "Refunded" };

            return View(payments);
        }

        // GET: AdminDashboard/PaymentDetails/5 - Detalii plată
        public ActionResult PaymentDetails(int id)
        {
            var payment = _paymentBL.GetPaymentById(id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            // Obținem comanda asociată plății
            var order = _orderBL.GetOrderById(payment.OrderId);
            ViewBag.Order = order;

            return View(payment);
        }

        // GET: AdminDashboard/Analytics - Pagină de analiză detaliată
        public ActionResult Analytics()
        {
            // Date pentru graficele din dashboard

            // 1. Vânzări lunare pentru anul curent
            DateTime startOfYear = new DateTime(DateTime.Now.Year, 1, 1);
            ViewBag.MonthlySales = _orderBL.GetMonthlySalesData(DateTime.Now.Year);

            // 2. Vânzări pe categorii de produse
            ViewBag.CategorySales = _orderBL.GetCategorySalesData();

            // 3. Rata de conversie (comenzi vs vizitatori)
            ViewBag.ConversionRate = _orderBL.GetConversionRate();

            // 4. Statistici despre metode de plată
            ViewBag.PaymentMethods = _paymentBL.GetPaymentMethodStats();

            return View();
        }

        // POST: AdminDashboard/UpdateUserStatus/5 - Activare/dezactivare utilizator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserStatus(int id, bool isActive)
        {
            bool result = _sessionBL.UpdateUserStatus(id, isActive);

            if (!result)
            {
                return HttpNotFound();
            }

            TempData["SuccessMessage"] = isActive
                ? "Utilizatorul a fost activat cu succes."
                : "Utilizatorul a fost dezactivat cu succes.";

            return RedirectToAction("UserDetails", new { id = id });
        }

        // POST: AdminDashboard/UpdatePaymentStatus - Actualizare status plată
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePaymentStatus(int id, string status)
        {
            bool result = _paymentBL.UpdatePaymentStatus(id, status);

            if (!result)
            {
                return HttpNotFound();
            }

            TempData["SuccessMessage"] = "The payment status has been updated successfully.";

            return RedirectToAction("PaymentDetails", new { id = id });
        }

        // GET: AdminDashboard/Settings - Website settings
        public ActionResult Settings()
        {
            // You can add ViewModel logic here if needed
            return View();
        }

        // GET: AdminDashboard/AdminProfile - Admin profile page
        public ActionResult AdminProfile()
        {
            string userEmail = Session["UserEmail"] as string;

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get admin user details
            var adminUser = _sessionBL.GetUserProfile(userEmail);

            if (adminUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(adminUser);
        }

        // POST: AdminDashboard/UpdateProfile - Update admin profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(UProfileData model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool result = _sessionBL.UpdateUserProfile(model);

                    if (result)
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update profile.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error: " + ex.Message;
                }
            }

            return RedirectToAction("AdminProfile");
        }

        // POST: AdminDashboard/UploadProfileImage - Upload profile picture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadProfileImage(HttpPostedFileBase profileImage)
        {
            if (profileImage != null && profileImage.ContentLength > 0)
            {
                try
                {
                    string userEmail = Session["UserEmail"] as string;

                    if (string.IsNullOrEmpty(userEmail))
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    // Save the image and get the file path
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                    string filePath = Path.Combine(Server.MapPath("~/Content/Images/Profiles"), fileName);

                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    // Save the file
                    profileImage.SaveAs(filePath);

                    // Update user profile with image path
                    string imageUrl = "/Content/Images/Profiles/" + fileName;
                    bool result = _sessionBL.UpdateUserProfileImage(userEmail, imageUrl);

                    if (result)
                    {
                        // Update the session variable for profile image
                        Session["UserProfileImage"] = imageUrl;
                        TempData["SuccessMessage"] = "Profile picture updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update profile picture.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please select a valid image file.";
            }

            return RedirectToAction("AdminProfile");
        }

        // POST: AdminDashboard/ChangePassword - Change admin password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(int id, string CurrentPassword, string NewPassword)
        {
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword))
            {
                TempData["ErrorMessage"] = "Current password and new password are required.";
                return RedirectToAction("AdminProfile");
            }

            try
            {
                string userEmail = Session["UserEmail"] as string;

                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                bool result = _sessionBL.ChangeUserPassword(userEmail, CurrentPassword, NewPassword);

                if (result)
                {
                    TempData["SuccessMessage"] = "Password changed successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to change password. Please check your current password.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction("AdminProfile");
        }

        // GET: AdminDashboard/Products - Gestionare produse
        public ActionResult Products()
        {
            var products = _productBL.GetAllProducts();
            return View(products);
        }

        // GET: AdminDashboard/ProductCreate
        public ActionResult ProductCreate()
        {
            return View();
        }

        // POST: AdminDashboard/ProductCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductCreate(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int productId = _productBL.AddProduct(product);
                    TempData["SuccessMessage"] = "Product added successfully!";
                    return RedirectToAction("Products");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving: " + ex.Message);
                    return View(product);
                }
            }
            return View(product);
        }

        // GET: AdminDashboard/ProductEdit/5
        public ActionResult ProductEdit(int id)
        {
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: AdminDashboard/ProductEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductEdit(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _productBL.UpdateProduct(product);
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction("Products");
                }
                catch
                {
                    return View(product);
                }
            }
            return View(product);
        }

        // GET: AdminDashboard/ProductDelete/5
        public ActionResult ProductDelete(int id)
        {
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: AdminDashboard/ProductDelete/5
        [HttpPost, ActionName("ProductDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult ProductDeleteConfirmed(int id)
        {
            try
            {
                _productBL.DeleteProduct(id);
                TempData["SuccessMessage"] = "Product deleted successfully!";
                return RedirectToAction("Products");
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminDashboard/ProductDetails/5
        public ActionResult ProductDetails(int id)
        {
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Get additional product data if needed
            // For example, you might want to get orders related to this product

            return View(product);
        }

        // GET: AdminDashboard/UpdateStock/5
        public ActionResult UpdateStock(int id)
        {
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: AdminDashboard/UpdateStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStock(int id, int stockQuantity)
        {
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            product.StockQuantity = stockQuantity;
            product.LastUpdated = DateTime.Now;

            bool result = _productBL.UpdateProduct(product);

            if (result)
            {
                TempData["SuccessMessage"] = "Stock updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update stock.";
            }

            return RedirectToAction("ProductDetails", new { id = id });
        }

        // GET: AdminDashboard/ManagePromotions/5
        public ActionResult ManagePromotions(int id)
        {
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Obținem toate promoțiile pentru acest produs
            var promotions = _promotionBL.GetPromotionsForProduct(id);

            ViewBag.Product = product;
            ViewBag.PromotionTypes = new List<string> { "PercentOff", "FixedAmount", "BuyXGetY" };

            return View(promotions);
        }

        // GET: AdminDashboard/CreatePromotion/5
        public ActionResult CreatePromotion(int productId)
        {
            var product = _productBL.GetProductById(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Product = product;
            ViewBag.PromotionTypes = new List<string> { "PercentOff", "FixedAmount", "BuyXGetY" };

            var promotion = new Promotion
            {
                ProductId = productId,
                StartDate = DateTime.Now,
                IsActive = true
            };

            return View(promotion);
        }

        // POST: AdminDashboard/CreatePromotion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int promotionId = _promotionBL.AddPromotion(promotion);
                    TempData["SuccessMessage"] = "Promoție adăugată cu succes!";
                    return RedirectToAction("ManagePromotions", new { id = promotion.ProductId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Eroare la salvare: " + ex.Message);
                }
            }

            // Reîncarcă datele pentru view în caz de eroare
            var product = _productBL.GetProductById(promotion.ProductId);
            ViewBag.Product = product;
            ViewBag.PromotionTypes = new List<string> { "PercentOff", "FixedAmount", "BuyXGetY" };

            return View(promotion);
        }

        // GET: AdminDashboard/EditPromotion/5
        public ActionResult EditPromotion(int id)
        {
            var promotion = _promotionBL.GetPromotionById(id);
            if (promotion == null)
            {
                return HttpNotFound();
            }

            ViewBag.Product = promotion.Product;
            ViewBag.PromotionTypes = new List<string> { "PercentOff", "FixedAmount", "BuyXGetY" };

            return View(promotion);
        }

        // POST: AdminDashboard/EditPromotion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPromotion(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    promotion.LastUpdated = DateTime.Now;
                    bool result = _promotionBL.UpdatePromotion(promotion);

                    if (result)
                    {
                        TempData["SuccessMessage"] = "Promoție actualizată cu succes!";
                        return RedirectToAction("ManagePromotions", new { id = promotion.ProductId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Nu s-a putut actualiza promoția.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Eroare la actualizare: " + ex.Message);
                }
            }

            // Reîncarcă datele pentru view în caz de eroare
            var product = _productBL.GetProductById(promotion.ProductId);
            ViewBag.Product = product;
            ViewBag.PromotionTypes = new List<string> { "PercentOff", "FixedAmount", "BuyXGetY" };

            return View(promotion);
        }

        // POST: AdminDashboard/DeletePromotion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePromotion(int id, int productId)
        {
            try
            {
                bool result = _promotionBL.DeletePromotion(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Promoție ștearsă cu succes!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Nu s-a putut șterge promoția.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Eroare la ștergere: " + ex.Message;
            }

            return RedirectToAction("ManagePromotions", new { id = productId });
        }

        #region Blog Management

        // GET: AdminDashboard/BlogPosts
        public ActionResult BlogPosts()
        {
            try
            {
                var blogPosts = _blogBL.GetAllBlogPosts(includeUnpublished: true);

                // Separate posts by status for better organization
                ViewBag.PublishedPosts = blogPosts.Where(p => p.IsPublished).ToList();
                ViewBag.PendingPosts = blogPosts.Where(p => !p.IsPublished).ToList();

                return View(blogPosts);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading blog posts: " + ex.Message;
                return View(new List<BlogPost>());
            }
        }

        // GET: AdminDashboard/BlogCreate
        public ActionResult BlogCreate()
        {
            var model = new BlogPost
            {
                PublishDate = DateTime.Now,
                IsPublished = true
            };
            return View(model);
        }

        // POST: AdminDashboard/BlogCreate
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult BlogCreate(BlogPost blogPost, HttpPostedFileBase featuredImage)
        {
            // Allow HTML content only for Content field from trusted admin users
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
                        blogPost.Author = userProfile?.FullName ?? "Admin";
                    }
                    else
                    {
                        blogPost.Author = "Admin";
                    }

                    int blogPostId = _blogBL.AddBlogPost(blogPost);
                    TempData["SuccessMessage"] = "Blog post created successfully!";
                    return RedirectToAction("BlogPosts");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating blog post: " + ex.Message);
                }
            }
            return View(blogPost);
        }

        // GET: AdminDashboard/BlogEdit/5
        public ActionResult BlogEdit(int id)
        {
            try
            {
                var blogPost = _blogBL.GetBlogPostById(id);
                if (blogPost == null)
                {
                    return HttpNotFound();
                }
                return View(blogPost);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading blog post: " + ex.Message;
                return RedirectToAction("BlogPosts");
            }
        }

        // POST: AdminDashboard/BlogEdit/5
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult BlogEdit(BlogPost blogPost, HttpPostedFileBase featuredImage)
        {
            // Allow HTML content only for Content field from trusted admin users
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

                            // Delete old image if exists
                            if (!string.IsNullOrEmpty(blogPost.FeaturedImage))
                            {
                                var oldImagePath = Server.MapPath(blogPost.FeaturedImage);
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

                    bool result = _blogBL.UpdateBlogPost(blogPost);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Blog post updated successfully!";
                        return RedirectToAction("BlogPosts");
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

        // GET: AdminDashboard/BlogDelete/5
        public ActionResult BlogDelete(int id)
        {
            try
            {
                var blogPost = _blogBL.GetBlogPostById(id);
                if (blogPost == null)
                {
                    return HttpNotFound();
                }
                return View(blogPost);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading blog post: " + ex.Message;
                return RedirectToAction("BlogPosts");
            }
        }

        // POST: AdminDashboard/BlogDelete/5
        [HttpPost, ActionName("BlogDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult BlogDeleteConfirmed(int id)
        {
            try
            {
                var blogPost = _blogBL.GetBlogPostById(id);
                if (blogPost != null)
                {
                    // Delete featured image if exists
                    if (!string.IsNullOrEmpty(blogPost.FeaturedImage))
                    {
                        var imagePath = Server.MapPath(blogPost.FeaturedImage);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
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
            return RedirectToAction("BlogPosts");
        }

        // GET: AdminDashboard/BlogComments
        public ActionResult BlogComments()
        {
            try
            {
                var comments = _blogBL.GetRecentComments(50); // Get last 50 comments
                return View(comments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading comments: " + ex.Message;
                return View(new List<BlogComment>());
            }
        }

        // POST: AdminDashboard/ApproveComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveComment(int commentId)
        {
            try
            {
                bool result = _blogBL.ApproveComment(commentId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Comment approved successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve comment.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error approving comment: " + ex.Message;
            }
            return RedirectToAction("BlogComments");
        }

        // POST: AdminDashboard/DeleteComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(int commentId)
        {
            try
            {
                bool result = _blogBL.DeleteComment(commentId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Comment deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete comment.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting comment: " + ex.Message;
            }
            return RedirectToAction("BlogComments");
        }

        // POST: AdminDashboard/ApprovePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApprovePost(int id)
        {
            try
            {
                var blogPost = _blogBL.GetBlogPostById(id);
                if (blogPost != null)
                {
                    blogPost.IsPublished = true;
                    bool result = _blogBL.UpdateBlogPost(blogPost);

                    if (result)
                    {
                        TempData["SuccessMessage"] = $"Blog post '{blogPost.Title}' has been approved and published!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to approve blog post.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Blog post not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error approving blog post: " + ex.Message;
            }
            return RedirectToAction("BlogPosts");
        }

        // POST: AdminDashboard/RejectPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectPost(int id)
        {
            try
            {
                var blogPost = _blogBL.GetBlogPostById(id);
                if (blogPost != null)
                {
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
                        TempData["SuccessMessage"] = $"Blog post '{blogPost.Title}' has been rejected and deleted.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to reject blog post.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Blog post not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error rejecting blog post: " + ex.Message;
            }
            return RedirectToAction("BlogPosts");
        }

        // POST: AdminDashboard/ClearSampleBlogs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearSampleBlogs()
        {
            try
            {
                var allPosts = _blogBL.GetAllBlogPosts(includeUnpublished: true);
                int deletedCount = 0;

                foreach (var post in allPosts)
                {
                    // Delete featured image if exists
                    if (!string.IsNullOrEmpty(post.FeaturedImage))
                    {
                        var imagePath = Server.MapPath(post.FeaturedImage);
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    bool result = _blogBL.DeleteBlogPost(post.BlogPostId);
                    if (result)
                    {
                        deletedCount++;
                    }
                }

                TempData["SuccessMessage"] = $"Successfully deleted {deletedCount} sample blog posts!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error clearing sample blogs: " + ex.Message;
            }
            return RedirectToAction("BlogPosts");
        }

        #endregion
    }
}