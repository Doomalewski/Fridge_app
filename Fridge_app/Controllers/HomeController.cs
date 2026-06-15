using Fridge_app.Data;
using Fridge_app.Models;
using Fridge_app.Models.ViewModels;
using Fridge_app.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;

namespace Fridge_app.Controllers
{
    public class HomeController : AppControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserService _userService;
        private readonly ProductService _productService;
        private readonly StoredProductService _storedProductService;
        private readonly GeminiService _geminiService;
        private readonly FridgeDbContext _context;
        private readonly IMealService _mealService;


        [ActivatorUtilitiesConstructor]
        public HomeController(
            ILogger<HomeController> logger,
            UserService userService,
            ProductService productService,
            StoredProductService storedProductService,
            GeminiService geminiService,
            FridgeDbContext context,
            IMealService mealService)
        {
            _logger = logger;
            _userService = userService;
            _productService = productService;
            _storedProductService = storedProductService;
            _geminiService = geminiService;
            _context = context;
            _mealService = mealService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                {
                    _logger.LogWarning("Nie uda?o si? odczyta? Id u?ytkownika z Claims");
                    return RedirectToAction("Logout", "User");
                }

                var user = await _userService.GetUserByIdAsync(userId);
                var meals = (await _mealService.GetAllMealsAsync()).ToList();

                var storedPlan = LoadPlanFromSession();
                var today = DateTime.Today;
                var todayPlan = storedPlan?.Days?.FirstOrDefault(d => d.Date.Date == today) ?? BuildTodayPlan(meals);

                var vm = new HomeDashboardViewModel
                {
                    CurrentUser = user,
                    AvailableMeals = meals,
                    TodayPlan = todayPlan
                };

                return View("HomeAuthenticated", vm);
            }

