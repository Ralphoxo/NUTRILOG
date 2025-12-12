using System;

namespace NutriLog
{
    public class FoodLogEntry
    {
        public DateTime Date { get; set; }
        public string FoodName { get; set; } = "";
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public double Quantity { get; set; } = 1.0;
        public string Category { get; set; } = "Normal";
    }
}