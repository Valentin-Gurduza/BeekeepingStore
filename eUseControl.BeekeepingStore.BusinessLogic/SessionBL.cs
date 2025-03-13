using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BeekeepingStore.BusinessLogic.Core;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
     public class SessionBL : UserApi, ISession
     {
          public UserLogin UserLogin(ULoginData data)
          {
               return new UserLogin(data);
          }
     }
}
