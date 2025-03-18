using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.Domain.Entities.User;

namespace eUseControl.BeekeepingStore.BusinessLogic.Interfaces
{
     public interface ISession
     {
          UserLogin UserLogin(ULoginData data);
     }
}
