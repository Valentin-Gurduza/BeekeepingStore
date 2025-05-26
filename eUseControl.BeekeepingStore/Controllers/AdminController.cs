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
        private readonly ISession _sessionBL;

        public AdminController()
        {
            // Use the main BusinessLogic class to get properly configured instances
            var businessLogic = new BusinessLogic.BusinessLogic();
            _sessionBL = businessLogic.GetSessionBL;
        }

        // Alternative constructor for dependency injection (if DI container is used)
        public AdminController(ISession sessionBL)
        {
            _sessionBL = sessionBL ?? throw new ArgumentNullException(nameof(sessionBL));
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
                // Find the user by username or email using the interface
                System.Diagnostics.Debug.WriteLine($"Finding user with username='{username}'");
                var user = _sessionBL.GetUserProfile(username);
                System.Diagnostics.Debug.WriteLine($"GetUserProfile result: {(user != null ? $"Found user {user.UserName}, ID={user.Id}, Level={user.Level}" : "Not Found")}");

                if (user != null)
                {
                    // Update the user profile with admin rights using the interface
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
                // Find the user by username or email using the interface
                var user = _sessionBL.GetUserProfile(username);

                if (user != null)
                {
                    // Update the user profile with regular user rights using the interface
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
                System.Diagnostics.Debug.WriteLine($"ERROR in MakeUser: {ex.Message}\n{ex.StackTrace}");
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
                // Find the user by username or email using the interface
                var user = _sessionBL.GetUserProfile(username);

                if (user != null)
                {
                    // Update the user profile with visitor rights using the interface
                    user.Level = 50; // Visitor level (50)
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
                System.Diagnostics.Debug.WriteLine($"ERROR in MakeVisitor: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Users", "AdminDashboard");
        }

        // GET: /Admin/CreateRoles
        public ActionResult CreateRoles()
        {
            try
            {
                if (Roles.Enabled)
                {
                    // Create standard roles if they don't exist
                    string[] rolesToCreate = { "Admin", "Moderator", "User", "Visitor" };

                    foreach (string role in rolesToCreate)
                    {
                        if (!Roles.RoleExists(role))
                        {
                            Roles.CreateRole(role);
                            System.Diagnostics.Debug.WriteLine($"Created role: {role}");
                        }
                    }

                    TempData["SuccessMessage"] = "All standard roles have been created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Role management is not enabled in this application.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in CreateRoles: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error creating roles: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: /Admin/SetAdminCookie
        public ActionResult SetAdminCookie()
        {
            try
            {
                // Set an admin cookie for testing purposes
                HttpCookie adminCookie = new HttpCookie("AdminAccess", "true");
                adminCookie.Expires = DateTime.Now.AddHours(24); // Valid for 24 hours
                Response.Cookies.Add(adminCookie);

                TempData["SuccessMessage"] = "Admin cookie has been set successfully!";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SetAdminCookie: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error setting admin cookie: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: /Admin/SearchUser
        [HttpGet]
        public ActionResult SearchUser(string username, string actionName)
        {
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Please provide a username to search.";
                return RedirectToAction("Index");
            }

            try
            {
                // Search for user using the interface
                var user = _sessionBL.GetUserProfile(username);

                if (user != null)
                {
                    // Store username in TempData for the target action
                    TempData["Username"] = username;

                    // Redirect to the specified action
                    if (!string.IsNullOrEmpty(actionName))
                    {
                        return RedirectToAction(actionName);
                    }
                    else
                    {
                        return RedirectToAction("MakeAdmin");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"User '{username}' was not found.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SearchUser: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error searching for user: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: /Admin/SearchUserById
        [HttpGet]
        public ActionResult SearchUserById(int id, string actionName)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Please provide a valid user ID to search.";
                return RedirectToAction("Index");
            }

            try
            {
                // Search for user by ID using the interface
                var user = _sessionBL.GetUserById(id);

                if (user != null)
                {
                    // Store username in TempData for the target action
                    TempData["Username"] = user.UserName;

                    // Redirect to the specified action
                    if (!string.IsNullOrEmpty(actionName))
                    {
                        return RedirectToAction(actionName);
                    }
                    else
                    {
                        return RedirectToAction("MakeAdmin");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"User with ID '{id}' was not found.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in SearchUserById: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = $"Error searching for user: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}