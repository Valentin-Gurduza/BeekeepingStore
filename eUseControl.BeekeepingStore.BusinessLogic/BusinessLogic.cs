using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.User;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
    public class BusinessLogic
    {
        public ISession GetSessionBL => new SessionBL();
    }

    public class SessionBL : ISession
    {
        public void CreateSession(UUserData data)
        {
            throw new NotImplementedException();
        }

        public void LogError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void LogoutUser(UUserData data)
        {
            throw new NotImplementedException();
        }

        public void LogUserActivity(UUserData data, string activity)
        {
            throw new NotImplementedException();
        }

        public void RegisterUser(ULoginData data)
        {
            throw new NotImplementedException();
        }

        public void TerminateSession(string sessionId)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserProfile(UProfileData data)
        {
            throw new NotImplementedException();
        }

        public UserLoginResult UserLogin(ULoginData data)
        {
            if (data.Credential == "validUser" && data.Password == "validPassword")
            {
                return new UserLoginResult
                {
                    Status = true,
                    StatusMsg = "Login success"
                };
            }
            else
            {
                return new UserLoginResult
                {
                    Status = false,
                    StatusMsg = "Login failed"
                };
            }
        }

        public bool ValidateSession(string sessionId)
        {
            throw new NotImplementedException();
        }
    }
}
