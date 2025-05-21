using System;
using System.Collections.Generic;
using eUseControl.BeekeepingStore.Domain.Entities.Order;
using eUseControl.BeekeepingStore.Domain.Entities.Product;

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

        // Dashboard functionality
        List<Order> GetRecentOrders(int count);
        decimal GetTotalRevenue();
        List<SalesDataPoint> GetSalesDataByDateRange(DateTime startDate, DateTime endDate);
        List<TopSellingProduct> GetTopSellingProducts(int count);
        List<MonthlySalesData> GetMonthlySalesData(int year);
        List<CategorySalesData> GetCategorySalesData();
        double GetConversionRate();
    }

    public class SalesDataPoint
    {
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class TopSellingProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
    }

    public class MonthlySalesData
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalSales { get; set; }
        public int OrderCount { get; set; }
    }

    public class CategorySalesData
    {
        public string Category { get; set; }
        public decimal TotalSales { get; set; }
    }
}