using System;

namespace NutriLog
{
    public class UserProfile
    {
        public string Gender { get; set; } = "M";
        public int Age { get; set; } = 18;
        public double WeightKg { get; set; } = 60;
        public double HeightCm { get; set; } = 170;
        public double DailyCalorieGoal { get; set; } = 2000;

        public double CalculateBMI()
        {
            double h = HeightCm / 100.0;
            return h > 0 ? Math.Round(WeightKg / (h * h), 1) : 0;
        }


        public string GetBmiCategory()
        {
            double bmi = CalculateBMI();
            if (bmi < 18.5) return "Underweight";
            if (bmi < 25) return "Normal weight";
            if (bmi < 30) return "Overweight";
            return "Obese";
        }


        public double GetSuggestedCalories()
        {
            double bmi = CalculateBMI();
            if (bmi < 18.5) return 2500;
            if (bmi < 25) return 2000;
            if (bmi < 30) return 1800;
            return 1600;
        }
    }
}