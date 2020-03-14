using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using CRMTicketingSystem.Utility;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRMTicketingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
            var parameter = new DynamicParameters();
            parameter.Add("@Id", id);
            coverType= _unitofwork.SP_call.OneRecord<CoverType>(SD.Proc_CoverType_Get, parameter);
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
                var Parameter = new DynamicParameters();
                Parameter.Add("@Name", coverType.Name);
                if (coverType.Id == 0)
                {
                    _unitofwork.SP_call.Execute(SD.Proc_CoverType_Create, Parameter);
                }
                else
                {
                    Parameter.Add("@Id", coverType.Id);
                    _unitofwork.SP_call.Execute(SD.Proc_CoverType_Update, Parameter);
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
            var allObj = _unitofwork.SP_call.List<CoverType>(SD.Proc_CoverType_GetAll,null);
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Id",id);
            var Dbobj = _unitofwork.SP_call.OneRecord<CoverType>(SD.Proc_CoverType_Get,parameter);
            if (Dbobj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitofwork.SP_call.Execute(SD.Proc_CoverType_Delete,parameter);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}