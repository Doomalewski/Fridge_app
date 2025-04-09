using System.ComponentModel.DataAnnotations;

namespace Fridge_app.Models
{
    public class HumanStats
    {
        [Key]
        public int Id { get; set; }

        public int Age { get; set; }

        public ICollection<WeightEntry> Weight { get; set; }

        public float Height { get; set; }

        public string Sex { get; set; }

        public string Goal { get; set; }
    }

}
