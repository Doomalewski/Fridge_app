using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Fridge_app.Models.ViewModels
{
    public class ShoppingListItemViewModel
    {
        [Display(Name = "Produkt")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Iloœæ")]
        public string Quantity { get; set; } = string.Empty;

        [Display(Name = "Kategoria")]
        public string? Category { get; set; }
    }

    public class ShoppingListViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Nazwa listy")]
        [Required(ErrorMessage = "Nazwa listy jest wymagana")]
        [StringLength(120, ErrorMessage = "Nazwa jest za d³uga")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Bud¿et (PLN)")]
        [Range(0, 100000, ErrorMessage = "Bud¿et musi byæ wartoœci¹ dodatni¹")]
        public decimal? Budget { get; set; }

        [Display(Name = "Rodzaj kuchni")]
        public string? Cuisine { get; set; }

        [Display(Name = "Co chcesz ugotowaæ?")]
        [StringLength(200, ErrorMessage = "Opis dania jest za d³ugi")]
        public string? DesiredDish { get; set; }

        [Display(Name = "Uwagi / preferencje")]
        [StringLength(500, ErrorMessage = "Uwagi s¹ zbyt d³ugie")]
        public string? AdditionalNotes { get; set; }

        public List<ShoppingListItemViewModel> Items { get; set; } = new();

        public IEnumerable<string> AvailableCuisines { get; set; } = Enumerable.Empty<string>();

        public bool HasGeneratedList => Items?.Any() == true;
    }
}
