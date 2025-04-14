using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fridge_app.Models
{
    public class ProductWithAmount
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Range(0.01, double.MaxValue)]
        public double Amount { get; set; }
        public int RecipeId { get; set; } 

        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; }
    }

}
