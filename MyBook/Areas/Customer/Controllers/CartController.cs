using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MyBook.Models.ViewModels;
using MyBook.DataAccess.Repository.IRepository;
using System.Security.Claims;
using MyBook.Utility;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using MyBook.Models;
using Stripe;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace MyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IEmailSender _emailSender;

        private TwilioSettings _twilioOptions { get; set; }
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitofWork unitofWork, IEmailSender emailSender, 
                                UserManager<IdentityUser> userManager, IOptions<TwilioSettings> twilioOptions)
        {
            _unitofWork = unitofWork;
            _emailSender = emailSender;
            _userManager = userManager;
            _twilioOptions = twilioOptions.Value;
        }
        
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);    

            ShoppingCartVM = new ShoppingCartVM();
            {
                ShoppingCartVM.OrderHeader = new Models.OrderHeader();
                ShoppingCartVM.ListCart = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");
            };

            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value, includeProperties: "Company");

            foreach(var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);

                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                list.Product.Description = SD.ConvertToRawHtml(list.Product.Description);
                if(list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
                }
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitofWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email is empty.");
            }
    
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code},
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");


            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return RedirectToAction("Index");
        }


        public IActionResult Plus(int cartId)
        {
            var cart = _unitofWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId, includeProperties: "Product");

            cart.Count++;
            cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                                                    cart.Product.Price100);
            _unitofWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitofWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId, includeProperties: "Product");

            if(cart.Count == 1)
            {
                var cnt = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                _unitofWork.ShoppingCart.Remove(cart);
                _unitofWork.Save();
                HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);
            }
            else
            {
                cart.Count--;
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50,
                                                        cart.Product.Price100);
                _unitofWork.Save();
            }


            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitofWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId, includeProperties: "Product");

            var cnt = _unitofWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            _unitofWork.ShoppingCart.Remove(cart);
            _unitofWork.Save();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, cnt - 1);

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCart = _unitofWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value, 
                                                           includeProperties: "Product")
            };
            
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofWork.ApplicationUser.GetFirstOrDefault(
                                                                            c => c.Id == claim.Value,
                                                                            includeProperties: "Company");

            foreach(var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50,
                                                         list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
            }

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofWork.ApplicationUser.GetFirstOrDefault(c => c.Id == claim.Value, includeProperties: "Company");

            ShoppingCartVM.ListCart = _unitofWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.Orderstatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

            _unitofWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitofWork.Save();
             

            foreach(var item in ShoppingCartVM.ListCart)
            {
                item.Price = SD.GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                OrderDetails orderDetails = new OrderDetails();
                {
                    orderDetails.ProductId = item.ProductId;
                    orderDetails.OrderId = ShoppingCartVM.OrderHeader.Id;
                    orderDetails.Price = item.Price;
                    orderDetails.Count = item.Count;
                };
                ShoppingCartVM.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
                _unitofWork.OrderDetails.Add(orderDetails);
            }
            _unitofWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitofWork.Save();
            HttpContext.Session.SetInt32(SD.ssShoppingCart, 0);

            if(stripeToken == null)
            {
                // order will be created for delayed payment for authorized company
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.Orderstatus = SD.StatusApproved;
            }
            else
            {
                //process the payment
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID : " + ShoppingCartVM.OrderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if(charge.Id == null)
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                   ShoppingCartVM.OrderHeader.TransactionId = charge.Id;
                }

                if(charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.Orderstatus = SD.StatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
                }
            }

            _unitofWork.Save();


            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }


        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitofWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            TwilioClient.Init(_twilioOptions.AccountSid, _twilioOptions.AuthToken);
            try
            {
                var message = MessageResource.Create(
                    body: "Order placed on MyBook. Your Order ID : " + id,
                    from: new Twilio.Types.PhoneNumber(_twilioOptions.PhoneNumber),
                    to: new Twilio.Types.PhoneNumber(orderHeader.PhoneNumber)
                    );
            }
            catch(Exception ex)
            {
            }

            return View(id);
        }

    }

}
