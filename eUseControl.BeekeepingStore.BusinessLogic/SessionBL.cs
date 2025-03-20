using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.BusinessLogic.Core;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.User;

    public class SessionBL : UserApi, ISession
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

    public bool ValidateSession(string sessionId)
    {
        throw new NotImplementedException();
    }
}
