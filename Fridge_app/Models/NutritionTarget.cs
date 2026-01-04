using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fridge_app.Models.Enums;

namespace Fridge_app.Models
{
    public class NutritionTarget
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [Range(500, 10000, ErrorMessage = "Kalorie musz¹ byæ w przedziale 500-10000")]
        public int CaloriesPerDay { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Bia³ko musi byæ w przedziale 0-1000g")]
        public double ProteinGrams { get; set; }

        [Required]
        [Range(0, 500, ErrorMessage = "T³uszcze musz¹ byæ w przedziale 0-500g")]
        public double FatGrams { get; set; }

        [Required]
        [Range(0, 1500, ErrorMessage = "Wêglowodany musz¹ byæ w przedziale 0-1500g")]
        public double CarbsGrams { get; set; }

        [Required]
        public GoalType Goal { get; set; }

        // Poziom aktywnoœci u¿ytkownika
        [Required]
        public ActivityLevel ActivityLevel { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public NutritionTarget()
        {
            CreatedAt = DateTime.UtcNow;
            ValidFrom = DateTime.UtcNow;
        }
    }
}

