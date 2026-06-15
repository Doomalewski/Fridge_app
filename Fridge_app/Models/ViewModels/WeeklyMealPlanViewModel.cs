using Fridge_app.Models;

namespace Fridge_app.Models.ViewModels
{
    public class WeeklyMealPlanViewModel
    {
        public DateTime WeekStart { get; set; }

        public List<WeeklyMealDayViewModel> Days { get; set; } = new();

        public List<Meal> AvailableMeals { get; set; } = new();

        public bool AutoSelected { get; set; }
    }

    public class WeeklyMealDayViewModel
    {
        public DateTime Date { get; set; }
        public int? BreakfastMealId { get; set; }
        public int? LunchMealId { get; set; }
        public int? DinnerMealId { get; set; }
        public int? SnackMealId { get; set; }
    }
}
