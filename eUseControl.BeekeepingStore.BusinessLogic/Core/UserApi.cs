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
                    if (context.Users.Any(u => u.Username == data.Credential))
                    {
                        throw new Exception("User already exists");
                    }

                    var newUser = new User
                    {
                        Username = data.Credential,
                        Password = HashPassword(data.Password),

                        CreatedAt = DateTime.UtcNow,
                        FullName = data.FullName
                    };
                    context.Users.Add(newUser);
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
                    var user = context.Users.FirstOrDefault(u => u.Username == data.Credential && u.Password == HashPassword(data.Password));
                    if (user != null)
                    {
                        return new UserLoginResult { Success = true, UserId = user.UserId, FullName = user.FullName };
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
