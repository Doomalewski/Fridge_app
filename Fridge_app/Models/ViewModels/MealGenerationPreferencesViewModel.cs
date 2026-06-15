namespace Fridge_app.Models.ViewModels
{
    public class MealGenerationPreferencesViewModel
    {
        public string? MealType { get; set; }
        public string? Difficulty { get; set; }
        public string? PrepTime { get; set; }
        public string? CuisineType { get; set; }
        public bool IsVegan { get; set; }

        public bool IsVegetarian { get; set; }

        public string? AdditionalPreferences { get; set; }

        public string Language { get; set; } = "pl";
    }
}
