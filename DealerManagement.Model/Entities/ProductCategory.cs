using System;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class ProductCategory
    {
        public int ProductCategoryID { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Operation { get; set; }
    }
}

