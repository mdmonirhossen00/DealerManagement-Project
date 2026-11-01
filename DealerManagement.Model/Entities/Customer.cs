using System;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class Customer
    {
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Customer Code is required.")]
        [StringLength(30)]
        [Display(Name = "Customer / Dealer Code")]
        public string CustomerCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer Name is required.")]
        [StringLength(150)]
        [Display(Name = "Customer / Dealer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer Type is required.")]
        [StringLength(30)]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; } = "Dealer"; // Dealer, Retailer, Corporate

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(30)]
        [Display(Name = "Alternate Phone")]
        public string? AlternatePhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Area { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [Display(Name = "Credit Limit")]
        [Range(0, double.MaxValue, ErrorMessage = "Credit limit must be positive.")]
        public decimal CreditLimit { get; set; } = 0;

        [Display(Name = "Opening Date")]
        [DataType(DataType.Date)]
        public DateTime? OpeningDate { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Operation { get; set; }
    }
}

