using Fridge_app.Models;
using Fridge_app.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Text.Json;

namespace Fridge_app.Controllers
{
    /// <summary>
    /// Shared helpers for building and persisting the weekly meal plan, used by both
    /// HomeController and MealController.
    /// </summary>
    internal static class WeeklyPlanHelpers
    {
        internal const string SessionKey = "WeeklyMealPlan";

        private static readonly CompareInfo CategoryComparer =
            CultureInfo.GetCultureInfo("pl-PL").CompareInfo;

        private const CompareOptions CategoryCompareOptions =
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

        private static readonly JsonSerializerOptions SerializerOptions =
            new() { PropertyNameCaseInsensitive = true };

        internal static List<Meal> MealsByCategory(IEnumerable<Meal> meals, params string[] categories)
        {
            if (meals == null)
                return new List<Meal>();

            var normalizedCategories = categories?
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .ToArray() ?? Array.Empty<string>();

            if (normalizedCategories.Length == 0)
                return new List<Meal>();

            return meals
                .Where(m => m != null && normalizedCategories.Any(c =>
                    CategoryComparer.Compare(m.Category?.Trim() ?? string.Empty, c, CategoryCompareOptions) == 0))
                .ToList();
        }

        internal static int? PickRandomMealId(IReadOnlyList<Meal> meals)
        {
            if (meals == null || meals.Count == 0)
                return null;

            return meals[Random.Shared.Next(meals.Count)].Id;
        }

        internal static WeeklyMealPlanViewModel? LoadFromSession(ISession session)
        {
            var serialized = session.GetString(SessionKey);
            if (string.IsNullOrWhiteSpace(serialized))
                return null;

            try
            {
                return JsonSerializer.Deserialize<WeeklyMealPlanViewModel>(serialized, SerializerOptions);
            }
            catch
            {
                return null;
            }
        }

        internal static void SaveToSession(ISession session, WeeklyMealPlanViewModel model)
        {
            var snapshot = new WeeklyMealPlanViewModel
            {
                WeekStart = model.WeekStart,
                Days = model.Days ?? new List<WeeklyMealDayViewModel>(),
                AutoSelected = model.AutoSelected
            };

            session.SetString(SessionKey, JsonSerializer.Serialize(snapshot));
        }
    }
}
