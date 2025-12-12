using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using NutriLog;

namespace NutriLog
{
    class Program
    {
        static List<Food> foodDatabase = new();
        static UserProfile user = new();
        static List<FoodLogEntry> foodLog = new();
        static DataService dataService = new DataService();


        static void Main(string[] args)
        {
            LoadAllData();
            ConsoleHelper.PrintHeader("Welcome to NutriLog - Smart Tracker");

            if (!IsProfileValid(user))
            {
                Console.WriteLine("Let's set up your user profile first.");
                SetupUserProfile();
                dataService.SaveUserProfile(user);
            }

            int choice;
            do
            {
                Console.Clear();
                ShowMainMenu();
                bool ok = int.TryParse(Console.ReadLine(), out choice);
                if (!ok)
                {
                    ConsoleHelper.PrintError("Invalid input. Press any key.");
                    Console.ReadKey();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        SetupUserProfile();
                        dataService.SaveUserProfile(user);
                        break;
                    case 2:
                        LogFoodInteractive();
                        break;
                    case 3:
                        ViewLoggedFoods();
                        break;
                    case 4:
                        ViewDailySummary();
                        break;
                    case 5:
                        ShowBmiAndGoal();
                        break;
                    case 6:
                        SuggestFoodsForCalorieGoal();
                        break;
                    case 7:
                        ManageFoodDatabase();
                        break;
                    case 8:
                        SaveAllData();
                        ConsoleHelper.PrintSuccess("Data saved. Exiting. Thank you for using NutriLog!");
                        break;
                    default:
                        ConsoleHelper.PrintError("Invalid choice. Press any key.");
                        Console.ReadKey();
                        break;
                }
            } while (choice != 8);
        }

        #region Menu
        static void ShowMainMenu()
        {
            ConsoleHelper.PrintHeader("NutriLog - Daily Nutrition Tracker");
            Console.WriteLine("1. Set / View User Profile");
            Console.WriteLine("2. Log Food");
            Console.WriteLine("3. View Logged Foods");
            Console.WriteLine("4. View Daily Summary");
            Console.WriteLine("5. View BMI and Calorie Goal");
            Console.WriteLine("6. Meal plan suggestion");
            Console.WriteLine("7. Manage Food Database");
            Console.WriteLine("8. Exit");
            Console.Write("Enter your choice: ");
        }
        #endregion

        #region File Handling
        static void LoadAllData()
        {
            foodDatabase = dataService.LoadFoodDatabase();
            user = dataService.LoadUserProfile();
            foodLog = dataService.LoadFoodLog();
        }

        static void SaveAllData()
        {
            dataService.SaveUserProfile(user);
            dataService.SaveFoodLog(foodLog);
            dataService.SaveFoodDatabase(foodDatabase);
        }
        #endregion

        #region User Profile
        static bool IsProfileValid(UserProfile p) => p.Age > 0 && p.WeightKg > 0 && p.HeightCm > 0;

        static void SetupUserProfile()
        {
            Console.Clear();
            ConsoleHelper.PrintHeader("--- User Profile Setup ---");

            while (true)
            {
                Console.Write("Enter Gender (M/F): ");
                string g = (Console.ReadLine()?.Trim().ToUpper() ?? "");
                if (g == "M" || g == "F") { user.Gender = g; break; }
                ConsoleHelper.PrintError("Invalid input. Please enter M or F.");
            }

            user.Age = ReadInt("Enter Age (years): ");
            user.WeightKg = ReadDouble("Enter Weight (kg): ");
            user.HeightCm = ReadDouble("Enter Height (cm): ");

            double bmi = user.CalculateBMI();
            string category = user.GetBmiCategory();
            double suggestedCalories = user.GetSuggestedCalories();

            Console.WriteLine($"\nYour BMI is {bmi} ({category})");
            Console.WriteLine($"Suggested daily calorie goal: {suggestedCalories} kcal");

            Console.Write("Would you like to adjust this value? (Y/N): ");
            string choice = Console.ReadLine()?.Trim().ToUpper() ?? "N";
            user.DailyCalorieGoal = choice == "Y" ? ReadDouble("Enter your preferred calorie goal: ") : suggestedCalories;

            ConsoleHelper.PrintSuccess($"\nDaily goal set to {user.DailyCalorieGoal} kcal");
            Console.WriteLine("Press any key to return to menu...");
            Console.ReadKey();
        }

