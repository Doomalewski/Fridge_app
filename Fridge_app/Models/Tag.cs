using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Type { get; set; }
        public List<Meal> Meals { get; set; } = new List<Meal>();

    }

}
