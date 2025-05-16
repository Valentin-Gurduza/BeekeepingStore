using System.Collections.Generic;
using eUseControl.BeekeepingStore.Domain.Entities.User;

namespace eUseControl.BeekeepingStore.BusinessLogic.Interfaces
{
    public interface IWishlist
    {
        /// <summary>
        /// Adaugă un produs în wishlist
        /// </summary>
        /// <param name="userId">Id-ul utilizatorului</param>
        /// <param name="productId">Id-ul produsului</param>
        /// <returns>true dacă a fost adăugat cu succes, false dacă era deja în wishlist</returns>
        bool AddToWishlist(int userId, int productId);

        /// <summary>
        /// Șterge un produs din wishlist
        /// </summary>
        /// <param name="userId">Id-ul utilizatorului</param>
        /// <param name="productId">Id-ul produsului</param>
        /// <returns>true dacă a fost șters cu succes, false în caz contrar</returns>
        bool RemoveFromWishlist(int userId, int productId);

        /// <summary>
        /// Verifică dacă un produs este în wishlist-ul utilizatorului
        /// </summary>
        /// <param name="userId">Id-ul utilizatorului</param>
        /// <param name="productId">Id-ul produsului</param>
        /// <returns>true dacă produsul este în wishlist, false în caz contrar</returns>
        bool IsInWishlist(int userId, int productId);

        /// <summary>
        /// Obține toate produsele din wishlist-ul unui utilizator
        /// </summary>
        /// <param name="userId">Id-ul utilizatorului</param>
        /// <returns>Lista de produse din wishlist</returns>
        List<Wishlist> GetUserWishlist(int userId);
    }
}