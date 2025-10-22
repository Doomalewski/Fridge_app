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