using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class ProductSize
    {
        public int ProductSizeID { get; set; }

        [Required(ErrorMessage = "Size Name is required.")]
        [StringLength(50)]
        [Display(Name = "Size Name")]
        public string SizeName { get; set; } = string.Empty;

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public string? Operation { get; set; }
    }
}

