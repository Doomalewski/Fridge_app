using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models.ViewModels
{
    public class EditProductViewModel
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ilość musi być większa od zera")]
        public double NewQuantity { get; set; }
    }
}
