using System.ComponentModel.DataAnnotations;

namespace DealerManagement.Model.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Please enter Username.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter Password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
        public string? Message { get; set; }
    }
}

