using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Data.Account;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
            var emailClaim = loginInfo.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var userClaim = loginInfo.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

            if (emailClaim != null && userClaim != null)
            {
                var user = new User { Email = emailClaim.Value, UserName = userClaim.Value };

                await _userManager.CreateAsync(user);

                await _signInManager.SignInAsync(user, false);
            }
            return RedirectToPage("/Index");
        }
    }
}