            return View("HomeGuest");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ShoppingLists()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "User");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MyShoppingLists()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "User");
            }

            var userId = GetCurrentUserId();
            var lists = await _context.ShoppingLists
                .Where(sl => sl.UserId == userId)
                .Include(sl => sl.Items)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();

            var estimationTasks = lists
                .Select(async list => (list.Id, await _geminiService.EstimateShoppingListCostAsync(list)))
                .ToList();
            var estimations = await Task.WhenAll(estimationTasks);
            var estimationLookup = estimations.ToDictionary(x => x.Id, x => x.Item2);

            var summaries = lists.Select(list =>
            {
                var (min, max) = estimationLookup.TryGetValue(list.Id, out var estimate)
                    ? estimate
                    : (0m, 0m);

                return new ShoppingListSummaryViewModel
                {
                    Id = list.Id,
                    Name = list.Name,
                    CreatedAt = list.CreatedAt,
                    Cuisine = list.Cuisine,
                    DesiredDish = list.DesiredDish,
                    Budget = list.Budget,
                    ItemsCount = list.Items?.Count ?? 0,
                    EstimatedMin = min,
                    EstimatedMax = max,
                    IsCompleted = list.IsCompleted,
                    CompletedAt = list.CompletedAt
                };
            }).ToList();

            return View(summaries);
        }

        [HttpGet]
        public IActionResult CreateShoppingList()
        {
            var model = new ShoppingListViewModel
            {
                AvailableCuisines = GetCuisineOptions()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShoppingList(ShoppingListViewModel model, string submitAction = "generate")
        {
            model.AvailableCuisines = GetCuisineOptions();

            if (!ModelState.IsValid && submitAction == "generate")
            {
                return View(model);
            }

            if (submitAction == "save")
            {
                if (model.Items == null || !model.Items.Any())
                {
                    ModelState.AddModelError(string.Empty, "Wygeneruj lub dodaj pozycje listy przed zapisem.");
                    return View(model);
                }

                try
                {
                    var userId = GetCurrentUserId();
                    var list = new ShoppingList
                    {
                        Name = model.Name,
                        Budget = model.Budget,
                        Cuisine = model.Cuisine,
                        DesiredDish = model.DesiredDish,
                        AdditionalNotes = model.AdditionalNotes,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        Items = model.Items
                            .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                            .Select(i => new ShoppingListItem
                            {
                                Name = i.Name,
                                Quantity = i.Quantity,
                                Category = i.Category
                            }).ToList()
                    };

                    _context.ShoppingLists.Add(list);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Lista zosta�a zapisana.";
                    return RedirectToAction(nameof(MyShoppingLists));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Nie uda�o si� zapisa� listy: {ex.Message}");
                    return View(model);
                }
            }

            try
            {
                model.Items = await _geminiService.GenerateShoppingListAsync(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Nie uda�o si� wygenerowa� listy: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ShoppingListDetails(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "User");
            }

            var userId = GetCurrentUserId();
            var list = await _context.ShoppingLists
                .Include(sl => sl.Items)
                .FirstOrDefaultAsync(sl => sl.Id == id && sl.UserId == userId);

            if (list == null)
            {
                TempData["Error"] = "Lista nie zosta�a znaleziona.";
                return RedirectToAction(nameof(MyShoppingLists));
            }

            var (min, max) = await _geminiService.EstimateShoppingListCostAsync(list);

            var vm = new ShoppingListDetailsViewModel(list, min, max);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItemToFridge(int itemId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "User");
            }

            var userId = GetCurrentUserId();
            var item = await _context.ShoppingListItems
                .Include(i => i.ShoppingList)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.ShoppingList!.UserId == userId);

            if (item == null)
            {
                TempData["Error"] = "Nie znaleziono pozycji listy.";
                return RedirectToAction(nameof(MyShoppingLists));
            }

            if (item.IsAddedToFridge)
            {
                TempData["Success"] = "Pozycja zosta�a ju� dodana do lod�wki.";
                return RedirectToAction(nameof(ShoppingListDetails), new { id = item.ShoppingListId });
            }

            await AddItemToFridgeInternal(userId, item);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Dodano {item.Name} do lod�wki.";
            return RedirectToAction(nameof(ShoppingListDetails), new { id = item.ShoppingListId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteShoppingList(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "User");
            }

            var userId = GetCurrentUserId();
            var list = await _context.ShoppingLists
                .Include(sl => sl.Items)
                .FirstOrDefaultAsync(sl => sl.Id == id && sl.UserId == userId);

            if (list == null)
            {
                TempData["Error"] = "Lista nie zosta�a znaleziona.";
                return RedirectToAction(nameof(MyShoppingLists));
            }

            if (list.IsCompleted)
            {
                TempData["Success"] = "Ta lista zosta�a ju� oznaczona jako zrealizowana.";
                return RedirectToAction(nameof(ShoppingListDetails), new { id });
            }

            foreach (var item in list.Items.Where(i => !i.IsAddedToFridge))
            {
                await AddItemToFridgeInternal(userId, item);
            }

            list.IsCompleted = true;
            list.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lista zosta�a oznaczona jako zrealizowana, a produkty dodano do lod�wki.";
            return RedirectToAction(nameof(ShoppingListDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteShoppingList(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "User");
            }

            var userId = GetCurrentUserId();
            var list = await _context.ShoppingLists
                .Include(sl => sl.Items)
                .FirstOrDefaultAsync(sl => sl.Id == id && sl.UserId == userId);

            if (list == null)
            {
                TempData["Error"] = "Lista nie zosta�a znaleziona.";
                return RedirectToAction(nameof(MyShoppingLists));
            }

            _context.ShoppingLists.Remove(list);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lista zosta�a usuni�ta.";
            return RedirectToAction(nameof(MyShoppingLists));
        }

        private static IEnumerable<string> GetCuisineOptions() => new[]
        {
            "Polska",
            "W�oska",
            "�r�dziemnomorska",
            "Azjatycka",
            "Meksyka�ska",
            "Ameryka�ska",
            "Indyjska",
            "Wega�ska",
            "Wegetaria�ska"
        };

        private static double ParseQuantity(string? quantity)
        {
            if (string.IsNullOrWhiteSpace(quantity))
            {
                return 1;
            }

            var numericPart = new string(quantity
                .TakeWhile(c => char.IsDigit(c) || c == ',' || c == '.')
                .ToArray());

            if (double.TryParse(numericPart.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return value > 0 ? value : 1;
            }

            return 1;
        }

        private static ProductCategory MapCategory(string? category)
        {
            if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ProductCategory>(category, true, out var parsed))
            {
                return parsed;
            }

            return ProductCategory.Other;
        }

        private async Task<Product> GetOrCreateProductAsync(string name, string? category)
        {
            var normalizedName = name.Trim();
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name.ToLower() == normalizedName.ToLower());

            if (product != null)
            {
                return product;
            }

            product = new Product
            {
                Name = normalizedName,
                ProductCategory = MapCategory(category),
                Unit = "szt",
                Kcal = 0,
                Fat = 0,
                UnsaturatedFat = 0,
                Carbohydrates = 0,
                Sugar = 0,
                Protein = 0,
                Fiber = 0,
                Salt = 0,
                PriceMin = 0,
                PriceMax = 0
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        private async Task AddItemToFridgeInternal(int userId, ShoppingListItem item)
        {
            var product = await GetOrCreateProductAsync(item.Name, item.Category);
            var quantity = ParseQuantity(item.Quantity);
            var storedProduct = new StoredProduct
            {
                ProductId = product.Id,
                UserId = userId,
                Quantity = quantity,
                ExpirationDate = DateTime.UtcNow.AddDays(7)
            };

            _context.StoredProducts.Add(storedProduct);
            item.IsAddedToFridge = true;
        }

        private WeeklyMealDayViewModel BuildTodayPlan(List<Meal> meals)
        {
            var today = DateTime.Today;
            var day = new WeeklyMealDayViewModel { Date = today };

            var breakfastMeals = MealsByCategory(meals, "?niadanie", "Sniadanie", "Breakfast");
            var lunchMeals = MealsByCategory(meals, "Obiad", "Lunch");
            var dinnerMeals = MealsByCategory(meals, "Kolacja", "Dinner", "Supper");
            var snackMeals = MealsByCategory(meals, "Przek?ska", "Przekaska", "Snack");

            day.BreakfastMealId = PickRandomMealId(breakfastMeals);
            day.LunchMealId = PickRandomMealId(lunchMeals);
            day.DinnerMealId = PickRandomMealId(dinnerMeals);
            day.SnackMealId = PickRandomMealId(snackMeals);

            return day;
        }

        private WeeklyMealPlanViewModel? LoadPlanFromSession()
            => WeeklyPlanHelpers.LoadFromSession(HttpContext.Session);

        private static List<Meal> MealsByCategory(IEnumerable<Meal> meals, params string[] categories)
            => WeeklyPlanHelpers.MealsByCategory(meals, categories);

        private static int? PickRandomMealId(IReadOnlyList<Meal> meals)
            => WeeklyPlanHelpers.PickRandomMealId(meals);

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
                return Unauthorized(new { error = "U�ytkownik niezalogowany" });

            if (!ModelState.IsValid)
                return BadRequest(new { error = "Nieprawid�owe dane formularza" });

            try
            {
                var userId = GetCurrentUserId();
                await _storedProductService.AddProductToFridgeAsync(userId, model);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "B��d dodawania produktu");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}
