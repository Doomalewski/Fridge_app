using Fridge_app.Models.ViewModels;
using Fridge_app.Models;
using Fridge_app.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fridge_app.Controllers
{
    public class FridgeController : Controller
    {
        private readonly ProductService _productService;
        private readonly StoredProductService _storedProductService;

        public FridgeController(
            ProductService productService,
            StoredProductService storedProductService)
        {
            _productService = productService;
            _storedProductService = storedProductService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "User");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var fridgeItems = await _storedProductService.GetUserFridgeAsync(userId);

            var userModel = new User
            {
                Fridge = fridgeItems.ToList()
            };

            return View(userModel);
        }



        [HttpGet]
        public async Task<IActionResult> AddProduct(ProductSearchViewModel searchModel)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "User");

            searchModel.Results = (await _productService.SearchProductsAsync(
                searchModel.SearchTerm,
                searchModel.Category)).ToList();

            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(AddProductViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "User");

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    await _storedProductService.AddProductToFridgeAsync(userId, model);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Wystąpił błąd podczas zapisywania produktu");
                }
            }

            // Jeśli wystąpiły błędy, wróć do widoku z formularzem
            return RedirectToAction("AddProduct");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(EditProductViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Nieprawidłowa ilość produktu";
                    return RedirectToAction("Index");
                }

                await _storedProductService.UpdateProductAsync(model);
                TempData["Success"] = "Produkt został zaktualizowany";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _storedProductService.DeleteProductAsync(id);
                TempData["Success"] = "Produkt został usunięty";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}