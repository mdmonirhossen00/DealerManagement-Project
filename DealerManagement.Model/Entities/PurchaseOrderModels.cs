using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class PurchaseOrderMaster
    {
        public long PurchaseOrderID { get; set; }

        [Required(ErrorMessage = "PO Number is required.")]
        [StringLength(30)]
        [Display(Name = "PO Number")]
        public string PurchaseOrderNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supplier is required.")]
        [Display(Name = "Supplier")]
        public int SupplierID { get; set; }

        [Required(ErrorMessage = "Order Date is required.")]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Display(Name = "Expected Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpectedDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Approved, PartiallyReceived, Completed, Cancelled

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display properties
        public string? SupplierName { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Operation { get; set; }

        // Master-Details lines list
        public List<PurchaseOrderDetails> DetailsList { get; set; } = new();
    }

    public class PurchaseOrderDetails
    {
        public long PurchaseOrderDetailID { get; set; }
        public long PurchaseOrderID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be non-negative.")]
        public decimal UnitPrice { get; set; }

        public decimal DiscountAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal LineTotal { get; set; } // (Quantity * UnitPrice) - DiscountAmount + TaxAmount

        [StringLength(250)]
        public string? Remarks { get; set; }

        // Display properties
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
    }
}

