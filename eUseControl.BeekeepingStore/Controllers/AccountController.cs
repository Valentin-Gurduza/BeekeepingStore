using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string fullName, string email, string password)
        {
            if (ModelState.IsValid)
            {
                // Add logic to handle registration process, e.g., save user data to the database
                // For now, just redirect to the login page
                return RedirectToAction("Login");
            }

            return View();
        }
    }
}