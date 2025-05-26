using System;
using System.Collections.Generic;
using eUseControl.BeekeepingStore.BusinessLogic.Core;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Order;
using eUseControl.BeekeepingStore.Domain.Entities.Product;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
    /// <summary>
    /// OrderBL class that delegates all order operations to OrderApi
    /// Following the delegation pattern: OrderBL : IOrder -> OrderApi (actual implementation)
    /// </summary>
    public class OrderBL : IOrder
    {
        private readonly OrderApi _orderApi;

        public OrderBL()
        {
            _orderApi = new OrderApi();
        }

        public int CreateOrder(CreateOrderRequest request)
        {
            return _orderApi.CreateOrder(request);
        }

        public OrderResponse GetOrderById(int orderId)
        {
            return _orderApi.GetOrderById(orderId);
        }

        public List<OrderResponse> GetOrdersByUserId(int userId)
        {
            return _orderApi.GetOrdersByUserId(userId);
        }

        public List<OrderResponse> GetAllOrders()
        {
            return _orderApi.GetAllOrders();
        }

        public List<OrderResponse> GetFilteredOrders(string status, int? userId, string sortBy, bool ascending)
        {
            return _orderApi.GetFilteredOrders(status, userId, sortBy, ascending);
        }

        public bool UpdateOrderStatus(UpdateOrderStatusRequest request)
        {
            return _orderApi.UpdateOrderStatus(request);
        }

        public bool CancelOrder(int orderId)
        {
            return _orderApi.CancelOrder(orderId);
        }

        public bool AddShippingInfo(int orderId, string trackingNumber, DateTime shippedDate)
        {
            return _orderApi.AddShippingInfo(orderId, trackingNumber, shippedDate);
        }

        public List<Order> GetRecentOrders(int count)
        {
            return _orderApi.GetRecentOrders(count);
        }

        public decimal GetTotalRevenue()
        {
            return _orderApi.GetTotalRevenue();
        }

        public List<SalesDataPoint> GetSalesDataByDateRange(DateTime startDate, DateTime endDate)
        {
            return _orderApi.GetSalesDataByDateRange(startDate, endDate);
        }

        public List<TopSellingProduct> GetTopSellingProducts(int count)
        {
            return _orderApi.GetTopSellingProducts(count);
        }

        public List<MonthlySalesData> GetMonthlySalesData(int year)
        {
            return _orderApi.GetMonthlySalesData(year);
        }

        public List<CategorySalesData> GetCategorySalesData()
        {
            return _orderApi.GetCategorySalesData();
        }

        public double GetConversionRate()
        {
            return _orderApi.GetConversionRate();
        }
    }
}