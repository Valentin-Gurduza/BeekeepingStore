using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace eUseControl.BeekeepingStore.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class UserModAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Verifică dacă utilizatorul este autentificat folosind variabilele de sesiune
            var httpContext = filterContext.HttpContext;
            var isAuthenticated = httpContext.Session["UserIsAuthenticated"] != null &&
                                  (bool)httpContext.Session["UserIsAuthenticated"] == true;

            if (!isAuthenticated)
            {
                // Utilizatorul nu este autentificat, redirecționează către pagina de login
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "returnUrl", filterContext.HttpContext.Request.RawUrl }
                    }
                );
                return;
            }

            // Verifică dacă utilizatorul are cel puțin rolul de User folosind variabilele de sesiune
            var userRole = httpContext.Session["UserRole"] as string;
            bool hasRequiredRole = userRole != null &&
                                  (userRole == "User" || userRole == "Admin" ||
                                   userRole == "Administrator" || userRole == "Moderator");

            if (!hasRequiredRole)
            {
                // Utilizatorul nu are rolul necesar, redirecționează către o pagină de acces refuzat
                filterContext.Result = new ViewResult
                {
                    ViewName = "AccessDenied"
                };
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}