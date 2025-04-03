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
    Banned = 1,
    Deleted = 3,
    User = 100,
    Moderator = 200,
    Vip = 300,
    Administrator = 400,

    }
}
