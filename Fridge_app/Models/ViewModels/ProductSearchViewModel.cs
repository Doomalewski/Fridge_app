namespace Fridge_app.Models.ViewModels
{
    public class ProductSearchViewModel
    {
        public string SearchTerm { get; set; }
        public ProductCategory? Category { get; set; }
        public List<Product> Results { get; set; } = new List<Product>();
    }
}