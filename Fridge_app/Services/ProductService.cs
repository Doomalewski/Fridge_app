using System.Threading.Tasks;
using Fridge_app.Models;
using Fridge_app.Repositories;

namespace Fridge_app.Services
{
    public class ProductService
    {
        private readonly IRepository<Product> _repository;

        public ProductService(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task AddProductAsync(Product product) => await _repository.AddAsync(product);
        public async Task UpdateProductAsync(Product product) => await _repository.UpdateAsync(product);
        public async Task DeleteProductAsync(Product product) => await _repository.DeleteAsync(product);
        public async Task<Product> GetProductAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task<IEnumerable<Product>> GetAllProductsAsync() => await _repository.GetAllAsync();
    }
}