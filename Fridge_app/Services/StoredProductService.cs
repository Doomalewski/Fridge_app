using Fridge_app.Models.ViewModels;
using Fridge_app.Models;
using Fridge_app.Repositories;
using Microsoft.EntityFrameworkCore;
using GenerativeAI.Types;

namespace Fridge_app.Services
{
    public class StoredProductService
    {
        private readonly IRepository<StoredProduct> _storedProductRepository;

        public StoredProductService(IRepository<StoredProduct> storedProductRepository)
        {
            _storedProductRepository = storedProductRepository;
        }

        // W StoredProductService.cs
        public async Task AddProductToFridgeAsync(int userId, AddProductViewModel model)
        {
            var storedProduct = new StoredProduct
            {
                ProductId = model.ProductId,
                UserId = userId,
                Quantity = model.Quantity,
                ExpirationDate = model.ExpirationDate?.ToUniversalTime() ?? DateTime.UtcNow,
            };

            await _storedProductRepository.AddAsync(storedProduct);
        }

        public async Task<IEnumerable<StoredProduct>> GetUserFridgeAsync(int userId)
        {
            return await _storedProductRepository
                .Where(p => p.UserId == userId)
                .Include(p => p.Product)
                .ToListAsync();
        }
        public async Task UpdateProductAsync(EditProductViewModel model)
        {
            var storedProduct = await _storedProductRepository.GetByIdAsync(model.Id);
            if (storedProduct == null)
                throw new ArgumentException("Produkt nie istnieje");

            storedProduct.Quantity = model.NewQuantity;
            await _storedProductRepository.UpdateAsync(storedProduct);
        }

        public async Task DeleteProductAsync(int id)
        {
            var storedProduct = await _storedProductRepository.GetByIdAsync(id);
            if (storedProduct == null)
                throw new ArgumentException("Produkt nie istnieje");

            await _storedProductRepository.DeleteAsync(storedProduct);
        }

    }
}