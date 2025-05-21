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
        UserLoginResult UserLogin(ULoginData data);
        void RegisterUser(ULoginData data);
        bool UpdateUserProfile(UProfileData data);
        bool UpdateUserProfileImage(string userEmail, string imageUrl);
        bool ChangeUserPassword(string userEmail, string currentPassword, string newPassword);
        void LogoutUser(UUserData data);
        void CreateSession(UUserData data);
        bool ValidateSession(string sessionId);
        void TerminateSession(string sessionId);
        void LogError(Exception ex);
        void LogUserActivity(UUserData data, string activity);
        UProfileData GetUserProfile(string username);
        // Add these methods to support the admin dashboard
        int GetUserCount();
        List<UProfileData> GetRecentUsers(int count);
        List<UProfileData> GetFilteredUsers(string searchTerm, int page, int pageSize, out int totalCount);
        UProfileData GetUserById(int id);
        bool UpdateUserStatus(int id, bool isActive);
    }
}
