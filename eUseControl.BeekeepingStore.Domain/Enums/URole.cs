using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eUseControl.BeekeepingStore.Domain.Enums
{
    public enum URole
    {
        None = 0,
        Visitor = 10,
        Banned = 20,
        Deleted = 30,
        User = 100,
        Moderator = 200,
        Admin = 400,
        Administrator = 400

    }
}
