using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.User;
using System.Security.Cryptography;

namespace eUseControl.BeekeepingStore.BusinessLogic.Core
{
    public class UserApi
    {
        public void RegisterUser(ULoginData data)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Validate input data
                    if (string.IsNullOrEmpty(data.Credential) || data.Credential.Length < 8)
                    {
                        throw new Exception("Email must be at least 8 characters long");
                    }

                    if (string.IsNullOrEmpty(data.Password) || data.Password.Length < 8)
                    {
                        throw new Exception("Password must be at least 8 characters long");
                    }

                    // Check if user already exists by email
                    if (context.UDBTables.Any(u => u.Email == data.Credential))
                    {
                        throw new Exception("Email already exists in the system");
                    }

                    // Ensure UserIp meets validation requirements (min 7 characters)
                    string validUserIp = data.LoginIp;
                    if (string.IsNullOrEmpty(validUserIp) || validUserIp.Length < 7)
                    {
                        validUserIp = "127.0.0.1"; // Default valid IP
                    }

                    // Ensure UserName meets validation requirements (min 5 characters)
                    // Use FullName if provided, otherwise use email as fallback
                    string validUserName = !string.IsNullOrEmpty(data.FullName) ? data.FullName : data.Credential;
                    if (validUserName.Length < 5)
                    {
                        validUserName = validUserName.PadRight(5, '0'); // Pad with zeros if too short
                    }

                    // Create new user in UDBTables
                    var newUser = new UDBTable
                    {
                        UserName = validUserName,
                        Email = data.Credential,
                        Password = HashPassword(data.Password),
                        Last_Login = DateTime.Now,
                        UserIp = validUserIp,
                        Level = 100, // Default level for regular users (100 = User role)
                        RegisterDate = DateTime.Now
                    };

                    context.UDBTables.Add(newUser);
                    SaveChangesWithValidation(context);

