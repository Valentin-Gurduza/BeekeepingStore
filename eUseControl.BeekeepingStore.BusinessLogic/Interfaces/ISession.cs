using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.Domain.Entities.User;

namespace eUseControl.BeekeepingStore.BusinessLogic.Interfaces
{
     public interface ISession
     {
          UserLogin UserLogin(ULoginData data);
        void RegisterUser(ULoginData data);
        void UpdateUserProfile(UProfileData data);
        void LogoutUser(UUserData data);
        void CreateSession(UUserData data);
        bool ValidateSession(string sessionId);
        void TerminateSession(string sessionId);
        void LogError(Exception ex);
        void LogUserActivity(UUserData data, string activity);
    }
}
