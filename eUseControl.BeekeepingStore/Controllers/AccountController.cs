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
        private readonly BusinessLogic.BusinessLogic _businessLogic;

        public AccountController()
        {
            _businessLogic = new BusinessLogic.BusinessLogic();
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

                    var session = _businessLogic.GetSessionBL;
                    System.Diagnostics.Debug.WriteLine("Session object type: " + session.GetType().FullName);

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

                    System.Diagnostics.Debug.WriteLine("Calling session.UserLogin...");

                    try
                    {
                        // Login the user
                        var result = session.UserLogin(loginData);

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
                            session.CreateSession(userData);

                            // Înregistrăm activitatea utilizatorului
                            session.LogUserActivity(userData, "User logged in");

                            // Setăm autentificarea prin sesiune (evităm FormsAuthentication)
                            Session["UserIsAuthenticated"] = true;
                            Session["UserEmail"] = email;
                            Session["UserName"] = result.FullName;
                            Session["UserId"] = result.UserId;

                            // Setăm rolul utilizatorului, folosind o valoare implicită "User" dacă UserLevel nu este definit
                            if (result.UserLevel == 400)
                            {
                                Session["UserRole"] = "Admin";
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
                        ModelState.AddModelError("fullName", "Full Name must be at least 5 characters long.");
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

                    var session = _businessLogic.GetSessionBL;
                    System.Diagnostics.Debug.WriteLine("Session object type: " + session.GetType().FullName);

                    // Obținem adresa IP a utilizatorului
                    string userIp = GetUserIP();
                    System.Diagnostics.Debug.WriteLine($"User IP: {userIp}");

                    // Create a login data object with all required fields
                    var loginData = new ULoginData
                    {
                        Credential = email, // Using email as the credential
                        Password = password,
                        FullName = fullName, // Using fullName from form
                        LoginIp = userIp, // IP modificat
                        LoginDateTime = DateTime.Now
                    };

                    System.Diagnostics.Debug.WriteLine("Calling session.RegisterUser...");

                    try
                    {
                        // Register the user
                        session.RegisterUser(loginData);

                        System.Diagnostics.Debug.WriteLine("User registered successfully!");

                        // Redirect to login page after successful registration
                        return RedirectToAction("Login");
                    }
                    catch (System.Data.Entity.Core.EntityException dbEx)
                    {
                        System.Diagnostics.Debug.WriteLine("Entity Framework exception: " + dbEx.ToString());
                        if (dbEx.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Inner Exception: " + dbEx.InnerException.ToString());
                        }
                        ModelState.AddModelError("", "Database error: " + dbEx.Message);
                    }
                    catch (System.InvalidOperationException ioEx)
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid operation exception: " + ioEx.ToString());
                        ModelState.AddModelError("", "Configuration error: " + ioEx.Message);
                    }
                }
                catch (Exception ex)
                {
                    // Adăugare logging detaliat pentru depanare
                    System.Diagnostics.Debug.WriteLine("Error registering user: " + ex.ToString());
                    // Include și excepția internă dacă există
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.ToString());
                    }
                    ModelState.AddModelError("", "An error occurred while registering the user: " + ex.Message);
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

        // GET: Account/Profile
        [UserMod]
        public new ActionResult Profile()
        {
            try
            {
                // Verificăm dacă utilizatorul este autentificat folosind variabilele de sesiune
                if (Session["UserIsAuthenticated"] == null || !(bool)Session["UserIsAuthenticated"])
                {
                    return RedirectToAction("Login");
                }

                var sessionBL = _businessLogic.GetSessionBL;
                string userEmail = Session["UserEmail"] as string;

                // Obținem detaliile utilizatorului
                var currentUserProfile = sessionBL.GetUserProfile(userEmail);

                if (currentUserProfile == null)
                {
                    return RedirectToAction("Login");
                }

                return View(currentUserProfile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Profile: " + ex.ToString());
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea profilului: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Account/EditProfile
        [UserMod]
        public ActionResult EditProfile()
        {
            try
            {
                // Verificăm dacă utilizatorul este autentificat folosind variabilele de sesiune
                if (Session["UserIsAuthenticated"] == null || !(bool)Session["UserIsAuthenticated"])
                {
                    return RedirectToAction("Login");
                }

                var sessionBL = _businessLogic.GetSessionBL;
                string userEmail = Session["UserEmail"] as string;

                // Obținem detaliile utilizatorului
                var currentUserProfile = sessionBL.GetUserProfile(userEmail);

                if (currentUserProfile == null)
                {
                    return RedirectToAction("Login");
                }

                return View(currentUserProfile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in EditProfile: " + ex.ToString());
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea profilului: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        // POST: Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserMod]
        public ActionResult EditProfile(UProfileData model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vă rugăm să completați toate câmpurile obligatorii.";
                return View(model);
            }

            try
            {
                var sessionBL = _businessLogic.GetSessionBL;

                // Asigurăm-ne că avem ID-ul utilizatorului din sesiune
                if (Session["UserId"] == null)
                {
                    TempData["ErrorMessage"] = "Sesiunea dumneavoastră a expirat. Vă rugăm să vă autentificați din nou.";
                    return RedirectToAction("Login");
                }

                // Setăm ID-ul utilizatorului din sesiune
                model.UserId = Convert.ToInt32(Session["UserId"]);

                // Actualizăm profilul
                sessionBL.UpdateUserProfile(model);

                // Actualizăm și datele din sesiune
                Session["UserEmail"] = model.Email;
                Session["UserName"] = model.FullName;

                TempData["SuccessMessage"] = "Profilul dumneavoastră a fost actualizat cu succes!";
                return RedirectToAction("Profile");
            }
            catch (ArgumentNullException)
            {
                TempData["ErrorMessage"] = "Vă rugăm să completați toate câmpurile obligatorii.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException)
            {
                TempData["ErrorMessage"] = "Nu s-a putut găsi contul dumneavoastră. Vă rugăm să vă autentificați din nou.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in EditProfile POST: " + ex.ToString());
                TempData["ErrorMessage"] = "A apărut o eroare la actualizarea profilului. Vă rugăm să încercați din nou.";
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            try
            {
                // Obținem cookie-ul de sesiune pentru a identifica utilizatorul
                var userSessionCookie = Request.Cookies["sessionId"];
                if (userSessionCookie != null)
                {
                    var sessionBL = _businessLogic.GetSessionBL;

                    // Creăm un obiect cu datele utilizatorului pentru logout
                    var userData = new UUserData
                    {
                        SessionId = userSessionCookie.Value
                    };

                    // Închidem sesiunea
                    sessionBL.LogoutUser(userData);

                    // Ștergem cookie-ul
                    var expiredCookie = new HttpCookie("sessionId")
                    {
                        Expires = DateTime.Now.AddDays(-1)
                    };
                    Response.Cookies.Add(expiredCookie);
                }

                // Curățăm sesiunea
                Session.Clear();
                Session.Abandon();

                // Mesaj de succes
                TempData["SuccessMessage"] = "Ați fost deconectat cu succes!";

                // Redirecționăm către pagina de start
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Logout: " + ex.ToString());
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Account/Wishlist
        [UserMod]
        public ActionResult Wishlist()
        {
            try
            {
                // Verificăm dacă utilizatorul este autentificat folosind variabilele de sesiune
                if (Session["UserIsAuthenticated"] == null || !(bool)Session["UserIsAuthenticated"])
                {
                    return RedirectToAction("Login");
                }

                // Aici ar trebui să obținem lista de dorințe din baza de date
                // Pentru moment, afișăm doar o pagină goală
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Wishlist: " + ex.ToString());
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea listei de dorințe: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        // GET: Account/Orders
        [UserMod]
        public ActionResult Orders()
        {
            try
            {
                // Verificăm dacă utilizatorul este autentificat folosind variabilele de sesiune
                if (Session["UserIsAuthenticated"] == null || !(bool)Session["UserIsAuthenticated"])
                {
                    return RedirectToAction("Login");
                }

                // Aici ar trebui să obținem lista de comenzi din baza de date
                // Pentru moment, afișăm doar o pagină goală
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in Orders: " + ex.ToString());
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea comenzilor: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        // Metodă pentru a obține IP-ul utilizatorului într-un format utilizabil
        private string GetUserIP()
        {
            string ipAddress = Request.UserHostAddress;

            // Dacă este o adresă IPv6 localhost, transformăm în IPv4 pentru compatibilitate
            if (ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }

            // Dacă folosim un proxy, încercăm să obținem adresa reală
            if (Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }

            // Ne asigurăm că IP-ul nu depășește lungimea maximă din baza de date
            if (ipAddress?.Length > 30)
            {
                ipAddress = ipAddress.Substring(0, 30);
            }

            // Dacă din orice motiv nu am obținut un IP, folosim o valoare default
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "127.0.0.1";
            }

            return ipAddress;
        }
    }
}
