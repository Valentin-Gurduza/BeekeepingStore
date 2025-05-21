using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.Models;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProduct _productBL;

        public HomeController()
        {
            _productBL = new ProductBL();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Products()
        {
            // Obținem toate produsele din baza de date
            var dbProducts = _productBL.GetAllProducts();

            // Convertim entitățile din baza de date în modelul pentru view
            var products = dbProducts.Select(p => new Product
            {
                Id = p.ProductId.ToString(),
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Category = p.Category,
                DateAdded = p.DateAdded,
                Image = !string.IsNullOrEmpty(p.ImageUrl)
                        ? (p.ImageUrl.StartsWith("http") ? p.ImageUrl : Url.Content(p.ImageUrl))
                        : Url.Content("~/Content/Images/products/default-product.png")
            }).ToList();

            return View(products);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Blog()
        {
            // Redirect to the Blog controller's Index action
            return RedirectToAction("Index", "Blog");
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult Cart()
        {
            return View();
        }

        public ActionResult Checkout()
        {
            return View();
        }
    }
}