                    // Also add to Users table for backward compatibility
                    var legacyUser = new User
                    {
                        Username = validUserName,
                        Password = HashPassword(data.Password),
                        CreatedAt = DateTime.UtcNow,
                        FullName = data.FullName ?? validUserName,
                        Email = data.Credential,
                        UserIp = validUserIp
                    };
                    context.Users.Add(legacyUser);
                    SaveChangesWithValidation(context);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public UserLoginResult LoginUser(ULoginData data)
        {
            try
            {
                // Calculate password hash outside the query
                string hashedPassword = HashPassword(data.Password);

                using (var context = new DataContext())
                {
                    // Try to find user in UDBTables first
                    var udbUser = context.UDBTables.FirstOrDefault(u =>
                        (u.Email == data.Credential) &&
                        u.Password == hashedPassword);

                    if (udbUser != null)
                    {
                        // Update Last_Login and UserIp with validation
                        udbUser.Last_Login = DateTime.Now;

                        // Ensure UserIp meets validation requirements (min 7 characters)
                        string validUserIp = data.LoginIp;
                        if (string.IsNullOrEmpty(validUserIp) || validUserIp.Length < 7)
                        {
                            validUserIp = "127.0.0.1"; // Default valid IP
                        }
                        udbUser.UserIp = validUserIp;

                        SaveChangesWithValidation(context);

                        // Generate a session ID
                        var sessionId = Guid.NewGuid().ToString();

                        return new UserLoginResult
                        {
                            Success = true,
                            Status = true,
                            UserId = udbUser.Id,
                            UserLevel = udbUser.Level,
                            SessionId = sessionId
                        };
                    }

                    // Fallback to legacy Users table
                    var user = context.Users.FirstOrDefault(u =>
                        u.Email == data.Credential &&
                        u.Password == hashedPassword);

                    if (user != null)
                    {
                        // Update login date for legacy users
                        user.LastLogin = DateTime.Now;

                        // Ensure UserIp is valid for legacy users too
                        string validUserIp = data.LoginIp;
                        if (string.IsNullOrEmpty(validUserIp) || validUserIp.Length < 7)
                        {
                            validUserIp = "127.0.0.1"; // Default valid IP
                        }
                        user.UserIp = validUserIp;

                        SaveChangesWithValidation(context);

                        // Generate a session ID
                        var sessionId = Guid.NewGuid().ToString();

                        return new UserLoginResult
                        {
                            Success = true,
                            Status = true,
                            UserId = user.UserId,
                            FullName = user.Username,
                            SessionId = sessionId,
                            UserLevel = 100 // User role (100 conform enum URole)
                        };
                    }

                    return new UserLoginResult
                    {
                        Success = false,
                        Status = false,
                        StatusMsg = "Invalid username or password"
                    };
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool UpdateUserProfile(UProfileData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Profile data cannot be null");

            if (string.IsNullOrEmpty(data.Email))
                throw new ArgumentException("Email cannot be empty", nameof(data.Email));

            if (string.IsNullOrEmpty(data.FullName))
                throw new ArgumentException("Full name cannot be empty", nameof(data.FullName));

            try
            {
                System.Diagnostics.Debug.WriteLine($"UpdateUserProfile called for user {data.UserName}, ID={data.Id}, setting Level={data.Level}");

                using (var context = new DataContext())
                {
                    // First try to update in UDBTables
                    var udbUser = context.UDBTables.FirstOrDefault(u => u.Id == data.Id || u.Id == data.UserId);
                    if (udbUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found user in UDBTables. Current Level={udbUser.Level}, setting to {data.Level}");
                        udbUser.Email = data.Email;
                        udbUser.UserName = data.FullName;
                        udbUser.PhoneNumber = data.Phone ?? data.PhoneNumber;
                        udbUser.Address = data.Address;
                        udbUser.ProfilePicture = data.ProfileImage ?? data.ProfilePicture;
                        udbUser.Level = data.Level;

                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"User updated successfully in UDBTables. New Level={udbUser.Level}");
                        return true;
                    }

                    // If not found in UDBTables, try Users table
                    var user = context.Users.FirstOrDefault(u => u.UserId == data.Id || u.UserId == data.UserId);
                    if (user != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found user in Users table. Setting Level in UDBTables not possible for legacy users.");
                        user.Email = data.Email;
                        user.FullName = data.FullName;
                        user.Username = data.UserName ?? data.FullName;
                        user.PhoneNumber = data.Phone ?? data.PhoneNumber;
                        user.Address = data.Address;
                        user.ProfilePicture = data.ProfileImage ?? data.ProfilePicture;
                        // Note: Legacy Users table doesn't have a Level property

                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine("User updated successfully in Users table.");
                        return true;
                    }

                    System.Diagnostics.Debug.WriteLine($"User not found in either UDBTables or Users tables.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UpdateUserProfile: {ex.Message}");
                LogError(ex);
                return false;
            }
        }

        public bool UpdateUserProfileImage(string userEmail, string imageUrl)
        {
            if (string.IsNullOrEmpty(userEmail))
                throw new ArgumentNullException(nameof(userEmail), "User email cannot be null");

            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentNullException(nameof(imageUrl), "Image URL cannot be null");

            try
            {
                using (var context = new DataContext())
                {
                    // First try to update in UDBTables
                    var udbUser = context.UDBTables.FirstOrDefault(u => u.Email == userEmail);
                    if (udbUser != null)
                    {
                        udbUser.ProfilePicture = imageUrl;
                        context.SaveChanges();
                        return true;
                    }

                    // If not found in UDBTables, try Users table
                    var user = context.Users.FirstOrDefault(u => u.Email == userEmail);
                    if (user != null)
                    {
                        user.ProfilePicture = imageUrl;
                        context.SaveChanges();
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        public bool ChangeUserPassword(string userEmail, string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(userEmail))
                throw new ArgumentNullException(nameof(userEmail), "User email cannot be null");

            if (string.IsNullOrEmpty(currentPassword))
                throw new ArgumentNullException(nameof(currentPassword), "Current password cannot be null");

            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentNullException(nameof(newPassword), "New password cannot be null");

            try
            {
                string hashedCurrentPassword = HashPassword(currentPassword);
                string hashedNewPassword = HashPassword(newPassword);

                using (var context = new DataContext())
                {
                    // First try to update in UDBTables
                    var udbUser = context.UDBTables.FirstOrDefault(u => u.Email == userEmail && u.Password == hashedCurrentPassword);
                    if (udbUser != null)
                    {
                        udbUser.Password = hashedNewPassword;
                        context.SaveChanges();
                        return true;
                    }

                    // If not found in UDBTables, try Users table
                    var user = context.Users.FirstOrDefault(u => u.Email == userEmail && u.Password == hashedCurrentPassword);
                    if (user != null)
                    {
                        user.Password = hashedNewPassword;
                        context.SaveChanges();
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        public UProfileData GetUserProfile(string username)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Search first in UDBTables
                    var udbUser = context.UDBTables.FirstOrDefault(u => u.Email == username || u.UserName == username);

                    if (udbUser != null)
                    {
                        return new UProfileData
                        {
                            Id = udbUser.Id,
                            UserId = udbUser.Id,
                            UserName = udbUser.UserName,
                            FullName = udbUser.UserName,
                            Email = udbUser.Email,
                            RegisterDate = udbUser.RegisterDate,
                            LastLogin = udbUser.Last_Login,
                            Last_Login = udbUser.Last_Login,
                            Phone = udbUser.PhoneNumber,
                            PhoneNumber = udbUser.PhoneNumber,
                            Address = udbUser.Address,
                            ProfileImage = udbUser.ProfilePicture,
                            ProfilePicture = udbUser.ProfilePicture,
                            Level = udbUser.Level,
                            UserIp = udbUser.UserIp
                        };
                    }

                    // If not found in UDBTables, search in legacy Users table
                    var user = context.Users.FirstOrDefault(u => u.Username == username || u.Email == username);

                    if (user != null)
                    {
                        return new UProfileData
                        {
                            Id = user.UserId,
                            UserId = user.UserId,
                            UserName = user.Username,
                            FullName = user.FullName,
                            Email = user.Email,
                            RegisterDate = user.CreatedAt,
                            LastLogin = user.LastLogin,
                            Last_Login = user.LastLogin,
                            Phone = user.PhoneNumber,
                            PhoneNumber = user.PhoneNumber,
                            Address = user.Address,
                            ProfileImage = user.ProfilePicture,
                            ProfilePicture = user.ProfilePicture,
                            Level = 100, // Default level for users from the legacy table
                            UserIp = user.UserIp
                        };
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public int GetUserCount()
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.UDBTables.Count();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return 0;
            }
        }

        public List<UProfileData> GetRecentUsers(int count)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var users = context.UDBTables
                        .OrderByDescending(u => u.RegisterDate)
                        .Take(count)
                        .ToList();

                    return users.Select(u => new UProfileData
                    {
                        Id = u.Id,
                        UserId = u.Id,
                        UserName = u.UserName,
                        FullName = u.UserName,
                        Email = u.Email,
                        RegisterDate = u.RegisterDate,
                        LastLogin = u.Last_Login,
                        Last_Login = u.Last_Login,
                        Phone = u.PhoneNumber,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        ProfileImage = u.ProfilePicture,
                        ProfilePicture = u.ProfilePicture,
                        Level = u.Level,
                        UserIp = u.UserIp
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<UProfileData>();
            }
        }

        public List<UProfileData> GetFilteredUsers(string searchTerm, int page, int pageSize, out int totalCount)
        {
            totalCount = 0;
            try
            {
                using (var context = new DataContext())
                {
                    var query = context.UDBTables.AsQueryable();

                    // Apply search filter if provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        searchTerm = searchTerm.ToLower();
                        query = query.Where(u =>
                            u.Email.ToLower().Contains(searchTerm) ||
                            u.UserName.ToLower().Contains(searchTerm));
                    }

                    // Get total count for pagination
                    totalCount = query.Count();

                    // Apply pagination
                    var users = query
                        .OrderByDescending(u => u.RegisterDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    return users.Select(u => new UProfileData
                    {
                        Id = u.Id,
                        UserId = u.Id,
                        UserName = u.UserName,
                        FullName = u.UserName,
                        Email = u.Email,
                        RegisterDate = u.RegisterDate,
                        LastLogin = u.Last_Login,
                        Last_Login = u.Last_Login,
                        Phone = u.PhoneNumber,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        ProfileImage = u.ProfilePicture,
                        ProfilePicture = u.ProfilePicture,
                        Level = u.Level,
                        UserIp = u.UserIp
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<UProfileData>();
            }
        }

        public UProfileData GetUserById(int id)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var user = context.UDBTables.FirstOrDefault(u => u.Id == id);

                    if (user != null)
                    {
                        return new UProfileData
                        {
                            Id = user.Id,
                            UserId = user.Id,
                            UserName = user.UserName,
                            FullName = user.UserName,
                            Email = user.Email,
                            RegisterDate = user.RegisterDate,
                            LastLogin = user.Last_Login,
                            Last_Login = user.Last_Login,
                            Phone = user.PhoneNumber,
                            PhoneNumber = user.PhoneNumber,
                            Address = user.Address,
                            ProfileImage = user.ProfilePicture,
                            ProfilePicture = user.ProfilePicture,
                            Level = user.Level,
                            UserIp = user.UserIp
                        };
                    }

                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public bool UpdateUserStatus(int id, bool isActive)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var user = context.UDBTables.FirstOrDefault(u => u.Id == id);

                    if (user != null)
                    {
                        // Set level to 100 if active, 0 if inactive
                        user.Level = isActive ? 100 : 0;
                        context.SaveChanges();
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        public void CreateSession(UUserData data)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Use the existing Session entity instead of non-existent UserSession
                    var session = new Session
                    {
                        SessionId = data.SessionId ?? Guid.NewGuid().ToString(),
                        UserId = data.UserId,
                        CreatedAt = DateTime.Now
                    };

                    context.Sessions.Add(session);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public void LogoutUser(UUserData data)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Find and remove the session
                    var session = context.Sessions.FirstOrDefault(s =>
                        s.SessionId == data.SessionId);

                    if (session != null)
                    {
                        context.Sessions.Remove(session);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public bool ValidateSession(string sessionId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var session = context.Sessions.FirstOrDefault(s =>
                        s.SessionId == sessionId);

                    return session != null;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        public void TerminateSession(string sessionId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var session = context.Sessions.FirstOrDefault(s =>
                        s.SessionId == sessionId);

                    if (session != null)
                    {
                        context.Sessions.Remove(session);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public void LogUserActivity(UUserData data, string activity)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Use the existing UserActivity entity instead of non-existent UserActivityLog
                    var activityLog = new UserActivity
                    {
                        UserId = data.UserId,
                        Activity = activity,
                        CreatedAt = DateTime.Now
                    };

                    context.UserActivities.Add(activityLog);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private void LogError(Exception ex)
        {
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

        private void SaveChangesWithValidation(DataContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                System.Diagnostics.Debug.WriteLine($"Entity Validation Error: {exceptionMessage}");
                LogError(new Exception(exceptionMessage, ex));
                throw new Exception(exceptionMessage, ex);
            }
        }
    }
}
