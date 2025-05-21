using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Product;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
    public class ProductBL : IProduct
    {
        public int AddProduct(Product product)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Setarea datei de adăugare
                    product.DateAdded = DateTime.Now;

                    // Adăugare produs în baza de date
                    context.Products.Add(product);
                    context.SaveChanges();

                    return product.ProductId;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Obținem produsul din baza de date
                    var existingProduct = context.Products.Find(product.ProductId);

                    if (existingProduct == null)
                        return false;

                    // Actualizăm proprietățile
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.Category = product.Category;
                    existingProduct.ImageUrl = product.ImageUrl;
                    existingProduct.IsActive = product.IsActive;

                    // Salvăm modificările
                    context.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool DeleteProduct(int productId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Obținem produsul din baza de date
                    var product = context.Products.Find(productId);

                    if (product == null)
                        return false;

                    // Ștergem produsul
                    context.Products.Remove(product);
                    context.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public Product GetProductById(int productId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.Products.Find(productId);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<Product> GetAllProducts()
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.Products.ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<Product> GetProductsByCategory(string category)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.Products
                        .Where(p => p.Category == category)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<Product> GetFilteredProducts(decimal? minPrice = null, decimal? maxPrice = null,
                                                string searchTerm = null, bool activeOnly = true)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var query = context.Products.AsQueryable();

                    // Filtrare după preț minim
                    if (minPrice.HasValue)
                        query = query.Where(p => p.Price >= minPrice.Value);

                    // Filtrare după preț maxim
                    if (maxPrice.HasValue)
                        query = query.Where(p => p.Price <= maxPrice.Value);

                    // Filtrare după termen de căutare
                    if (!string.IsNullOrEmpty(searchTerm))
                        query = query.Where(p => p.Name.Contains(searchTerm) ||
                                                 p.Description.Contains(searchTerm));

                    // Filtrare după starea produsului
                    if (activeOnly)
                        query = query.Where(p => p.IsActive);

                    return query.ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<Product> GetLowStockProducts(int threshold, int limit)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.Products
                        .Where(p => p.StockQuantity < threshold && p.IsActive)
                        .OrderBy(p => p.StockQuantity)
                        .Take(limit)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<Product>();
            }
        }

        private void LogError(Exception ex)
        {
            using (var context = new DataContext())
            {
                var errorLog = new ErrorLog
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    CreatedAt = DateTime.Now
                };
                context.ErrorLogs.Add(errorLog);
                context.SaveChanges();
            }
        }
    }
}