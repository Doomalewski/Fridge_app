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
        private readonly ProductService _productService;
        public GeminiService(ProductService productService)
        {
            _productService = productService;
            _apiKey = "AIzaSyBTINRGGSfQ7kkVKm_ERG53hZUgD_cedaM" ?? throw new ArgumentNullException(nameof(_apiKey));
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
            var productsString = string.Join(", ", storedProducts.Select(sp => sp.ToString()));
            var cookingToolsString = string.Join(", ", cookingTools.Select(ct => ct.ToString()));

            var prompt = $$"""
            Na podstawie poniższych danych wygeneruj propozycję posiłku w formacie **czystego JSON** zgodnego ze schematem.

            **Składniki (produkty z lodówki):**
            {{productsString}}

            **Dostępne narzędzia kuchenne:**
            {{cookingToolsString}}

            ---

            Wygeneruj obiekt JSON zgodny ze schematem:
            {
                "Description": "string",
                "Calories": number,
                "Category": "Breakfast" | "Lunch" | "Dinner" | "Snack" | "Other",
                "Recipe": {
                    "Name": "string",
                    "Difficulty": "Easy" | "Medium" | "Hard",
                    "CreatedAt": "yyyy-MM-ddTHH:mm:ss",
                    "Ingridients": [
                        { "ProductId": number, "Amount": number }
                    ],
                    "Steps": [
                        { "StepNumber": number, "Instruction": "string", "StepTime": number }
                    ],
                    "CookingTools": [
                        { "Id": number, "Name": "string" }
                    ]
                },
                "SelectedProducts": [
                    { "ProductId": number, "Amount": number }
                ]
            }

            ---

            ### Zasady:
            - **Odpowiedź MUSI być czystym JSON-em** – bez komentarzy, opisu, ani dodatkowego tekstu.
            - **Wszystkie wartości tekstowe w podwójnych cudzysłowach.**
            - **Difficulty**: tylko `"Easy"`, `"Medium"`, `"Hard"`.
            - **Category**: tylko `"Breakfast"`, `"Lunch"`, `"Dinner"`, `"Snack"`, `"Other"`.
            - **Amount** podawaj w gramach lub mililitrach (bez jednostek, tylko liczba).
            - Używaj składników tylko z listy produktów.
            - Narzędzia ogranicz do tych dostępnych w sekcji „Dostępne narzędzia kuchenne”.
            - Czas (`StepTime`) w minutach.
            - `CreatedAt` ustaw na bieżącą datę.

            ---

            ### Przykład poprawnego formatu:
            {
                "Description": "Sałatka z kurczakiem i warzywami",
                "Category": "Lunch",
                "Recipe": {
                    "Name": "Sałatka z kurczakiem",
                    "Difficulty": "Medium",
                    "CreatedAt": "2025-10-22T15:00:00",
                    "Ingridients": [
                        { "ProductId": 1, "Amount": 200 },
                        { "ProductId": 2, "Amount": 100 }
                    ],
                    "Steps": [
                        { "StepNumber": 1, "Instruction": "Pokrój kurczaka w kostkę i podsmaż.", "StepTime": 10 },
                        { "StepNumber": 2, "Instruction": "Dodaj warzywa i wymieszaj z sosem.", "StepTime": 5 }
                    ],
                    "CookingTools": [
                        { "Id": 1, "Name": "Patelnia" },
                        { "Id": 2, "Name": "Miska" }
                    ]
                },
                "SelectedProducts": [
                    { "ProductId": 1, "Amount": 200 },
                    { "ProductId": 2, "Amount": 100 }
                ]
            }
            """;


            try
            {
                var response = await _model.GenerateContentAsync(prompt);
                var jsonResponse = CleanJsonResponse(response?.Text);
                var mealViewModel = JsonConvert.DeserializeObject<MealCreateViewModel>(jsonResponse);
                if (mealViewModel != null)
                {
                    mealViewModel.AvailableProducts = _productService.GetAllProductsAsync().Result.ToList();
                    mealViewModel.Calories = await _productService.getCaloriesFromProductsWithAmounts(mealViewModel.SelectedProducts);
                }
                else
                    throw new ApplicationException("Nie udało się zdeserializować odpowiedzi AI do modelu MealCreateViewModel.");
                return mealViewModel;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Błąd przetwarzania odpowiedzi AI", ex);
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