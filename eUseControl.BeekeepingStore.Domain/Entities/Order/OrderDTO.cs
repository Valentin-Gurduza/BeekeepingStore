using System;
using System.Collections.Generic;

namespace eUseControl.BeekeepingStore.Domain.Entities.Order
{
    // Used for creating a new order
    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public List<OrderItemRequest> Items { get; set; }
        public string Notes { get; set; }
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // Used for order response/display
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? ShippedDate { get; set; }
        public string TrackingNumber { get; set; }
        public string Notes { get; set; }
        public List<OrderItemResponse> Items { get; set; }
    }

    public class OrderItemResponse
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }

    // Used for updating order status
    public class UpdateOrderStatusRequest
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public string Notes { get; set; }
    }
}