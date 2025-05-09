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
        private readonly OrderBL _orderBL;
        private readonly ProductBL _productBL;

        public BusinessLogic()
        {
            _sessionBL = new SessionBL();
            _orderBL = new OrderBL();
            _productBL = new ProductBL();
        }

        public ISession GetSessionBL => _sessionBL;
        public IOrder GetOrderBL => _orderBL;
        public IProduct GetProductBL => _productBL;
    }
}
