using Fridge_app.Data;
using Fridge_app.Models.ViewModels;
using Fridge_app.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Fridge_app.Services;

public class UserController : Controller
{
    private readonly FridgeDbContext _context;
    private readonly UserService _userService;
    private readonly GeminiService _geminiService;
    private readonly IMealService _mealService;
    public UserController(FridgeDbContext context, UserService userService, GeminiService geminiService,IMealService mealService)
    {
        _context = context;
        _userService = userService;
        _geminiService = geminiService;
        _mealService = mealService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email jest już zarejestrowany");
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Diet = null,
                Fridge = null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await Authenticate(user);
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                await Authenticate(user);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Nieprawidłowe dane logowania");
        }
        return View(model);
    }

    private async Task Authenticate(User user)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Email)
    };

        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }
    [HttpPost]
    public async Task<IActionResult> GenerateMeal()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userService.GetUserByIdAsync(userId);

            if (user?.Fridge == null || !user.Fridge.Any())
            {
                TempData["Error"] = "Brak produktów w lodówce do generowania przepisów";
                return RedirectToAction("Dashboard"); // Przekieruj gdzieś odpowiednio
            }

            var generatedMeal = await _geminiService.GenerateMealAsync(user.Fridge, user.CookingTools);
            return View("MealPreview", generatedMeal);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd generowania przepisu: {ex.Message}";
            return RedirectToAction("Dashboard");
        }
    }
    [HttpPost]
    public async Task<IActionResult> SaveMeal([Bind("Description,Calories,Category,Recipe,SelectedProducts")] MealCreateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Nieprawidłowe dane przepisu";
                return RedirectToAction("Dashboard");
            }

            // Mapowanie do modelu domenowego
            var meal = new Meal
            {
                Description = model.Description,
                Category = model.Category,
            };

            var recipe = new Recipe
            {
                Name = model.Recipe.Name,
                Difficulty = model.Recipe.Difficulty,
                CreatedAt = model.Recipe.CreatedAt,
                Ingridients = model.Recipe.Ingridients,
                Steps = model.Recipe.Steps,
                CookingTools = model.Recipe.CookingTools
            };


            var products = new List<ProductWithAmount>();
            foreach (var selectedProduct in model.SelectedProducts)
            {
                // Weryfikacja istnienia produktu
                var productExists = await _context.Products
                    .AnyAsync(p => p.Id == selectedProduct.ProductId);

                if (!productExists)
                {
                    TempData["Error"] = $"Produkt o ID {selectedProduct.ProductId} nie istnieje";
                    return RedirectToAction("Dashboard");
                }

                products.Add(new ProductWithAmount
                {
                    ProductId = selectedProduct.ProductId,
                    Amount = selectedProduct.Amount
                });
            }

            // Wywołanie serwisu
            await _mealService.CreateMealAsync(meal, recipe, products);

            TempData["Success"] = "Przepis został pomyślnie zapisany!";
            return RedirectToAction("MealDetails", new { id = meal.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd zapisywania: {ex.Message}";
            return RedirectToAction("Dashboard");
        }
    }
}