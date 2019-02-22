using System.ComponentModel.DataAnnotations;

namespace MonteCristo.Web.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Trường yêu cầu bắt buộc")]
        [EmailAddress(ErrorMessage = "Tài khoản phải là một email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Trường yêu cầu bắt buộc")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Mật khẩu chưa khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
