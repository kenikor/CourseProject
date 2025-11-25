using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CourseProgect_Planeta35.Models;

namespace CourseProgect_Planeta35.Services
{
    // Простой файловый сервис для чтения/записи JSON.
    // Структура файлов: %AppData%/CourseProgect_Planeta35/data/{name}.json
    public static class StorageService
    {
        private static readonly string BasePath;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        static StorageService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            BasePath = Path.Combine(appData, "CourseProgect_Planeta35", "data");
            Directory.CreateDirectory(BasePath);
        }

        private static string PathFor(string name) => Path.Combine(BasePath, name + ".json");

        private static List<T> LoadList<T>(string fileName)
        {
            try
            {
                var path = PathFor(fileName);
                if (!File.Exists(path)) return new List<T>();
                var txt = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<T>>(txt, JsonOptions) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        private static void SaveList<T>(string fileName, List<T> list)
        {
            try
            {
                var path = PathFor(fileName);
                var txt = JsonSerializer.Serialize(list, JsonOptions);
                File.WriteAllText(path, txt);
            }
            catch
            {
                // swallow errors for now
            }
        }

        // Users (используем реальные модели User из проекта)
        public static List<User> GetUsers() => LoadList<User>("users");
        public static void SaveUsers(List<User> users) => SaveList("users", users);

        // Departments
        public static List<Department> GetDepartments() => LoadList<Department>("departments");
        public static void SaveDepartments(List<Department> deps) => SaveList("departments", deps);

        // Roles
        public static List<Role> GetRoles() => LoadList<Role>("roles");
        public static void SaveRoles(List<Role> roles) => SaveList("roles", roles);

        // Assets / Categories
        public static List<Asset> GetAssets() => LoadList<Asset>("assets");
        public static void SaveAssets(List<Asset> assets) => SaveList("assets", assets);

        public static List<AssetCategory> GetCategories() => LoadList<AssetCategory>("categories");
        public static void SaveCategories(List<AssetCategory> cats) => SaveList("categories", cats);

        // Inventory items / inventories
        public static List<InventoryItem> GetInventoryItems() => LoadList<InventoryItem>("inventoryItems");
        public static void SaveInventoryItems(List<InventoryItem> items) => SaveList("inventoryItems", items);

        public static List<InventoryCheck> GetInventoryChecks() => LoadList<InventoryCheck>("inventoryChecks");
        public static void SaveInventoryChecks(List<InventoryCheck> checks) => SaveList("inventoryChecks", checks);
    }
}