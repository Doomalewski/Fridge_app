using Fridge_app.Models;
using Fridge_app.Models.ViewModels;
using Fridge_app.Exceptions;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Fridge_app.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly string _modelName;
        private readonly string _endpoint;
        private readonly HttpClient _httpClient;
        private readonly ProductService _productService;

        public GeminiService(ProductService productService, IOptions<GeminiServiceOptions> options, HttpClient httpClient)
        {
            _productService = productService;
            _apiKey = options.Value.ApiKey ?? throw new ArgumentNullException(nameof(options), "API Key is not configured in appsettings.json");
            _modelName = options.Value.ModelName;
            _endpoint = options.Value.Endpoint;
            _httpClient = httpClient;
        }

        private async Task<string> CallOpenRouterAsync(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    model = _modelName,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                request.Content = httpContent;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObj = JObject.Parse(responseBody);

                var text = responseObj["choices"]?[0]?["message"]?["content"]?.ToString();
                return text?.Trim() ?? "Nie udało się wygenerować odpowiedzi";
            }
            catch (Exception ex)
            {
                return $"Błąd podczas generowania odpowiedzi: {ex.Message}";
            }
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            return await CallOpenRouterAsync(prompt);
        }

        public async Task<MealCreateViewModel> GenerateMealAsync(IEnumerable<StoredProduct> storedProducts, List<CookingTool> cookingTools, MealGenerationPreferencesViewModel? preferences = null)
        {
            var productsList = storedProducts?.ToList() ?? new List<StoredProduct>();

            if (!productsList.Any())
            {
                throw new InsufficientProductsException("Brak produkt�w w lod�wce. Dodaj produkty, aby wygenerowa� przepis.");
            }

            var totalProductAmount = productsList.Sum(p => p.Quantity);
            if (totalProductAmount <= 0)
            {
                throw new InsufficientProductsException("Ilo�� produkt�w w lod�wce jest za ma�a. Dodaj wi�cej produkt�w, aby wygenerowa� przepis.");
            }

            var productsString = string.Join(", ", productsList.Select(sp => sp.ToString()));
            var cookingToolsString = string.Join(", ", cookingTools.Select(ct => ct.ToString()));

            var preferencesString = BuildPreferencesString(preferences);

            var languageInstruction = GetLanguageInstruction(preferences?.Language ?? "pl");

            var prompt = languageInstruction + Environment.NewLine + Environment.NewLine +
             "Na podstawie poni�szych danych wygeneruj propozycj� posi�ku w formacie czystego JSON zgodnego ze schematem." + Environment.NewLine +
      Environment.NewLine +
             "Sk�adniki (produkty z lod�wki):" + Environment.NewLine +
         productsString + Environment.NewLine +
             Environment.NewLine +
               "Dost�pne narz�dzia kuchenne:" + Environment.NewLine +
       cookingToolsString + Environment.NewLine +
            (string.IsNullOrWhiteSpace(preferencesString) ? "" : "Preferencje u�ytkownika:" + Environment.NewLine + preferencesString + Environment.NewLine) +
          "---" + Environment.NewLine +
               "Wygeneruj obiekt JSON zgodny ze schematem:" + Environment.NewLine +
             "{\"Description\": \"string\",\"Calories\": number,\"Category\": \"Breakfast | Lunch | Dinner | Snack | Other\",\"Recipe\": {\"Name\": \"string\",\"Difficulty\": \"Easy | Medium | Hard\",\"CreatedAt\": \"yyyy-MM-ddTHH:mm:ss\",\"Ingridients\": [{\"ProductId\": number, \"Amount\": number}],\"Steps\": [{\"StepNumber\": number, \"Instruction\": \"string\", \"StepTime\": number}],\"CookingTools\": [{\"Id\": number, \"Name\": \"string\"}]},\"SelectedProducts\": [{\"ProductId\": number, \"Amount\": number}]}" +
         Environment.NewLine + "---" + Environment.NewLine +
       "Zasady:" + Environment.NewLine +
      "- Odpowied� MUSI by� czystym JSON-em bez komentarzy, opisu, ani dodatkowego tekstu." + Environment.NewLine +
           "- Wszystkie warto�ci tekstowe w podw�jnych cudz�ach." + Environment.NewLine +
              "- Difficulty: tylko Easy, Medium, Hard." + Environment.NewLine +
           "- Category: tylko Breakfast, Lunch, Dinner, Snack, Other." + Environment.NewLine +
           "- Amount podawaj w gramach lub mililitrach (bez jednostek, tylko liczba)." + Environment.NewLine +
            "- U�ywaj sk�adnik�w tylko z listy produkt�w." + Environment.NewLine +
            "- Narz�dzia ogranicz do tych dost�pnych w sekcji Dost�pne narz�dzia kuchenne." + Environment.NewLine +
           "- Czas (StepTime) w minutach." + Environment.NewLine +
        "- CreatedAt ustaw na bie��c� dat�.";

            try
            {
                var responseText = await CallOpenRouterAsync(prompt);
                var jsonResponse = CleanJsonResponse(responseText);

                if (string.IsNullOrWhiteSpace(jsonResponse))
                {
                    throw new InsufficientProductsException("AI nie potrafiło wygenerować przepisu z dostępnych produktów. Spróbuj dodać więcej różnych produktów do lodówki.");
                }

                var mealViewModel = JsonConvert.DeserializeObject<MealCreateViewModel>(jsonResponse);
                if (mealViewModel == null)
                {
                    throw new InsufficientProductsException("Nie udało się przetworzyć odpowiedzi AI. Spróbuj ponownie.");
                }

                if (mealViewModel.Recipe?.Ingridients == null || !mealViewModel.Recipe.Ingridients.Any())
                {
                    throw new InsufficientProductsException("AI wygenerowało przepis bez składników. Dodaj więcej produktów do lodówki i spróbuj ponownie.");
                }

                if (mealViewModel.Recipe?.CookingTools != null && mealViewModel.Recipe.CookingTools.Any())
                {
                    var availableToolNames = new HashSet<string>(cookingTools.Select(ct => ct.Name), StringComparer.OrdinalIgnoreCase);
                    var missingTools = mealViewModel.Recipe.CookingTools
                   .Where(rt => !availableToolNames.Contains(rt.Name))
                  .Select(rt => rt.Name)
                   .Distinct()
                    .ToList();

                    if (missingTools.Any())
                    {
                        var missingToolsString = string.Join(", ", missingTools);
                        throw new InsufficientProductsException($"Brakuje następujących narzędzi kuchennych do przygotowania tego posiłku: {missingToolsString}. Dodaj brakujące narzędzia lub spróbuj ponownie.");
                    }
                }

                mealViewModel.AvailableProducts = _productService.GetAllProductsAsync().Result.ToList();
                mealViewModel.Calories = await _productService.getCaloriesFromProductsWithAmounts(mealViewModel.SelectedProducts);

                return mealViewModel;
            }
            catch (InsufficientProductsException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                throw new InsufficientProductsException("Błąd przetwarzania odpowiedzi AI. Odpowiedź nie była poprawnym JSON-em. Spróbuj ponownie.", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Błąd przetwarzania odpowiedzi AI", ex);
            }
        }

        public async Task<List<ShoppingListItemViewModel>> GenerateShoppingListAsync(ShoppingListViewModel request)
        {
            var budgetInfo = request.Budget.HasValue
                ? $"{request.Budget.Value:0.##} PLN"
                : "brak określonego limitu";

            var cuisineInfo = string.IsNullOrWhiteSpace(request.Cuisine)
                ? "dowolna"
                : request.Cuisine;

            var dishInfo = string.IsNullOrWhiteSpace(request.DesiredDish)
                ? "brak konkretnego dania"
                : request.DesiredDish;

            var notesInfo = string.IsNullOrWhiteSpace(request.AdditionalNotes)
                ? "brak dodatkowych preferencji"
                : request.AdditionalNotes;

            var prompt = "Przygotuj listę zakupów w języku polskim na podstawie poniższych danych wejściowych. " +
                         "Zwróć czysty JSON bez komentarzy ani dodatkowego tekstu." + Environment.NewLine +
                         $"Budżet: {budgetInfo}{Environment.NewLine}" +
                         $"Rodzaj kuchni: {cuisineInfo}{Environment.NewLine}" +
                         $"Planowane danie / cel zakupów: {dishInfo}{Environment.NewLine}" +
                         $"Uwagi: {notesInfo}{Environment.NewLine}" +
                         "JSON schema: {\"items\":[{\"name\":\"string\",\"quantity\":\"string\",\"category\":\"string\"}]}" + Environment.NewLine +
                         "Zasady:" + Environment.NewLine +
                         "- Maksymalnie 15 pozycji." + Environment.NewLine +
                         "- Ilości podawaj w sensownych jednostkach: g/ml dla produktów na wagę, sztuki/opakowania/butelki/puszki/kartony dla reszty (np. butelka oliwy, paczka ryżu, bochenek chleba)." + Environment.NewLine +
                         "- Unikaj gramów tam, gdzie typowo kupuje się produkt w opakowaniu (nie \"100g oliwy\", tylko \"butelka oliwy\")." + Environment.NewLine +
                         "- Dopasuj listę do budżetu (tańsze zamienniki gdy trzeba)." + Environment.NewLine +
                         "- Preferuj składniki łatwo dostępne w Polsce." + Environment.NewLine +
                         "- Zwróć TYLKO JSON dokładnie w podanym schemacie.";

            try
            {
                var responseText = await CallOpenRouterAsync(prompt);
                var jsonResponse = CleanJsonResponse(responseText);

                var aiResult = JsonConvert.DeserializeObject<ShoppingListAiResponse>(jsonResponse);
                if (aiResult?.Items == null || !aiResult.Items.Any())
                {
                    throw new ApplicationException("AI nie zwróciło pozycji listy zakupów.");
                }

                return aiResult.Items;
            }
            catch (JsonException ex)
            {
                throw new ApplicationException("Nie udało się przetworzyć odpowiedzi AI dla listy zakupów.", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Błąd generowania listy zakupów", ex);
            }
        }

        public async Task<(decimal min, decimal max)> EstimateShoppingListCostAsync(ShoppingList list)
        {
            if (list?.Items == null || !list.Items.Any())
                return (0m, 0m);

            try
            {
                var itemsDescription = string.Join(", ", list.Items.Select(i =>
                {
                    var qty = string.IsNullOrWhiteSpace(i.Quantity) ? "1 szt" : i.Quantity;
                    var category = string.IsNullOrWhiteSpace(i.Category) ? string.Empty : $" ({i.Category})";
                    return $"{i.Name} ({qty}{category})";
                }));

                var budgetInfo = list.Budget.HasValue ? $"{list.Budget.Value:0.##} PLN" : "brak budżetu";
                var cuisineInfo = string.IsNullOrWhiteSpace(list.Cuisine) ? "dowolna" : list.Cuisine;
                var dishInfo = string.IsNullOrWhiteSpace(list.DesiredDish) ? "brak" : list.DesiredDish;

                var prompt =
                    "Oszacuj realny minimalny i maksymalny łączny koszt zakupów (PLN) dla poniższej listy w typowych polskich marketach. " +
                    "Uwzględnij orientacyjne ceny popularnych marek i marek własnych, bez luksusowych produktów. " +
                    "Zwróć TYLKO czysty JSON bez komentarzy w formacie {\"min\": number, \"max\": number} z kropką jako separatorem dziesiętnym.\n" +
                    $"Budżet użytkownika: {budgetInfo}\n" +
                    $"Preferowana kuchnia: {cuisineInfo}\n" +
                    $"Planowane danie/cel zakupów: {dishInfo}\n" +
                    $"Pozycje listy: {itemsDescription}\n" +
                    "Zasady: (1) nie dodawaj waluty w wartościach, (2) nie dodawaj komentarzy, (3) min <= max.";

                var responseText = await CallOpenRouterAsync(prompt);
                var jsonResponse = CleanJsonResponse(responseText);

                if (string.IsNullOrWhiteSpace(jsonResponse))
                    return (0m, 0m);

                var estimate = JsonConvert.DeserializeObject<ShoppingListCostEstimate>(jsonResponse);
                if (estimate != null && (estimate.Min > 0 || estimate.Max > 0))
                {
                    return NormalizeEstimate(estimate.Min, estimate.Max);
                }

                var token = JToken.Parse(jsonResponse);
                var minToken = token["min"] ?? token["estimatedMin"] ?? token["low"] ?? token["minPrice"];
                var maxToken = token["max"] ?? token["estimatedMax"] ?? token["high"] ?? token["maxPrice"];

                var min = ParseDecimal(minToken);
                var max = ParseDecimal(maxToken);

                return NormalizeEstimate(min, max);
            }
            catch
            {
                return (0m, 0m);
            }
        }

        private static (decimal min, decimal max) NormalizeEstimate(decimal min, decimal max)
        {
            if (min < 0) min = 0;
            if (max < 0) max = 0;
            if (max == 0 && min > 0) max = min;
            if (max < min) max = min;
            return (min, max);
        }

        private static decimal ParseDecimal(JToken? token)
        {
            if (token == null) return 0m;
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                return token.Value<decimal>();
            }

            var raw = token.ToString();
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.GetCultureInfo("pl-PL"), out value))
            {
                return value;
            }

            return 0m;
        }

        private class ShoppingListAiResponse
        {
            public List<ShoppingListItemViewModel> Items { get; set; } = new();
        }

        private class ShoppingListCostEstimate
        {
            [JsonProperty("min")]
            public decimal Min { get; set; }

            [JsonProperty("max")]
            public decimal Max { get; set; }
        }

        private string BuildPreferencesString(MealGenerationPreferencesViewModel? preferences)
        {
            if (preferences == null)
                return string.Empty;

            var preferencesLines = new List<string>();

            if (!string.IsNullOrWhiteSpace(preferences.MealType))
            {
                preferencesLines.Add($"- Typ posi�ku: {preferences.MealType}");
            }

            if (!string.IsNullOrWhiteSpace(preferences.Difficulty))
            {
                preferencesLines.Add($"- Poziom trudno�ci: {preferences.Difficulty}");
            }

            if (!string.IsNullOrWhiteSpace(preferences.PrepTime))
            {
                preferencesLines.Add($"- Czas przygotowania: {preferences.PrepTime}");
            }

            if (!string.IsNullOrWhiteSpace(preferences.CuisineType))
            {
                preferencesLines.Add($"- Rodzaj kuchni: {preferences.CuisineType}");
            }

            if (preferences.IsVegan)
            {
                preferencesLines.Add("- Posi�ek MUSI by� wega�ski (bez produkt�w zwierz�cych)");
            }

            if (preferences.IsVegetarian && !preferences.IsVegan)
            {
                preferencesLines.Add("- Posi�ek MUSI by� wegetaria�ski (bez mi�sa)");
            }

            if (!string.IsNullOrWhiteSpace(preferences.AdditionalPreferences))
            {
                preferencesLines.Add($"- Dodatkowe preferencje: {preferences.AdditionalPreferences}");
            }

            return string.Join(Environment.NewLine, preferencesLines);
        }

        private string GetLanguageInstruction(string language)
        {
            if (language?.ToLower() == "en")
            {
                return "LANGUAGE: English. Description, Recipe Name, and Steps Instructions MUST be in English. Category MUST remain in English format (Breakfast, Lunch, Dinner, Snack, Other). Respond ONLY in English for all text.";
            }
            else
            {
                return "LANGUAGE: Polish. Description, Recipe Name, and Steps Instructions MUST be in Polish. Category MUST remain in English format (Breakfast, Lunch, Dinner, Snack, Other). Respond in Polish for text, but keep Category values in English.";
            }
        }

        private string CleanJsonResponse(string rawResponse)
        {
            var cleaned = Regex.Replace(rawResponse, @"```json|```", string.Empty);
            return cleaned.Trim()
                        .Replace("\n", "")
                        .Replace("\t", "")
                        .Replace("'", "\"");
        }
    }
}
