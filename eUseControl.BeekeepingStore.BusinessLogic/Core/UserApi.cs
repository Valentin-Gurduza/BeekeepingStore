using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.User;

namespace eUseControl.BeekeepingStore.BusinessLogic.Core
{
    public class UserApi
    {
        public void RegisterUser(ULoginData data)
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
                    Password = data.Password, // Ensure you hash the password
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(newUser);
                context.SaveChanges();
            }
        }
        public UserLoginResult LoginUser(ULoginData data)
        {
            using (var context = new DataContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Username == data.Credential && u.Password == data.Password);
                if (user != null)
                {
                    return new UserLoginResult { Success = true, UserId = user.UserId };
                }
                return new UserLoginResult { Success = false };
            }
        }
        public void UpdateUserProfile(UProfileData data)
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
    }
}
