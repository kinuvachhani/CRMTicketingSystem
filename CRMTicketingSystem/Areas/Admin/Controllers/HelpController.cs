using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Enum;
using CRMTicketingSystem.Models;
using CRMTicketingSystem.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRMTicketingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class HelpController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public HelpController(IUnitOfWork unitOfWork, ApplicationDbContext db, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Reviewed()
        {
            return View();
        }

        public IActionResult Review(int id)
        {
            Help help = new Help();
            //this is for edit
            help = _unitOfWork.Help.GetFirstOrDefault(i=>i.Id==id);
            if (help != null && help.TicketStatus == "9")
            {
                return RedirectToAction("Reviewed", "Ticket");
            }
            return View(help);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Review(Help help)
        {
            var temp = _unitOfWork.Help.GetFirstOrDefault(i => i.Id == help.Id);
            temp.Review = help.Review;

            _unitOfWork.Help.Update(help);
            _unitOfWork.Save();

            EmailTemplate emailTemplate = _db.EmailTemplates.Where(e => e.Id == Convert.ToInt32(EnEmailTemplate.TicketReview)).FirstOrDefault();
            emailTemplate.Content = emailTemplate.Content.Replace("###Description###", temp.Description);
            emailTemplate.Content = emailTemplate.Content.Replace("###Review###", temp.Review);
            _emailSender.SendEmailAsync(temp.Email, emailTemplate.Subject, emailTemplate.Content);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Resolve(int id)
        {
            var objFromDb = _db.Helps.FirstOrDefault(s => s.Id == id);
            if (objFromDb != null)
            {
                if(objFromDb.TicketStatus== "9")
                {
                    return Json(new { success = false, message = "Ticket Already Resolved." });
                }
                objFromDb.TicketStatus = "9";
                _unitOfWork.Save();

                EmailTemplate emailTemplate = _db.EmailTemplates.Where(e => e.Id == Convert.ToInt32(EnEmailTemplate.TicketResolve)).FirstOrDefault();
                var appuser = _db.Helps.FirstOrDefault(u => u.Email == objFromDb.Email);
                _emailSender.SendEmailAsync(objFromDb.Email, emailTemplate.Subject, emailTemplate.Content);

                return Json(new { success = true, message = "Resolve Successful." });
            }
            return RedirectToAction(nameof(Index));
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Help.GetAll();
            return Json(new { data = allObj });
        }
        #endregion
    }
}