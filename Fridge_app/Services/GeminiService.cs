using GenerativeAI.Types;
using GenerativeAI;
using Fridge_app.Models;
using Fridge_app.Models.ViewModels;
using Newtonsoft.Json;
using System.Text.RegularExpressions;


namespace Fridge_app.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly GenerativeModel _model;

        public GeminiService()
        {
            _apiKey = "AIzaSyCbs99F9XIq7XgvtHegspxU8k6Ri7k_5CM" ?? throw new ArgumentNullException(nameof(_apiKey));
            var googleAI = new GoogleAi(_apiKey);
            _model = googleAI.CreateGenerativeModel("gemini-2.0-flash");
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            try
            {
                var response = await _model.GenerateContentAsync(prompt);
                return response?.Text?.Trim() ?? "Nie udało się wygenerować odpowiedzi";
            }
            catch (Exception ex)
            {
                return $"Błąd podczas generowania odpowiedzi: {ex.Message}";
            }
        }
        public async Task<MealCreateViewModel> GenerateMealAsync(IEnumerable<StoredProduct> storedProducts, List<CookingTool> cookingTools)
        {
            var productsString = FormatProductsForPrompt(storedProducts);
            var cookingToolsString = string.Join(", ", cookingTools.Select(ct=>ct.ToString()));

        var prompt = $$"""
        Na podstawie tych składników:
        {{productsString}}
        I tych narzędzi:
        {{cookingTools}}

        Wygeneruj posiłek w formacie JSON według schematu:
        {
            "Description": "string",
            "Calories": number,
            "Category": "string",
            "Recipe": {
                "Name": "string",
                "Difficulty": "string",
                "CreatedAt": "yyyy-MM-ddTHH:mm:ss",
                "Ingridients": [ { "ProductId": number, "Amount": number } ],
                "Steps": [ { "StepNumber": number, "Instruction": "string", "StepTime": number } ],
                "CookingTools": [ { "Id": number, "Name": "string" } ]
            },
            "SelectedProducts": [
                { "ProductId": number, "Amount": number }
            ]
        }


        Zasady:
        - Odpowiedź musi być CZYSTYM JSON bez dodatkowego tekstu
        - Używaj podwójnych cudzysłowów dla wszystkich wartości tekstowych
        - Difficulty może przyjmować tylko wartości: Easy, Medium, Hard
        - Category może przyjmować tylko wartości: Breakfast, Lunch, Dinner, Snack, Other
        - Amount podawaj w gramach/mililitrach
        - Kalorie dla CAŁEGO posiłku

        Przykład poprawnej odpowiedzi:
        {
            "Description": "Sałatka z kurczakiem",
            "Calories": 650,
            "Category": "Lunch",
            "Recipe": {
                "TimePrep": 25,
                "MakingSteps": "1. Pokrój kurczaka...",
                "Difficulty": "Medium"
            },
            "SelectedProducts": [
                {"ProductId": 123, "Amount": 200},
                {"ProductId": 456, "Amount": 150}
            ]
        }
        """;

            try
            {
                var response = await _model.GenerateContentAsync(prompt);
                var jsonResponse = CleanJsonResponse(response?.Text);
                return JsonConvert.DeserializeObject<MealCreateViewModel>(jsonResponse);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Błąd przetwarzania odpowiedzi AI", ex);
            }
        }
        private string FormatProductsForPrompt(IEnumerable<StoredProduct> storedProducts)
        {
            return string.Join("\n", storedProducts.Select((p, index) =>
                $"{index + 1}. {FormatProduct(p)}"));
        }
        private string FormatProduct(StoredProduct storedProduct)
        {
            var product = storedProduct.Product;
            return $"{storedProduct.Quantity} {product.Unit} {product.Name} " +
                   $"(ważne do: {storedProduct.ExpirationDate:yyyy-MM-dd})";
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