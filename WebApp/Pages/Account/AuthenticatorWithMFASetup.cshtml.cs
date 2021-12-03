using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    [Authorize]
    public class AuthenticatorWithMFASetupModel : PageModel
    {
        [BindProperty]
        public SetupMFAViewModel ViewModel { get; set; }

        [BindProperty]
        public bool Succeeded { get; set; }

        private readonly UserManager<User> _userManager;

        public AuthenticatorWithMFASetupModel(UserManager<User> userManager)
        {
            _userManager = userManager;
            ViewModel = new SetupMFAViewModel();
            Succeeded = false;
        }
        
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(base.User);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            ViewModel.Key = key;

            ViewModel.QRCodeBytes = GenerateQRCodeBytes("my web app", key, user.Email);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);

            var result = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, ViewModel.SecurityCode);

            if (result)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                Succeeded = true;
            }
            else
            {
                ModelState.AddModelError("AuthenticatorSetup", "Something went wrong with authenticator setup");
            }

            return Page();
        }

        private Byte[] GenerateQRCodeBytes(string provider, string key, string userEmail)
        {
            var qrCodeGenerator = new QRCodeGenerator();
            var qrCodeData = qrCodeGenerator.CreateQrCode($"otpauth://totp/{provider}:{userEmail}?secret={key}&issuer={provider}", QRCodeGenerator.ECCLevel.Q);

            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            return BitmapToByteArray(qrCodeImage);
        }

        private Byte[] BitmapToByteArray(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
        
    }

    public class SetupMFAViewModel
    {
        public string Key { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string SecurityCode { get; set; }

        public Byte[] QRCodeBytes { get; set; }
    }
}
