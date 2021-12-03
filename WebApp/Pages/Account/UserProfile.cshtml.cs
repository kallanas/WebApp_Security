using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    [Authorize]
    public class UserProfileModel : PageModel
    {
        [BindProperty]
        public UserProfileViewModel UserProfile { get; set; }
        [BindProperty]
        public string SuccessMessage { get; set; }


        private readonly UserManager<User> _userManager;

        public UserProfileModel(UserManager<User> userManager)
        {
            _userManager = userManager;
            UserProfile = new UserProfileViewModel();
            SuccessMessage = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            SuccessMessage = string.Empty;

            var (user, departmentClaim, positionClaim) = await GetUserInfoAsync();
            UserProfile.Department = departmentClaim?.Value;
            UserProfile.Position = positionClaim?.Value;
            UserProfile.Email = user.Email;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            try
            {
                var (user, departmentClaim, positionClaim) = await GetUserInfoAsync();
                await _userManager.ReplaceClaimAsync(user, departmentClaim, new Claim(departmentClaim.Type, UserProfile.Department));
                await _userManager.ReplaceClaimAsync(user, positionClaim, new Claim(positionClaim.Type, UserProfile.Position));
            }
            catch
            {
                ModelState.AddModelError("Profile", "Error occured when saving user profile.");
            }

            SuccessMessage = "The user profile is saved successfully!";

            return Page();
        }

        private async Task<(Data.Account.User, Claim, Claim)> GetUserInfoAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var claims = await _userManager.GetClaimsAsync(user);
            var departmentClaim = claims.FirstOrDefault(x => x.Type == "Department");
            var positionClaim = claims.FirstOrDefault(x => x.Type == "Position");

            return (user, departmentClaim, positionClaim);
        }
    }

    public class UserProfileViewModel
    {
        public string Email { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public string Position { get; set; }
    }
}
