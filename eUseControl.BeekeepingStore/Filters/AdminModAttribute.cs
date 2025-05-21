using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace eUseControl.BeekeepingStore.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AdminModAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
           
            if (filterContext.HttpContext.Session["UserIsAuthenticated"] == null ||
                !(bool)filterContext.HttpContext.Session["UserIsAuthenticated"])
            {
                
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "returnUrl", filterContext.HttpContext.Request.RawUrl }
                    }
                );
                return;
            }

            
            if (filterContext.HttpContext.Session["UserRole"] == null ||
                (filterContext.HttpContext.Session["UserRole"].ToString() != "Admin" &&
                 filterContext.HttpContext.Session["UserRole"].ToString() != "Moderator"))
            {
                
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