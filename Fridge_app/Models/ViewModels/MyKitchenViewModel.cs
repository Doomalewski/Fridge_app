namespace Fridge_app.Models.ViewModels
{
    public class MyKitchenViewModel
    {
        public List<CookingTool> UserTools { get; set; } = new List<CookingTool>();
        public List<CookingTool> AvailableTools { get; set; } = new List<CookingTool>();

        public List<CookingTool> ToolsNotInKitchen => AvailableTools
            .Where(t => !UserTools.Any(ut => ut.Id == t.Id))
            .ToList();
    }
}
