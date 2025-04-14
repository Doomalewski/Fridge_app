using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models.ViewModels
{
    public class MealCreateViewModel
    {
        public MealCreateViewModel()
        {
            AvailableProducts = new List<Product>();
            SelectedProducts = new List<SelectedProductViewModel>();
        }
        [Required]
        public string Description { get; set; }

        [Range(0, double.MaxValue)]
        public double Calories { get; set; }

        public List<Tag> Tags { get; set; } = new List<Tag>();
        [Required(ErrorMessage = "Wybierz kategorię")]
        [Display(Name = "Kategoria")]
        public string Category { get; set; }

        public RecipeViewModel Recipe { get; set; } = new RecipeViewModel();
        public List<SelectedProductViewModel> SelectedProducts { get; set; } = new List<SelectedProductViewModel>();
        public List<Product> AvailableProducts { get; set; }
    }
}
