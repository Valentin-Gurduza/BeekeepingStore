using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Order;
using eUseControl.BeekeepingStore.Domain.Entities.Product;
using eUseControl.BeekeepingStore.Domain.Entities.User;
using eUseControl.BeekeepingStore.Domain.Enums;

namespace eUseControl.BeekeepingStore.BusinessLogic.Core
{
    public class OrderApi
    {
        public int CreateOrder(CreateOrderRequest request)
        {
            try
            {
                using (var db = new DataContext())
                {
                    // Validate user exists
                    var user = db.Users.FirstOrDefault(u => u.UserId == request.UserId);
                    if (user == null)
                    {
                        throw new ArgumentException("Invalid user ID");
                    }

                    // Check if items exist
                    if (request.Items == null || !request.Items.Any())
                    {
                        throw new ArgumentException("Order must contain at least one item");
                    }

                    // Create new order
                    var order = new Order
                    {
                        UserId = request.UserId,
                        OrderDate = DateTime.Now,
                        OrderStatus = OrderStatus.Pending,
                        ShippingAddress = request.ShippingAddress,
                        BillingAddress = request.BillingAddress ?? request.ShippingAddress,
                        PaymentMethod = request.PaymentMethod,
                        PaymentStatus = PaymentStatus.Pending,
                        Notes = request.Notes,
                        OrderItems = new List<OrderItem>()
                    };

                    decimal totalAmount = 0;

                    // Add order items
                    foreach (var item in request.Items)
                    {
                        // Get product from DB
                        var product = db.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                        if (product == null)
                        {
                            throw new ArgumentException($"Product with ID {item.ProductId} not found");
                        }

                        // Check stock
                        if (product.StockQuantity < item.Quantity)
                        {
                            throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}");
                        }

                        // Calculate subtotal
                        decimal subtotal = product.Price * item.Quantity;
                        totalAmount += subtotal;

                        // Create order item
                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,
                            ProductId = product.ProductId,
                            ProductName = product.Name,
                            UnitPrice = product.Price,
                            Quantity = item.Quantity,
                            Subtotal = subtotal
                        };

                        order.OrderItems.Add(orderItem);

                        // Update product stock
                        product.StockQuantity -= item.Quantity;
                    }

                    // Set total amount
                    order.TotalAmount = totalAmount;

                    // Save order
                    db.Orders.Add(order);
                    db.SaveChanges();

                    return order.OrderId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CreateOrder: {ex}");
                LogError(ex);
                throw;
            }
        }

