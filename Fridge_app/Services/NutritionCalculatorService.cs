using Fridge_app.Models.Enums;

namespace Fridge_app.Services
{
    public class NutritionCalculatorService
    {
        /// <summary>
        /// Oblicza BMR (Basal Metabolic Rate) u¿ywaj¹c wzoru Mifflin-St Jeor
        /// </summary>
        public double CalculateBMR(double weightKg, double heightCm, int age, string sex)
        {
            // Wzór Mifflin-St Jeor:
            // Mê¿czyŸni: BMR = (10 × waga w kg) + (6.25 × wzrost w cm) - (5 × wiek w latach) + 5
            // Kobiety: BMR = (10 × waga w kg) + (6.25 × wzrost w cm) - (5 × wiek w latach) - 161

            double bmr = (10 * weightKg) + (6.25 * heightCm) - (5 * age);

            if (sex?.ToLower() == "mê¿czyzna" || sex?.ToLower() == "male")
            {
                bmr += 5;
            }
            else if (sex?.ToLower() == "kobieta" || sex?.ToLower() == "female")
            {
                bmr -= 161;
            }
            else
            {
                // Dla "Inna" - u¿yj œredniej
                bmr -= 78; // Œrednia z +5 i -161
            }

            return Math.Round(bmr, 0);
        }

        /// <summary>
        /// Oblicza TDEE (Total Daily Energy Expenditure) na podstawie BMR i poziomu aktywnoœci
        /// </summary>
        public double CalculateTDEE(double bmr, ActivityLevel activityLevel)
        {
            double multiplier = activityLevel switch
            {
                ActivityLevel.Sedentary => 1.2,          // Siedz¹cy tryb ¿ycia
                ActivityLevel.LightlyActive => 1.375,    // Lekko aktywny
                ActivityLevel.ModeratelyActive => 1.55,  // Umiarkowanie aktywny
                ActivityLevel.VeryActive => 1.725,       // Bardzo aktywny
                ActivityLevel.ExtraActive => 1.9,        // Ekstremalnie aktywny
                _ => 1.2
            };

            return Math.Round(bmr * multiplier, 0);
        }

        /// <summary>
        /// Oblicza dzienne zapotrzebowanie kaloryczne na podstawie celu
        /// </summary>
        public int CalculateTargetCalories(double tdee, GoalType goal)
        {
            return goal switch
            {
                GoalType.Cut => (int)Math.Round(tdee * 0.8),      // Deficyt 20% dla redukcji
                GoalType.Maintain => (int)Math.Round(tdee),        // TDEE dla utrzymania
                GoalType.Bulk => (int)Math.Round(tdee * 1.1),     // Nadwy¿ka 10% dla masy
                _ => (int)Math.Round(tdee)
            };
        }

        /// <summary>
        /// Oblicza makrosk³adniki na podstawie kalorii i celu
        /// </summary>
        public (double protein, double fat, double carbs) CalculateMacros(
            int targetCalories, 
            double weightKg, 
            GoalType goal)
        {
            double proteinGrams, fatGrams, carbsGrams;

            switch (goal)
            {
                case GoalType.Cut: // Redukcja - wysokie bia³ko, niskie wêgle
                    proteinGrams = weightKg * 2.2;  // 2.2g bia³ka na kg masy cia³a
                    fatGrams = weightKg * 0.8;      // 0.8g t³uszczu na kg
                    
                    // Reszta kalorii z wêglowodanów
                    double remainingCalories = targetCalories - (proteinGrams * 4) - (fatGrams * 9);
                    carbsGrams = Math.Max(remainingCalories / 4, 50); // Min 50g wêgli
                    break;

                case GoalType.Bulk: // Masa - wysokie bia³ko i wêgle
                    proteinGrams = weightKg * 2.0;  // 2.0g bia³ka na kg
                    fatGrams = weightKg * 0.9;      // 0.9g t³uszczu na kg
                    
                    remainingCalories = targetCalories - (proteinGrams * 4) - (fatGrams * 9);
                    carbsGrams = remainingCalories / 4;
                    break;

                case GoalType.Maintain: // Utrzymanie - zbalansowane
                default:
                    proteinGrams = weightKg * 1.8;  // 1.8g bia³ka na kg
                    fatGrams = weightKg * 0.9;      // 0.9g t³uszczu na kg
                    
                    remainingCalories = targetCalories - (proteinGrams * 4) - (fatGrams * 9);
                    carbsGrams = remainingCalories / 4;
                    break;
            }

            return (
                Math.Round(proteinGrams, 1),
                Math.Round(fatGrams, 1),
                Math.Round(carbsGrams, 1)
            );
        }

        /// <summary>
        /// Kompletne obliczenie celów ¿ywieniowych
        /// </summary>
        public NutritionCalculationResult CalculateNutritionTargets(
            double weightKg,
            double heightCm,
            int age,
            string sex,
            ActivityLevel activityLevel,
            GoalType goal)
        {
            var bmr = CalculateBMR(weightKg, heightCm, age, sex);
            var tdee = CalculateTDEE(bmr, activityLevel);
            var targetCalories = CalculateTargetCalories(tdee, goal);
            var (protein, fat, carbs) = CalculateMacros(targetCalories, weightKg, goal);

            return new NutritionCalculationResult
            {
                BMR = bmr,
                TDEE = tdee,
                TargetCalories = targetCalories,
                ProteinGrams = protein,
                FatGrams = fat,
                CarbsGrams = carbs,
                ActivityLevel = activityLevel,
                Goal = goal
            };
        }
    }

    /// <summary>
    /// Wynik obliczeñ celów ¿ywieniowych
    /// </summary>
    public class NutritionCalculationResult
    {
        public double BMR { get; set; }
        public double TDEE { get; set; }
        public int TargetCalories { get; set; }
        public double ProteinGrams { get; set; }
        public double FatGrams { get; set; }
        public double CarbsGrams { get; set; }
        public ActivityLevel ActivityLevel { get; set; }
        public GoalType Goal { get; set; }

        public string GetDescription()
        {
            return $"BMR: {BMR} kcal | TDEE: {TDEE} kcal | Cel: {TargetCalories} kcal";
        }
    }
}
