using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBook.DataAccess.Repository.IRepository;
using MyBook.Models;
using MyBook.Models.ViewModels;
using MyBook.Utility;
using Stripe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitofWork _unitOfWork;

        [BindProperty]
        public OrderDetailsVM OrderVM { get; set; }

        public OrderController(IUnitofWork unitofWork)
        {
            _unitOfWork = unitofWork;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderDetailsVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser"),

                OrderDetails = _unitOfWork.OrderDetails.GetAll(o => o.OrderId == id, includeProperties: "Product")
            };

            return View(OrderVM);
        }


        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            orderHeader.Orderstatus = SD.StatusInProcess;
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.Orderstatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Details")]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id,
                                        includeProperties: "ApplicationUser");
            if (stripeToken != null)
            {
                //process the payment
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID : " + orderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.Id == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }

                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderHeader.PaymentDate = DateTime.Now;
                }

                _unitOfWork.Save();
            }

            return RedirectToAction("Details", "Order", new { id = orderHeader.Id });
        }



            [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            if(orderHeader.PaymentStatus == SD.StatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                orderHeader.Orderstatus = SD.StatusRefunded;
                orderHeader.PaymentStatus = SD.StatusRefunded;
            }
            else
            {
                orderHeader.Orderstatus = SD.StatusCancelled;
                orderHeader.PaymentStatus= SD.StatusCancelled;
            }


            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            IEnumerable<OrderHeader> orderHeaderList;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "rejected":
                    orderHeaderList = orderHeaderList.Where(o => o.Orderstatus == SD.StatusCancelled ||
                                                                 o.Orderstatus == SD.StatusRefunded ||
                                                                 o.Orderstatus == SD.PaymentStatusRejected);
                    break;
                case "completed":
                    orderHeaderList = orderHeaderList.Where(o => o.Orderstatus == SD.StatusShipped);
                    break;
                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(o => o.Orderstatus == SD.StatusApproved ||
                                                                 o.Orderstatus == SD.StatusInProcess ||
                                                                 o.Orderstatus == SD.StatusPending);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaderList });
        }
        #endregion
    }
}
