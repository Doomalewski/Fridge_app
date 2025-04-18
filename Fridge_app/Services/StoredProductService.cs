﻿using Fridge_app.Models.ViewModels;
using Fridge_app.Models;
using Fridge_app.Repositories;
using Microsoft.EntityFrameworkCore;

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


    }
}