        static void ShowBmiAndGoal()
        {
            Console.Clear();
            ConsoleHelper.PrintHeader("--- BMI & Calorie Goal ---");

            double bmi = user.CalculateBMI();
            string category = user.GetBmiCategory();

            Console.WriteLine($"Gender: {user.Gender}, Age: {user.Age}, Weight: {user.WeightKg}kg, Height: {user.HeightCm}cm");
            Console.WriteLine($"BMI: {bmi} ({category})");
            Console.WriteLine($"Daily Calorie Goal: {user.DailyCalorieGoal} kcal");
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey();
        }
        #endregion

        #region Food Logging
        static void LogFoodInteractive()
        {
            Console.Clear();
            ConsoleHelper.PrintHeader("--- Log Food ---");

            if (foodDatabase.Count == 0)
            {
                ConsoleHelper.PrintError("No foods available in foods.json.");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter food (chicken, beef, pork, fish, etc.): ");
            string input = Console.ReadLine()?.Trim().ToLower() ?? "";

            var matches = foodDatabase.Where(f => f.Name.Contains(input, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matches.Count == 0)
            {
                ConsoleHelper.PrintError("Food not found. Please add it to foods.json first.");
                Console.ReadKey();
                return;
            }

            Food selectedFood;
            if (matches.Count == 1)
            {
                selectedFood = matches[0];
            }
            else
            {
                Console.WriteLine("Multiple matches found:");
                for (int i = 0; i < matches.Count; i++)
                    Console.WriteLine($"{i + 1}. {matches[i].Name} ({matches[i].Category})");
                int choice = ReadInt("Enter number: ") - 1;
                if (choice < 0 || choice >= matches.Count)
                {
                    ConsoleHelper.PrintError("Invalid selection. Returning...");
                    Console.ReadKey();
                    return;
                }
                selectedFood = matches[choice];
            }

            double grams = ReadDouble($"Enter amount in grams for {selectedFood.Name}: ");
            double factor = grams / 100.0;

            var entry = new FoodLogEntry
            {
                Date = DateTime.Now,
                FoodName = selectedFood.Name,
                Quantity = grams,
                Calories = selectedFood.Calories * factor,
                Protein = selectedFood.Protein * factor,
                Carbs = selectedFood.Carbs * factor,
                Fat = selectedFood.Fat * factor,
                Category = selectedFood.Category 
            };

            foodLog.Add(entry);
            dataService.SaveFoodLog(foodLog);

            Console.WriteLine($"\nLogged: {entry.FoodName} ({entry.Category}) — {grams}g");
            ConsoleHelper.PrintSuccess($"Calories: {entry.Calories:F0} kcal | Protein: {entry.Protein:F0}g | Carbs: {entry.Carbs:F0}g | Fat: {entry.Fat:F0}g");
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static void ViewLoggedFoods()
        {
            Console.Clear();
            ConsoleHelper.PrintHeader("--- Logged Foods ---");

            var today = DateTime.Now.Date;
            var todays = foodLog.Where(e => e.Date.Date == today).ToList();

            if (!todays.Any())
                Console.WriteLine("No foods logged for today yet.");
            else
            {
                foreach (var e in todays)
                {
                    Console.WriteLine($"{e.Date:t} | {e.FoodName} [{e.Category}] x{e.Quantity}g — {Math.Round(e.Calories)} kcal");
                }
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        static void ViewDailySummary()
        {
            Console.Clear();
            ConsoleHelper.PrintHeader("--- Daily Summary ---");

            var today = DateTime.Now.Date;
            var todays = foodLog.Where(e => e.Date.Date == today).ToList();

            double totalCalories = todays.Sum(e => e.Calories);
            double totalProtein = todays.Sum(e => e.Protein);
            double totalCarbs = todays.Sum(e => e.Carbs);
            double totalFat = todays.Sum(e => e.Fat);

            ConsoleHelper.DrawProgressBar(totalCalories, user.DailyCalorieGoal);

            Console.WriteLine($"\n--- Totals ---");
            Console.WriteLine($"Protein: {Math.Round(totalProtein)}g, Carbs: {Math.Round(totalCarbs)}g, Fat: {Math.Round(totalFat)}g");

            Console.WriteLine("\n--- Status ---");
            if (totalCalories < user.DailyCalorieGoal)
                ConsoleHelper.PrintSuccess("Status: Below goal. You can eat more nutritious food!");
            else if (totalCalories == user.DailyCalorieGoal)
                ConsoleHelper.PrintSuccess("Status: Perfect! You met your calorie goal!");
            else
                ConsoleHelper.PrintError("Status: Above goal. Try balancing tomorrow.");

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }
        #endregion

        #region Food Suggestion
        static void SuggestFoodsForCalorieGoal()
        {
            Console.Clear();
            ConsoleHelper.PrintHeader("--- Meal Suggestion ---");

            if (foodDatabase.Count == 0)
            {
                ConsoleHelper.PrintError("No foods available in foods.json.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Choose your diet preference for this meal:");
            Console.WriteLine("1. Normal (All Foods)");
            Console.WriteLine("2. Healthy (Fruits, Veggies, Grains, Lean Meats)");
            Console.WriteLine("3. Carnivore (Meat, Eggs, Fish, Dairy)");
            int pref = ReadInt("Enter choice: ");

            List<Food> candidates;
            if (pref == 2) 
            {
                candidates = foodDatabase.Where(f => f.Category == "Healthy" || f.Category == "Carnivore").ToList();
            }
            else if (pref == 3) 
            {
                candidates = foodDatabase.Where(f => f.Category == "Carnivore").ToList();
            }
            else 
            {
                candidates = foodDatabase; 
            }

            if (candidates.Count == 0)
            {
                ConsoleHelper.PrintError("No foods found for this category! Try adding more foods.");
                Console.ReadKey();
                return;
            }

            double totalCalories = user.DailyCalorieGoal;
            double perMealCalories = totalCalories / 3.0;
            Random rand = new();

            List<FoodLogEntry> GenerateMeal(double targetCalories)
            {
                var meal = new List<FoodLogEntry>();
                var shuffled = candidates.OrderBy(f => rand.Next()).Take(3).ToList();
                double caloriesPerFood = targetCalories / 3.0;

                foreach (var food in shuffled)
                {
                    double grams = (caloriesPerFood / food.Calories) * 100;
                    grams = Math.Clamp(grams, 50, 250);
                    double factor = grams / 100.0;

                    meal.Add(new FoodLogEntry
                    {
                        FoodName = food.Name,
                        Quantity = Math.Round(grams, 1),
                        Calories = food.Calories * factor,
                        Protein = food.Protein * factor,
                        Carbs = food.Carbs * factor,
                        Fat = food.Fat * factor,
                        Category = food.Category 
                    });
                }
                return meal;
            }

            void PrintMeal(string title, List<FoodLogEntry> meal)
            {
                Console.WriteLine($"\n--- {title} ---");
                foreach (var f in meal)
                {
                    Console.WriteLine($"- {f.FoodName} [{f.Category}] ({f.Quantity}g) -> {Math.Round(f.Calories)} kcal (P:{Math.Round(f.Protein)}g C:{Math.Round(f.Carbs)}g F:{Math.Round(f.Fat)}g)");
                }
                double totalCalories = meal.Sum(x => x.Calories);
                double totalProtein = meal.Sum(x => x.Protein);
                double totalCarbs = meal.Sum(x => x.Carbs);
                double totalFat = meal.Sum(x => x.Fat);
                Console.WriteLine($"  [Meal Total: {Math.Round(totalCalories)} kcal | P: {Math.Round(totalProtein)}g C: {Math.Round(totalCarbs)}g F: {Math.Round(totalFat)}g]");
            }

            while (true)
            {
                Console.Clear();
                ConsoleHelper.PrintHeader("--- Meal Suggestion ---");
                string filterName = pref == 2 ? "Healthy" : (pref == 3 ? "Carnivore" : "Normal");
                Console.WriteLine($"Generating {filterName} meals...");

                var breakfast = GenerateMeal(perMealCalories);
                var lunch = GenerateMeal(perMealCalories);
                var dinner = GenerateMeal(perMealCalories);

                PrintMeal("Breakfast", breakfast);
                PrintMeal("Lunch", lunch);
                PrintMeal("Dinner", dinner);

                double totalCaloriesDay = breakfast.Sum(x => x.Calories) + lunch.Sum(x => x.Calories) + dinner.Sum(x => x.Calories);

                Console.WriteLine("\n===================================");
                Console.WriteLine($"Total Daily Calories: {Math.Round(totalCaloriesDay)} kcal (Goal: {user.DailyCalorieGoal} kcal)");
                Console.WriteLine("===================================");

                Console.Write("\nDo you want to generate a new meal suggestion? (Y/N): ");
                string redo = Console.ReadLine()?.Trim().ToUpper() ?? "N";
                if (redo != "Y") break;
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }
        #endregion

        #region Food Database Management

        static void ManageFoodDatabase()
        {
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                ConsoleHelper.PrintHeader("--- Manage Food Database ---");
                Console.WriteLine("1. Add New Food");
                Console.WriteLine("2. Delete Food");
                Console.WriteLine("3. List All Foods");
                Console.WriteLine("4. Return to Main Menu");
                Console.Write("Enter your choice: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        AddNewFood();
                        break;
                    case "2":
                        DeleteFood();
                        break;
                    case "3":
                        ListAllFoods();
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        ConsoleHelper.PrintError("Invalid choice. Press any key.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void AddNewFood()
        {
            Console.Clear();
            ConsoleHelper.PrintHeader("--- Add New Food ---");

            string name = ReadString("Enter Food Name: ");
            double calories = ReadDouble("Enter Calories (per 100g): ");
            double protein = ReadDouble("Enter Protein (per 100g): ");
            double carbs = ReadDouble("Enter Carbs (per 100g): ");
            double fat = ReadDouble("Enter Fat (per 100g): ");

            Console.WriteLine("Enter Category (1. Normal, 2. Healthy, 3. Carnivore): ");
            int catChoice = ReadInt("Choice: ");
            string cat = "Normal";
            if (catChoice == 2) cat = "Healthy";
            if (catChoice == 3) cat = "Carnivore";

            var newFood = new Food
            {
                Name = name,
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat,
                Category = cat
            };

            foodDatabase.Add(newFood);
            dataService.SaveFoodDatabase(foodDatabase);

            ConsoleHelper.PrintSuccess($"\n{newFood.Name} ({cat}) has been added.");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey();
        }

        static void DeleteFood()
        {
            Console.Clear();
            ConsoleHelper.PrintHeader("--- Delete Food ---");

            if (foodDatabase.Count == 0)
            {
                ConsoleHelper.PrintError("Food database is empty.");
                Console.WriteLine("Press any key to return...");
                Console.ReadKey();
                return;
            }

            ListAllFoods(false);
            Console.WriteLine("\n-----------------------------");

            int indexToDelete = ReadInt("Enter the number of the food to delete (or 0 to cancel): ") - 1;

            if (indexToDelete < -1 || indexToDelete >= foodDatabase.Count)
            {
                ConsoleHelper.PrintError("Invalid number.");
                Console.WriteLine("Press any key to return...");
                Console.ReadKey();
                return;
            }

            if (indexToDelete == -1)
            {
                Console.WriteLine("Deletion cancelled.");
                Console.WriteLine("Press any key to return...");
                Console.ReadKey();
                return;
            }

            var food = foodDatabase[indexToDelete];
            foodDatabase.RemoveAt(indexToDelete);
            dataService.SaveFoodDatabase(foodDatabase);

            ConsoleHelper.PrintSuccess($"\n{food.Name} has been deleted.");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey();
        }

        static void ListAllFoods(bool wait = true)
        {
            Console.Clear();
            ConsoleHelper.PrintHeader("--- All Foods in Database ---");

            if (foodDatabase.Count == 0)
            {
                Console.WriteLine("Food database is empty.");
            }
            else
            {
                for (int i = 0; i < foodDatabase.Count; i++)
                {
                    var f = foodDatabase[i];
                    Console.WriteLine($"{i + 1}. {f.Name} [{f.Category}] | Cal: {f.Calories:F0} | P: {f.Protein:F0}g | C: {f.Carbs:F0}g");
                }
            }

            if (wait)
            {
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
            }
        }

        #endregion

        #region Input Helpers

        static double ReadDouble(string message)
        {
            double val;
            Console.Write(message);
            while (!double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out val) || val < 0)
                ConsoleHelper.PrintError("Invalid input. Enter a positive number: ");
            return val;
        }

        static int ReadInt(string message)
        {
            int val;
            Console.Write(message);
            while (!int.TryParse(Console.ReadLine(), out val) || val < 0)
                ConsoleHelper.PrintError("Invalid input. Enter a valid number: ");
            return val;
        }

        static string ReadString(string message)
        {
            string val;
            Console.Write(message);
            while (true)
            {
                val = Console.ReadLine()?.Trim() ?? "";
                if (!string.IsNullOrEmpty(val))
                    return val;

                ConsoleHelper.PrintError("Input cannot be empty. Please try again: ");
            }
        }
        #endregion
    }
}