using System;
using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.Entities
{
    public class Role
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Role Name is required.")]
        [StringLength(100, ErrorMessage = "Role Name cannot exceed 100 characters.")]
        [Display(Name = "Role Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // UI / Display properties
        public string? Operation { get; set; }
        public int AssignedMenuCount { get; set; }
    }
}

