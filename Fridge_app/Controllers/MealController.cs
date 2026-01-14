using Fridge_app.Extensions;
using Fridge_app.Models;
using Fridge_app.Models.ViewModels;
using Fridge_app.Repositories;
using Fridge_app.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Fridge_app.Controllers
{

    public class MealController : Controller
    {
        private readonly IMealService _mealService;
        private readonly IRepository<Product> _productRepository;
        private static readonly CompareInfo CategoryComparer = CultureInfo.GetCultureInfo("pl-PL").CompareInfo;
        private const CompareOptions CategoryCompareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;
        private const string WeeklyPlanSessionKey = "WeeklyMealPlan";
        private static readonly JsonSerializerOptions PlanSerializerOptions = new() { PropertyNameCaseInsensitive = true };

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
            ModelState.Remove("Recipe.MakingSteps");
            if (ModelState.IsValid)
            {
                var validCategories = new[] { "Śniadanie", "Obiad", "Kolacja", "Przekąska", "Deser" };

                var recipe = new Recipe
                {
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

        [HttpGet]
        public async Task<IActionResult> WeeklyPlan()
        {
            var availableMeals = (await _mealService.GetAllMealsAsync()).ToList();
            var model = LoadPlanFromSession();

            if (model == null)
            {
                model = await BuildWeeklyPlanModelAsync(availableMeals);
            }
            else
            {
                EnsureDays(model);
                model.AutoSelected = false;
                model.AvailableMeals = availableMeals;
            }

            SavePlanToSession(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WeeklyPlan(WeeklyMealPlanViewModel model)
        {
            model ??= new WeeklyMealPlanViewModel();
            EnsureDays(model);
            model.AvailableMeals = (await _mealService.GetAllMealsAsync()).ToList();
            model.AutoSelected = false;

            SavePlanToSession(model);
            TempData["Success"] = "Plan tygodniowy zapisany w wersji roboczej. Możesz go wykorzystać w kuchni lub listach zakupów.";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveWeeklyPlan([FromBody] WeeklyMealPlanViewModel model)
        {
            model ??= new WeeklyMealPlanViewModel();
            EnsureDays(model);
            model.AutoSelected = false;
            SavePlanToSession(model);
            return Ok(new { success = true });
        }

        private async Task<WeeklyMealPlanViewModel> BuildWeeklyPlanModelAsync(List<Meal>? meals = null)
        {
            var availableMeals = meals ?? (await _mealService.GetAllMealsAsync()).ToList();
            var startDate = DateTime.Today;

            var days = Enumerable.Range(0, 7)
                .Select(i => new WeeklyMealDayViewModel
                {
                    Date = startDate.AddDays(i)
                })
                .ToList();

            var model = new WeeklyMealPlanViewModel
            {
                WeekStart = startDate,
                Days = days,
                AvailableMeals = availableMeals,
                AutoSelected = availableMeals.Any()
            };

            AutoAssignMeals(model.Days, availableMeals);
            return model;
        }

        private void EnsureDays(WeeklyMealPlanViewModel model)
        {
            var weekStart = model.WeekStart == default ? DateTime.Today : model.WeekStart.Date;
            model.WeekStart = weekStart;

            var existing = model.Days ?? new List<WeeklyMealDayViewModel>();
            var updated = new List<WeeklyMealDayViewModel>();

            for (var i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i).Date;
                var dayPlan = existing.FirstOrDefault(d => d.Date.Date == date) ?? new WeeklyMealDayViewModel { Date = date };
                dayPlan.Date = date;
                updated.Add(dayPlan);
            }

            model.Days = updated;
        }

        private static void AutoAssignMeals(List<WeeklyMealDayViewModel> days, List<Meal> meals)
        {
            if (days == null || meals == null || days.Count == 0 || meals.Count == 0)
            {
                return;
            }

            var breakfastMeals = MealsByCategory(meals, "Śniadanie", "Sniadanie", "Breakfast");
            var lunchMeals = MealsByCategory(meals, "Obiad", "Lunch");
            var dinnerMeals = MealsByCategory(meals, "Kolacja", "Dinner", "Supper");
            var snackMeals = MealsByCategory(meals, "Przekąska", "Przekaska", "Snack");

            for (var i = 0; i < days.Count; i++)
            {
                var day = days[i];
                day.BreakfastMealId ??= PickRandomMealId(breakfastMeals);
                day.LunchMealId ??= PickRandomMealId(lunchMeals);
                day.DinnerMealId ??= PickRandomMealId(dinnerMeals);
                day.SnackMealId ??= PickRandomMealId(snackMeals);
            }
        }

        private static List<Meal> MealsByCategory(IEnumerable<Meal> meals, params string[] categories)
        {
            if (meals == null)
            {
                return new List<Meal>();
            }

            var normalizedCategories = categories?
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToArray() ?? Array.Empty<string>();

            if (normalizedCategories.Length == 0)
            {
                return new List<Meal>();
            }

            return meals
                .Where(m => m != null && normalizedCategories.Any(c => CategoryComparer.Compare(m.Category?.Trim() ?? string.Empty, c, CategoryCompareOptions) == 0))
                .ToList();
        }

        private static int? PickRandomMealId(IReadOnlyList<Meal> meals)
        {
            if (meals == null || meals.Count == 0)
            {
                return null;
            }

            var randomIndex = Random.Shared.Next(meals.Count);
            return meals[randomIndex].Id;
        }

        private WeeklyMealPlanViewModel? LoadPlanFromSession()
        {
            var serialized = HttpContext.Session.GetString(WeeklyPlanSessionKey);
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<WeeklyMealPlanViewModel>(serialized, PlanSerializerOptions);
            }
            catch
            {
                return null;
            }
        }

        private void SavePlanToSession(WeeklyMealPlanViewModel model)
        {
            var snapshot = new WeeklyMealPlanViewModel
            {
                WeekStart = model.WeekStart,
                Days = model.Days ?? new List<WeeklyMealDayViewModel>(),
                AutoSelected = model.AutoSelected
            };

            var serialized = JsonSerializer.Serialize(snapshot);
            HttpContext.Session.SetString(WeeklyPlanSessionKey, serialized);
        }
    }
}
