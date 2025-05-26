using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.BusinessLogic.Core;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.User;
using eUseControl.BeekeepingStore.Filters;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ISession _sessionBL;
        private readonly IOrder _orderBL;
        private readonly IWishlist _wishlistBL;

        public AccountController()
        {
            // Use the main BusinessLogic class to get properly configured instances
            var businessLogic = new BusinessLogic.BusinessLogic();
            _sessionBL = businessLogic.GetSessionBL;
            _orderBL = businessLogic.GetOrderBL;
            _wishlistBL = businessLogic.GetWishlistBL;
        }

        // Alternative constructor for dependency injection (if DI container is used)
        public AccountController(ISession sessionBL, IOrder orderBL, IWishlist wishlistBL)
        {
            _sessionBL = sessionBL ?? throw new ArgumentNullException(nameof(sessionBL));
            _orderBL = orderBL ?? throw new ArgumentNullException(nameof(orderBL));
            _wishlistBL = wishlistBL ?? throw new ArgumentNullException(nameof(wishlistBL));
        }

        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            // Adaugă debugging pentru parametrii de intrare
            System.Diagnostics.Debug.WriteLine("Login parameters:");
            System.Diagnostics.Debug.WriteLine($"email: {email ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"password: {password ?? "null"}");

            if (ModelState.IsValid)
            {
                try
                {
                    // Validare manuală înainte de a trimite
                    if (string.IsNullOrEmpty(email))
                    {
                        ModelState.AddModelError("email", "Email is required.");
                        return View();
                    }

                    if (string.IsNullOrEmpty(password))
                    {
                        ModelState.AddModelError("password", "Password is required.");
                        return View();
                    }

                    // Use the injected interface instead of accessing through properties
                    System.Diagnostics.Debug.WriteLine("Session object type: " + _sessionBL.GetType().FullName);

                    // Obținem adresa IP a utilizatorului
                    string userIp = GetUserIP();
                    System.Diagnostics.Debug.WriteLine($"User IP: {userIp}");

                    // Create a login data object with all required fields
                    var loginData = new ULoginData
                    {
                        Credential = email, // Using email as the credential
                        Password = password,
                        LoginIp = userIp,
                        LoginDateTime = DateTime.Now
                    };

                    System.Diagnostics.Debug.WriteLine("Calling _sessionBL.UserLogin...");

                    try
                    {
                        // Login the user using the interface
                        var result = _sessionBL.UserLogin(loginData);

                        if (result.Success)
                        {
                            System.Diagnostics.Debug.WriteLine("User logged in successfully!");

                            // Salvăm sesiunea în cookie pentru a păstra utilizatorul autentificat
                            var userData = new UUserData
                            {
                                UserId = result.UserId,
                                FullName = result.FullName,
                                SessionId = result.SessionId,
                                Username = email
                            };

                            // Creem o sesiune pentru utilizator
                            _sessionBL.CreateSession(userData);

                            // Înregistrăm activitatea utilizatorului
                            _sessionBL.LogUserActivity(userData, "User logged in");

                            // Setăm autentificarea prin sesiune (evităm FormsAuthentication)
                            Session["UserIsAuthenticated"] = true;
                            Session["UserEmail"] = email;
                            Session["UserName"] = result.FullName;
                            Session["UserId"] = result.UserId;

                            // Get user profile to get the profile image
                            var userProfile = _sessionBL.GetUserProfile(email);
                            if (userProfile != null && !string.IsNullOrEmpty(userProfile.ProfileImage))
                            {
                                Session["UserProfileImage"] = userProfile.ProfileImage;
                            }

                            // Setăm rolul utilizatorului, folosind o valoare implicită "User" dacă UserLevel nu este definit
                            if (result.UserLevel >= 400)
                            {
                                Session["UserRole"] = "Admin";
                            }
                            else if (result.UserLevel >= 200)
                            {
                                Session["UserRole"] = "Moderator";
                            }
                            else
                            {
                                Session["UserRole"] = "User";
                            }

                            // Adăugăm un cookie de sesiune pentru identificarea utilizatorului
                            HttpCookie sessionCookie = new HttpCookie("sessionId", result.SessionId);
                            sessionCookie.Expires = DateTime.Now.AddHours(1); // cookie valid pentru 1 oră
                            Response.Cookies.Add(sessionCookie);

                            // Redirect to home page after successful login
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Login failed: {result.StatusMsg}");
                            ModelState.AddModelError("", "Invalid email or password. Please try again.");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Login exception: " + ex.ToString());
                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.ToString());
                        }
                        ModelState.AddModelError("", "An error occurred while logging in: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    // Adăugare logging detaliat pentru depanare
                    System.Diagnostics.Debug.WriteLine("Error in login: " + ex.ToString());
                    // Include și excepția internă dacă există
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.ToString());
                    }
                    ModelState.AddModelError("", "An error occurred: " + ex.Message);
                }
            }
            else
            {
                // Log model state errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Model Error: {error.ErrorMessage}");
                    }
                }
            }

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
            // Adaugă debugging pentru parametrii de intrare
            System.Diagnostics.Debug.WriteLine("Received parameters:");
            System.Diagnostics.Debug.WriteLine($"fullName: {fullName ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"email: {email ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"password: {password ?? "null"}");

            if (ModelState.IsValid)
            {
                try
                {
                    // Validează datele manual înainte de a le trimite
                    if (string.IsNullOrEmpty(fullName) || fullName.Length < 5)
                    {
                        ModelState.AddModelError("fullName", "Full name must be at least 5 characters long.");
                        return View();
                    }

                    if (string.IsNullOrEmpty(email) || email.Length < 8)
                    {
                        ModelState.AddModelError("email", "Email must be at least 8 characters long.");
                        return View();
                    }

                    if (string.IsNullOrEmpty(password) || password.Length < 8)
                    {
                        ModelState.AddModelError("password", "Password must be at least 8 characters long.");
                        return View();
                    }

                    // Obținem adresa IP a utilizatorului
                    string userIp = GetUserIP();
                    System.Diagnostics.Debug.WriteLine($"User IP: {userIp}");

                    // Create a registration data object
                    var registrationData = new ULoginData
                    {
                        FullName = fullName,
                        Credential = email,
                        Password = password,
                        LoginIp = userIp,
                        LoginDateTime = DateTime.Now
                    };

                    System.Diagnostics.Debug.WriteLine("Calling _sessionBL.RegisterUser...");

                    // Register the user using the interface
                    _sessionBL.RegisterUser(registrationData);

                    System.Diagnostics.Debug.WriteLine("User registered successfully!");

                    // Set success message and redirect to login
                    TempData["SuccessMessage"] = "Registration successful! Please log in with your credentials.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Registration error: " + ex.ToString());
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.ToString());
                    }
                    ModelState.AddModelError("", "Registration failed: " + ex.Message);
                }
            }
            else
            {
                // Log model state errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Model Error: {error.ErrorMessage}");
                    }
                }
            }

            return View();
        }

        // GET: Profile
        [UserMod]
        public new ActionResult Profile()
        {
            try
            {
                string userEmail = Session["UserEmail"]?.ToString();
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login");
                }

                // Use the interface to get user profile
                var userProfile = _sessionBL.GetUserProfile(userEmail);
                if (userProfile == null)
                {
                    TempData["ErrorMessage"] = "User profile not found.";
                    return RedirectToAction("Login");
                }

                return View(userProfile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Profile: " + ex.ToString());
                TempData["ErrorMessage"] = "Error loading profile: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: EditProfile
        [UserMod]
        public ActionResult EditProfile()
        {
            try
            {
                string userEmail = Session["UserEmail"]?.ToString();
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login");
                }

                // Use the interface to get user profile
                var userProfile = _sessionBL.GetUserProfile(userEmail);
                if (userProfile == null)
                {
                    TempData["ErrorMessage"] = "User profile not found.";
                    return RedirectToAction("Login");
                }

                return View(userProfile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in EditProfile: " + ex.ToString());
                TempData["ErrorMessage"] = "Error loading profile: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        // POST: EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserMod]
        public ActionResult EditProfile(UProfileData model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Use the interface to update user profile
                    bool result = _sessionBL.UpdateUserProfile(model);

                    if (result)
                    {
                        TempData["SuccessMessage"] = "Profile updated successfully!";

                        // Update session data if name changed
                        if (!string.IsNullOrEmpty(model.FullName))
                        {
                            Session["UserName"] = model.FullName;
                        }

                        return RedirectToAction("Profile");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update profile. Please try again.");
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in EditProfile POST: " + ex.ToString());
                ModelState.AddModelError("", "Error updating profile: " + ex.Message);
                return View(model);
            }
        }

        // GET: Logout
        public ActionResult Logout()
        {
            try
            {
                // Get user data from session
                var userId = Session["UserId"];
                var sessionId = Request.Cookies["sessionId"]?.Value;

                if (userId != null && !string.IsNullOrEmpty(sessionId))
                {
                    var userData = new UUserData
                    {
                        UserId = Convert.ToInt32(userId),
                        SessionId = sessionId
                    };

                    // Use the interface to logout user
                    _sessionBL.LogoutUser(userData);
                }

                // Clear all session data
                Session.Clear();
                Session.Abandon();

                // Remove session cookie
                if (Request.Cookies["sessionId"] != null)
                {
                    var cookie = new HttpCookie("sessionId");
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(cookie);
                }

                TempData["SuccessMessage"] = "You have been logged out successfully.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Logout: " + ex.ToString());
                // Still clear session even if logout fails
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Wishlist
        [UserMod]
        public ActionResult Wishlist()
        {
            try
            {
                var userId = Session["UserId"];
                if (userId == null)
                {
                    return RedirectToAction("Login");
                }

                // Use the interface to get wishlist items
                var wishlistItems = _wishlistBL.GetUserWishlist(Convert.ToInt32(userId));
                return View(wishlistItems);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Wishlist: " + ex.ToString());
                TempData["ErrorMessage"] = "Error loading wishlist: " + ex.Message;
                return View(new List<object>()); // Return empty list on error
            }
        }

        // GET: Orders
        [UserMod]
        public ActionResult Orders()
        {
            try
            {
                var userId = Session["UserId"];
                if (userId == null)
                {
                    return RedirectToAction("Login");
                }

                // Use the interface to get user orders
                var orders = _orderBL.GetOrdersByUserId(Convert.ToInt32(userId));
                return View(orders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Orders: " + ex.ToString());
                TempData["ErrorMessage"] = "Error loading orders: " + ex.Message;
                return View(new List<object>()); // Return empty list on error
            }
        }

        private string GetUserIP()
        {
            string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return Request.ServerVariables["REMOTE_ADDR"];
        }
    }
}
