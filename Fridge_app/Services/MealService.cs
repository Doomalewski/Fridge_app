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
        Task DeleteRecipeAsync(int recipeId);
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
                // Ensure ingredients are attached only once to avoid duplicate inserts
                if (products?.Any() == true)
                {
                    recipe.Ingridients = products;
                }

                await _recipeRepository.AddAsync(recipe); // Saves recipe with its ingredients

                meal.RecipeId = recipe.Id;
                await _mealRepository.AddAsync(meal);

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

        public async Task DeleteRecipeAsync(int recipeId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Usuń wszystkie składniki (ProductWithAmount) związane z przepisem
                var ingredients = await _context.Set<ProductWithAmount>()
                    .Where(p => p.RecipeId == recipeId)
                    .ToListAsync();

                foreach (var ingredient in ingredients)
                {
                    _context.Set<ProductWithAmount>().Remove(ingredient);
                }

                await _context.SaveChangesAsync();

                // Usuń sam przepis
                var recipe = await _recipeRepository.GetByIdAsync(recipeId);
                if (recipe != null)
                {
                    await _recipeRepository.DeleteAsync(recipe);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}