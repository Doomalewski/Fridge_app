using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}