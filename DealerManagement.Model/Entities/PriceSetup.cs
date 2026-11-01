using System;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class PriceSetup
    {
        public long PriceSetupID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        [Display(Name = "Product")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Price Type is required.")]
        [StringLength(30)]
        [Display(Name = "Price Type")]
        public string PriceType { get; set; } = "Dealer"; // Purchase, Dealer, Retail, Wholesale

        [Required(ErrorMessage = "Unit Price is required.")]
        [Display(Name = "Unit Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Effective From date is required.")]
        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        [Display(Name = "Effective To")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display properties
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
        public string? Operation { get; set; }
    }
}

