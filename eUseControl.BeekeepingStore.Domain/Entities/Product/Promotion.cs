using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eUseControl.BeekeepingStore.Domain.Entities.Product
{
    [Table("Promotions")]
    public class Promotion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PromotionId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        // Tipul de promoție: "PercentOff", "FixedAmount", "BuyXGetY", etc.
        [Required]
        [MaxLength(50)]
        public string PromotionType { get; set; }

        // Valoarea reducerii (procent sau sumă fixă)
        public decimal DiscountValue { get; set; }

        // Pentru promoții de tip BuyXGetY
        public int? BuyQuantity { get; set; }
        public int? GetQuantity { get; set; }

        // Pentru limitarea numărului de utilizări
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }

        // Pentru limitarea promoției la un anumit grup de utilizatori
        [MaxLength(50)]
        public string CustomerGroup { get; set; }

        // Pentru promoții cu cod de cupon
        [MaxLength(50)]
        public string CouponCode { get; set; }

        // Perioada de valabilitate
        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}