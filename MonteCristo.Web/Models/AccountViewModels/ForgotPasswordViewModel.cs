using System.ComponentModel.DataAnnotations;

namespace MonteCristo.Web.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Trường yêu cầu bắt buộc")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
