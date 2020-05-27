using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Enum;
using CRMTicketingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace CRMTicketingSystem.Areas.Identity.Pages.Account.Manage
{
    public class ShowRecoveryCodesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IUnitOfWork _unitofwork;
        private readonly IEmailSender _emailSender;

        [TempData]
        public string[] RecoveryCodes { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public ShowRecoveryCodesModel(ApplicationDbContext db, IUnitOfWork unitofwork,IEmailSender emailSender)
        {
            _db = db;
            _unitofwork = unitofwork;
            _emailSender = emailSender;
        }
        public IActionResult OnGet()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var user = _unitofwork.ApplicationUser.GetFirstOrDefault(i=>i.Id == claim.Value);
            string s = string.Join("  |  ", RecoveryCodes);


            EmailTemplate emailTemplate = _db.EmailTemplates.Where(e => e.Id == Convert.ToInt32(EnEmailTemplate.TwoFAmail)).FirstOrDefault();
            emailTemplate.Content = emailTemplate.Content.Replace("###Name###", user.Name);
            emailTemplate.Content = emailTemplate.Content.Replace("###Code###", s);
            _emailSender.SendEmailAsync(user.Email, emailTemplate.Subject, emailTemplate.Content);

            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }

            return Page();
        }
    }
}
