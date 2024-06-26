﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartsList = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == userId, IncludeProperties: "Product"),
                OrderHeader = new ()
            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach (var cart in ShoppingCartVM.ShoppingCartsList)
            {
                cart.Product.ProductImages = productImages.Where( u => u.Id == cart.Product.Id ).ToList();
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartsList = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == userId, IncludeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartsList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartsList = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == userId, IncludeProperties: "Product");
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

			foreach (var cart in ShoppingCartVM.ShoppingCartsList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

            if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // Is a regular user 
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                // Company user 
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartsList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    ProductId = cart.ProductId,
                    Count = cart.Count,
                    Price = cart.Price
				};
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                // Is a regular user and we need to capture payment
                // stripe logic
                string domain = Request.Scheme + "://" + Request.Host.Value +"/";
				var options = new Stripe.Checkout.SessionCreateOptions
				{
					SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?orderId={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "Customer/Cart/Index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

                foreach (var item in ShoppingCartVM.ShoppingCartsList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), //$20.50 => 2050
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

                var service = new Stripe.Checkout.SessionService();
				Stripe.Checkout.Session session =  service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
			}

			return RedirectToAction(nameof(OrderConfirmation), new { orderId = ShoppingCartVM.OrderHeader.Id} );
		}

        public IActionResult OrderConfirmation (int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == orderId);
            if (orderHeader != null)
            {
                if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
                {
                    // Customer order

                    var service = new SessionService();
                    Session session = service.Get(orderHeader.SessionId);
                    if(session.PaymentStatus.ToLower() == "paid") // order paid successfuly
                    {
                        _unitOfWork.OrderHeader.UpdateStripePaymentID(orderId, session.Id, session.PaymentIntentId);
                        _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.StatusApproved, SD.PaymentStatusApproved);
                        _unitOfWork.Save();
                    }
                    HttpContext.Session.Clear();
                }

                List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                        .GetAll(s => s.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
                _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                _unitOfWork.Save();
            }
            return View(orderId);
        }

		public IActionResult Plus (int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId);
            cart.Count += 1;
            _unitOfWork.ShoppingCart.Update(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId, tracked:true);
            if(cart.Count <= 1) 
            {
                // Add Cart items count to session
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).Count());
                _unitOfWork.ShoppingCart.Remove(cart);

            }
            else
            {
                cart.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cart);
            }
            
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(c => c.Id == cartId, tracked:true);
            // Add Cart items count to session
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).Count());
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if(shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
