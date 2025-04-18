using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Product;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class ProductTestController : Controller
    {
        private readonly IProduct _productBL;

        public ProductTestController()
        {
            _productBL = new ProductBL();
        }

        // GET: ProductTest
        public ActionResult Index()
        {
            var products = _productBL.GetAllProducts();
            return View("~/Views/Product/Index.cshtml", products);
        }

        // GET: ProductTest/Create
        public ActionResult Create()
        {
            return View("~/Views/Product/Create.cshtml");
        }

        // POST: ProductTest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _productBL.AddProduct(product);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Eroare la salvare: " + ex.Message);
                    return View("~/Views/Product/Create.cshtml", product);
                }
            }
            return View("~/Views/Product/Create.cshtml", product);
        }
    }
}