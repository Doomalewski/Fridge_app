using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models
{
    public class WeightEntry
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public float Weight { get; set; }
    }

}
