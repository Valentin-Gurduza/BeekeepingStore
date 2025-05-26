using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.BusinessLogic.Core;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.User;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
    /// <summary>
    /// SessionBL class that delegates all user and session operations to UserApi
    /// Following the delegation pattern: SessionBL : ISession -> UserApi (actual implementation)
    /// </summary>
    public class SessionBL : ISession
    {
        private readonly UserApi _userApi;

        public SessionBL()
        {
            _userApi = new UserApi();
        }

        public void CreateSession(UUserData data)
        {
            _userApi.CreateSession(data);
        }

        public void LogError(Exception ex)
        {
            // UserApi has its own LogError method, but we can also delegate this
            try
            {
                using (var context = new DataContext())
                {
                    var errorLog = new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.ErrorLogs.Add(errorLog);
                    context.SaveChanges();
                }
            }
            catch (Exception logEx)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log error: {logEx.Message}");
            }
        }

        public void LogoutUser(UUserData data)
        {
            _userApi.LogoutUser(data);
        }

        public void LogUserActivity(UUserData data, string activity)
        {
            _userApi.LogUserActivity(data, activity);
        }

        public void TerminateSession(string sessionId)
        {
            _userApi.TerminateSession(sessionId);
        }

        public bool ValidateSession(string sessionId)
        {
            return _userApi.ValidateSession(sessionId);
        }

        public int GetUserCount()
        {
            return _userApi.GetUserCount();
        }

        public List<UProfileData> GetFilteredUsers(string searchTerm, int page, int pageSize, out int totalCount)
        {
            return _userApi.GetFilteredUsers(searchTerm, page, pageSize, out totalCount);
        }

        public UProfileData GetUserById(int id)
        {
            return _userApi.GetUserById(id);
        }

        public List<UProfileData> GetRecentUsers(int count)
        {
            return _userApi.GetRecentUsers(count);
        }

        public bool UpdateUserStatus(int id, bool isActive)
        {
            return _userApi.UpdateUserStatus(id, isActive);
        }

        public UserLoginResult UserLogin(ULoginData data)
        {
            return _userApi.LoginUser(data);
        }

        public void RegisterUser(ULoginData data)
        {
            _userApi.RegisterUser(data);
        }

        public bool UpdateUserProfile(UProfileData data)
        {
            return _userApi.UpdateUserProfile(data);
        }

        public bool UpdateUserProfileImage(string userEmail, string imageUrl)
        {
            return _userApi.UpdateUserProfileImage(userEmail, imageUrl);
        }

        public bool ChangeUserPassword(string userEmail, string currentPassword, string newPassword)
        {
            return _userApi.ChangeUserPassword(userEmail, currentPassword, newPassword);
        }

        public UProfileData GetUserProfile(string username)
        {
            return _userApi.GetUserProfile(username);
        }
    }
}
