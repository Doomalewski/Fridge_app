using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models.ViewModels
{
    public class RecipeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<ProductWithAmount> Ingridients { get; set; } = new();
        public List<RecipeStep> Steps { get; set; } = new();
        public List<CookingTool> CookingTools { get; set; } = new();
    }

}
