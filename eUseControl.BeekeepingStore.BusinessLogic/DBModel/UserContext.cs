using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.Domain.Entities.User;

namespace eUseControl.BeekeepingStore.BusinessLogic.DBModel
{
    class UserContext : DbContext
    {
        public UserContext() : base("name=BeekeepingStoreDB")
        {
        }
        public virtual DbSet<UDBTable> Users { get; set; }
    }
}
