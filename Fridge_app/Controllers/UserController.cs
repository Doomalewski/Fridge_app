using Fridge_app.Data;
using Fridge_app.Models.ViewModels;
using Fridge_app.Models;
using Fridge_app.Models.Enums;
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
    private readonly NutritionCalculatorService _nutritionCalculator;
    
    public UserController(
        FridgeDbContext context, 
        UserService userService, 
        GeminiService geminiService,
        IMealService mealService,
        NutritionCalculatorService nutritionCalculator)
    {
        _context = context;
        _userService = userService;
        _geminiService = geminiService;
        _mealService = mealService;
        _nutritionCalculator = nutritionCalculator;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
      try
        {
    var users = await _context.Users
  .Include(u => u.Fridge)
   .Include(u => u.CookingTools)
   .Include(u => u.HumanStats)
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
    public async Task<IActionResult> ViewMealDetails(int id)
    {
      try
    {
  var meal = await _context.Meals
 .Include(m => m.Recipe)
     .ThenInclude(r => r.Ingridients)
 .ThenInclude(i => i.Product)
      .Include(m => m.Recipe)
  .ThenInclude(r => r.Steps)
   .Include(m => m.Recipe)
   .ThenInclude(r => r.CookingTools)
    .Include(m => m.Tags)
  .FirstOrDefaultAsync(m => m.Id == id);

    if (meal == null)
          {
     TempData["Error"] = "Przepis nie został znaleziony";
  return RedirectToAction("Index");
     }

  return View("MealDetails", meal);
        }
      catch (Exception ex)
 {
    TempData["Error"] = $"Błąd wczytywania przepisu: {ex.Message}";
     return RedirectToAction("Index");
      }
    }

    [HttpGet]
    public async Task<IActionResult> ViewUserMeals(int userId)
    {
      try
        {
// Pobierz dane użytkownika, którego przepisy chcemy zobaczyć
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
  {
            TempData["Error"] = "Użytkownik nie został znaleziony";
         return RedirectToAction("Index");
            }

    // Pobierz wszystkie przepisy tego użytkownika
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

ViewData["UserEmail"] = user.Email;
     return View("ViewUserMeals", meals);
        }
        catch (Exception ex)
   {
            TempData["Error"] = $"Błąd wczytywania przepisów: {ex.Message}";
            return RedirectToAction("Index");
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

    [HttpGet]
    public IActionResult ShowGenerateMealPreferences()
     {
     var preferences = new MealGenerationPreferencesViewModel();
        return PartialView("_GenerateMealPreferencesModal", preferences);
        }

   [HttpPost]
 public async Task<IActionResult> GenerateMeal(MealGenerationPreferencesViewModel? preferences)
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

      // Próba wygenerowania przepisu z preferencjami
     var generatedMeal = await _geminiService.GenerateMealAsync(user.Fridge, user.CookingTools, preferences);
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

    [HttpPost]
    public async Task<IActionResult> DeleteMeal(int id)
    {
      try
        {
          var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    var meal = await _context.Meals
       .Include(m => m.Recipe)
       .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

   if (meal == null)
          {
          TempData["Error"] = "Przepis nie został znaleziony";
    return RedirectToAction("MyMeals");
      }

            // Usuń przepis powiązany z posiłkiem
   if (meal.RecipeId.HasValue)
        {
         await _mealService.DeleteRecipeAsync(meal.RecipeId.Value);
            }

            // Usuń sam posiłek
            await _mealService.DeleteMealAsync(id);

            TempData["Success"] = "Przepis został pomyślnie usunięty!";
            return RedirectToAction("MyMeals");
        }
        catch (Exception ex)
   {
            TempData["Error"] = $"Błąd usuwania przepisu: {ex.Message}";
  return RedirectToAction("MyMeals");
  }
    }

    [HttpGet]
    public async Task<IActionResult> AccountSettings()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .Include(u => u.HumanStats)
                    .ThenInclude(hs => hs.Weight)
                .Include(u => u.NutritionTargets)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                TempData["Error"] = "Użytkownik nie został znaleziony";
                return RedirectToAction("Index", "Home");
            }

            // Wczytaj typy diet z diets.json
            var dietsJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "diets.json");
            var dietsJson = await System.IO.File.ReadAllTextAsync(dietsJsonPath);
            var dietsData = System.Text.Json.JsonDocument.Parse(dietsJson);
            var dietTypes = dietsData.RootElement
                .GetProperty("diets")
                .EnumerateArray()
                .Select(d => d.GetProperty("name").GetString())
                .ToList();

            // Pobierz aktywny cel żywieniowy
            var activeNutritionTarget = user.NutritionTargets?
                .Where(nt => nt.ValidTo == null || nt.ValidTo > DateTime.UtcNow)
                .OrderByDescending(nt => nt.ValidFrom)
                .FirstOrDefault();

            // Odczytaj ActivityLevel z TempData (po obliczeniu) lub z bazy
            ActivityLevel? activityLevel = null;
            if (TempData["ActivityLevel"] != null && int.TryParse(TempData["ActivityLevel"].ToString(), out var activityLevelInt))
            {
                activityLevel = (ActivityLevel)activityLevelInt;
            }
            else if (activeNutritionTarget != null)
            {
                // Odczytaj z bazy jeśli nie ma w TempData
                activityLevel = activeNutritionTarget.ActivityLevel;
            }

            // Odczytaj obliczone wartości z TempData (z preview) lub z bazy
            int? calculatedCalories = null;
            double? calculatedProtein = null;
            double? calculatedFat = null;
            double? calculatedCarbs = null;
            double? bmr = null;
            double? tdee = null;

            // Z TempData (po obliczeniu preview)
            if (TempData["CalculatedCalories"] != null && int.TryParse(TempData["CalculatedCalories"].ToString(), out var caloriesInt))
                calculatedCalories = caloriesInt;
            else
                calculatedCalories = activeNutritionTarget?.CaloriesPerDay;

            if (TempData["CalculatedProtein"] is string proteinStr && double.TryParse(proteinStr, out var proteinVal))
                calculatedProtein = proteinVal;
            else
                calculatedProtein = activeNutritionTarget?.ProteinGrams;

            if (TempData["CalculatedFat"] is string fatStr && double.TryParse(fatStr, out var fatVal))
                calculatedFat = fatVal;
            else
                calculatedFat = activeNutritionTarget?.FatGrams;

            if (TempData["CalculatedCarbs"] is string carbsStr && double.TryParse(carbsStr, out var carbsVal))
                calculatedCarbs = carbsVal;
            else
                calculatedCarbs = activeNutritionTarget?.CarbsGrams;

            if (TempData["BMR"] is string bmrStr && double.TryParse(bmrStr, out var bmrVal))
                bmr = bmrVal;

            if (TempData["TDEE"] is string tdeeStr && double.TryParse(tdeeStr, out var tdeeVal))
                tdee = tdeeVal;

            // Odczytaj dane formularza z TempData (po preview) lub z bazy
            var age = TempData["Age"] != null ? (int?)int.Parse(TempData["Age"].ToString()) : user.HumanStats?.Age;
            var height = TempData["Height"] != null ? (float?)float.Parse(TempData["Height"].ToString()) : user.HumanStats?.Height;
            var currentWeight = TempData["CurrentWeight"] != null ? (float?)float.Parse(TempData["CurrentWeight"].ToString()) : user.HumanStats?.Weight?.OrderByDescending(w => w.Date).FirstOrDefault()?.Weight;
            var sex = TempData["Sex"]?.ToString() ?? user.HumanStats?.Sex;
            var nutritionGoal = TempData["NutritionGoal"] != null ? (GoalType?)(GoalType)int.Parse(TempData["NutritionGoal"].ToString()) : activeNutritionTarget?.Goal;
            var selectedDietType = TempData["SelectedDietType"]?.ToString() ?? user.HumanStats?.Diet; // ✅ Zmieniono na HumanStats.Diet

            var model = new AccountSettingsViewModel
            {
                Email = user.Email,
                SelectedDietType = selectedDietType,
                AvailableDietTypes = dietTypes,
                Age = age,
                Height = height,
                CurrentWeight = currentWeight,
                Sex = sex,
                Goal = user.HumanStats?.Goal,
                ActivityLevel = activityLevel,
                NutritionGoal = nutritionGoal,
                CalculatedCalories = calculatedCalories,
                CalculatedProtein = calculatedProtein,
                CalculatedFat = calculatedFat,
                CalculatedCarbs = calculatedCarbs,
                BMR = bmr,
                TDEE = tdee
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd wczytywania ustawień: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AccountSettings(AccountSettingsViewModel model)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Users
                .Include(u => u.HumanStats)
                    .ThenInclude(hs => hs.Weight)
                .Include(u => u.NutritionTargets)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                TempData["Error"] = "Użytkownik nie został znaleziony";
                return RedirectToAction("Index", "Home");
            }

            // Waliduj dane wymagane
            if (!model.Age.HasValue || !model.Height.HasValue || !model.CurrentWeight.HasValue || 
                string.IsNullOrEmpty(model.Sex) || !model.ActivityLevel.HasValue || !model.NutritionGoal.HasValue)
            {
                TempData["Error"] = "Proszę wypełnić wszystkie wymagane pola.";
                
                // Wczytaj diety dla formularza
                var dietsJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "diets.json");
                var dietsJson = await System.IO.File.ReadAllTextAsync(dietsJsonPath);
                var dietsData = System.Text.Json.JsonDocument.Parse(dietsJson);
                model.AvailableDietTypes = dietsData.RootElement
                    .GetProperty("diets")
                    .EnumerateArray()
                    .Select(d => d.GetProperty("name").GetString())
                    .ToList();
                
                return View(model);
            }

            // Aktualizuj lub utwórz dietę - teraz w HumanStats
            // Nie trzeba już tworzyć osobnej tabeli Diet
            
            // Aktualizuj lub utwórz statystyki użytkownika
            if (user.HumanStats == null)
            {
                user.HumanStats = new HumanStats
                {
                    Weight = new List<WeightEntry>()
                };
                _context.HumanStats.Add(user.HumanStats);
            }

            user.HumanStats.Age = model.Age.Value;
            user.HumanStats.Height = model.Height.Value;
            user.HumanStats.Sex = model.Sex;
            user.HumanStats.Goal = model.Goal ?? model.NutritionGoal.ToString();
            user.HumanStats.Diet = string.IsNullOrWhiteSpace(model.SelectedDietType) ? "Brak diety" : model.SelectedDietType;

            // Dodaj nowy wpis wagi
            var latestWeight = user.HumanStats.Weight?.OrderByDescending(w => w.Date).FirstOrDefault();
            if (latestWeight == null || Math.Abs(latestWeight.Weight - model.CurrentWeight.Value) > 0.01)
            {
                var newWeightEntry = new WeightEntry
                {
                    Date = DateTime.UtcNow,
                    Weight = model.CurrentWeight.Value
                };
                user.HumanStats.Weight.Add(newWeightEntry);
            }

            // Zapisz zmiany HumanStats do bazy
            await _context.SaveChangesAsync();

            // OBLICZENIE lub UŻYCIE OBLICZONYCH CELÓW ŻYWIENIOWYCH
            NutritionCalculationResult nutritionResult;
            
            // Jeśli mamy już obliczone wartości z preview (hidden inputs)
            if (model.CalculatedCalories.HasValue && model.CalculatedProtein.HasValue && 
                model.CalculatedFat.HasValue && model.CalculatedCarbs.HasValue)
            {
                // Użyj wartości z formularza
                nutritionResult = new NutritionCalculationResult
                {
                    TargetCalories = model.CalculatedCalories.Value,
                    ProteinGrams = model.CalculatedProtein.Value,
                    FatGrams = model.CalculatedFat.Value,
                    CarbsGrams = model.CalculatedCarbs.Value,
                    BMR = model.BMR ?? 0,
                    TDEE = model.TDEE ?? 0,
                    ActivityLevel = model.ActivityLevel.Value,
                    Goal = model.NutritionGoal.Value
                };
            }
            else
            {
                // Oblicz na nowo jeśli nie ma obliczonych wartości
                nutritionResult = _nutritionCalculator.CalculateNutritionTargets(
                    weightKg: (double)model.CurrentWeight.Value,
                    heightCm: (double)model.Height.Value,
                    age: model.Age.Value,
                    sex: model.Sex,
                    activityLevel: model.ActivityLevel.Value,
                    goal: model.NutritionGoal.Value
                );
            }

            // Zakończ poprzedni aktywny cel
            var activeCurrent = user.NutritionTargets?
                .Where(nt => nt.ValidTo == null || nt.ValidTo > DateTime.UtcNow)
                .ToList();
            
            if (activeCurrent != null)
            {
                foreach (var target in activeCurrent)
                {
                    target.ValidTo = DateTime.UtcNow;
                }
            }

            // Utwórz i zapisz nowy cel żywieniowy do bazy
            var newTarget = new NutritionTarget
            {
                UserId = userId,
                Goal = model.NutritionGoal.Value,
                ActivityLevel = model.ActivityLevel.Value,
                CaloriesPerDay = nutritionResult.TargetCalories,
                ProteinGrams = nutritionResult.ProteinGrams,
                FatGrams = nutritionResult.FatGrams,
                CarbsGrams = nutritionResult.CarbsGrams,
                ValidFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            user.NutritionTargets.Add(newTarget);

            // Zapisz nowy NutritionTarget do bazy
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ustawienia zapisane! Kalorie: {nutritionResult.TargetCalories} kcal, Białko: {nutritionResult.ProteinGrams:F0}g, Tłuszcze: {nutritionResult.FatGrams:F0}g, Węglowodany: {nutritionResult.CarbsGrams:F0}g";
            return RedirectToAction("AccountSettings");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd zapisywania ustawień: {ex.Message}";
            
            // Ponownie wczytaj diety z JSON dla formularza
            var dietsJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "diets.json");
            var dietsJson = await System.IO.File.ReadAllTextAsync(dietsJsonPath);
            var dietsData = System.Text.Json.JsonDocument.Parse(dietsJson);
            model.AvailableDietTypes = dietsData.RootElement
                .GetProperty("diets")
                .EnumerateArray()
                .Select(d => d.GetProperty("name").GetString())
                .ToList();
            
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CalculateNutritionPreview(
        int age, 
        float height, 
        float currentWeight, 
        string sex, 
        int activityLevel, 
        int nutritionGoal,
        string selectedDietType = "")
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Oblicz cele żywieniowe
            var result = _nutritionCalculator.CalculateNutritionTargets(
                weightKg: (double)currentWeight,
                heightCm: (double)height,
                age: age,
                sex: sex,
                activityLevel: (ActivityLevel)activityLevel,
                goal: (GoalType)nutritionGoal
            );

            // Zapisz wyniki do TempData
            TempData["CalculatedCalories"] = result.TargetCalories;
            TempData["CalculatedProtein"] = result.ProteinGrams.ToString("F1");
            TempData["CalculatedFat"] = result.FatGrams.ToString("F1");
            TempData["CalculatedCarbs"] = result.CarbsGrams.ToString("F1");
            TempData["BMR"] = result.BMR.ToString("F0");
            TempData["TDEE"] = result.TDEE.ToString("F0");
            TempData["ActivityLevel"] = activityLevel;
            
            // Zapisz parametry formularza
            TempData["Age"] = age;
            TempData["Height"] = height.ToString("F1");
            TempData["CurrentWeight"] = currentWeight.ToString("F1");
            TempData["Sex"] = sex;
            TempData["NutritionGoal"] = nutritionGoal;
            TempData["SelectedDietType"] = selectedDietType;

            TempData["Info"] = "Cele żywieniowe obliczone! Kliknij 'Zapisz wszystkie ustawienia' aby zapisać do bazy.";
            return RedirectToAction("AccountSettings");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd obliczania: {ex.Message}";
            return RedirectToAction("AccountSettings");
        }
    }
}