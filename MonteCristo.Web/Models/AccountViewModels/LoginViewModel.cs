using System.ComponentModel.DataAnnotations;

namespace MonteCristo.Web.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Trường yêu cầu bắt buộc")]
        [EmailAddress(ErrorMessage = "Tài khoản phải là một email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Trường yêu cầu bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}