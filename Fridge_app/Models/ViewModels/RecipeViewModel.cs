using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models.ViewModels
{
    public class RecipeViewModel
    {
        [Range(0, double.MaxValue)]
        public double TimePrep { get; set; }

        [Required]
        public string Difficulty { get; set; }
    }
}
