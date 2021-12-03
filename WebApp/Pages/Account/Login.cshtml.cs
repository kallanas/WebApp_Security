using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;

        public LoginModel(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public CredentialViewModel Credential { get; set; }

        [BindProperty]
        public IEnumerable<AuthenticationScheme> ExternalLoginProviders { get; set; }

        public async Task OnGetAsync()
        {
            ExternalLoginProviders = await _signInManager.GetExternalAuthenticationSchemesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.PasswordSignInAsync(Credential.Email, Credential.Password, Credential.RememberMe, false);
            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("/Account/LoginTwoFactorWithAuthenticator",
                        new
                        {
                            //Email = Credential.Email,     that was passed when i redirect to the previous way of 2FA: Email 2fA Authentication  
                            RememberMe = Credential.RememberMe
                        });
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login", "You are locked out.");
                }
                else
                {
                    ModelState.AddModelError("Login", "Failed to login");
                }

                return Page();
            }

        }

        public IActionResult OnPostLoginExternally(string provider)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, null);
            properties.RedirectUri = Url.Action("ExternalLoginCallback", "Account");

            return Challenge(properties, provider);
        }
    }  
    
    public class CredentialViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(dataType:DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
