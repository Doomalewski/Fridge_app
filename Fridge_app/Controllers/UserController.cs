using Fridge_app.Data;
using Fridge_app.Models.ViewModels;
using Fridge_app.Models;
using Fridge_app.Exceptions;
using Fridge_app.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;

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
    public async Task<IActionResult> Index()
    {
      try
        {
    var users = await _context.Users
  .Include(u => u.Fridge)
   .Include(u => u.CookingTools)
     .OrderByDescending(u => u.Id)
   .ToListAsync();

      return View(users);
      }
        catch (Exception ex)
        {
          TempData["Error"] = $"Błąd wczytywania użytkowników: {ex.Message}";
          return RedirectToAction("Index", "Home");
        }
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

    [HttpGet]
    public async Task<IActionResult> MyMeals()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var meals = await _context.Meals
                .Where(m => m.UserId == userId)
                .Include(m => m.Recipe)
                    .ThenInclude(r => r.Ingridients)
                        .ThenInclude(i => i.Product)
                .Include(m => m.Recipe)
                    .ThenInclude(r => r.Steps)
                .Include(m => m.Recipe)
                    .ThenInclude(r => r.CookingTools)
                .Include(m => m.Tags)
                .OrderByDescending(m => m.Recipe.CreatedAt)
                .ToListAsync();

            return View(meals);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd wczytywania przepisów: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> MealDetails(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var meal = await _context.Meals
                .Include(m => m.Recipe)
                    .ThenInclude(r => r.Ingridients)
                        .ThenInclude(i => i.Product)
                .Include(m => m.Recipe)
                    .ThenInclude(r => r.Steps)
                .Include(m => m.Recipe)
                    .ThenInclude(r => r.CookingTools)
                .Include(m => m.Tags)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (meal == null)
            {
                TempData["Error"] = "Przepis nie został znaleziony";
                return RedirectToAction("MyMeals");
            }

            return View(meal);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd wczytywania przepisu: {ex.Message}";
            return RedirectToAction("MyMeals");
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyKitchen()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .Include(u => u.CookingTools)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var allTools = await _context.CookingTools.ToListAsync();

            var model = new MyKitchenViewModel
            {
                UserTools = user?.CookingTools?.ToList() ?? new List<CookingTool>(),
                AvailableTools = allTools
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd wczytywania kuchni: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddTool(int toolId)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .Include(u => u.CookingTools)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var tool = await _context.CookingTools.FindAsync(toolId);

            if (user == null || tool == null)
            {
                TempData["Error"] = "Nie znaleziono użytkownika lub narzędzia";
                return RedirectToAction("MyKitchen");
            }

            if (!user.CookingTools.Any(t => t.Id == toolId))
            {
                user.CookingTools.Add(tool);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Dodano narzędzie: {tool.Name}";
            }
            else
            {
                TempData["Info"] = "To narzędzie jest już w Twojej kuchni";
            }

            return RedirectToAction("MyKitchen");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd dodawania narzędzia: {ex.Message}";
            return RedirectToAction("MyKitchen");
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveTool(int toolId)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .Include(u => u.CookingTools)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                TempData["Error"] = "Nie znaleziono użytkownika";
                return RedirectToAction("MyKitchen");
            }

            var toolToRemove = user.CookingTools.FirstOrDefault(t => t.Id == toolId);
            if (toolToRemove != null)
            {
                user.CookingTools.Remove(toolToRemove);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Usunięto narzędzie: {toolToRemove.Name}";
            }

            return RedirectToAction("MyKitchen");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd usuwania narzędzia: {ex.Message}";
            return RedirectToAction("MyKitchen");
        }
    }

    [HttpPost]
    public async Task<IActionResult> GenerateMeal()
    {
        try
        {
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
     var user = await _userService.GetUserByIdAsync(userId);

            // Sprawdzenie czy użytkownik istnieje
            if (user == null)
            {
    TempData["Error"] = "Użytkownik nie został znaleziony";
return RedirectToAction("Index", "Home");
        }

         // Sprawdzenie czy lodówka jest pusta
            if (user?.Fridge == null || !user.Fridge.Any())
  {
       TempData["Error"] = "Brak produktów w lodówce do generowania przepisów. Dodaj produkty, aby wygenerować przepis.";
            return RedirectToAction("Index", "Home");
  }

   // Próba wygenerowania przepisu
 var generatedMeal = await _geminiService.GenerateMealAsync(user.Fridge, user.CookingTools);
            return View("MealPreview", generatedMeal);
 }
        catch (InsufficientProductsException ex)
   {
            TempData["Error"] = $"Nie udało się wygenerować przepisu: {ex.Message}";
       return RedirectToAction("Index", "Home");
      }
        catch (ApplicationException ex)
      {
     TempData["Error"] = $"Błąd aplikacji: {ex.Message}";
       return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
     TempData["Error"] = $"Nieoczekiwany błąd: {ex.Message}";
   return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveMeal([Bind("Description,Calories,Category,Tags,Recipe,SelectedProducts")] MealCreateViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Nieprawidłowe dane przepisu.";
                return RedirectToAction("Dashboard");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userService.GetUserByIdAsync(userId);

            // Tworzymy nowy posiłek
            var meal = new Meal
            {
                Description = model.Description,
                Category = model.Category,
                Tags = model.Tags,
                UserId = userId
            };

            // Tworzymy nowy przepis
            var recipe = new Recipe
            {
                Name = model.Recipe?.Name ?? "Bez nazwy",
                Difficulty = model.Recipe?.Difficulty ?? "Nieokreślony",
                CreatedAt = model.Recipe?.CreatedAt ?? DateTime.Now
            };

            // ✅ SKŁADNIKI (Ingridients)
            if (model.Recipe?.Ingridients != null && model.Recipe.Ingridients.Any())
            {
                foreach (var ing in model.Recipe.Ingridients)
                {
                    var product = await _context.Products.FindAsync(ing.ProductId);
                    if (product != null)
                    {
                        recipe.Ingridients.Add(new ProductWithAmount
                        {
                            ProductId = product.Id,
                            Amount = ing.Amount,
                            Product = product
                        });
                    }
                }
            }
            else if (model.SelectedProducts != null && model.SelectedProducts.Any())
            {
                // Alternatywnie — jeśli dane pochodzą z SelectedProducts
                foreach (var selectedProduct in model.SelectedProducts)
                {
                    var product = await _context.Products.FindAsync(selectedProduct.ProductId);
                    if (product != null)
                    {
                        recipe.Ingridients.Add(new ProductWithAmount
                        {
                            ProductId = product.Id,
                            Amount = selectedProduct.Amount,
                            Product = product
                        });
                    }
                }
            }

            // ✅ KROKI (Steps)
            if (model.Recipe?.Steps != null && model.Recipe.Steps.Any())
            {
                foreach (var step in model.Recipe.Steps)
                {
                    recipe.Steps.Add(new RecipeStep
                    {
                        StepNumber = step.StepNumber,
                        Instruction = step.Instruction,
                        StepTime = step.StepTime
                    });
                }
            }

            // ✅ NARZĘDZIA KUCHENNE (CookingTools)
            if (model.Recipe?.CookingTools != null && model.Recipe.CookingTools.Any())
            {
                foreach (var tool in model.Recipe.CookingTools)
                {
                    if (string.IsNullOrWhiteSpace(tool.Name))
                        continue;

                    var dbTool = await _context.CookingTools
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == tool.Name.ToLower());

                    if (dbTool == null)
                    {
                        dbTool = new CookingTool { Name = tool.Name };
                        _context.CookingTools.Add(dbTool);
                        await _context.SaveChangesAsync();
                    }

                    recipe.CookingTools.Add(dbTool);
                }
            }

            // ✅ Zapisujemy cały posiłek z przepisem i składnikami
            await _mealService.CreateMealAsync(meal, recipe, recipe.Ingridients);

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