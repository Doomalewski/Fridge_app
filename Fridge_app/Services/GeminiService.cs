using GenerativeAI.Types;
using GenerativeAI;
using Fridge_app.Models;
using Fridge_app.Models.ViewModels;
using Fridge_app.Exceptions;
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
      return response?.Text?.Trim() ?? "Nie uda³o siê wygenerowaæ odpowiedzi";
            }
        catch (Exception ex)
        {
   return $"B³¹d podczas generowania odpowiedzi: {ex.Message}";
    }
        }

  public async Task<MealCreateViewModel> GenerateMealAsync(IEnumerable<StoredProduct> storedProducts, List<CookingTool> cookingTools)
        {
            var productsList = storedProducts?.ToList() ?? new List<StoredProduct>();

  // Sprawdzenie czy mamy jakieœ produkty
    if (!productsList.Any())
      {
    throw new InsufficientProductsException("Brak produktów w lodówce. Dodaj produkty, aby wygenerowaæ przepis.");
            }

   // Obliczenie ca³kowitej iloœci produktów
   var totalProductAmount = productsList.Sum(p => p.Quantity);
if (totalProductAmount <= 0)
  {
             throw new InsufficientProductsException("Iloœæ produktów w lodówce jest za ma³a. Dodaj wiêcej produktów, aby wygenerowaæ przepis.");
       }

 var productsString = string.Join(", ", productsList.Select(sp => sp.ToString()));
       var cookingToolsString = string.Join(", ", cookingTools.Select(ct => ct.ToString()));

  var prompt = "Na podstawie poni¿szych danych wygeneruj propozycjê posi³ku w formacie czystego JSON zgodnego ze schematem." + Environment.NewLine +
       Environment.NewLine +
       "Sk³adniki (produkty z lodówki):" + Environment.NewLine +
      productsString + Environment.NewLine +
      Environment.NewLine +
    "Dostêpne narzêdzia kuchenne:" + Environment.NewLine +
  cookingToolsString + Environment.NewLine +
                "---" + Environment.NewLine +
         "Wygeneruj obiekt JSON zgodny ze schematem:" + Environment.NewLine +
            "{\"Description\": \"string\",\"Calories\": number,\"Category\": \"Breakfast | Lunch | Dinner | Snack | Other\",\"Recipe\": {\"Name\": \"string\",\"Difficulty\": \"Easy | Medium | Hard\",\"CreatedAt\": \"yyyy-MM-ddTHH:mm:ss\",\"Ingridients\": [{\"ProductId\": number, \"Amount\": number}],\"Steps\": [{\"StepNumber\": number, \"Instruction\": \"string\", \"StepTime\": number}],\"CookingTools\": [{\"Id\": number, \"Name\": \"string\"}]},\"SelectedProducts\": [{\"ProductId\": number, \"Amount\": number}]}" +
          Environment.NewLine + "---" + Environment.NewLine +
     "Zasady:" + Environment.NewLine +
     "- OdpowiedŸ MUSI byæ czystym JSON-em bez komentarzy, opisu, ani dodatkowego tekstu." + Environment.NewLine +
     "- Wszystkie wartoœci tekstowe w podwójnych cudzys³owach." + Environment.NewLine +
        "- Difficulty: tylko Easy, Medium, Hard." + Environment.NewLine +
    "- Category: tylko Breakfast, Lunch, Dinner, Snack, Other." + Environment.NewLine +
          "- Amount podawaj w gramach lub mililitrach (bez jednostek, tylko liczba)." + Environment.NewLine +
         "- U¿ywaj sk³adników tylko z listy produktów." + Environment.NewLine +
                "- Narzêdzia ogranicz do tych dostêpnych w sekcji Dostêpne narzêdzia kuchenne." + Environment.NewLine +
        "- Czas (StepTime) w minutach." + Environment.NewLine +
     "- CreatedAt ustaw na bie¿¹c¹ datê.";

    try
   {
   var response = await _model.GenerateContentAsync(prompt);
     var jsonResponse = CleanJsonResponse(response?.Text);

      if (string.IsNullOrWhiteSpace(jsonResponse))
                {
         throw new InsufficientProductsException("AI nie potrafi³ wygenerowaæ przepisu z dostêpnych produktów. Spróbuj dodaæ wiêcej ró¿nych produktów do lodówki.");
    }

       var mealViewModel = JsonConvert.DeserializeObject<MealCreateViewModel>(jsonResponse);
       if (mealViewModel == null)
    {
   throw new InsufficientProductsException("Nie uda³o siê przetworzyæ odpowiedzi AI. Spróbuj ponownie.");
       }

      // Sprawdzenie czy przepis ma sk³adniki
 if (mealViewModel.Recipe?.Ingridients == null || !mealViewModel.Recipe.Ingridients.Any())
          {
         throw new InsufficientProductsException("AI wygenerowa³ przepis bez sk³adników. Dodaj wiêcej produktów do lodówki i spróbuj ponownie.");
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
     throw new InsufficientProductsException("B³¹d przetwarzania odpowiedzi AI. OdpowiedŸ nie by³a poprawnym JSON-em. Spróbuj ponownie.", ex);
   }
      catch (Exception ex)
    {
 throw new ApplicationException("B³¹d przetwarzania odpowiedzi AI", ex);
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
