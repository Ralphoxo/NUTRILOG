using System;

namespace NutriLog
{

    public static class ConsoleHelper
    {
        public static void PrintHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            string border = "======================================";
            Console.WriteLine(border);
            Console.WriteLine($"   {title}");
            Console.WriteLine(border);
            Console.ResetColor();
        }

        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <param name="currentValue">The current value (e.g., calories eaten)</param>
        /// <param name="maxValue">The maximum value (e.g., calorie goal)</param>
        public static void DrawProgressBar(double currentValue, double maxValue)
        {
            if (maxValue <= 0) return;

            const int barWidth = 30;
            double percentage = Math.Clamp(currentValue / maxValue, 0.0, 1.0);
            int progressChars = (int)(percentage * barWidth);

            string bar = new string('█', progressChars); 
            string background = new string('░', barWidth - progressChars); 

            Console.ForegroundColor = currentValue > maxValue ? ConsoleColor.Red : ConsoleColor.Green;
            Console.Write($"[{bar}{background}]");
            Console.ResetColor();

            Console.WriteLine($" {Math.Round(currentValue)} / {Math.Round(maxValue)} kcal ({percentage:P0})");
        }
    }
}