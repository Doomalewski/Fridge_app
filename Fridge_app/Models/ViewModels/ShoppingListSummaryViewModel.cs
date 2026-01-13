using System;
using System.Collections.Generic;

namespace Fridge_app.Models.ViewModels
{
    public class ShoppingListSummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Cuisine { get; set; }
        public string? DesiredDish { get; set; }
        public decimal? Budget { get; set; }
        public int ItemsCount { get; set; }
        public decimal EstimatedMin { get; set; }
        public decimal EstimatedMax { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class ShoppingListDetailsViewModel
    {
        public ShoppingList List { get; set; }
        public decimal EstimatedMin { get; set; }
        public decimal EstimatedMax { get; set; }

        public ShoppingListDetailsViewModel(ShoppingList list, decimal estimatedMin, decimal estimatedMax)
        {
            List = list;
            EstimatedMin = estimatedMin;
            EstimatedMax = estimatedMax;
        }
    }
}
