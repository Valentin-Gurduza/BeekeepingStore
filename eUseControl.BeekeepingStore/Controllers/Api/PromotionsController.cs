using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using eUseControl.BeekeepingStore.BusinessLogic;
using eUseControl.BeekeepingStore.Models;
using DomainProduct = eUseControl.BeekeepingStore.Domain.Entities.Product.Product;

namespace eUseControl.BeekeepingStore.Controllers.Api
{
    [System.Web.Http.RoutePrefix("api/promotions")]
    public class PromotionsController : ApiController
    {
        private readonly BusinessLogic.BusinessLogic _businessLogic = new BusinessLogic.BusinessLogic();

        /// <summary>
        /// Gets current promotional prices for all products
        /// </summary>
        /// <returns>List of products with their promotional prices</returns>
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("current")]
        public IHttpActionResult GetCurrentPromotionalPrices()
        {
            try
            {
                // Get all active promotions
                var promotionBL = _businessLogic.GetPromotionBL;
                var productBL = _businessLogic.GetProductBL;

                // Get all products and their promotional prices
                var domainProducts = productBL.GetAllProducts();
                var result = new List<object>();

                foreach (var domainProduct in domainProducts)
                {
                    // The domain Product entity uses ProductId, not Id
                    int productId = domainProduct.ProductId;

                    var promotionalPrice = promotionBL.GetPromotionalPrice(productId);

                    if (promotionalPrice.HasValue && promotionalPrice.Value < domainProduct.Price)
                    {
                        // Asigurăm-ne că prețul promotional este rotunjit corect la 2 zecimale
                        decimal roundedPrice = Math.Round(promotionalPrice.Value, 2, MidpointRounding.AwayFromZero);

                        // Adaugă product cu promotional price la rezultat
                        var resultItem = new
                        {
                            productId = productId,
                            name = domainProduct.Name,
                            originalPrice = domainProduct.Price,
                            promotionalPrice = roundedPrice
                        };

                        // Log pentru a verifica ce valori sunt trimise
                        System.Diagnostics.Debug.WriteLine($"API: Product {productId} ({domainProduct.Name}): Original={domainProduct.Price}, Promotional={roundedPrice}");

                        result.Add(resultItem);
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}