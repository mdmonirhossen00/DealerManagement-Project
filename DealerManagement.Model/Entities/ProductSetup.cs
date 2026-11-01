using System;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class ProductSetup
    {
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product Code is required.")]
        [StringLength(80)]
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product Name is required.")]
        [Display(Name = "Product Name")]
        public int ProductNameID { get; set; }

        [Display(Name = "Size")]
        public int? ProductSizeID { get; set; }

        [Display(Name = "Color")]
        public int? ProductColorID { get; set; }

        [Display(Name = "Grade")]
        public int? ProductGradeID { get; set; }

        [StringLength(100)]
        [Display(Name = "Barcode")]
        public string? Barcode { get; set; }

        [Required(ErrorMessage = "Unit is required.")]
        [StringLength(30)]
        [Display(Name = "Unit Name")]
        public string UnitName { get; set; } = "PCS";

        [Display(Name = "Reorder Level")]
        [Range(0, double.MaxValue, ErrorMessage = "Reorder level must be positive.")]
        public decimal ReorderLevel { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display properties for grid and views
        public string? ProductNameText { get; set; }
        public string? CategoryName { get; set; }
        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
        public string? GradeName { get; set; }
        public string? Operation { get; set; }
    }
}

