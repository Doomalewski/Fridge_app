using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models
{
    public class Diet
    {
        [Key]
        public int Id { get; set; }

        public ICollection<Meal> Meals { get; set; }

        public double TimeSpan { get; set; }

        public string DietType { get; set; }

        public string Goal { get; set; }

        public int Calories { get; set; }
    }

}
