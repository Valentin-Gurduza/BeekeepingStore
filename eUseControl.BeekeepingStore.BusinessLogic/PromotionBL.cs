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
                    return context.Promotions
                        .Include(p => p.Product)
                        .Where(p => p.ProductId == productId &&
                               p.IsActive &&
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
                            // Convertim explicit la decimal pentru a evita diviziunea întreagă
                            promotionalPrice = originalPrice * (1 - (promotion.DiscountValue / 100.0m));
                            // Rotunjim la 2 zecimale pentru precizie monetară corectă
                            promotionalPrice = Math.Round(promotionalPrice, 2, MidpointRounding.AwayFromZero);
                            break;

                        case "FixedAmount":
                            promotionalPrice = originalPrice - promotion.DiscountValue;
                            // Rotunjim la 2 zecimale pentru precizie monetară corectă
                            promotionalPrice = Math.Round(promotionalPrice, 2, MidpointRounding.AwayFromZero);
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