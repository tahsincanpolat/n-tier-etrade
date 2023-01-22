using ETICARET.Business.Abstract;
using ETICARET.Entities;
using ETICARET.WebUI.Identity;
using ETICARET.WebUI.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderItem = ETICARET.Entities.OrderItem;

namespace ETICARET.WebUI.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private ICartService _cartService;
        private IOrderService _orderService;
        private UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService,IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));

            return View(new CartModel()
            {
                CardId = cart.Id,
                CartItems = cart.CartItems.Select(i => new CartItemModel()
                {
                    CartItemId = i.Id,
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    Price = i.Product.Price,
                    ImageUrl = i.Product.Images[0].ImageUrl,
                    Quantity = i.Quantity

                }).ToList()
            });
        }

        [HttpPost]
        public IActionResult AddToCart(int productId,int quantity)
        {
            _cartService.AddToCart(_userManager.GetUserId(User),productId,quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteFromCart(int productId)
        {
            _cartService.DeleteFromCart(_userManager.GetUserId(User),productId);
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCartByUserId(_userManager.GetUserId(User));

            var orderModel = new OrderModel();

            orderModel.CartModel = new CartModel()
            {
                CardId = cart.Id,
                CartItems = cart.CartItems.Select(i => new CartItemModel()
                {
                    CartItemId = i.Id,
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    Price = i.Product.Price,
                    ImageUrl = i.Product.Images[0].ImageUrl,
                    Quantity = i.Quantity

                }).ToList()
            };

            return View(orderModel);
        }

        [HttpPost]
        public IActionResult Checkout(OrderModel model)
        {
            ModelState.Remove("CartModel");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var cart = _cartService.GetCartByUserId(userId);

                model.CartModel = new CartModel()
                {
                    CardId = cart.Id,
                    CartItems = cart.CartItems.Select(i => new CartItemModel()
                    {
                        CartItemId = i.Id,
                        ProductId = i.ProductId,
                        Name = i.Product.Name,
                        Price = i.Product.Price,
                        ImageUrl = i.Product.Images[0].ImageUrl,
                        Quantity = i.Quantity

                    }).ToList()
                };

                // Ödeme

                var payment = PaymentProcess(model);

                if(payment.Status == "success")
                {
                    SaveOrder(model,payment,userId);
                    ClearCart(cart.Id.ToString());
                    return View("Success");
                }

            }

            return View(model);
        }

        private Payment PaymentProcess(OrderModel model)
        {
            Options options = new Options();
            options.ApiKey = "";
            options.SecretKey = "";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            // Devam edilecek.


        }

        public void ClearCart(string cartId)
        {
            _cartService.ClearCart(cartId);
        }

        public void SaveOrder(OrderModel model, Payment payment, string userId)
        {
            var order = new Order()
            {
                OrderNumber = new Random().Next(111111, 999999).ToString(), // New Guid().ToString() ile de yapılabilir
                OrderState = EnumOrderState.completed,
                PaymentTypes = EnumPaymentTypes.CreditCart,
                PaymentId = payment.PaymentId,
                PaymentToken = Guid.NewGuid().ToString(),
                ConversionId = payment.ConversationId,
                OrderDate = new DateTime(),
                OrderNote = "zile basma",
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                UserId = userId

            };

            foreach (var item in model.CartModel.CartItems)
            {
                var orderItem = new OrderItem()
                {
                    Price = item.Price,
                    Quatinty = item.Quantity,
                    ProductId = item.ProductId
                };

                order.OrderItems.Add(orderItem);
            }

            _orderService.Create(order);
        }
    }
}
