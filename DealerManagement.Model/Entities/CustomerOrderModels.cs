using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class CustomerOrderMaster
    {
        public long CustomerOrderID { get; set; }

        [Required(ErrorMessage = "Order No is required.")]
        [StringLength(30)]
        [Display(Name = "Order No")]
        public string CustomerOrderNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer / Dealer is required.")]
        [Display(Name = "Customer / Dealer")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Order Date is required.")]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Display(Name = "Required Date")]
        [DataType(DataType.Date)]
        public DateTime? RequiredDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Customer Reference / PO Ref")]
        public string? CustomerReference { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Approved, PartiallyDelivered, Completed, Cancelled

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public string? CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Operation { get; set; }

        public List<CustomerOrderDetails> DetailsList { get; set; } = new();
    }

    public class CustomerOrderDetails
    {
        public long CustomerOrderDetailID { get; set; }
        public long CustomerOrderID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be non-negative.")]
        public decimal UnitPrice { get; set; }

        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal LineTotal { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }

        // Display
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
    }
}

