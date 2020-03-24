using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Data;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using CRMTicketingSystem.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMTicketingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class TicketController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;

        public TicketController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Review(int? id)
        {
            Ticket ticket = new Ticket();
            //this is for edit
            ticket = _unitOfWork.Ticket.Get(id.GetValueOrDefault());
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Review(Ticket ticket)
        {
            _unitOfWork.Ticket.Update(ticket);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Resolve(int id)
        {
            var objFromDb = _db.Tickets.FirstOrDefault(s => s.Id == id);
            if (objFromDb != null)
            {
                objFromDb.TicketStatus = 9;
                _unitOfWork.Save();
                return Json(new { success = true, message = "Resolve Successful." });
            }
            return RedirectToAction(nameof(Index));
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Ticket.GetAll(includeProperties: "Product");
            return Json(new { data = allObj });
        }

        #endregion
    }
}