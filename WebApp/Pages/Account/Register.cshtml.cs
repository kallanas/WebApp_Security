using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;
using WebApp.Services;

namespace WebApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public RegisterModel(UserManager<User> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        public RegisterViewModel RegisterViewModel { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            //validating email address ---> optional, when i already have configured in startup: options.User.RequireUniqueEmail = true;

            // create the user if the email does not exist in db
            var user = new User
            {
                Email = RegisterViewModel.Email,
                UserName = RegisterViewModel.Email,
                //Department = RegisterViewModel.Department,
                //Position = RegisterViewModel.Position     /////another way to collect user infos are with claims, instead of extending the aspnetusers table with these 2 columns
            };

            var claimDepartment = new Claim("Department", RegisterViewModel.Department);
            var claimPosition = new Claim("Position", RegisterViewModel.Position); //these 2 lines are the first step of the other way to collect infos from a user and store them to db as claims

            var result = await _userManager.CreateAsync(user, RegisterViewModel.Password);

            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, claimDepartment);
                await _userManager.AddClaimAsync(user, claimPosition); //second step is here, adding two claims relative to the current user


                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.PageLink(pageName: "/Account/ConfirmEmail", values: new { userId = user.Id, token = confirmationToken });

                await _emailService.SendAsync("cal.anastasiadou@outlook.com", user.Email, "Please confirm your email",
                     $"Please click on this link to confirm your email address: {confirmationLink}");

                return RedirectToPage("/Account/Login");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }
                return Page();
            }
        }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required]
        [DataType(dataType: DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public string Position { get; set; }
    }
}
