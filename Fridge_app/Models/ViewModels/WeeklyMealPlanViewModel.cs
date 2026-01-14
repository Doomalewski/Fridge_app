using Fridge_app.Models;

namespace Fridge_app.Models.ViewModels
{
    public class WeeklyMealPlanViewModel
    {
        /// <summary>
        /// Data rozpoczêcia planu tygodniowego (poniedzia³ek).
        /// </summary>
        public DateTime WeekStart { get; set; }

        /// <summary>
        /// Poszczególne dni z przypisanymi posi³kami.
        /// </summary>
        public List<WeeklyMealDayViewModel> Days { get; set; } = new();

        /// <summary>
        /// Lista dostêpnych posi³ków do wyboru.
        /// </summary>
        public List<Meal> AvailableMeals { get; set; } = new();

        /// <summary>
        /// Okreœla, czy plan zosta³ wstêpnie uzupe³niony automatycznie.
        /// </summary>
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
