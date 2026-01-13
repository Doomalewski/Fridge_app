using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models
{
    public class ShoppingList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        public decimal? Budget { get; set; }
        public string? Cuisine { get; set; }
        public string? DesiredDish { get; set; }
        public string? AdditionalNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }

        public virtual ICollection<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
        public virtual User? User { get; set; }
    }
}
