using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eUseControl.BeekeepingStore.Domain.Entities.User
{
    public class ULoginData
    {
        public string Credential { get; set; }
        public string Password { get; set; }
        public string LoginIp { get; set; }
        public DateTime LoginDateTime { get; set; }
        public string FullName { get; set; }
    }

    public class UserLoginResult
    {
        public bool Status { get; set; }
        public string StatusMsg { get; set; }
        public bool Success { get; set; }
        public int UserId { get; set; }
    }
}
