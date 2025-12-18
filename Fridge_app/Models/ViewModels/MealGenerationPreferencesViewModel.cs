namespace Fridge_app.Models.ViewModels
{
    public class MealGenerationPreferencesViewModel
    {
        // Typ posi³ku
        public string? MealType { get; set; } // Breakfast, Lunch, Dinner, Snack, Other

        // Poziom trudnoœci
        public string? Difficulty { get; set; } // Easy, Medium, Hard

        // Czas przygotowania
        public string? PrepTime { get; set; } // Quick (0-15 min), Medium (15-45 min), Long (45+ min)

        // Rodzaj kuchni
        public string? CuisineType { get; set; } // np. Italian, Asian, Mediterranean, etc.

        // Czy ma byæ wegañskie
        public bool IsVegan { get; set; }

        // Czy ma byæ wegetariañskie
        public bool IsVegetarian { get; set; }

        // Dodatkowe preferencje
        public string? AdditionalPreferences { get; set; }

        // Jêzyk przepisu
        public string Language { get; set; } = "pl"; // "pl" lub "en"
    }
}
