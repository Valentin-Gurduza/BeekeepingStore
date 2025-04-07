using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace eUseControl.BeekeepingStore
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Home",
                url: "home",
                defaults: new { controller = "Home", action = "Index" }
                );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                );

                routes.MapRoute(
                name: "About",
                url: "about",
                defaults: new { controller = "Home", action = "About" }
            );

            routes.MapRoute(
                name: "Blog",
                url: "blog",
                defaults: new { controller = "Home", action = "Blog" }
            );

            routes.MapRoute(
                name: "BlogDetails",
                url: "blog-details",
                defaults: new { controller = "Home", action = "BlogDetails" }
            );

            routes.MapRoute(
                name: "Contact",
                url: "contact",
                defaults: new { controller = "Home", action = "Contact" }
            );

            routes.MapRoute(
            name: "Cart",
            url: "cart",
                    defaults: new { controller = "Home", action = "Cart" }
            );

            routes.MapRoute(
               name: "Login",
               url: "login",
               defaults: new { controller = "Home", action = "Login" }
           );

            routes.MapRoute(
                name: "Register",
                url: "register",
                defaults: new { controller = "Home", action = "Register" }
            );
        }
    }
}