        public OrderResponse GetOrderById(int orderId)
        {
            try
            {
                using (var db = new DataContext())
                {
                    var order = db.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefault(o => o.OrderId == orderId);

                    if (order == null)
                        return null;

                    // Get customer name
                    var user = db.Users.FirstOrDefault(u => u.UserId == order.UserId);
                    string customerName = user?.Username ?? "Unknown";

                    return new OrderResponse
                    {
                        OrderId = order.OrderId,
                        UserId = order.UserId,
                        CustomerName = customerName,
                        OrderDate = order.OrderDate,
                        OrderStatus = order.OrderStatus,
                        TotalAmount = order.TotalAmount,
                        ShippingAddress = order.ShippingAddress,
                        BillingAddress = order.BillingAddress,
                        PaymentMethod = order.PaymentMethod,
                        PaymentStatus = order.PaymentStatus,
                        ShippedDate = order.ShippedDate,
                        TrackingNumber = order.TrackingNumber,
                        Notes = order.Notes,
                        Items = order.OrderItems?.Select(oi => new OrderItemResponse
                        {
                            OrderItemId = oi.OrderItemId,
                            ProductId = oi.ProductId,
                            ProductName = oi.ProductName,
                            UnitPrice = oi.UnitPrice,
                            Quantity = oi.Quantity,
                            Subtotal = oi.Subtotal
                        }).ToList() ?? new List<OrderItemResponse>()
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetOrderById: {ex}");
                LogError(ex);
                return null;
            }
        }

        public List<OrderResponse> GetOrdersByUserId(int userId)
        {
            try
            {
                using (var db = new DataContext())
                {
                    var orders = db.Orders
                        .Include(o => o.OrderItems)
                        .Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.OrderDate)
                        .ToList();

                    // Get customer name
                    var user = db.Users.FirstOrDefault(u => u.UserId == userId);
                    string customerName = user?.Username ?? "Unknown";

                    return orders.Select(order => new OrderResponse
                    {
                        OrderId = order.OrderId,
                        UserId = order.UserId,
                        CustomerName = customerName,
                        OrderDate = order.OrderDate,
                        OrderStatus = order.OrderStatus,
                        TotalAmount = order.TotalAmount,
                        ShippingAddress = order.ShippingAddress,
                        BillingAddress = order.BillingAddress,
                        PaymentMethod = order.PaymentMethod,
                        PaymentStatus = order.PaymentStatus,
                        ShippedDate = order.ShippedDate,
                        TrackingNumber = order.TrackingNumber,
                        Notes = order.Notes,
                        Items = order.OrderItems?.Select(oi => new OrderItemResponse
                        {
                            OrderItemId = oi.OrderItemId,
                            ProductId = oi.ProductId,
                            ProductName = oi.ProductName,
                            UnitPrice = oi.UnitPrice,
                            Quantity = oi.Quantity,
                            Subtotal = oi.Subtotal
                        }).ToList() ?? new List<OrderItemResponse>()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetOrdersByUserId: {ex}");
                LogError(ex);
                return new List<OrderResponse>();
            }
        }

        public List<OrderResponse> GetAllOrders()
        {
            try
            {
                using (var db = new DataContext())
                {
                    var orders = db.Orders
                        .Include(o => o.OrderItems)
                        .OrderByDescending(o => o.OrderDate)
                        .ToList();

                    return orders.Select(order => new OrderResponse
                    {
                        OrderId = order.OrderId,
                        UserId = order.UserId,
                        CustomerName = GetCustomerName(db, order.UserId),
                        OrderDate = order.OrderDate,
                        OrderStatus = order.OrderStatus,
                        TotalAmount = order.TotalAmount,
                        ShippingAddress = order.ShippingAddress,
                        BillingAddress = order.BillingAddress,
                        PaymentMethod = order.PaymentMethod,
                        PaymentStatus = order.PaymentStatus,
                        ShippedDate = order.ShippedDate,
                        TrackingNumber = order.TrackingNumber,
                        Notes = order.Notes,
                        Items = order.OrderItems?.Select(oi => new OrderItemResponse
                        {
                            OrderItemId = oi.OrderItemId,
                            ProductId = oi.ProductId,
                            ProductName = oi.ProductName,
                            UnitPrice = oi.UnitPrice,
                            Quantity = oi.Quantity,
                            Subtotal = oi.Subtotal
                        }).ToList() ?? new List<OrderItemResponse>()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetAllOrders: {ex}");
                LogError(ex);
                return new List<OrderResponse>();
            }
        }

        public List<OrderResponse> GetFilteredOrders(string status, int? userId, string sortBy, bool ascending)
        {
            try
            {
                using (var db = new DataContext())
                {
                    var query = db.Orders.Include(o => o.OrderItems).AsQueryable();

                    // Apply filters
                    if (!string.IsNullOrEmpty(status))
                    {
                        query = query.Where(o => o.OrderStatus == status);
                    }

                    if (userId.HasValue)
                    {
                        query = query.Where(o => o.UserId == userId.Value);
                    }

                    // Apply sorting
                    switch (sortBy?.ToLower())
                    {
                        case "date":
                            query = ascending ? query.OrderBy(o => o.OrderDate) : query.OrderByDescending(o => o.OrderDate);
                            break;
                        case "total":
                            query = ascending ? query.OrderBy(o => o.TotalAmount) : query.OrderByDescending(o => o.TotalAmount);
                            break;
                        case "status":
                            query = ascending ? query.OrderBy(o => o.OrderStatus) : query.OrderByDescending(o => o.OrderStatus);
                            break;
                        default:
                            query = query.OrderByDescending(o => o.OrderDate);
                            break;
                    }

                    var orders = query.ToList();

                    return orders.Select(order => new OrderResponse
                    {
                        OrderId = order.OrderId,
                        UserId = order.UserId,
                        CustomerName = GetCustomerName(db, order.UserId),
                        OrderDate = order.OrderDate,
                        OrderStatus = order.OrderStatus,
                        TotalAmount = order.TotalAmount,
                        ShippingAddress = order.ShippingAddress,
                        BillingAddress = order.BillingAddress,
                        PaymentMethod = order.PaymentMethod,
                        PaymentStatus = order.PaymentStatus,
                        ShippedDate = order.ShippedDate,
                        TrackingNumber = order.TrackingNumber,
                        Notes = order.Notes,
                        Items = order.OrderItems?.Select(oi => new OrderItemResponse
                        {
                            OrderItemId = oi.OrderItemId,
                            ProductId = oi.ProductId,
                            ProductName = oi.ProductName,
                            UnitPrice = oi.UnitPrice,
                            Quantity = oi.Quantity,
                            Subtotal = oi.Subtotal
                        }).ToList() ?? new List<OrderItemResponse>()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetFilteredOrders: {ex}");
                LogError(ex);
                return new List<OrderResponse>();
            }
        }

        public bool UpdateOrderStatus(UpdateOrderStatusRequest request)
        {
            try
            {
                using (var db = new DataContext())
                {
                    var order = db.Orders.FirstOrDefault(o => o.OrderId == request.OrderId);
                    if (order == null)
                        return false;

                    order.OrderStatus = request.OrderStatus;
                    if (!string.IsNullOrEmpty(request.Notes))
                    {
                        order.Notes = request.Notes;
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdateOrderStatus: {ex}");
                LogError(ex);
                return false;
            }
        }

        public bool CancelOrder(int orderId)
        {
            try
            {
                using (var db = new DataContext())
                {
                    var order = db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderId == orderId);
                    if (order == null)
                        return false;

                    // Only allow cancellation if order is pending or processing
                    if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Processing)
                        return false;

                    // Restore product stock
                    foreach (var item in order.OrderItems)
                    {
                        var product = db.Products.FirstOrDefault(p => p.ProductId == item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                        }
                    }

                    // Update order status
                    order.OrderStatus = OrderStatus.Cancelled;
                    order.PaymentStatus = PaymentStatus.Refunded;

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CancelOrder: {ex}");
                LogError(ex);
                return false;
            }
        }

        public bool AddShippingInfo(int orderId, string trackingNumber, DateTime shippedDate)
        {
            try
            {
                using (var db = new DataContext())
                {
                    var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
                    if (order == null)
                        return false;

                    order.TrackingNumber = trackingNumber;
                    order.ShippedDate = shippedDate;
                    order.OrderStatus = OrderStatus.Shipped;

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AddShippingInfo: {ex}");
                LogError(ex);
                return false;
            }
        }

        public List<Order> GetRecentOrders(int count)
        {
            try
            {
                using (var db = new DataContext())
                {
                    return db.Orders
                        .OrderByDescending(o => o.OrderDate)
                        .Take(count)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetRecentOrders: {ex}");
                LogError(ex);
                return new List<Order>();
            }
        }

        public decimal GetTotalRevenue()
        {
            try
            {
                using (var db = new DataContext())
                {
                    return db.Orders
                        .Where(o => o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Shipped)
                        .Sum(o => o.TotalAmount);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetTotalRevenue: {ex}");
                LogError(ex);
                return 0;
            }
        }

        public List<SalesDataPoint> GetSalesDataByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var db = new DataContext())
                {
                    return db.Orders
                        .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate &&
                                   (o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Shipped))
                        .GroupBy(o => DbFunctions.TruncateTime(o.OrderDate))
                        .Select(g => new SalesDataPoint
                        {
                            Date = g.Key.Value,
                            TotalSales = g.Sum(o => o.TotalAmount)
                        })
                        .OrderBy(s => s.Date)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetSalesDataByDateRange: {ex}");
                LogError(ex);
                return new List<SalesDataPoint>();
            }
        }

        public List<TopSellingProduct> GetTopSellingProducts(int count)
        {
            try
            {
                using (var db = new DataContext())
                {
                    return db.Orders
                        .Where(o => o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Shipped)
                        .SelectMany(o => o.OrderItems)
                        .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                        .Select(g => new TopSellingProduct
                        {
                            ProductId = g.Key.ProductId,
                            ProductName = g.Key.ProductName,
                            TotalSold = g.Sum(oi => oi.Quantity)
                        })
                        .OrderByDescending(p => p.TotalSold)
                        .Take(count)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetTopSellingProducts: {ex}");
                LogError(ex);
                return new List<TopSellingProduct>();
            }
        }

        public List<MonthlySalesData> GetMonthlySalesData(int year)
        {
            try
            {
                using (var db = new DataContext())
                {
                    return db.Orders
                        .Where(o => o.OrderDate.Year == year &&
                                   (o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Shipped))
                        .GroupBy(o => o.OrderDate.Month)
                        .Select(g => new MonthlySalesData
                        {
                            Month = g.Key,
                            Year = year,
                            TotalSales = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(m => m.Month)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetMonthlySalesData: {ex}");
                LogError(ex);
                return new List<MonthlySalesData>();
            }
        }

        public List<CategorySalesData> GetCategorySalesData()
        {
            try
            {
                using (var db = new DataContext())
                {
                    return db.Orders
                        .Where(o => o.OrderStatus == OrderStatus.Delivered || o.OrderStatus == OrderStatus.Shipped)
                        .SelectMany(o => o.OrderItems)
                        .Join(db.Products, oi => oi.ProductId, p => p.ProductId, (oi, p) => new { oi, p })
                        .GroupBy(x => x.p.Category)
                        .Select(g => new CategorySalesData
                        {
                            Category = g.Key,
                            TotalSales = g.Sum(x => x.oi.Subtotal)
                        })
                        .OrderByDescending(c => c.TotalSales)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetCategorySalesData: {ex}");
                LogError(ex);
                return new List<CategorySalesData>();
            }
        }

        public double GetConversionRate()
        {
            try
            {
                using (var db = new DataContext())
                {
                    var totalOrders = db.Orders.Count();
                    var completedOrders = db.Orders.Count(o => o.OrderStatus == OrderStatus.Delivered);

                    if (totalOrders == 0)
                        return 0;

                    return (double)completedOrders / totalOrders * 100;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetConversionRate: {ex}");
                LogError(ex);
                return 0;
            }
        }

        private string GetCustomerName(DataContext db, int userId)
        {
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            return user?.Username ?? "Unknown";
        }

        private void LogError(Exception ex)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var errorLog = new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.ErrorLogs.Add(errorLog);
                    context.SaveChanges();
                }
            }
            catch (Exception logEx)
            {
                Debug.WriteLine($"Failed to log error: {logEx.Message}");
            }
        }
    }
}