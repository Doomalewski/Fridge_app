using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    namespace Fridge_app.Models.ViewModels
    {
        public class RecipeViewModel
        {
            public string Name { get; set; } = string.Empty;
            public string Difficulty { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public List<ProductWithAmountViewModel> Ingridients { get; set; } = new();
            public List<RecipeStepViewModel> Steps { get; set; } = new();
            public List<CookingToolViewModel> CookingTools { get; set; } = new();
        }
    }


}
