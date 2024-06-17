using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe.Checkout;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        public IOrderHeaderRepository OrderHeader => _unitOfWork.OrderHeader;
        public IOrderDetailRepository OrderDetail => _unitOfWork.OrderDetail;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(
            IUnitOfWork unitOfWork, 
            IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            return View(OrderVM);
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PayNow()
        {
            OrderVM.OrderHeader =
                OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetails = OrderDetail.GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id,
                includeProperties: "Product");
            
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + "Admin/Order/Details?orderId=" + OrderVM.OrderHeader.Id,
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book",
                $"<p>New Order Created - {orderHeader.Id}</p>");

            _unitOfWork.Save();

            return View(orderHeaderId);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = OrderHeader.Get(o => o.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if(!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if(!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            
            TempData["Success"] = "Order details updated successfully";
            
            return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            TempData["Success"] = "Order successfully updated to In Process";
            UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess, "Order successfully updated to In Process");
            
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }
        
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.StatusShipped, "Order successfully updated to Shipped");
            
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.StatusCancelled, "Order successfully updated to Cancelled");
            
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            Expression<Func<OrderHeader, bool>> filter = null;

            switch (status)
            {
                case "pending":
                    filter = u => u.PaymentStatus == SD.PaymentStatusDelayedPayment;
                    break;
                case "inprocess":
                    filter = u => u.OrderStatus == SD.StatusInProcess;
                    break;
                case "completed":
                    filter = u => u.OrderStatus == SD.StatusShipped;
                    break;
                case "approved":
                    filter = u => u.OrderStatus == SD.StatusApproved;
                    break;
            }

            if (filter != null && (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee)))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                Expression<Func<OrderHeader, bool>> filterUser = u => u.ApplicationUserId == userId;
                filter = Expression.Lambda<Func<OrderHeader, bool>>(
                    Expression.AndAlso(
                        filter.Body, 
                        Expression.Invoke(filterUser, filter.Parameters)
                    ),
                    filter.Parameters
                );
            }

            orderHeaders = this.OrderHeader.GetAll(filter: filter, includeProperties: "ApplicationUser");

            return Json(new { data = orderHeaders });
        }
        #endregion

        
        #region Private Methods
        private bool UpdateOrderStatus(int headerId, string newStatus, string message)
        {
            try
            {
                OrderHeader.UpdateStatus(headerId, newStatus);
                _unitOfWork.Save();
            
                TempData["Success"] = message;
                
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion
    }
}