using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using CRMTicketingSystem.Models.ViewModels;
using CRMTicketingSystem.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRMTicketingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller

    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfwork, IWebHostEnvironment hostEnvironment)
        {
            _unitofwork = unitOfwork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product =new Product(),
                CategoryList = _unitofwork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text=i.Name,
                    Value=i.Id.ToString()
                }),
                CoverTypeList = _unitofwork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            if (id == null)
            {
                // this is for create
                return View(productVM);
            }
            //this is for edit
            productVM.Product = _unitofwork.Product.Get(id.GetValueOrDefault());
            if (productVM.Product == null)
            {
                return NotFound();
            }
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if(files.Count >0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\Products");
                    var extension = Path.GetExtension(files[0].FileName);

                    if(productVM.Product.ImageUrl !=null)
                    {
                        //this is edit and need for remove older images
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }
                    productVM.Product.ImageUrl = @"\images\Products\" + fileName + extension;
                }
                else
                {
                    //update when not change image
                    if(productVM.Product.Id !=0)
                    {
                        Product objFromDb = _unitofwork.Product.Get(productVM.Product.Id);
                        productVM.Product.ImageUrl = objFromDb.ImageUrl;
                    }
                }

                //For book preview file uploade
                string webRootPath1 = _hostEnvironment.WebRootPath;
                var files1 = HttpContext.Request.Form.Files;
                if (files1.Count > 0)
                {
                    string fileName1 = Guid.NewGuid().ToString();
                    var uploads1 = Path.Combine(webRootPath1, @"PdfViewer\");
                    var extension1 = Path.GetExtension(files1[1].FileName);

                    if (productVM.Product.PreviewUrl != null)
                    {
                        //this is edit and need for remove older images
                        var previewPath = Path.Combine(webRootPath1, productVM.Product.PreviewUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(previewPath))
                        {
                            System.IO.File.Delete(previewPath);
                        }
                    }
                    using (var fileStreams1 = new FileStream(Path.Combine(uploads1, fileName1 + extension1), FileMode.Create))
                    {
                        files[1].CopyTo(fileStreams1);
                    }
                    productVM.Product.PreviewUrl = @"\PdfViewer\" + fileName1 + extension1;
                }
                else
                {
                    //update when not change image
                    if (productVM.Product.Id != 0)
                    {
                        Product objFromDb = _unitofwork.Product.Get(productVM.Product.Id);
                        productVM.Product.PreviewUrl = objFromDb.PreviewUrl;
                    }
                }
                if (productVM.Product.Id == 0)
                {
                    productVM.Product.RemainingQuantity = productVM.Product.Quantity;
                    _unitofwork.Product.Add(productVM.Product);
                }
                else
                {
                    
                    _unitofwork.Product.Update(productVM.Product);
                }
                _unitofwork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                productVM.CategoryList = _unitofwork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                productVM.CoverTypeList = _unitofwork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                if (productVM.Product.Id != 0)
                {
                    productVM.Product = _unitofwork.Product.Get(productVM.Product.Id);
                }
            }
            return View(productVM);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitofwork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = allObj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var Dbobj = _unitofwork.Product.Get(id);
            if (Dbobj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            string webRootPath = _hostEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, Dbobj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _unitofwork.Product.Remove(Dbobj);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}