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
        private readonly IOrder _orderBL;
        private readonly IProduct _productBL;
        private readonly IPayment _paymentBL;
        private readonly IWishlist _wishlistBL;
        private readonly IBlog _blogBL;
        private readonly IPromotion _promotionBL;

        public BusinessLogic()
        {
            _sessionBL = new SessionBL();
            _orderBL = new OrderBL();
            _productBL = new ProductBL();
            _paymentBL = new PaymentBL();
            _wishlistBL = new WishlistBL();
            _blogBL = new BlogBL();
            _promotionBL = new PromotionBL();
        }

        public ISession GetSessionBL => _sessionBL;
        public IOrder GetOrderBL => _orderBL;
        public IProduct GetProductBL => _productBL;
        public IPayment GetPaymentBL => _paymentBL;
        public IWishlist GetWishlistBL => _wishlistBL;
        public IBlog GetBlogBL => _blogBL;
        public IPromotion GetPromotionBL => _promotionBL;
    }
}
