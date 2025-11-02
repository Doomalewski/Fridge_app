using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fridge_app.Models
{
    public class Meal
    {
        [Key]
        public int Id { get; set; }

        public int? RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; }

        public string Description { get; set; }

        [Required]
        public string Category { get; set; }

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
