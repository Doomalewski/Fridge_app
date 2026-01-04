using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fridge_app.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres email.")]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public int? HumanStatsId { get; set; }
        [ForeignKey("HumanStatsId")]
        public HumanStats HumanStats { get; set; }

        public ICollection<StoredProduct> Fridge { get; set; }
        public List<CookingTool> CookingTools { get; set; }
        public ICollection<NutritionTarget> NutritionTargets { get; set; }

        public User()
        {
            Fridge = new HashSet<StoredProduct>();
            CookingTools = new List<CookingTool>();
            NutritionTargets = new HashSet<NutritionTarget>();
        }
    }


}
