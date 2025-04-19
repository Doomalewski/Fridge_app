using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models
{
    public class Recipe
    {
        [Key]
        public int Id { get; set; }

        public ICollection<ProductWithAmount> Products { get; set; }

        public double TimePrep { get; set; }
        public string MakingSteps { get; set; }
        public string Difficulty { get; set; }
    }

}
