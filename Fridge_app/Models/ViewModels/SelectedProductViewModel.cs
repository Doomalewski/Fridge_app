using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models.ViewModels
{
    public class SelectedProductViewModel
    {
        public int ProductId { get; set; }

        [Range(0.01, double.MaxValue)]
        public double Amount { get; set; }
    }
}
