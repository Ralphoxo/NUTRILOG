using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using NutriLog;

namespace NutriLog
{
    public class DataService
    {
        private const string FoodDbFile = "foods.json";
        private const string ProfileFile = "userprofile.json";
        private const string LogFile = "log.json";

        private JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public List<Food> LoadFoodDatabase()
        {
            if (File.Exists(FoodDbFile))
            {
                string json = File.ReadAllText(FoodDbFile);
                return JsonSerializer.Deserialize<List<Food>>(json) ?? new();
            }
            return new();
        }

        public UserProfile LoadUserProfile()
        {
            if (File.Exists(ProfileFile))
            {
                string json = File.ReadAllText(ProfileFile);
                return JsonSerializer.Deserialize<UserProfile>(json) ?? new();
            }
            return new();
        }

        public List<FoodLogEntry> LoadFoodLog()
        {
            if (File.Exists(LogFile))
            {
                string json = File.ReadAllText(LogFile);
                return JsonSerializer.Deserialize<List<FoodLogEntry>>(json) ?? new();
            }
            return new();
        }

        public void SaveUserProfile(UserProfile user)
        {
            string json = JsonSerializer.Serialize(user, _jsonOptions);
            File.WriteAllText(ProfileFile, json);
        }

        public void SaveFoodLog(List<FoodLogEntry> foodLog)
        {
            string json = JsonSerializer.Serialize(foodLog, _jsonOptions);
            File.WriteAllText(LogFile, json);
        }

        public void SaveFoodDatabase(List<Food> foodDatabase)
        {
            string json = JsonSerializer.Serialize(foodDatabase, _jsonOptions);
            File.WriteAllText(FoodDbFile, json);
        }
    }
}