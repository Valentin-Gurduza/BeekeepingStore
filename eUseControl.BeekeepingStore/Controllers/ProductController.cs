using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Product;
using eUseControl.BeekeepingStore.Filters;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProduct _productBL;

        public ProductController()
        {
            _productBL = new ProductBL();
        }

        // GET: Product
        [AdminMod]
        public ActionResult Index()
        {
            var products = _productBL.GetAllProducts();
            return View(products);
        }

        // GET: Product/Details/5
        public ActionResult Details(int id)
        {
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Product/Create
        [AdminMod]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminMod]
        public ActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int productId = _productBL.AddProduct(product);
                    TempData["SuccessMessage"] = "Produsul a fost adăugat cu succes!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Eroare la salvare: " + ex.Message);
                    return View(product);
                }
            }
            return View(product);
        }

        // GET: Product/Edit/5
        [AdminMod]
        public ActionResult Edit(int id)
        {
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminMod]
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _productBL.UpdateProduct(product);
                    TempData["SuccessMessage"] = "Produsul a fost actualizat cu succes!";
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View(product);
                }
            }
            return View(product);
        }

        // GET: Product/Delete/5
        [AdminMod]
        public ActionResult Delete(int id)
        {
            var product = _productBL.GetProductById(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminMod]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                _productBL.DeleteProduct(id);
                TempData["SuccessMessage"] = "Produsul a fost șters cu succes!";
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Product/Category/Honey
        public ActionResult Category(string id)
        {
            var products = _productBL.GetProductsByCategory(id);
            ViewBag.Category = id;
            return View(products);
        }

        // GET: Product/Search
        public ActionResult Search(string searchTerm, decimal? minPrice, decimal? maxPrice)
        {
            var products = _productBL.GetFilteredProducts(minPrice, maxPrice, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            return View(products);
        }
    }
}