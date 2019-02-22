using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using MonteCristo.Application.Models.Framework;
using MonteCristo.Application.Services;
using MonteCristo.Web.Extensions;
using MonteCristo.Web.Models;
using MonteCristo.Web.Models.AccountViewModels;

namespace MonteCristo.Web.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IUserService userService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _userService = userService;
            _logger = logger;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = "/")
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { RememberMe = true });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return CustomRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng. Vui lòng thử lại!");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError(string.Empty, string.Join("\n", GetMessages(ModelState)));
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email, Name = model.Email };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    string callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created a new account with password.");

                    return Json(new { isSuccess = true, message = "Đăng ký thành công" });
                }
                AddErrors(result);
            }

            List<string> errors = new List<string>();
            foreach (string item in GetMessages(ModelState))
            {
                if (item.Contains("is already taken"))
                {
                    errors.Add($"Tài khoản {model.Email} đã tồn tại. Vui lòng chọn tài khoản khác");
                }
            }

            // If we got this far, something failed, redisplay form
            return Json(new { isSuccess = false, message = string.Join("\n", errors) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            string redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else // In case user does not exists, create user
            {
                string username = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString().Remove('-');
                string name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? username;
                string email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
                string dob = info.Principal.FindFirstValue(ClaimTypes.DateOfBirth);
                string gender = info.Principal.FindFirstValue(ClaimTypes.Gender);
                string avatar = "";
                if (info.LoginProvider == "Facebook")
                {
                    avatar = $"https://graph.facebook.com/{info.ProviderKey}/picture?type=large";
                }
                if (info.LoginProvider == "Google")
                {
                    avatar = info.Principal.FindFirstValue("image");
                    // avatar = $"https://www.googleapis.com/plus/v1/people/{info.ProviderKey}?fields=image&key={googleApiKey}";
                }

                ApplicationUser user = new ApplicationUser { UserName = username, Email = email, Name = name, Avatar = avatar, Gender = gender };
                IdentityResult result1 = await _userManager.CreateAsync(user);
                if (result1.Succeeded)
                {
                    result1 = await _userManager.AddLoginAsync(user, info);
                    if (result1.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }

                return RedirectToLocal(returnUrl);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                IdentityResult result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //[HttpGet]
        //public async Task<IActionResult> MyProFile()
        //{
        //    string id = _userManager.GetUserId(User);
        //    Profile user = await _userService.GetProfile(id);
        //    if (user == null)
        //    {
        //        throw new ApplicationException($"Unable to load user with ID '{id}'.");
        //    }
        //    return View(user);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> MyProFile(Profile model)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return View(model);
        //        }

        //        ApplicationUser user = await _userManager.GetUserAsync(User);
        //        if (user == null)
        //        {
        //            throw new ApplicationException($"Unable to load user with ID '{model.Id}'.");
        //        }
        //        if (!string.IsNullOrEmpty(model.AvatarBase64))
        //        {
        //            model.Avatar = await _fileService.UpsertAsync(model.AvatarBase64, $"{model.Id}.jpg", "avatar", createSubDateFolder: false);
        //            //model.Avatar = await _fileService.UpsertAsync(model.AvatarBase64, $"{model.Id}/{Guid.NewGuid()}.jpg", "RoomTimeShare", createSubDateFolder: false);
        //        }
        //        else
        //        {
        //            model.Avatar = user.Avatar;
        //        }

        //        user = model.UpdateProfileTo(user);
        //        await _userManager.UpdateAsync(user);

        //        return RedirectToAction(nameof(MyProFile));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Lỗi update MyProFile", ex);
        //    }
        //    return View(model);
        //}

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
                // if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return Json(new { isSuccess = true, message = "Gửi yêu cầu thành công" });
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                string callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Đặt lại mật khẩu",
                   $"Vui lòng click vào link dưới để đặt lại mật khẩu: <a href='{callbackUrl}'>link</a>");
                return Json(new { isSuccess = true, message = "Gửi yêu cầu thành công" });
            }

            // If we got this far, something failed, redisplay form
            return Json(new { isSuccess = false, message = "Gửi yêu cầu không thành công" });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            ResetPasswordViewModel model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { isSuccess = false, message = "Gửi yêu cầu không thành công" });
            }
            ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Json(new { isSuccess = true, message = "Gửi yêu cầu thành công" });
            }
            IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return Json(new { isSuccess = true, message = "Gửi yêu cầu thành công" });
            }
            AddErrors(result);

            List<string> errors = new List<string>();
            foreach (string item in GetMessages(ModelState))
            {
                if (item.Contains("Invalid token."))
                {
                    errors.Add($"Yêu cầu không tồn tại hoặc đã hết hạn. Vui lòng kiểm tra lại.");
                }
            }

            // If we got this far, something failed, redisplay form
            return Json(new { isSuccess = false, message = string.Join("\n", errors) });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private List<string> GetMessages(ModelStateDictionary modelState)
        {
            return modelState.Keys
                    .SelectMany(key => modelState[key].Errors.Select(x => x.ErrorMessage))
                    .ToList();
        }

        #endregion
    }
}
