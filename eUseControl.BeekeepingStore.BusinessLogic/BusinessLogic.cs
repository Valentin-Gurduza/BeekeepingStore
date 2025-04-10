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
        private readonly SessionBL _sessionBL;

        public BusinessLogic()
        {
            _sessionBL = new SessionBL();
        }

        public ISession GetSessionBL => _sessionBL;
    }
}
