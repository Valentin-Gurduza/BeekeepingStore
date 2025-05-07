using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using eUseControl.BeekeepingStore.Domain.Enums;
using eUseControl.BeekeepingStore.Filters;

namespace eUseControl.BeekeepingStore.Controllers
{
    [AdminMod]
    public class AdminController : Controller
    {
        // GET: /Admin/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Admin/MakeAdmin
        public ActionResult MakeAdmin(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return View();
            }

            // Adaugă rolul de Admin pentru utilizatorul specificat
            if (!Roles.RoleExists("Admin"))
            {
                Roles.CreateRole("Admin");
            }

            try
            {
                Roles.AddUserToRole(username, "Admin");
                TempData["SuccessMessage"] = $"Utilizatorul {username} a primit rolul de Admin!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Eroare: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: /Admin/MakeUser
        public ActionResult MakeUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return View();
            }

            // Adaugă rolul de User pentru utilizatorul specificat
            if (!Roles.RoleExists("User"))
            {
                Roles.CreateRole("User");
            }

            try
            {
                Roles.AddUserToRole(username, "User");
                TempData["SuccessMessage"] = $"Utilizatorul {username} a primit rolul de User!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Eroare: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: /Admin/MakeVisitor
        public ActionResult MakeVisitor(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return View();
            }

            // Adaugă rolul de Visitor pentru utilizatorul specificat
            if (!Roles.RoleExists("Visitor"))
            {
                Roles.CreateRole("Visitor");
            }

            try
            {
                Roles.AddUserToRole(username, "Visitor");
                TempData["SuccessMessage"] = $"Utilizatorul {username} a primit rolul de Visitor!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Eroare: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: /Admin/CreateRoles
        public ActionResult CreateRoles()
        {
            try
            {
                // Creează toate rolurile dacă nu există
                if (!Roles.RoleExists("Admin"))
                    Roles.CreateRole("Admin");

                if (!Roles.RoleExists("User"))
                    Roles.CreateRole("User");

                if (!Roles.RoleExists("Visitor"))
                    Roles.CreateRole("Visitor");

                TempData["SuccessMessage"] = "Toate rolurile au fost create cu succes!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Eroare la crearea rolurilor: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: /Admin/SetAdminCookie
        public ActionResult SetAdminCookie()
        {
            // Alternativa simplă pentru testare - setează un cookie care indică rolul Admin
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,                              // versiunea cookie-ului
                "AdminUser",                    // numele utilizatorului
                DateTime.Now,                   // data de start
                DateTime.Now.AddHours(1),       // data de expirare
                false,                          // persistent (salvat)
                "Admin",                        // date utilizator (rol)
                FormsAuthentication.FormsCookiePath // cookie path
            );

            // Criptează cookie-ul pentru securitate
            string encTicket = FormsAuthentication.Encrypt(ticket);

            // Adaugă cookie-ul în răspuns
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

            return Content("Cookie de admin setat. Acum ar trebui să ai acces la paginile de administrare.");
        }
    }
}