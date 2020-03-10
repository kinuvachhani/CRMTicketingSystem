using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace CRMTicketingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitofwork;

        public CoverTypeController(IUnitOfWork unitOfwork)
        {
            _unitofwork = unitOfwork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null)
            {
                // this is for create
                return View(coverType);
            }
            //this is for edit
            coverType = _unitofwork.CoverType.Get(id.GetValueOrDefault());
            if (coverType == null)
            {
                return NotFound();
            }
            return View(coverType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                if (coverType.Id == 0)
                {
                    _unitofwork.CoverType.Add(coverType);
                }
                else
                {
                    _unitofwork.CoverType.Update(coverType);
                }
                _unitofwork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(coverType);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitofwork.CoverType.GetAll();
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var Dbobj = _unitofwork.CoverType.Get(id);
            if (Dbobj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitofwork.CoverType.Remove(Dbobj);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}