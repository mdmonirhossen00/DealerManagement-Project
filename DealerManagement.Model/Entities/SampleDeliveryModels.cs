using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class SampleDeliveryMaster
    {
        public long SampleDeliveryID { get; set; }

        [Required(ErrorMessage = "Sample Delivery No is required.")]
        [StringLength(30)]
        [Display(Name = "Sample Delivery No")]
        public string SampleDeliveryNo { get; set; } = string.Empty;

        [Display(Name = "Customer / Dealer")]
        public int? CustomerID { get; set; }

        [StringLength(150)]
        [Display(Name = "Delivered To / Recipient")]
        public string? DeliveryTo { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [Display(Name = "Delivery Date")]
        [DataType(DataType.Date)]
        public DateTime DeliveryDate { get; set; } = DateTime.Today;

        [StringLength(250)]
        public string? Purpose { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Posted, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public string? CustomerName { get; set; }
        public string? Operation { get; set; }

        public List<SampleDeliveryDetails> DetailsList { get; set; } = new();
    }

    public class SampleDeliveryDetails
    {
        public long SampleDeliveryDetailID { get; set; }
        public long SampleDeliveryID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; }

        public decimal? UnitCost { get; set; }
        public bool IsReturnable { get; set; } = false;

        [DataType(DataType.Date)]
        [Display(Name = "Expected Return Date")]
        public DateTime? ExpectedReturnDate { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }

        // Display
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
    }
}

