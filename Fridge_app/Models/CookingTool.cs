namespace Fridge_app.Models
{
    public class CookingTool
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ToString()
        {
            return $"[Name: {Name}, Id: {Id}]";
        }
    }
    public class CookingToolViewModel
    {
        public string Name { get; set; } = string.Empty;
    }

}
