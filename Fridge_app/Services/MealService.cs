using Fridge_app.Data;
using Fridge_app.Models;
using Fridge_app.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fridge_app.Services
{
    public interface IMealService
    {
        Task<IEnumerable<Meal>> GetAllMealsAsync();
        Task<Meal> GetMealByIdAsync(int id);
        Task CreateMealAsync(Meal meal, Recipe recipe, List<ProductWithAmount> products);
        Task UpdateMealAsync(Meal meal);
        Task DeleteMealAsync(int id);
    }

    public class MealService : IMealService
    {
        private readonly IRepository<Meal> _mealRepository;
        private readonly IRepository<Recipe> _recipeRepository;
        private readonly IRepository<ProductWithAmount> _productAmountRepository;
        private readonly FridgeDbContext _context;

        public MealService(
            IRepository<Meal> mealRepository,
            IRepository<Recipe> recipeRepository,
            IRepository<ProductWithAmount> productAmountRepository,
            FridgeDbContext context)
        {
            _mealRepository = mealRepository;
            _recipeRepository = recipeRepository;
            _productAmountRepository = productAmountRepository;
            _context = context;
        }

        public async Task<IEnumerable<Meal>> GetAllMealsAsync()
        {
            return await _mealRepository.GetAllAsync();
        }

        public async Task<Meal> GetMealByIdAsync(int id)
        {
            return await _mealRepository.GetByIdWithIncludesAsync(
                id,
                query => query
                    .Include(m => m.Recipe)
                        .ThenInclude(r => r.Ingridients)
                            .ThenInclude(p => p.Product)
                    .Include(m => m.Tags)
            );
        }

        public async Task CreateMealAsync(Meal meal, Recipe recipe, List<ProductWithAmount> products)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _recipeRepository.AddAsync(recipe);
                await _context.SaveChangesAsync(); // Ustawia recipe.Id

                foreach (var product in products)
                {
                    product.Id = 0; // Ustawienie Id na 0, aby baza wygenerowała nowy klucz
                    product.RecipeId = recipe.Id;
                    await _productAmountRepository.AddAsync(product);
                }

                await _context.SaveChangesAsync(); // Zapisuje produkty

                meal.RecipeId = recipe.Id;
                await _mealRepository.AddAsync(meal);
                await _context.SaveChangesAsync(); // Zapisuje posiłek

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task UpdateMealAsync(Meal meal)
        {
            await _mealRepository.UpdateAsync(meal);
        }

        public async Task DeleteMealAsync(int id)
        {
            var meal = await _mealRepository.GetByIdAsync(id);
            if (meal != null)
            {
                await _mealRepository.DeleteAsync(meal);
            }
        }
    }
}