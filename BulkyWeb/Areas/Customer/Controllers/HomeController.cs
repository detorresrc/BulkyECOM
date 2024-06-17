using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Bulky.Utility;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private IProductRepository Product => _unitOfWork.Product;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = Product.GetAll(includeProperties: "Category,ProductImages");

            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            Product product = Product.Get(p => p.Id == productId, includeProperties: "Category,ProductImages");
            ShoppingCart shoppingCart = new ShoppingCart
            {
                Product = product,
                Count = 1,
                ProductId = product.Id
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            _unitOfWork.ShoppingCart.AddItem(shoppingCart);

            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart
                    .GetAll(sc => sc.ApplicationUserId == shoppingCart.ApplicationUserId)
                        .Count());

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
