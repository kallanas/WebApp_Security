using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;
using WebApp.Services;

namespace WebApp.Pages.Account
{
    public class LoginTwoFactorModel : PageModel
    {
        [BindProperty]
        public EmailMFA EmailMFA { get; set; }

        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly SignInManager<User> _signInManager;

        public LoginTwoFactorModel(UserManager<User> userManager, IEmailService emailService, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
            EmailMFA = new EmailMFA();
        }
        public async Task OnGetAsync(string email, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);

            EmailMFA.SecurityCode = string.Empty;
            EmailMFA.RememberMe = rememberMe;

            //generate the code
            var securityCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            //send the code to the user
            await _emailService.SendAsync("cal.anastasiadou@outlook.com", email, "My WebApp's code", $"Please use this code as the OTP: {securityCode}");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.TwoFactorSignInAsync("Email", EmailMFA.SecurityCode, EmailMFA.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login2FA", "You are locked out.");
                }
                else
                {
                    ModelState.AddModelError("Login2FA", "Failed to login");
                }

                return Page();
            }
        }
    }

    public class EmailMFA
    {
        [Required]
        [Display(Name = "Security code")]
        public string SecurityCode { get; set; }
        public bool RememberMe { get; set; }
    }
}
