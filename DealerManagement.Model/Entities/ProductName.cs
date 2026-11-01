using System;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class ProductName
    {
        public int ProductNameID { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public int ProductCategoryID { get; set; }

        [Required(ErrorMessage = "Product Code is required.")]
        [StringLength(50)]
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(150)]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display properties for grid & views
        public string? CategoryName { get; set; }
        public string? Operation { get; set; }
    }
}

