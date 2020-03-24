using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRMTicketingSystem.Models.ViewModels;
using CRMTicketingSystem.DataAccess.Repository.IRepository;
using CRMTicketingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CRMTicketingSystem.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Syncfusion.EJ2.PdfViewer;
using System.IO;
using Newtonsoft.Json;

namespace CRMTicketingSystem.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitofwork;
        [Obsolete]
        private readonly IHostingEnvironment _hostingEnvironment;

        [Obsolete]
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitofwork, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _unitofwork = unitofwork;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitofwork.Product.GetAll(
                includeProperties: "Category,CoverType");
            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var Claim = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(Claim != null)
            {
                var count = _unitofwork.ShoppingCart.GetAll(
                    c => c.ApplicationUserId == Claim.Value).ToList().Count();

                HttpContext.Session.SetInt32(SD.sessionShoppingCart, count);
            }
            return View(productList);
        }

        public IActionResult Details(int id)
        {
            var productFromDb = _unitofwork.Product.
                        GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType");
            ShoppingCart cartObj = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //then we will add to cart
                var ClaimsIdentity = (ClaimsIdentity)User.Identity;
                var Claim = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = Claim.Value;
                ShoppingCart cartFromDb = _unitofwork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId
                    , includeProperties: "Product"
                    );
                if(cartFromDb == null)
                {
                    //no records exist in database for that product for that user
                    _unitofwork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                    //_unitofwork.ShoppingCart.Update(cartFromDb);
                }
                _unitofwork.Save();

                var count = _unitofwork.ShoppingCart.GetAll(
                    c => c.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();

                HttpContext.Session.SetInt32(SD.sessionShoppingCart, count);
                

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = _unitofwork.Product.
                        GetFirstOrDefault(u => u.Id == CartObject.ProductId, 
                        includeProperties: "Category,CoverType");
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };
                return View(cartObj);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Preview()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //pdf viewer methods
        public IActionResult Load([FromBody] Dictionary<string, string> jsonObject,int id)
        {
            var pdfurl = _unitofwork.Product.GetFirstOrDefault(u => u.Id == id);

            PdfRenderer pdfviewer = new PdfRenderer();
            MemoryStream stream = new MemoryStream();
            object jsonResult = new object();
            if (jsonObject != null && jsonObject.ContainsKey("document"))
            {
                if (bool.Parse(jsonObject[pdfurl.PreviewUrl]))
                {
                    string documentPath = GetDocumentPath(jsonObject["document"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        stream = new MemoryStream(bytes);
                    }
                    else
                    {
                        return this.Content(jsonObject["document"] + " is not found");
                    }
                }
                else
                {
                    byte[] bytes = Convert.FromBase64String(jsonObject["document"]);
                    stream = new MemoryStream(bytes);
                }
            }
            jsonResult = pdfviewer.Load(stream, jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/RenderPdfPages")]
        public IActionResult RenderPdfPages([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object jsonResult = pdfviewer.GetPage(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/RenderAnnotationComments")]
        public IActionResult RenderAnnotationComments([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/Unload")]
        public IActionResult Unload([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            pdfviewer.ClearCache(jsonObject);
            return this.Content("Document cache is cleared");
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/RenderThumbnailImages")]
        public IActionResult RenderThumbnailImages([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object result = pdfviewer.GetThumbnailImages(jsonObject);
            return Content(JsonConvert.SerializeObject(result));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/Bookmarks")]
        public IActionResult Bookmarks([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object jsonResult = pdfviewer.GetBookmarks(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/Download")]
        public IActionResult Download([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
            return Content(documentBase);
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/PrintImages")]
        public IActionResult PrintImages([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            object pageImage = pdfviewer.GetPrintImage(jsonObject);
            return Content(JsonConvert.SerializeObject(pageImage));
        }

        private string GetDocumentPath(string document)
        {
            string documentPath = string.Empty;
            if (!System.IO.File.Exists(document))
            {
                string basePath = _hostingEnvironment.WebRootPath;
                string dataPath = string.Empty;
                dataPath = basePath + @"/fileuploads/";
                if (System.IO.File.Exists(dataPath + document))
                    documentPath = dataPath + document;
            }
            else
            {
                documentPath = document;
            }
            return documentPath;
        }

    }
}

