using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Enum;
using CRMTicketingSystem.Models;
using CRMTicketingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRMTicketingSystem.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class TicketController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public TicketController(IUnitOfWork unitOfWork, ApplicationDbContext db, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            TicketVM ticketVM = new TicketVM()
            {
                Ticket = new Ticket(),
                ProductList = _unitOfWork.Product.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Title,
                    Value = i.Id.ToString()
                })
            };

            if (id == null)
            {
                //this is for create
                return View(ticketVM);
            }
            if (ticketVM.Ticket == null)
            {
                return NotFound();
            }
            return View(ticketVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(TicketVM ticketVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Ticket.Add(ticketVM.Ticket);
                ticketVM.Ticket.CreatedDate = DateTime.Now;
                ticketVM.Ticket.TicketStatus = 1;
                _unitOfWork.Save();

                //Email send
                EmailTemplate emailTemplate = _db.EmailTemplates.Where(e => e.Id == Convert.ToInt32(EnEmailTemplate.TicketGenerate)).FirstOrDefault();
                var appuser = _db.Tickets.FirstOrDefault(u => u.Email == ticketVM.Ticket.Email);
                //replace data
                //emailTemplate.Content = emailTemplate.Content.Replace("###review###", ticket.review);
                _emailSender.SendEmailAsync(ticketVM.Ticket.Email, emailTemplate.Subject, emailTemplate.Content);

                return RedirectToAction(nameof(Index));

            }
            else
            {
                ticketVM.ProductList = _unitOfWork.Product.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Title,
                    Value = i.Id.ToString()
                });
                if (ticketVM.Ticket.Id != 0)
                {
                    ticketVM.Ticket = _unitOfWork.Ticket.Get(ticketVM.Ticket.Id);
                }
            }
            return View(ticketVM);
        }
    }
}