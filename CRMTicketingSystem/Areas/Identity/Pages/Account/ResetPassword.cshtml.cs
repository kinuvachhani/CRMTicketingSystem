using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace CRMTicketingSystem.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitofwork;

        public ResetPasswordModel(UserManager<IdentityUser> userManager, IUnitOfWork unitofwork)
        {
            _userManager = userManager;
            _unitofwork = unitofwork;
        }
        public string Email { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            //[Required]
            //[EmailAddress]
            //public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; }
            //public string Email { get; set; }
        }

        public IActionResult OnGet(string code = null, string email=null)
        {
            
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    //Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                    //Code = _userManager.GenerateEmailConfirmationTokenAsync()
                    Code = code
                };
                Email = email;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                ModelState.AddModelError(string.Empty, "User not Exist. You Registered your account first.");
            }
            Input.Code = WebUtility.UrlDecode(Input.Code);
            var Result = await _userManager.ConfirmEmailAsync(user, Input.Code);
            if (!Result.Succeeded)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user,Input.Code ,Input.Password);
            if (result.Succeeded)
            {
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}
