using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.BusinessLogic.Core;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;


namespace eUseControl.BeekeepingStore.BusinessLogic
{
    public class UserApi
    {
        public void RegisterUser()
        { }
        public void LoginUser()
        { }
        public void UpdateUserProfile()
        { }
    }
    public class SessionBL : UserApi, ISession
    {
        public UserLogin UserLogin(ULoginData data)
        {
            if (data.Credential == "admin" && data.Password == "admin")
            {
                return new UserLogin { IsSuccess = true, Message = "Login successful" };
            }
            else
            {
                return new UserLogin { IsSuccess = false, Message = "Invalid credentials" };
            }
        }
    }
}