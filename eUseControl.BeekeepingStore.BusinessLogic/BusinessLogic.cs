using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
            var sessionId = Guid.NewGuid().ToString();
            data.SessionId = sessionId;
            using (var context = new DataContext())
            {
                var session = new Session { UserId = data.UserId, SessionId = sessionId, CreatedAt = DateTime.UtcNow };
                context.Sessions.Add(session);
                context.SaveChanges();
            }

        }

        public void LogError(Exception ex)
        {
            using (var context = new DataContext())
            {
                var errorLog = new ErrorLog { Message = ex.Message, StackTrace = ex.StackTrace, CreatedAt = DateTime.UtcNow };
                context.ErrorLogs.Add(errorLog);
                context.SaveChanges();
            }
        }

        public void LogoutUser(UUserData data)
        {
            using (var context = new DataContext())
            {
                var session = context.Sessions.FirstOrDefault(s => s.UserId == data.UserId && s.SessionId == data.SessionId);
                if (session != null)
                {
                    context.Sessions.Remove(session);
                    context.SaveChanges();
                }
            }
        }

        public void LogUserActivity(UUserData data, string activity)
        {
            using (var context = new DataContext())
            {
                var userActivity = new UserActivity { UserId = data.UserId, Activity = activity, CreatedAt = DateTime.UtcNow };
                context.UserActivities.Add(userActivity);
                context.SaveChanges();
            }
        }

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
        public void TerminateSession(string sessionId)
        {
            using (var context = new DataContext())
            {
                var session = context.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
                if (session != null)
                {
                    context.Sessions.Remove(session);
                    context.SaveChanges();
                }
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
            using (var context = new DataContext())
            {
                return context.Sessions.Any(s => s.SessionId == sessionId && s.CreatedAt > DateTime.UtcNow.AddHours(-1));
            }
        }
    }
}
