using System;
using System.Web.Mvc;

namespace eUseControl.BeekeepingStore.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class VisitorModAttribute : ActionFilterAttribute
    {
        // Acest atribut nu face nicio restricție - permite accesul tuturor utilizatorilor,
        // inclusiv celor neautentificați (vizitatorilor)
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Permite accesul tuturor
            base.OnActionExecuting(filterContext);
        }
    }
}