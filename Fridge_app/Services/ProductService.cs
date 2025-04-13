using System.Linq.Expressions;
using System.Threading.Tasks;
using Fridge_app.Models;
using Fridge_app.Repositories;

namespace Fridge_app.Services
{
    public class ProductService
    {
        private readonly IRepository<Product> _productRepository;

        public ProductService(IRepository<Product> repository)
        {
            _productRepository = repository;
        }

        public async Task AddProductAsync(Product product) => await _productRepository.AddAsync(product);
        public async Task UpdateProductAsync(Product product) => await _productRepository.UpdateAsync(product);
        public async Task DeleteProductAsync(Product product) => await _productRepository.DeleteAsync(product);
        public async Task<Product> GetProductAsync(int id) => await _productRepository.GetByIdAsync(id);
        public async Task<IEnumerable<Product>> GetAllProductsAsync() => await _productRepository.GetAllAsync();
        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, ProductCategory? category)
        {
            var normalizedSearch = searchTerm?.ToLower() ?? string.Empty;

            Expression<Func<Product, bool>> predicate = p =>
                (string.IsNullOrEmpty(normalizedSearch) || p.Name.ToLower().Contains(normalizedSearch)) &&
                (!category.HasValue || p.ProductCategory == category.Value);

            return await _productRepository.FindAsync(predicate);
        }
    }
}