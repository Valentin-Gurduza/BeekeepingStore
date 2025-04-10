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
                    // Check if user already exists by email
                    if (context.UDBTables.Any(u => u.Email == data.Credential))
                    {
                        throw new Exception("Email already exists in the system");
                    }

                    // Create new user in UDBTables
                    var newUser = new UDBTable
                    {
                        UserName = data.Credential,
                        Email = data.Credential,
                        Password = HashPassword(data.Password),
                        Last_Login = DateTime.Now,
                        UserIp = data.LoginIp ?? "127.0.0.1",
                        Level = 1 // Default level for new users
                    };

                    context.UDBTables.Add(newUser);
                    context.SaveChanges();

                    // Also add to Users table for backward compatibility
                    var legacyUser = new User
                    {
                        Username = data.Credential,
                        Password = HashPassword(data.Password),
                        CreatedAt = DateTime.UtcNow,
                        FullName = data.FullName
                    };
                    context.Users.Add(legacyUser);
                    context.SaveChanges();
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
                using (var context = new DataContext())
                {
                    // Try to find user in UDBTables first
                    var udbUser = context.UDBTables.FirstOrDefault(u =>
                        (u.Email == data.Credential || u.UserName == data.Credential) &&
                        u.Password == HashPassword(data.Password));

                    if (udbUser != null)
                    {
                        // Update last login time and IP
                        udbUser.Last_Login = DateTime.Now;
                        udbUser.UserIp = data.LoginIp ?? udbUser.UserIp;
                        context.SaveChanges();

                        return new UserLoginResult
                        {
                            Success = true,
                            UserId = udbUser.Id,
                            UserLevel = udbUser.Level
                        };
                    }

                    // Fallback to legacy Users table
                    var user = context.Users.FirstOrDefault(u =>
                        u.Username == data.Credential &&
                        u.Password == HashPassword(data.Password));

                    if (user != null)
                    {
                        return new UserLoginResult
                        {
                            Success = true,
                            UserId = user.UserId,
                            FullName = user.FullName
                        };
                    }

                    return new UserLoginResult { Success = false };
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public void UpdateUserProfile(UProfileData data)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == data.UserId);
                    if (user != null)
                    {
                        user.Email = data.Email;
                        user.FullName = data.FullName;

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
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
            using (var context = new DataContext())
            {
                var errorLog = new ErrorLog { Message = ex.Message, StackTrace = ex.StackTrace, CreatedAt = DateTime.UtcNow };
                context.ErrorLogs.Add(errorLog);
                context.SaveChanges();
            }
        }
    }
}
