using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyBook.DataAccess.Repository.IRepository;
using MyBook.Models;
using MyBook.Models.ViewModels;
using MyBook.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        public HomeController(ILogger<HomeController> logger, 
                              IUnitofWork unitofWork,
                              IEmailSender emailSender)
        {
            _logger = logger;
            _unitOfWork = unitofWork;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value).ToList().Count();

                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

            }

            return View(productList);

        }

        public IActionResult Details(int id)
        {
            var productFromDb = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id, includeProperties:"Category,CoverType");
            ShoppingCart cartObj = new ShoppingCart();
            {

                cartObj.ProductId = productFromDb.Id;
                cartObj.Product = productFromDb;
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
                // then we will add to cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claims.Value;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId
                    , includeProperties: "Product"
                    );

                if(cartFromDb == null)
                {
                    // no record exists in database for that product for that user
                    _unitOfWork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                }
                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();

                HttpContext.Session.SetObject(SD.ssShoppingCart, CartObject);
                //HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == CartObject.Id, includeProperties: "Category,CoverType");
                ShoppingCart cartObj = new ShoppingCart();
                {

                    cartObj.ProductId = productFromDb.Id;
                    cartObj.Product = productFromDb;
                };
                return View(cartObj);

            }

                        
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Contact Contact)
        {
            if (ModelState.IsValid)
            {
                var user = new Contact
                {
                    Name = Contact.Name,
                    Email = Contact.Email,
                    Subject = Contact.Subject,
                    Message = Contact.Message,
                    PhoneNumber = Contact.PhoneNumber
                };


                 
                await _emailSender.SendEmailAsync(user.Email, user.Subject + " - " + user.Name, user.Message);

                return RedirectToAction("EmailConfirmation", "Home");
            }


            return View();
        }

        public IActionResult EmailConfirmation()
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
