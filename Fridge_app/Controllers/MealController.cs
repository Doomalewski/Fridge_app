using Fridge_app.Extensions;
using Fridge_app.Models;
using Fridge_app.Models.ViewModels;
using Fridge_app.Repositories;
using Fridge_app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Fridge_app.Controllers
{

    public class MealController : Controller
    {
        private readonly IMealService _mealService;
        private readonly IRepository<Product> _productRepository;

        public MealController(
            IMealService mealService,
            IRepository<Product> productRepository)
        {
            _mealService = mealService;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var meals = await _mealService.GetAllMealsAsync();
            return View(meals);
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();

                var viewModel = new MealCreateViewModel
                {
                    AvailableProducts = products?.ToList() ?? new List<Product>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(MealCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var validCategories = new[] { "Śniadanie", "Obiad", "Kolacja", "Przekąska", "Deser" };

                var recipe = new Recipe
                {
                    TimePrep = model.Recipe.TimePrep,
                    Difficulty = model.Recipe.Difficulty
                };

                var products = model.SelectedProducts.Select(p => new ProductWithAmount
                {
                    ProductId = p.ProductId,
                    Amount = p.Amount
                }).ToList();

                await _mealService.CreateMealAsync(
                    new Meal
                    {
                        Description = model.Description,
                        Calories = model.Calories,
                        Category = model.Category,
                        Tags = model.Tags
                    },
                    recipe,
                    products
                );

                return RedirectToAction(nameof(Index));
            }

            model.AvailableProducts = (await _productRepository.GetAllAsync()).ToList();
            return View(model);
        }

        // Pozostałe akcje: Edit, Delete, Details
    }
}
