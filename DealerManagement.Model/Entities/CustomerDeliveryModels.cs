using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class CustomerDeliveryMaster
    {
        public long CustomerDeliveryID { get; set; }

        [Required(ErrorMessage = "Delivery No is required.")]
        [StringLength(30)]
        [Display(Name = "Challan / Delivery No")]
        public string DeliveryNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer / Dealer is required.")]
        [Display(Name = "Customer / Dealer")]
        public int CustomerID { get; set; }

        [Display(Name = "Customer Order Reference")]
        public long? CustomerOrderID { get; set; }

        [Required(ErrorMessage = "Delivery Date is required.")]
        [Display(Name = "Delivery Date")]
        [DataType(DataType.Date)]
        public DateTime DeliveryDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        [Display(Name = "Delivery Address")]
        public string? DeliveryAddress { get; set; }

        [StringLength(50)]
        [Display(Name = "Vehicle No")]
        public string? VehicleNo { get; set; }

        [StringLength(100)]
        [Display(Name = "Driver Name")]
        public string? DriverName { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Draft"; // Draft, Dispatched, Delivered, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display
        public string? CustomerName { get; set; }
        public string? CustomerOrderNo { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Operation { get; set; }

        public List<CustomerDeliveryDetails> DetailsList { get; set; } = new();
    }

    public class CustomerDeliveryDetails
    {
        public long CustomerDeliveryDetailID { get; set; }
        public long CustomerDeliveryID { get; set; }
        public long? CustomerOrderDetailID { get; set; }

        [Required(ErrorMessage = "Product is required.")]
        public int ProductID { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be non-negative.")]
        public decimal UnitPrice { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }

        // Display
        public string? ProductCode { get; set; }
        public string? ProductNameText { get; set; }
        public string? UnitName { get; set; }
    }
}

