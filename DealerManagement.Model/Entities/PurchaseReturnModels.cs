using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class PurchaseReturnMaster
    {
        public long PurchaseReturnID { get; set; }

        [Required(ErrorMessage = "Return No is required.")]
        [StringLength(30)]
        [Display(Name = "Purchase Return No")]
        public string PurchaseReturnNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supplier is required.")]
        [Display(Name = "Supplier")]
        public int SupplierID { get; set; }

        [Display(Name = "Purchase Receipt Reference")]
        public long? PurchaseReceiptID { get; set; }

        [Required(ErrorMessage = "Return Date is required.")]
        [Display(Name = "Return Date")]
        [DataType(DataType.Date)]
        public DateTime ReturnDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Posted, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public string? SupplierName { get; set; }
        public string? ReceiptNo { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Operation { get; set; }

        public List<PurchaseReturnDetails> DetailsList { get; set; } = new();
    }

    public class PurchaseReturnDetails
    {
        public long PurchaseReturnDetailID { get; set; }
        public long PurchaseReturnID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be non-negative.")]
        public decimal UnitPrice { get; set; }

        [StringLength(250)]
        public string? Reason { get; set; }

        // Display
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
    }
}

