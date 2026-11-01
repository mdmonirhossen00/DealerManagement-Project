using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class ProductGrade
    {
        public int ProductGradeID { get; set; }

        [Required(ErrorMessage = "Grade Name is required.")]
        [StringLength(50)]
        [Display(Name = "Grade Name")]
        public string GradeName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public string? Operation { get; set; }
    }
}

