using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using eUseControl.BeekeepingStore.Domain.Enums;
using eUseControl.BeekeepingStore.Filters;
using eUseControl.BeekeepingStore.Domain.Entities.User;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;

namespace eUseControl.BeekeepingStore.Controllers
{
    [AdminMod]
    public class AdminController : Controller
    {
        private readonly SessionBL _sessionBL;

        public AdminController()
        {
            _sessionBL = new SessionBL();
        }

        // GET: /Admin/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Admin/MakeAdmin
        [HttpGet]
        public ActionResult MakeAdmin(string username)
        {
            System.Diagnostics.Debug.WriteLine($"==== MakeAdmin GET called with username='{username}' ====");

            // Check if username was passed in TempData (from search methods)
            if (string.IsNullOrEmpty(username) && TempData["Username"] != null)
            {
                username = TempData["Username"].ToString();
                System.Diagnostics.Debug.WriteLine($"Found username in TempData: '{username}'");
            }

            if (string.IsNullOrEmpty(username))
            {
                System.Diagnostics.Debug.WriteLine("Username is empty, showing initial form");
                return View("MakeAdmin");
            }

            try
            {
                // Find the user by username and show confirmation page
                System.Diagnostics.Debug.WriteLine($"Finding user with username='{username}'");
                var user = _sessionBL.GetUserProfile(username);
                System.Diagnostics.Debug.WriteLine($"GetUserProfile result: {(user != null ? $"Found user {user.UserName}, ID={user.Id}, Level={user.Level}" : "Not Found")}");

                if (user == null)
                {
                    TempData["ErrorMessage"] = $"User '{username}' was not found.";
                    return View("MakeAdmin");
                }

                // Pass the username to the view as ViewBag to avoid view name confusion
                ViewBag.Username = username;
                System.Diagnostics.Debug.WriteLine($"Showing confirmation form for user '{username}'");
                return View("MakeAdmin");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in MakeAdmin GET: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error retrieving user information: {ex.Message}";
                return View("MakeAdmin");
            }
        }

        // POST: /Admin/MakeAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MakeAdmin(string username, FormCollection form)
        {
            System.Diagnostics.Debug.WriteLine($"==== MakeAdmin POST called with username='{username}' ====");

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Username cannot be empty.";
                return View("MakeAdmin");
            }

