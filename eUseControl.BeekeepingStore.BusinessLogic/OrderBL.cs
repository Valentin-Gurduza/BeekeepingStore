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

namespace eUseControl.BeekeepingStore.BusinessLogic
{
    public class OrderBL : IOrder
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
                        BillingAddress = request.BillingAddress ?? request.ShippingAddress, // Default to shipping if billing not provided
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
                            OrderId = order.OrderId, // This will be set by EF when the Order is saved
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
                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
                throw;
            }
        }

        public OrderResponse GetOrderById(int orderId)
        {
            try
            {
                using (var db = new DataContext())
                {
                    // Get order with items
                    var order = db.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefault(o => o.OrderId == orderId);

                    if (order == null)
                    {
                        return null;
                    }

                    // Get user name
                    string customerName = "";
                    var user = db.Users.FirstOrDefault(u => u.UserId == order.UserId);
                    if (user != null)
                    {
                        customerName = user.Username;
                    }

                    // Convert to response object
                    var response = new OrderResponse
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
                        Items = order.OrderItems.Select(item => new OrderItemResponse
                        {
                            OrderItemId = item.OrderItemId,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            UnitPrice = item.UnitPrice,
                            Quantity = item.Quantity,
                            Subtotal = item.Subtotal
                        }).ToList()
                    };

                    return response;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetOrderById: {ex}");
                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
                return null;
            }
        }

        public List<OrderResponse> GetOrdersByUserId(int userId)
        {
            try
            {
                using (var db = new DataContext())
                {
                    // Get all orders for user
                    var orders = db.Orders
                        .Include(o => o.OrderItems)
                        .Where(o => o.UserId == userId)
                        .ToList();

                    // Get user name
                    string customerName = "";
                    var user = db.Users.FirstOrDefault(u => u.UserId == userId);
                    if (user != null)
                    {
                        customerName = user.Username;
                    }

                    // Convert to response objects
                    var responses = orders.Select(order => new OrderResponse
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
                        Items = order.OrderItems.Select(item => new OrderItemResponse
                        {
                            OrderItemId = item.OrderItemId,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            UnitPrice = item.UnitPrice,
                            Quantity = item.Quantity,
                            Subtotal = item.Subtotal
                        }).ToList()
                    }).ToList();

                    return responses;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetOrdersByUserId: {ex}");
                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
                return new List<OrderResponse>();
            }
        }

        public List<OrderResponse> GetAllOrders()
        {
            try
            {
                using (var db = new DataContext())
                {
                    // Get all orders with items
                    var orders = db.Orders
                        .Include(o => o.OrderItems)
                        .ToList();

                    // Get user names
                    var userIds = orders.Select(o => o.UserId).Distinct().ToList();
                    var users = db.Users.Where(u => userIds.Contains(u.UserId)).ToList();

                    // Convert to response objects
                    var responses = orders.Select(order =>
                    {
                        // Find user name
                        string customerName = "";
                        var user = users.FirstOrDefault(u => u.UserId == order.UserId);
                        if (user != null)
                        {
                            customerName = user.Username;
                        }

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
                            Items = order.OrderItems.Select(item => new OrderItemResponse
                            {
                                OrderItemId = item.OrderItemId,
                                ProductId = item.ProductId,
                                ProductName = item.ProductName,
                                UnitPrice = item.UnitPrice,
                                Quantity = item.Quantity,
                                Subtotal = item.Subtotal
                            }).ToList()
                        };
                    }).ToList();

                    return responses;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetAllOrders: {ex}");
                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
                return new List<OrderResponse>();
            }
        }

        public List<OrderResponse> GetFilteredOrders(string status, int? userId, string sortBy, bool ascending)
        {
            try
            {
                using (var db = new DataContext())
                {
                    // Get orders with items
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
                    if (!string.IsNullOrEmpty(sortBy))
                    {
                        switch (sortBy.ToLower())
                        {
                            case "date":
                                query = ascending
                                    ? query.OrderBy(o => o.OrderDate)
                                    : query.OrderByDescending(o => o.OrderDate);
                                break;
                            case "status":
                                query = ascending
                                    ? query.OrderBy(o => o.OrderStatus)
                                    : query.OrderByDescending(o => o.OrderStatus);
                                break;
                            case "total":
                                query = ascending
                                    ? query.OrderBy(o => o.TotalAmount)
                                    : query.OrderByDescending(o => o.TotalAmount);
                                break;
                            default:
                                query = query.OrderByDescending(o => o.OrderDate); // Default sort by date
                                break;
                        }
                    }
                    else
                    {
                        query = query.OrderByDescending(o => o.OrderDate); // Default sort by date descending
                    }

                    var orders = query.ToList();

                    // Get user names
                    var userIds = orders.Select(o => o.UserId).Distinct().ToList();
                    var users = db.Users.Where(u => userIds.Contains(u.UserId)).ToList();

                    // Convert to response objects
                    var responses = orders.Select(order =>
                    {
                        // Find user name
                        string customerName = "";
                        var user = users.FirstOrDefault(u => u.UserId == order.UserId);
                        if (user != null)
                        {
                            customerName = user.Username;
                        }

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
                            Items = order.OrderItems.Select(item => new OrderItemResponse
                            {
                                OrderItemId = item.OrderItemId,
                                ProductId = item.ProductId,
                                ProductName = item.ProductName,
                                UnitPrice = item.UnitPrice,
                                Quantity = item.Quantity,
                                Subtotal = item.Subtotal
                            }).ToList()
                        };
                    }).ToList();

                    return responses;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetFilteredOrders: {ex}");
                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
                return new List<OrderResponse>();
            }
        }

        public bool UpdateOrderStatus(UpdateOrderStatusRequest request)
        {
            try
            {
                using (var db = new DataContext())
                {
                    // Get order
                    var order = db.Orders.FirstOrDefault(o => o.OrderId == request.OrderId);
                    if (order == null)
                    {
                        return false;
                    }

                    // Update status
                    order.OrderStatus = request.OrderStatus;

                    // Update notes if provided
                    if (!string.IsNullOrEmpty(request.Notes))
                    {
                        if (string.IsNullOrEmpty(order.Notes))
                        {
                            order.Notes = request.Notes;
                        }
                        else
                        {
                            order.Notes += Environment.NewLine + request.Notes;
                        }
                    }

                    // Save changes
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdateOrderStatus: {ex}");
                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
                return false;
            }
        }

        public bool CancelOrder(int orderId)
        {
            try
            {
                using (var db = new DataContext())
                {
                    // Get order with items
                    var order = db.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefault(o => o.OrderId == orderId);

                    if (order == null)
                    {
                        return false;
                    }

                    // Only allow cancellation if order is in Pending or Processing status
                    if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Processing)
                    {
                        return false;
                    }

                    // Return products to inventory
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
                    order.PaymentStatus = order.PaymentStatus == PaymentStatus.Completed
                        ? PaymentStatus.Refunded
                        : order.PaymentStatus;

                    // Save changes
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CancelOrder: {ex}");
                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
                return false;
            }
        }

        public bool AddShippingInfo(int orderId, string trackingNumber, DateTime shippedDate)
        {
            try
            {
                using (var db = new DataContext())
                {
                    // Get order
                    var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
                    if (order == null)
                    {
                        return false;
                    }

                    // Update shipping info
                    order.TrackingNumber = trackingNumber;
                    order.ShippedDate = shippedDate;

                    // Update order status to Shipped if it was Processing
                    if (order.OrderStatus == OrderStatus.Processing)
                    {
                        order.OrderStatus = OrderStatus.Shipped;
                    }

                    // Save changes
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AddShippingInfo: {ex}");
                // Log error
                using (var db = new DataContext())
                {
                    db.ErrorLogs.Add(new ErrorLog
                    {
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        ErrorSource = ex.Source,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
                return false;
            }
        }

        // Add implementations for the dashboard methods
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
                        .Where(o => o.OrderStatus != "Cancelled")
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
                        .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.OrderStatus != "Cancelled")
                        .GroupBy(o => o.OrderDate.Date)
                        .Select(g => new SalesDataPoint
                        {
                            Date = g.Key,
                            TotalSales = g.Sum(o => o.TotalAmount)
                        })
                        .OrderBy(x => x.Date)
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
                    return db.OrderItems
                        .GroupBy(item => item.ProductId)
                        .Select(g => new TopSellingProduct
                        {
                            ProductId = g.Key,
                            TotalSold = g.Sum(i => i.Quantity),
                            ProductName = g.FirstOrDefault().ProductName
                        })
                        .OrderByDescending(x => x.TotalSold)
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
                    DateTime startOfYear = new DateTime(year, 1, 1);
                    DateTime endOfYear = new DateTime(year, 12, 31);

                    return db.Orders
                        .Where(o => o.OrderDate >= startOfYear && o.OrderDate <= endOfYear && o.OrderStatus != "Cancelled")
                        .GroupBy(o => new { Month = o.OrderDate.Month, Year = o.OrderDate.Year })
                        .Select(g => new MonthlySalesData
                        {
                            Month = g.Key.Month,
                            Year = g.Key.Year,
                            TotalSales = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(x => x.Year)
                        .ThenBy(x => x.Month)
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
                    return db.OrderItems
                        .Join(db.Products,
                            oi => oi.ProductId,
                            p => p.ProductId,
                            (oi, p) => new { OrderItem = oi, Product = p })
                        .GroupBy(x => x.Product.Category)
                        .Select(g => new CategorySalesData
                        {
                            Category = g.Key,
                            TotalSales = g.Sum(x => x.OrderItem.UnitPrice * x.OrderItem.Quantity)
                        })
                        .OrderByDescending(x => x.TotalSales)
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
                    int totalVisitors = db.UserActivities.Select(a => a.UserId).Distinct().Count();
                    int totalCustomers = db.Orders.Select(o => o.UserId).Distinct().Count();

                    return totalVisitors > 0
                        ? (double)totalCustomers / totalVisitors * 100
                        : 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetConversionRate: {ex}");
                LogError(ex);
                return 0;
            }
        }

        private void LogError(Exception ex)
        {
            using (var db = new DataContext())
            {
                db.ErrorLogs.Add(new ErrorLog
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    ErrorSource = ex.Source,
                    CreatedAt = DateTime.Now
                });
                db.SaveChanges();
            }
        }
    }
}