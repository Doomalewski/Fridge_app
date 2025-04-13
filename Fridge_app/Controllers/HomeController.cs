using Fridge_app.Models;
using Fridge_app.Models.ViewModels;
using Fridge_app.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace Fridge_app.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserService _userService;
        private readonly ProductService _productService;
        private readonly StoredProductService _storedProductService;
        public HomeController(ILogger<HomeController> logger, UserService userService, ProductService productService, StoredProductService storedProductService)
        {
            _logger = logger;
            _userService = userService;
            _productService = productService;
            _storedProductService = storedProductService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _userService.GetUserByIdAsync(userId);
                return View(user);
            }
            return View();
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
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string searchTerm, ProductCategory? category)
        {
            var products = await _productService.SearchProductsAsync(searchTerm, category);
            return PartialView("_ProductSearchResults", products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] AddProductViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized(new { error = "U¿ytkownik niezalogowany" });

            if (!ModelState.IsValid)
                return BadRequest(new { error = "Nieprawid³owe dane formularza" });

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _storedProductService.AddProductToFridgeAsync(userId, model);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B³¹d dodawania produktu");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}
