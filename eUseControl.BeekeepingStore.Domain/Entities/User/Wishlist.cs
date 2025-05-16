using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eUseControl.BeekeepingStore.Domain.Entities.User
{
    public class Wishlist
    {
        [Key]
        public int WishlistId { get; set; }

        public int UserId { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("UserId")]
        public virtual UDBTable User { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product.Product Product { get; set; }

        public DateTime DateAdded { get; set; }
    }
}