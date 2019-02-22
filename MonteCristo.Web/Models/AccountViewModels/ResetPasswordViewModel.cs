using System.ComponentModel.DataAnnotations;

namespace MonteCristo.Web.Models.AccountViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Trường yêu cầu bắt buộc")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Trường yêu cầu bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu tối thiếu phải dài từ {2} đến {1} kí tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu chưa khớp.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
