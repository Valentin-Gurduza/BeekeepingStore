using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Filters;
using eUseControl.BeekeepingStore.Models;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlist _wishlistBL;
        private readonly IProduct _productBL;

        public WishlistController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _wishlistBL = bl.GetWishlistBL;
            _productBL = bl.GetProductBL;
        }

        // GET: Wishlist
        [UserMod]
        public ActionResult Index()
        {
            try
            {
                // Obținem ID-ul utilizatorului din sesiune
                int userId = Convert.ToInt32(Session["UserId"]);

                // Obținem produsele din wishlist
                var wishlistItems = _wishlistBL.GetUserWishlist(userId);

                // Convertim entitățile din baza de date în modelul pentru view
                var products = wishlistItems.Select(w => new Product
                {
                    Id = w.Product.ProductId.ToString(),
                    Name = w.Product.Name,
                    Description = w.Product.Description,
                    Price = w.Product.Price,
                    Category = w.Product.Category,
                    Image = !string.IsNullOrEmpty(w.Product.ImageUrl)
                            ? (w.Product.ImageUrl.StartsWith("http") ? w.Product.ImageUrl : Url.Content(w.Product.ImageUrl))
                            : Url.Content("~/Content/Images/products/default-product.png"),
                    DateAdded = w.DateAdded
                }).ToList();

                return View(products);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea listei de dorințe: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Wishlist/Add
        [HttpPost]
        [UserMod]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productId, string returnUrl)
        {
            try
            {
                // Obținem ID-ul utilizatorului din sesiune
                int userId = Convert.ToInt32(Session["UserId"]);

                // Adăugăm produsul în wishlist
                bool result = _wishlistBL.AddToWishlist(userId, productId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Produsul a fost adăugat în lista de dorințe.";
                }
                else
                {
                    TempData["InfoMessage"] = "Produsul este deja în lista de dorințe.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "A apărut o eroare la adăugarea produsului în lista de dorințe: " + ex.Message;
            }

            // Redirecționăm către pagina anterioară sau către pagina de produse
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Products", "Home");
        }

        // POST: Wishlist/Remove
        [HttpPost]
        [UserMod]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int productId, string returnUrl)
        {
            try
            {
                // Obținem ID-ul utilizatorului din sesiune
                int userId = Convert.ToInt32(Session["UserId"]);

                // Ștergem produsul din wishlist
                bool result = _wishlistBL.RemoveFromWishlist(userId, productId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Produsul a fost eliminat din lista de dorințe.";
                }
                else
                {
                    TempData["InfoMessage"] = "Produsul nu a fost găsit în lista de dorințe.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "A apărut o eroare la eliminarea produsului din lista de dorințe: " + ex.Message;
            }

            // Redirecționăm către pagina anterioară sau către pagina de wishlist
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index");
        }

        // AJAX: Wishlist/Check
        [HttpGet]
        [UserMod]
        public JsonResult Check(int productId)
        {
            try
            {
                // Obținem ID-ul utilizatorului din sesiune
                int userId = Convert.ToInt32(Session["UserId"]);

                // Verificăm dacă produsul este în wishlist
                bool isInWishlist = _wishlistBL.IsInWishlist(userId, productId);

                return Json(new { success = true, isInWishlist }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, isInWishlist = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}