            try
            {
                // Find the user by username or email
                System.Diagnostics.Debug.WriteLine($"Finding user with username='{username}'");
                var user = _sessionBL.GetUserProfile(username);
                System.Diagnostics.Debug.WriteLine($"GetUserProfile result: {(user != null ? $"Found user {user.UserName}, ID={user.Id}, Level={user.Level}" : "Not Found")}");

                if (user != null)
                {
                    // Update the user profile with admin rights
                    System.Diagnostics.Debug.WriteLine($"Setting Level=400 for user '{username}'");
                    user.Level = 400; // Admin level (400)
                    bool result = _sessionBL.UpdateUserProfile(user);
                    System.Diagnostics.Debug.WriteLine($"UpdateUserProfile result: {(result ? "Success" : "Failed")}");

                    if (result)
                    {
                        TempData["SuccessMessage"] = $"User {username} has been granted administrator rights!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Unable to update user profile.";
                        ViewBag.Username = username;
                        return View("MakeAdmin");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"User {username} was not found.";
                    return View("MakeAdmin");
                }

                // Also try to add user to Roles if ASP.NET Membership is used
                try
                {
                    if (Roles.Enabled)
                    {
                        if (!Roles.RoleExists("Admin"))
                        {
                            Roles.CreateRole("Admin");
                        }

                        if (!Roles.GetRolesForUser(username).Contains("Admin"))
                        {
                            Roles.AddUserToRole(username, "Admin");
                            System.Diagnostics.Debug.WriteLine($"Added user '{username}' to Admin role in ASP.NET Membership");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but continue as we already updated the database
                    System.Diagnostics.Debug.WriteLine($"Role error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in MakeAdmin POST: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                ViewBag.Username = username;
                return View("MakeAdmin");
            }

            return RedirectToAction("Users", "AdminDashboard");
        }

        // GET: /Admin/MakeUser
        public ActionResult MakeUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return View();
            }

            try
            {
                // Find the user by username or email
                var user = _sessionBL.GetUserProfile(username);

                if (user != null)
                {
                    // Update the user profile with regular user rights
                    user.Level = 100; // Regular user level (100)
                    bool result = _sessionBL.UpdateUserProfile(user);

                    if (result)
                    {
                        TempData["SuccessMessage"] = $"User {username} has been granted regular user rights!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Unable to update user profile.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"User {username} was not found.";
                }

                // Also try to add user to Roles if ASP.NET Membership is used
                if (Roles.Enabled && !Roles.RoleExists("User"))
                {
                    Roles.CreateRole("User");
                }

                try
                {
                    Roles.AddUserToRole(username, "User");
                }
                catch (Exception ex)
                {
                    // Log but continue as we already updated the database
                    System.Diagnostics.Debug.WriteLine($"Role error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Users", "AdminDashboard");
        }

        // GET: /Admin/MakeVisitor
        public ActionResult MakeVisitor(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return View();
            }

            try
            {
                // Find the user by username or email
                var user = _sessionBL.GetUserProfile(username);

                if (user != null)
                {
                    // Update the user profile with visitor rights
                    user.Level = 50; // Visitor level
                    bool result = _sessionBL.UpdateUserProfile(user);

                    if (result)
                    {
                        TempData["SuccessMessage"] = $"User {username} has been granted visitor rights!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Unable to update user profile.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"User {username} was not found.";
                }

                // Also try to add user to Roles if ASP.NET Membership is used
                if (Roles.Enabled && !Roles.RoleExists("Visitor"))
                {
                    Roles.CreateRole("Visitor");
                }

                try
                {
                    Roles.AddUserToRole(username, "Visitor");
                }
                catch (Exception ex)
                {
                    // Log but continue as we already updated the database
                    System.Diagnostics.Debug.WriteLine($"Role error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Users", "AdminDashboard");
        }

        // GET: /Admin/CreateRoles
        public ActionResult CreateRoles()
        {
            try
            {
                // Create all roles if they don't exist
                if (!Roles.RoleExists("Admin"))
                    Roles.CreateRole("Admin");

                if (!Roles.RoleExists("User"))
                    Roles.CreateRole("User");

                if (!Roles.RoleExists("Visitor"))
                    Roles.CreateRole("Visitor");

                TempData["SuccessMessage"] = "All roles were created successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating roles: {ex.Message}";
            }

            return RedirectToAction("Users", "AdminDashboard");
        }

        // GET: /Admin/SetAdminCookie
        public ActionResult SetAdminCookie()
        {
            // Simple alternative for testing - set a cookie that indicates the Admin role
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,                              // version of the cookie
                "AdminUser",                    // username
                DateTime.Now,                   // start date
                DateTime.Now.AddHours(1),       // expiration date
                false,                          // persistent (saved)
                "Admin",                        // user data (role)
                FormsAuthentication.FormsCookiePath // cookie path
            );

            // Encrypt the cookie for security
            string encTicket = FormsAuthentication.Encrypt(ticket);

            // Add the cookie to the response
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

            return Content("Admin cookie set. You should now have access to administration pages.");
        }

        // GET: /Admin/SearchUser
        [HttpGet]
        public ActionResult SearchUser(string username, string actionName)
        {
            System.Diagnostics.Debug.WriteLine($"==== SearchUser called with username='{username}', actionName='{actionName}' ====");

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Username cannot be empty.";
                return RedirectToAction(actionName ?? "MakeAdmin");
            }

            try
            {
                // Find the user by username
                System.Diagnostics.Debug.WriteLine($"Calling GetUserProfile with username: '{username}'");
                var user = _sessionBL.GetUserProfile(username);
                System.Diagnostics.Debug.WriteLine($"GetUserProfile result: {(user != null ? $"Found user {user.UserName}, ID={user.Id}, Level={user.Level}" : "Not Found")}");

                if (user == null)
                {
                    TempData["ErrorMessage"] = $"User '{username}' was not found in the database.";
                    return RedirectToAction(actionName ?? "MakeAdmin");
                }

                // Display success message
                TempData["SuccessMessage"] = $"User '{username}' found. You can now proceed.";

                // Redirect to the specified action with the username in ViewBag
                System.Diagnostics.Debug.WriteLine($"Redirecting to {actionName ?? "MakeAdmin"} with username={username}");
                TempData["Username"] = username; // Use TempData to persist across redirects
                return RedirectToAction(actionName ?? "MakeAdmin");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SearchUser: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error searching for user: {ex.Message}";
                return RedirectToAction(actionName ?? "MakeAdmin");
            }
        }

        // GET: /Admin/SearchUserById
        [HttpGet]
        public ActionResult SearchUserById(int id, string actionName)
        {
            System.Diagnostics.Debug.WriteLine($"==== SearchUserById called with id={id}, actionName='{actionName}' ====");

            try
            {
                // Find the user by ID
                System.Diagnostics.Debug.WriteLine($"Calling GetUserById with id: {id}");
                var user = _sessionBL.GetUserById(id);
                System.Diagnostics.Debug.WriteLine($"GetUserById result: {(user != null ? $"Found user {user.UserName}, ID={user.Id}, Level={user.Level}" : "Not Found")}");

                if (user == null)
                {
                    TempData["ErrorMessage"] = $"User with ID {id} was not found in the database.";
                    return RedirectToAction(actionName ?? "MakeAdmin");
                }

                // Display success message
                TempData["SuccessMessage"] = $"User '{user.UserName}' found. You can now proceed.";

                // Redirect to the specified action with the username in TempData
                string username = user.UserName;
                System.Diagnostics.Debug.WriteLine($"Redirecting to {actionName ?? "MakeAdmin"} with username={username}");
                TempData["Username"] = username; // Use TempData to persist across redirects
                return RedirectToAction(actionName ?? "MakeAdmin");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SearchUserById: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error searching for user: {ex.Message}";
                return RedirectToAction(actionName ?? "MakeAdmin");
            }
        }
    }
}