using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class PurchaseReceiptMaster
    {
        public long PurchaseReceiptID { get; set; }

        [Required(ErrorMessage = "Receipt No is required.")]
        [StringLength(30)]
        [Display(Name = "Receipt No")]
        public string ReceiptNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supplier is required.")]
        [Display(Name = "Supplier")]
        public int SupplierID { get; set; }

        [Display(Name = "Purchase Order")]
        public long? PurchaseOrderID { get; set; }

        [Required(ErrorMessage = "Receipt Date is required.")]
        [Display(Name = "Receipt Date")]
        [DataType(DataType.Date)]
        public DateTime ReceiptDate { get; set; } = DateTime.Today;

        [StringLength(50)]
        [Display(Name = "Supplier Invoice / Challan No")]
        public string? SupplierInvoiceNo { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Posted, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public string? SupplierName { get; set; }
        public string? PurchaseOrderNo { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Operation { get; set; }

        // Details lines
        public List<PurchaseReceiptDetails> DetailsList { get; set; } = new();
    }

    public class PurchaseReceiptDetails
    {
        public long PurchaseReceiptDetailID { get; set; }
        public long PurchaseReceiptID { get; set; }
        public long? PurchaseOrderDetailID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Accepted Quantity cannot be negative.")]
        public decimal AcceptedQuantity { get; set; }

        public decimal RejectedQuantity { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Unit Cost cannot be negative.")]
        public decimal UnitCost { get; set; }

        // Display
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
    }
}

