using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.Product;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
    public class PromotionBL : IPromotion
    {
        public int AddPromotion(Promotion promotion)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Setăm data creării
                    promotion.DateCreated = DateTime.Now;
                    promotion.UsageCount = 0;

                    // Adăugăm promoția în baza de date
                    context.Promotions.Add(promotion);
                    context.SaveChanges();

                    return promotion.PromotionId;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool UpdatePromotion(Promotion promotion)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Obținem promoția existentă
                    var existingPromotion = context.Promotions.Find(promotion.PromotionId);

                    if (existingPromotion == null)
                        return false;

                    // Actualizăm proprietățile
                    existingPromotion.Name = promotion.Name;
                    existingPromotion.Description = promotion.Description;
                    existingPromotion.PromotionType = promotion.PromotionType;
                    existingPromotion.DiscountValue = promotion.DiscountValue;
                    existingPromotion.BuyQuantity = promotion.BuyQuantity;
                    existingPromotion.GetQuantity = promotion.GetQuantity;
                    existingPromotion.UsageLimit = promotion.UsageLimit;
                    existingPromotion.CustomerGroup = promotion.CustomerGroup;
                    existingPromotion.CouponCode = promotion.CouponCode;
                    existingPromotion.StartDate = promotion.StartDate;
                    existingPromotion.EndDate = promotion.EndDate;
                    existingPromotion.IsActive = promotion.IsActive;
                    existingPromotion.LastUpdated = DateTime.Now;

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

        public bool DeletePromotion(int promotionId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Obținem promoția
                    var promotion = context.Promotions.Find(promotionId);

                    if (promotion == null)
                        return false;

                    // Ștergem promoția
                    context.Promotions.Remove(promotion);
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

        public Promotion GetPromotionById(int promotionId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.Promotions.Include(p => p.Product).FirstOrDefault(p => p.PromotionId == promotionId);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<Promotion> GetAllPromotions()
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.Promotions.Include(p => p.Product).ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<Promotion> GetPromotionsForProduct(int productId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.Promotions
                        .Include(p => p.Product)
                        .Where(p => p.ProductId == productId)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<Promotion> GetActivePromotionsForProduct(int productId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    DateTime now = DateTime.Now;

                    System.Diagnostics.Debug.WriteLine($"QUERY: Getting active promotions for product {productId} at {now}");

                    var promotions = context.Promotions
                        .Include(p => p.Product)
                        .Where(p => p.ProductId == productId &&
                               p.IsActive &&
                               p.StartDate <= now &&
                               (!p.EndDate.HasValue || p.EndDate >= now) &&
                               (!p.UsageLimit.HasValue || p.UsageCount < p.UsageLimit))
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"QUERY: Found {promotions.Count} active promotions for product {productId}");

                    foreach (var promotion in promotions)
                    {
                        System.Diagnostics.Debug.WriteLine($"QUERY: Promotion {promotion.PromotionId}:");
                        System.Diagnostics.Debug.WriteLine($"  Name: {promotion.Name}");
                        System.Diagnostics.Debug.WriteLine($"  Type: {promotion.PromotionType}");
                        System.Diagnostics.Debug.WriteLine($"  DiscountValue: {promotion.DiscountValue}");
                        System.Diagnostics.Debug.WriteLine($"  DiscountValue type: {promotion.DiscountValue.GetType()}");
                        System.Diagnostics.Debug.WriteLine($"  IsActive: {promotion.IsActive}");
                        System.Diagnostics.Debug.WriteLine($"  StartDate: {promotion.StartDate}");
                        System.Diagnostics.Debug.WriteLine($"  EndDate: {promotion.EndDate}");
                        System.Diagnostics.Debug.WriteLine($"  UsageLimit: {promotion.UsageLimit}");
                        System.Diagnostics.Debug.WriteLine($"  UsageCount: {promotion.UsageCount}");
                    }

                    return promotions;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<Promotion> GetActivePromotions()
        {
            try
            {
                using (var context = new DataContext())
                {
                    DateTime now = DateTime.Now;
                    return context.Promotions
                        .Include(p => p.Product)
                        .Where(p => p.IsActive &&
                               p.StartDate <= now &&
                               (!p.EndDate.HasValue || p.EndDate >= now) &&
                               (!p.UsageLimit.HasValue || p.UsageCount < p.UsageLimit))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool IsPromotionValid(int promotionId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    DateTime now = DateTime.Now;
                    var promotion = context.Promotions.Find(promotionId);

                    if (promotion == null)
                        return false;

                    return promotion.IsActive &&
                           promotion.StartDate <= now &&
                           (!promotion.EndDate.HasValue || promotion.EndDate >= now) &&
                           (!promotion.UsageLimit.HasValue || promotion.UsageCount < promotion.UsageLimit);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public decimal CalculatePromotionalPrice(int productId, decimal originalPrice)
        {
            try
            {
                var activePromotions = GetActivePromotionsForProduct(productId);

                if (activePromotions == null || !activePromotions.Any())
                    return originalPrice;

                // Putem implementa diferite strategii pentru a alege cea mai bună promoție
                // Aici vom folosi cea care oferă cel mai mic preț
                decimal minPrice = originalPrice;

                foreach (var promotion in activePromotions)
                {
                    decimal promotionalPrice = originalPrice;

                    switch (promotion.PromotionType)
                    {
                        case "PercentOff":
                            // Log the exact discount value being used
                            System.Diagnostics.Debug.WriteLine($"CALC: Processing PercentOff promotion for product {productId}:");
                            System.Diagnostics.Debug.WriteLine($"  Promotion Name: {promotion.Name}");
                            System.Diagnostics.Debug.WriteLine($"  Discount Value (raw): {promotion.DiscountValue}");
                            System.Diagnostics.Debug.WriteLine($"  Discount Value type: {promotion.DiscountValue.GetType()}");
                            System.Diagnostics.Debug.WriteLine($"  Original Price: {originalPrice}");
                            System.Diagnostics.Debug.WriteLine($"  Original Price type: {originalPrice.GetType()}");

                            // Step by step calculation with detailed logging
                            decimal discountDecimal = promotion.DiscountValue / 100.0m;
                            System.Diagnostics.Debug.WriteLine($"  Discount as decimal: {discountDecimal}");

                            decimal multiplier = 1 - discountDecimal;
                            System.Diagnostics.Debug.WriteLine($"  Multiplier (1 - discount): {multiplier}");

                            // Convertim explicit la decimal pentru a evita diviziunea întreagă
                            promotionalPrice = originalPrice * multiplier;
                            System.Diagnostics.Debug.WriteLine($"  Calculated Price (before rounding): {promotionalPrice}");
                            System.Diagnostics.Debug.WriteLine($"  Calculated Price type: {promotionalPrice.GetType()}");
                            System.Diagnostics.Debug.WriteLine($"  Is exactly 43.5? {promotionalPrice == 43.5m}");

                            // Rotunjim la 2 zecimale pentru precizie monetară corectă
                            promotionalPrice = Math.Round(promotionalPrice, 2, MidpointRounding.AwayFromZero);
                            System.Diagnostics.Debug.WriteLine($"  Final Price (after rounding): {promotionalPrice}");
                            System.Diagnostics.Debug.WriteLine($"  Final Price type: {promotionalPrice.GetType()}");
                            break;

                        case "FixedAmount":
                            System.Diagnostics.Debug.WriteLine($"CALC: Processing FixedAmount promotion for product {productId}:");
                            System.Diagnostics.Debug.WriteLine($"  Promotion Name: {promotion.Name}");
                            System.Diagnostics.Debug.WriteLine($"  Discount Value: {promotion.DiscountValue}");
                            System.Diagnostics.Debug.WriteLine($"  Original Price: {originalPrice}");

                            promotionalPrice = originalPrice - promotion.DiscountValue;
                            System.Diagnostics.Debug.WriteLine($"  Calculated Price (before rounding): {promotionalPrice}");

                            // Rotunjim la 2 zecimale pentru precizie monetară corectă
                            promotionalPrice = Math.Round(promotionalPrice, 2, MidpointRounding.AwayFromZero);
                            System.Diagnostics.Debug.WriteLine($"  Final Price (after rounding): {promotionalPrice}");
                            break;

                            // Alte tipuri de promoții pot fi adăugate aici
                    }

                    if (promotionalPrice < minPrice)
                        minPrice = promotionalPrice;
                }

                // Ne asigurăm că prețul nu este negativ
                return Math.Max(0, minPrice);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool ApplyCouponCode(string couponCode, int productId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    DateTime now = DateTime.Now;
                    var promotion = context.Promotions
                        .FirstOrDefault(p => p.CouponCode == couponCode &&
                                      p.ProductId == productId &&
                                      p.IsActive &&
                                      p.StartDate <= now &&
                                      (!p.EndDate.HasValue || p.EndDate >= now) &&
                                      (!p.UsageLimit.HasValue || p.UsageCount < p.UsageLimit));

                    if (promotion == null)
                        return false;

                    // Incrementăm contorul de utilizare
                    promotion.UsageCount++;
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

        public decimal? GetPromotionalPrice(int productId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var product = context.Products.Find(productId);

                    if (product == null)
                        return null;

                    return CalculatePromotionalPrice(productId, product.Price);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public bool IncrementPromotionUsage(int promotionId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    var promotion = context.Promotions.Find(promotionId);

                    if (promotion == null)
                        return false;

                    promotion.UsageCount++;
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