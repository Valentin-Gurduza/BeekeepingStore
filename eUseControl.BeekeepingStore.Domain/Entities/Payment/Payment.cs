using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eUseControl.BeekeepingStore.Domain.Entities.Payment
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(100)]
        public string TransactionId { get; set; }

        [StringLength(500)]
        public string TransactionDetails { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Last 4 digits of card (if applicable)
        [StringLength(4)]
        public string CardLast4 { get; set; }

        // Card brand (if applicable)
        [StringLength(50)]
        public string CardBrand { get; set; }

        // Error message (if payment failed)
        [StringLength(500)]
        public string ErrorMessage { get; set; }

        // Navigation property to Order
        [ForeignKey("OrderId")]
        public virtual Order.Order Order { get; set; }
    }
}