using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fridge_app.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int Kcal { get; set; }
        public double Fat { get; set; }
        public double UnsaturatedFat { get; set; }
        public double Carbohydrates { get; set; }
        public double Sugar { get; set; }
        public double Protein { get; set; }
        public double Fiber { get; set; }
        public double Salt { get; set; }

        // 🔧 Sugestia: rozdziel PriceRange na MinPrice / MaxPrice
        public double PriceMin { get; set; }
        public double PriceMax { get; set; }

        [Required]
        public ProductCategory ProductCategory { get; set; } = ProductCategory.Other;

        public string Unit { get; set; }
    }


}
