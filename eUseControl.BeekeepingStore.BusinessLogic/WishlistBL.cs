using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using eUseControl.BeekeepingStore.BusinessLogic.Interfaces;
using eUseControl.BeekeepingStore.Domain.Entities.User;

namespace eUseControl.BeekeepingStore.BusinessLogic
{
    public class WishlistBL : IWishlist
    {
        public bool AddToWishlist(int userId, int productId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Verificăm dacă produsul e deja în wishlist
                    var existingItem = context.Wishlists
                        .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

                    if (existingItem != null)
                    {
                        // Produsul există deja în wishlist
                        return false;
                    }

                    // Adăugăm produsul în wishlist
                    var wishlistItem = new Wishlist
                    {
                        UserId = userId,
                        ProductId = productId,
                        DateAdded = DateTime.Now
                    };

                    context.Wishlists.Add(wishlistItem);
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

        public bool RemoveFromWishlist(int userId, int productId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    // Găsim produsul în wishlist
                    var wishlistItem = context.Wishlists
                        .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);

                    if (wishlistItem == null)
                    {
                        // Produsul nu există în wishlist
                        return false;
                    }

                    // Ștergem produsul din wishlist
                    context.Wishlists.Remove(wishlistItem);
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

        public bool IsInWishlist(int userId, int productId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.Wishlists
                        .Any(w => w.UserId == userId && w.ProductId == productId);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        public List<Wishlist> GetUserWishlist(int userId)
        {
            try
            {
                using (var context = new DataContext())
                {
                    return context.Wishlists
                        .Include(w => w.Product)
                        .Where(w => w.UserId == userId)
                        .ToList();
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