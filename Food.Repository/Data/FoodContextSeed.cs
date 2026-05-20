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
        }
    }
}
