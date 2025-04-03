using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.Domain.Entities.User;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.BusinessLogic.Core;

namespace eUseControl.BeekeepingStore.Controllers
{
    public class LoginController : Controller
    {
        private readonly ISession _session;

        public LoginController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _session = bl.GetSessionBL;
        }

        //GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(UserLogin login)
        {
            if (ModelState.IsValid)
            {
                ULoginData data = new ULoginData
                {
                    Credential = login.Credential,
                    Password = login.Password,
                    LoginIp = Request.UserHostAddress,
                    LoginDateTime = DateTime.Now
                };
                try
                {
                    var userLogin = _session.UserLogin(data);
                    if(userLogin.Status)
                    {
                        //ADD COOKIE
                        var sessionCookie = new HttpCookie("sessionId", userLogin.SessionId)
                        {
                            Expires = DateTime.Now.AddHours(1),
                            HttpOnly = true
                        };
                        Response.Cookies.Add(sessionCookie);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", userLogin.StatusMsg);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while logging in: " + ex.Message);
                }
            }
            return View();
        }
    }
}
