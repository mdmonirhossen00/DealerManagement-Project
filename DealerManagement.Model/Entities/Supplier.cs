using System;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class Supplier
    {
        public int SupplierID { get; set; }

        [Required(ErrorMessage = "Supplier Code is required.")]
        [StringLength(30)]
        [Display(Name = "Supplier Code")]
        public string SupplierCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supplier Name is required.")]
        [StringLength(150)]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [StringLength(30)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(50)]
        [Display(Name = "Tax / BIN / TIN No")]
        public string? TaxRegistrationNo { get; set; }

        [Display(Name = "Opening Date")]
        [DataType(DataType.Date)]
        public DateTime? OpeningDate { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Operation { get; set; }
    }
}

