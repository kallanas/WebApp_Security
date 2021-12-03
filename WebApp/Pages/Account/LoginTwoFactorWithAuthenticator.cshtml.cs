using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    public class LoginTwoFactorWithAuthenticatorModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        [BindProperty]
        public AuthenticatorMFA AuthenticatorMFA { get; set; }

        public LoginTwoFactorWithAuthenticatorModel(SignInManager<User> signInManager)
        {
            AuthenticatorMFA = new AuthenticatorMFA();
            _signInManager = signInManager;
        }
        public void OnGet(bool rememberMe)
        {
            AuthenticatorMFA.SecurityCode = string.Empty;
            AuthenticatorMFA.RememberMe = rememberMe;

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(AuthenticatorMFA.SecurityCode, AuthenticatorMFA.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login2FAauthenticator", "You are locked out.");
                }
                else
                {
                    ModelState.AddModelError("Login2FAauthenticator", "Failed to login.");
                }
                return Page();
            }
        }
    }
    public class AuthenticatorMFA
    {
        [Required]
        [Display(Name = "Code")]
        public string SecurityCode { get; set; }
        public bool RememberMe { get; set; }
    }
}
