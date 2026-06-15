using Fridge_app.Models.Enums;

namespace Fridge_app.Services
{
    public class NutritionCalculatorService
    {
        public double CalculateBMR(double weightKg, double heightCm, int age, string sex)
        {

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
                bmr -= 78;
            }

            return Math.Round(bmr, 0);
        }

        public double CalculateTDEE(double bmr, ActivityLevel activityLevel)
        {
            double multiplier = activityLevel switch
            {
                ActivityLevel.Sedentary => 1.2,
                ActivityLevel.LightlyActive => 1.375,
                ActivityLevel.ModeratelyActive => 1.55,
                ActivityLevel.VeryActive => 1.725,
                ActivityLevel.ExtraActive => 1.9,
                _ => 1.2
            };

            return Math.Round(bmr * multiplier, 0);
        }

        public int CalculateTargetCalories(double tdee, GoalType goal)
        {
            return goal switch
            {
                GoalType.Cut => (int)Math.Round(tdee * 0.8),
                GoalType.Maintain => (int)Math.Round(tdee),
                GoalType.Bulk => (int)Math.Round(tdee * 1.1),
                _ => (int)Math.Round(tdee)
            };
        }

        public (double protein, double fat, double carbs) CalculateMacros(
int targetCalories,
double weightKg,
GoalType goal)
        {
            double proteinGrams, fatGrams, carbsGrams;

            switch (goal)
            {
                case GoalType.Cut:
                    proteinGrams = weightKg * 2.2; fatGrams = weightKg * 0.8;
                    double remainingCalories = targetCalories - (proteinGrams * 4) - (fatGrams * 9);
                    carbsGrams = Math.Max(remainingCalories / 4, 50); break;

                case GoalType.Bulk:
                    proteinGrams = weightKg * 2.0; fatGrams = weightKg * 0.9;
                    remainingCalories = targetCalories - (proteinGrams * 4) - (fatGrams * 9);
                    carbsGrams = remainingCalories / 4;
                    break;

                case GoalType.Maintain:
                default:
                    proteinGrams = weightKg * 1.8; fatGrams = weightKg * 0.9;
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
