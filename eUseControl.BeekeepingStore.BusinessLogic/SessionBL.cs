using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.User;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
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
                    // Log logout activity
                    LogUserActivity(data, "Logout");

                    context.Sessions.Remove(session);
                    context.SaveChanges();
                }
            }
        }

        public void LogUserActivity(UUserData data, string activity)
        {
            using (var context = new DataContext())
            {
                var userActivity = new UserActivity { UserId = data.UserId, Activity = activity, CreatedAt = DateTime.Now };
                context.UserActivities.Add(userActivity);
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

        public bool ValidateSession(string sessionId)
        {
            using (var context = new DataContext())
            {
                return context.Sessions.Any(s => s.SessionId == sessionId && s.CreatedAt > DateTime.UtcNow.AddHours(-1));
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

        public bool UpdateUserStatus(int id, bool isActive)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var user = context.UDBTables.FirstOrDefault(u => u.Id == id);

                    if (user != null)
                    {
                        // Set level to 1 if active, 0 if inactive
                        user.Level = isActive ? 1 : 0;
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

        public UserLoginResult UserLogin(ULoginData data)
        {
            try
            {
                // Calculăm hash-ul parolei în afara query-ului
                string hashedPassword = HashPassword(data.Password);

                using (var context = new DataContext())
                {
                    // Try to find user in UDBTables first
                    var udbUser = context.UDBTables.FirstOrDefault(u =>
                        (u.Email == data.Credential) &&
                        u.Password == hashedPassword);

                    if (udbUser != null)
                    {
                        // Update Last_Login and UserIp
                        udbUser.Last_Login = DateTime.Now;
                        udbUser.UserIp = data.LoginIp ?? udbUser.UserIp;
                        context.SaveChanges();

                        // Generate a session ID
                        var sessionId = Guid.NewGuid().ToString();

                        // Log login activity
                        var userData = new UUserData { UserId = udbUser.Id, SessionId = sessionId };
                        LogUserActivity(userData, "Login");

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
                        user.UserIp = data.LoginIp ?? user.UserIp;
                        context.SaveChanges();

                        // Generate a session ID
                        var sessionId = Guid.NewGuid().ToString();

                        // Log login activity
                        var userDataLegacy = new UUserData { UserId = user.UserId, SessionId = sessionId };
                        LogUserActivity(userDataLegacy, "Login");

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

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public void RegisterUser(ULoginData data)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting user registration: " + data.Credential);

                // Verifică dacă datele primite sunt complete
                System.Diagnostics.Debug.WriteLine($"FullName: {data.FullName ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"Credential: {data.Credential ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"Password: {(data.Password != null ? "***" : "null")}");
                System.Diagnostics.Debug.WriteLine($"LoginIp: {data.LoginIp ?? "null"}");

                using (var context = new DataContext())
                {
                    System.Diagnostics.Debug.WriteLine("Connection string: " + context.Database.Connection.ConnectionString);

                    // Validăm lungimea datelor pentru a evita erori de validare
                    string fullName = data.FullName ?? data.Credential.Split('@')[0];
                    if (fullName.Length < 5)
                    {
                        fullName = fullName.PadRight(5, '0'); // Adăugăm caractere pentru a îndeplini constrângerea
                        System.Diagnostics.Debug.WriteLine($"Username padded to: {fullName}");
                    }

                    string email = data.Credential;
                    if (email.Length < 8)
                    {
                        email = email + "@test.com"; // Adăugăm un domeniu pentru a îndeplini constrângerea
                        System.Diagnostics.Debug.WriteLine($"Email padded to: {email}");
                    }

                    string password = data.Password;
                    if (string.IsNullOrEmpty(password) || password.Length < 8)
                    {
                        password = "password123"; // Setăm o parolă implicită dacă nu este validă
                        System.Diagnostics.Debug.WriteLine("Using default password");
                    }

                    string userIp = data.LoginIp ?? "127.0.0.1";

                    // Verificăm dacă utilizatorul există deja
                    bool userExists = false;
                    try
                    {
                        userExists = context.UDBTables.Any(u => u.Email == email);
                        System.Diagnostics.Debug.WriteLine($"User exists check in UDBTables: {userExists}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error checking UDBTables: " + ex.Message);
                    }

                    if (!userExists)
                    {
                        try
                        {
                            userExists = context.Users.Any(u => u.Username == email);
                            System.Diagnostics.Debug.WriteLine($"User exists check in Users: {userExists}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error checking Users: " + ex.Message);
                        }
                    }

                    if (userExists)
                    {
                        System.Diagnostics.Debug.WriteLine("User already exists: " + email);
                        throw new Exception("User with this email already exists");
                    }

                    // Calculăm hash-ul parolei o singură dată
                    string hashedPassword = HashPassword(password);
                    System.Diagnostics.Debug.WriteLine("Hashed password length: " + hashedPassword.Length);

                    // Încercăm inserarea folosind ExecuteSqlCommand
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Trying direct SQL insertion");
                        System.Diagnostics.Debug.WriteLine($"Username: {fullName}");
                        System.Diagnostics.Debug.WriteLine($"Email: {email}");
                        System.Diagnostics.Debug.WriteLine($"UserIp: {userIp}");

                        // Folosim parametri explicit pentru a evita probleme de formatare
                        var sql = "INSERT INTO UDBTables (Username, Password, Email, Last_Login, UserIp, Level, RegisterDate) " +
                                  "VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)";

                        // Folosim un command explicit pentru a avea mai mult control
                        using (var command = context.Database.Connection.CreateCommand())
                        {
                            context.Database.Connection.Open();
                            command.CommandText = sql;

                            var p0 = command.CreateParameter();
                            p0.ParameterName = "@p0";
                            p0.Value = fullName;
                            command.Parameters.Add(p0);

                            var p1 = command.CreateParameter();
                            p1.ParameterName = "@p1";
                            p1.Value = hashedPassword;
                            command.Parameters.Add(p1);

                            var p2 = command.CreateParameter();
                            p2.ParameterName = "@p2";
                            p2.Value = email;
                            command.Parameters.Add(p2);

                            var p3 = command.CreateParameter();
                            p3.ParameterName = "@p3";
                            p3.Value = DateTime.Now;
                            command.Parameters.Add(p3);

                            var p4 = command.CreateParameter();
                            p4.ParameterName = "@p4";
                            p4.Value = userIp;
                            command.Parameters.Add(p4);

                            var p5 = command.CreateParameter();
                            p5.ParameterName = "@p5";
                            p5.Value = 100; // Utilizator normal (100 = User conform enum URole)
                            command.Parameters.Add(p5);

                            var p6 = command.CreateParameter();
                            p6.ParameterName = "@p6";
                            p6.Value = DateTime.Now; // Data înregistrării este aceeași cu data curentă
                            command.Parameters.Add(p6);

                            int result = command.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"Direct SQL insertion result: {result} rows affected");
                        }

                        // Adăugăm și în tabelul Users pentru compatibilitate
                        var legacyUser = new User
                        {
                            Username = fullName,
                            Password = hashedPassword,
                            CreatedAt = DateTime.Now,
                            Email = email,
                            LastLogin = DateTime.Now,
                            UserIp = userIp
                        };

                        context.Users.Add(legacyUser);
                        context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine("Registration completed successfully via direct SQL");
                        return;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Direct SQL insertion failed: " + ex.ToString());
                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.ToString());
                        }
                    }

                    // Dacă inserarea directă eșuează, încercăm cu Entity Framework
                    System.Diagnostics.Debug.WriteLine("Creating new UDBTable entry with Entity Framework");
                    try
                    {
                        var udbUser = new UDBTable
                        {
                            UserName = fullName,
                            Email = email,
                            Password = hashedPassword,
                            Last_Login = DateTime.Now,
                            UserIp = userIp,
                            Level = 100, // Utilizator normal (100 = User conform enum URole)
                            RegisterDate = DateTime.Now // Setăm data înregistrării
                        };

                        context.UDBTables.Add(udbUser);
                        System.Diagnostics.Debug.WriteLine("Saving UDBTable entry to database via EF");
                        try
                        {
                            context.SaveChanges();
                            System.Diagnostics.Debug.WriteLine("UDBTable entry saved successfully via EF");
                        }
                        catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                        {
                            // Afișăm detalii despre erorile de validare
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                                }
                            }
                            throw;
                        }

                        System.Diagnostics.Debug.WriteLine("Creating legacy User entry");
                        var legacyUserEF = new User
                        {
                            Username = fullName,
                            Password = hashedPassword,
                            CreatedAt = DateTime.Now,
                            Email = email,
                            LastLogin = DateTime.Now,
                            UserIp = userIp
                        };

                        context.Users.Add(legacyUserEF);
                        System.Diagnostics.Debug.WriteLine("Saving legacy User entry to database");
                        context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine("Legacy User entry saved successfully");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Entity Framework insertion failed: " + ex.ToString());
                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.ToString());
                        }
                        throw;
                    }
                }

                // Verificăm dacă inserarea a reușit
                using (var context = new DataContext())
                {
                    try
                    {
                        var count = context.UDBTables.Count();
                        System.Diagnostics.Debug.WriteLine($"Total records in UDBTables after registration: {count}");

                        var latestUser = context.UDBTables.OrderByDescending(u => u.Id).FirstOrDefault();
                        if (latestUser != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Latest user: {latestUser.Email}, ID: {latestUser.Id}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No users found in UDBTables");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Error checking results: " + ex.Message);
                    }
                }

                System.Diagnostics.Debug.WriteLine("User registration completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Registration error: " + ex.ToString());
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.ToString());
                }
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
                using (var context = new DataContext())
                {
                    // First try to update in UDBTables
                    var udbUser = context.UDBTables.FirstOrDefault(u => u.Id == data.Id || u.Id == data.UserId);
                    if (udbUser != null)
                    {
                        udbUser.Email = data.Email;
                        udbUser.UserName = data.FullName;
                        udbUser.PhoneNumber = data.Phone ?? data.PhoneNumber;
                        udbUser.Address = data.Address;
                        udbUser.ProfilePicture = data.ProfileImage ?? data.ProfilePicture;
                        context.SaveChanges();
                        return true;
                    }

                    // If not found in UDBTables, try Users table
                    var user = context.Users.FirstOrDefault(u => u.UserId == data.Id || u.UserId == data.UserId);
                    if (user != null)
                    {
                        user.Email = data.Email;
                        user.FullName = data.FullName;
                        user.Username = data.UserName ?? data.FullName;
                        user.PhoneNumber = data.Phone ?? data.PhoneNumber;
                        user.Address = data.Address;
                        user.ProfilePicture = data.ProfileImage ?? data.ProfilePicture;
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
                    // Căutăm mai întâi în tabela UDBTables
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

                    // Dacă nu găsim în UDBTables, căutăm în tabela legacy Users
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
    }
}
