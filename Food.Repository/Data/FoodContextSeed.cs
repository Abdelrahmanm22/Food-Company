using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Repository.Data
{
    public class FoodContextSeed
    {
        //Function To Seed
        public static async Task SeedAsync(FoodContext dbContext)
        {
            // Seed Restaurants
            if (!dbContext.Restaurants.Any())
            {
                var RestaurantsData = File.ReadAllText("../Food.Repository/Data/DataSeed/Restaurants.json");
                var Restaurants = JsonSerializer.Deserialize<List<Restaurant>>(RestaurantsData);
                if (Restaurants?.Count > 0)
                {
                    foreach(var restaurant in Restaurants)
                    {
                        await dbContext.Set<Restaurant>().AddAsync(restaurant);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
            //Seed Departments
            if (!dbContext.Departments.Any())
            {
                var DepartmentsData = File.ReadAllText("../Food.Repository/Data/DataSeed/Departments.json");
                var Departments = JsonSerializer.Deserialize<List<Department>>(DepartmentsData);
                if (Departments?.Count > 0)
                {
                    foreach(var department in Departments)
                    {
                        await dbContext.Set<Department>().AddAsync(department);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }

            //Seed Categories
            if (!dbContext.Categories.Any())
            {
                var CategoriesData = File.ReadAllText("../Food.Repository/Data/DataSeed/Categories.json");
                var Categories = JsonSerializer.Deserialize<List<Category>>(CategoriesData);
                if (Categories?.Count > 0)
                {
                    foreach (var category in Categories)
                    {
                        await dbContext.Set<Category>().AddAsync(category);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }

            //Seed Items
            if (!dbContext.Items.Any())
            {
                var ItemsData = File.ReadAllText("../Food.Repository/Data/DataSeed/Items.json");
                var Items = JsonSerializer.Deserialize<List<Item>>(ItemsData);
                if (Items?.Count > 0)
                {
                    foreach (var item in Items)
                    {
                        await dbContext.Set<Item>().AddAsync(item);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
