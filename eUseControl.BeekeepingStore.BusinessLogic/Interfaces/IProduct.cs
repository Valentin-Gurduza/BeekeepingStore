using eUseControl.BeekeepingStore.Domain.Entities.Product;
using System;
using System.Collections.Generic;

namespace eUseControl.BeekeepingStore.BusinessLogic.Interfaces
{
    public interface IProduct
    {
        // Adaugă un produs nou
        int AddProduct(Product product);

        // Actualizează un produs existent
        bool UpdateProduct(Product product);

        // Șterge un produs
        bool DeleteProduct(int productId);

        // Obține un produs după ID
        Product GetProductById(int productId);

        // Obține toate produsele
        List<Product> GetAllProducts();

        // Obține produse după categorie
        List<Product> GetProductsByCategory(string category);

        // Obține produse în promoție sau cu anumite filtre
        List<Product> GetFilteredProducts(decimal? minPrice = null, decimal? maxPrice = null,
                                          string searchTerm = null, bool activeOnly = true);

        // Obține produse cu stoc redus
        List<Product> GetLowStockProducts(int threshold, int limit);
    }
}