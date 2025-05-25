using eUseControl.BeekeepingStore.Domain.Entities.Product;
using System;
using System.Collections.Generic;

namespace eUseControl.BeekeepingStore.BusinessLogic.Interfaces
{
    public interface IPromotion
    {
        // Adaugă o promoție nouă
        int AddPromotion(Promotion promotion);

        // Actualizează o promoție existentă
        bool UpdatePromotion(Promotion promotion);

        // Șterge o promoție
        bool DeletePromotion(int promotionId);

        // Obține o promoție după ID
        Promotion GetPromotionById(int promotionId);

        // Obține toate promoțiile
        List<Promotion> GetAllPromotions();

        // Obține promoțiile pentru un produs specific
        List<Promotion> GetPromotionsForProduct(int productId);

        // Obține promoțiile active pentru un produs
        List<Promotion> GetActivePromotionsForProduct(int productId);

        // Obține promoțiile active
        List<Promotion> GetActivePromotions();

        // Verifică valabilitatea unei promoții
        bool IsPromotionValid(int promotionId);

        // Calculează prețul după aplicarea promoției
        decimal CalculatePromotionalPrice(int productId, decimal originalPrice);

        // Aplică un cod de cupon la un produs
        bool ApplyCouponCode(string couponCode, int productId);

        // Obține preț promoțional pentru un produs
        decimal? GetPromotionalPrice(int productId);

        // Incrementează contorul de utilizare al unei promoții
        bool IncrementPromotionUsage(int promotionId);
    }
}