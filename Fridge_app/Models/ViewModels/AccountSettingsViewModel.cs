using System.ComponentModel.DataAnnotations;
using Fridge_app.Models.Enums;

namespace Fridge_app.Models.ViewModels
{
    public class AccountSettingsViewModel
    {
        public string Email { get; set; }

        // Wybór diety, aktywnoœci i celu - to jedyne pola które u¿ytkownik wybiera
        [Display(Name = "Wybierz dietê")]
        public string SelectedDietType { get; set; }

        public List<string> AvailableDietTypes { get; set; }

        [Display(Name = "Poziom aktywnoœci")]
        [Required(ErrorMessage = "Wybierz poziom aktywnoœci")]
        public ActivityLevel? ActivityLevel { get; set; }

        [Display(Name = "Cel ¿ywieniowy")]
        [Required(ErrorMessage = "Wybierz cel ¿ywieniowy")]
        public GoalType? NutritionGoal { get; set; }

        // Statystyki zdrowotne
        [Display(Name = "Wiek")]
        [Range(1, 120, ErrorMessage = "Wiek musi byæ w przedziale 1-120")]
        [Required(ErrorMessage = "Wiek jest wymagany")]
        public int? Age { get; set; }

        [Display(Name = "Wzrost (cm)")]
        [Range(50, 250, ErrorMessage = "Wzrost musi byæ w przedziale 50-250 cm")]
        [Required(ErrorMessage = "Wzrost jest wymagany")]
        public float? Height { get; set; }

        [Display(Name = "Aktualna waga (kg)")]
        [Range(20, 500, ErrorMessage = "Waga musi byæ w przedziale 20-500 kg")]
        [Required(ErrorMessage = "Waga jest wymagana")]
        public float? CurrentWeight { get; set; }

        [Display(Name = "P³eæ")]
        [Required(ErrorMessage = "P³eæ jest wymagana")]
        public string Sex { get; set; }

        [Display(Name = "Cel")]
        public string Goal { get; set; }

        // Obliczone wartoœci (tylko do wyœwietlenia, nie do edycji)
        public int? CalculatedCalories { get; set; }
        public double? CalculatedProtein { get; set; }
        public double? CalculatedFat { get; set; }
        public double? CalculatedCarbs { get; set; }
        public double? BMR { get; set; }
        public double? TDEE { get; set; }

        public AccountSettingsViewModel()
        {
            AvailableDietTypes = new List<string>();
        }
    }
}





