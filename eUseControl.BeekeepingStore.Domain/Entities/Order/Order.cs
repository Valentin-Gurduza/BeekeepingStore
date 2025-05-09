using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eUseControl.BeekeepingStore.Domain.Entities.Order
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string OrderStatus { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [MaxLength(255)]
        public string ShippingAddress { get; set; }

        [MaxLength(255)]
        public string BillingAddress { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        [MaxLength(100)]
        public string PaymentStatus { get; set; }

        public DateTime? ShippedDate { get; set; }

        [MaxLength(100)]
        public string TrackingNumber { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation property for order items (used by Entity Framework)
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}