using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models
{
    public class ShoppingListItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShoppingListId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(80)]
        public string? Quantity { get; set; }

        [MaxLength(80)]
        public string? Category { get; set; }

        public bool IsAddedToFridge { get; set; }

        public virtual ShoppingList? ShoppingList { get; set; }
    }
}
