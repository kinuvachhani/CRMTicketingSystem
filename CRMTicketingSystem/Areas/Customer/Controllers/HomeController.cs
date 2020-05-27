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
using Microsoft.AspNetCore.Mvc.Rendering;
using CRMTicketingSystem.DataAccess.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using CRMTicketingSystem.Enum;

namespace CRMTicketingSystem.Areas.Customer.Controllers
{
    [IgnoreAntiforgeryToken(Order = 1001)]
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitofwork;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        [Obsolete]
        private readonly IHostingEnvironment _hostingEnvironment;
        private IMemoryCache _cache;

        [Obsolete]
        public HomeController(ILogger<HomeController> logger, 
            IUnitOfWork unitofwork,
            IMemoryCache memoryCache, 
            IHostingEnvironment hostingEnvironment,
            ApplicationDbContext db,
            IEmailSender emailSender)
        {
            _logger = logger;
            _unitofwork = unitofwork;
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
            _db = db;
            _emailSender = emailSender;
        }

        public IActionResult Help()
        {
            return View();
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

        //Customer Help
        public IActionResult Contact(int? id)
        {
            Help help = new Help();
            if (id == null)
            {
                //this is for create
                return View(help);
            }
            if (help == null)
            {
                return NotFound();
            }
            return View(help);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(Help help)
        {
            if (ModelState.IsValid)
            {
                _unitofwork.Help.Add(help);
                help.CreatedDate = DateTime.Now;
                help.TicketStatus = "1";
                _db.SaveChanges();

                EmailTemplate emailTemplate = _db.EmailTemplates.Where(e => e.Id == Convert.ToInt32(EnEmailTemplate.TicketGenerate)).FirstOrDefault();
                var appuser = _db.Helps.FirstOrDefault(u => u.Email == help.Email);
                _emailSender.SendEmailAsync(help.Email, emailTemplate.Subject, emailTemplate.Content);

                return RedirectToAction("Index", "Home");

            }
            else
            {
                if (help.Id != 0)
                {
                    help = _unitofwork.Help.Get(help.Id);
                }
            }
            return View(help);
        }
    }
}
