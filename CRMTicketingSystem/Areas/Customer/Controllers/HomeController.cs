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
using Microsoft.Extensions.Caching.Memory;

namespace CRMTicketingSystem.Areas.Customer.Controllers
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitofwork;
        [Obsolete]
        private readonly IHostingEnvironment _hostingEnvironment;
        private IMemoryCache _cache;

        [Obsolete]
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitofwork,
            IMemoryCache memoryCache, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _unitofwork = unitofwork;
            _cache = memoryCache;
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
            //var product = new Product();
            //ViewBag.Discount = (product.Price100 * 100) / product.ListPrice; 
            return View(productList);
        }

        [HttpGet]
        public FileResult OpenPDF(int id)
        {
            var productFromDb = _unitofwork.Product.
                        GetFirstOrDefault(u => u.Id == id);
            //"wwwroot/PdfViewer/Samplefile.PDF";
            string PDFpath = @"wwwroot/" + productFromDb.PreviewUrl;
            byte[] abc = System.IO.File.ReadAllBytes(PDFpath);
            System.IO.File.WriteAllBytes(PDFpath, abc);
            MemoryStream ms = new MemoryStream(abc);
            return new FileStreamResult(ms, "application/pdf");
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
                var product = _unitofwork.Product.GetFirstOrDefault(i => i.Id == CartObject.ProductId);
                product.RemainingQuantity = product.RemainingQuantity - CartObject.Count;
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

        public IActionResult Minus(int productId)
        {
            var product = _unitofwork.Product.GetFirstOrDefault(c => c.Id == productId);
            if(product.Quantity ==0 && product.RemainingQuantity ==0)
            {
                product.Quantity = 0;
                product.RemainingQuantity = 0;
            }
            else 
            {
                product.Quantity -= 1;
                product.RemainingQuantity -= 1;
            }
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Plus(int productId)
        {
            var product = _unitofwork.Product.GetFirstOrDefault(c => c.Id == productId);
            product.Quantity += 1;
            product.RemainingQuantity += 1;
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
