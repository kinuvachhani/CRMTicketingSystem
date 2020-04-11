using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using CRMTicketingSystem.DataAccess.Data;
using System.Linq;
using CRMTicketingSystem.Enum;
using CRMTicketingSystem.Models;

namespace CRMTicketingSystem.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender,ApplicationDbContext db)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                //Email send
                EmailTemplate emailTemplate = _db.EmailTemplates.Where(e => e.Id == Convert.ToInt32(EnEmailTemplate.ForgotPassword)).FirstOrDefault();
                var appuser = _db.ApplicationUsers.FirstOrDefault(u => u.Email == Input.Email);
                emailTemplate.Content = emailTemplate.Content.Replace("###CallbackUrl###", callbackUrl);
                await _emailSender.SendEmailAsync(appuser.Email, emailTemplate.Subject, emailTemplate.Content);


                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
