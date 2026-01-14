using Fridge_app.Models;

namespace Fridge_app.Models.ViewModels
{
    public class HomeDashboardViewModel
    {
        public User? CurrentUser { get; set; }
        public WeeklyMealDayViewModel? TodayPlan { get; set; }
        public List<Meal> AvailableMeals { get; set; } = new();
    }
}
