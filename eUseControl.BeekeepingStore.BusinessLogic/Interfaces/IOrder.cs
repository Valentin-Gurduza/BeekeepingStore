using System.Collections.Generic;
using eUseControl.BeekeepingStore.Domain.Entities.Order;

namespace eUseControl.BeekeepingStore.BusinessLogic.Interfaces
{
    public interface IOrder
    {
        // Create a new order
        int CreateOrder(CreateOrderRequest request);

        // Get order by ID
        OrderResponse GetOrderById(int orderId);

        // Get all orders for a user
        List<OrderResponse> GetOrdersByUserId(int userId);

        // Get all orders (admin function)
        List<OrderResponse> GetAllOrders();

        // Get filtered orders (admin function)
        List<OrderResponse> GetFilteredOrders(string status, int? userId, string sortBy, bool ascending);

        // Update order status
        bool UpdateOrderStatus(UpdateOrderStatusRequest request);

        // Cancel order
        bool CancelOrder(int orderId);

        // Add shipping/tracking information
        bool AddShippingInfo(int orderId, string trackingNumber, System.DateTime shippedDate);
    }
}