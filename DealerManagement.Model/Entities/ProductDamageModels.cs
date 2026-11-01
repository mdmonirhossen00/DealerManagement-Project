using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class ProductDamageMaster
    {
        public long ProductDamageMasterID { get; set; }

        [Required(ErrorMessage = "Report No is required.")]
        [StringLength(30)]
        [Display(Name = "Report No")]
        public string DamageReportNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Report Date is required.")]
        [Display(Name = "Report Date")]
        [DataType(DataType.Date)]
        public DateTime ReportDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Report Type is required.")]
        [StringLength(20)]
        [Display(Name = "Report Type")]
        public string ReportType { get; set; } = "Damaged"; // Damaged, Broken, Missing

        [StringLength(100)]
        [Display(Name = "Location / Warehouse")]
        public string? LocationName { get; set; }

        [StringLength(100)]
        [Display(Name = "Reported By")]
        public string? ReportedBy { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Posted, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public decimal TotalQuantity { get; set; }
        public decimal TotalLoss { get; set; }
        public string? Operation { get; set; }

        public List<ProductDamageDetails> DetailsList { get; set; } = new();
    }

    public class ProductDamageDetails
    {
        public long ProductDamageDetailID { get; set; }
        public long ProductDamageMasterID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; }

        [Display(Name = "Estimated Loss")]
        [Range(0, double.MaxValue, ErrorMessage = "Loss must be non-negative.")]
        public decimal EstimatedLoss { get; set; } = 0;

        [StringLength(250)]
        public string? Reason { get; set; }

        // Display
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
    }
}

