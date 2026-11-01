using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class ProductColor
    {
        public int ProductColorID { get; set; }

        [Required(ErrorMessage = "Color Name is required.")]
        [StringLength(50)]
        [Display(Name = "Color Name")]
        public string ColorName { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Color Code (Hex/RGB)")]
        public string? ColorCode { get; set; }

        public bool IsActive { get; set; } = true;
        public string? Operation { get; set; }
    }
}

