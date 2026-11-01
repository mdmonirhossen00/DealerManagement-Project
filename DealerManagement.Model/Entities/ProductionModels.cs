using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class ProductionMaster
    {
        public long ProductionID { get; set; }

        [Required(ErrorMessage = "Production No is required.")]
        [StringLength(30)]
        [Display(Name = "Production No")]
        public string ProductionNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required.")]
        [Display(Name = "Production Date")]
        [DataType(DataType.Date)]
        public DateTime ProductionDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Posted"; // Draft, Posted, Cancelled

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public decimal TotalQuantity { get; set; }
        public string? Operation { get; set; }

        public List<ProductionDetails> DetailsList { get; set; } = new();
    }

    public class ProductionDetails
    {
        public long ProductionDetailID { get; set; }
        public long ProductionID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; }

        public decimal? UnitCost { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }

        // Display
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
    }
}

