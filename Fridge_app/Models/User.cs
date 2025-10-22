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

        public int? DietId { get; set; }
        [ForeignKey("DietId")]
        public Diet Diet { get; set; }

        public ICollection<StoredProduct> Fridge { get; set; }
        public List<CookingTool> CookingTools { get; set; }
        public User()
        {
            Fridge = new HashSet<StoredProduct>();
            Diet = new Diet();
            CookingTools = new List<CookingTool>();
        }
    }


}
