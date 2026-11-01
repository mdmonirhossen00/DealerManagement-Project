using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class CustomerReturnMaster
    {
        public long CustomerReturnID { get; set; }

        [Required(ErrorMessage = "Return No is required.")]
        [StringLength(30)]
        [Display(Name = "Sales Return No")]
        public string CustomerReturnNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer / Dealer is required.")]
        [Display(Name = "Customer / Dealer")]
        public int CustomerID { get; set; }

        [Display(Name = "Delivery Challan Reference")]
        public long? CustomerDeliveryID { get; set; }

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
        public string? CustomerName { get; set; }
        public string? DeliveryNo { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Operation { get; set; }

        public List<CustomerReturnDetails> DetailsList { get; set; } = new();
    }

    public class CustomerReturnDetails
    {
        public long CustomerReturnDetailID { get; set; }
        public long CustomerReturnID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be non-negative.")]
        public decimal UnitPrice { get; set; }

        [StringLength(30)]
        [Display(Name = "Condition")]
        public string? ReturnCondition { get; set; } = "Good"; // Good, Damaged, Broken

        [StringLength(250)]
        public string? Reason { get; set; }

        // Display
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
    }
}

