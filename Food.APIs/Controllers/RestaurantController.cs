using Food.Domain.Models;
using Food.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    public class RestaurantController : APIBaseController
    {
        private readonly IGenericRepository<Restaurant> restaurantRepo;
        private readonly ILogger<RestaurantController> logger;

        public RestaurantController(IGenericRepository<Restaurant> restaurantRepo,ILogger<RestaurantController> logger)
        {
            this.restaurantRepo = restaurantRepo;
            this.logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetRestaurants()
        {
            try
            {
                var restaurants = await restaurantRepo.GetAllAsync();
                return Ok(restaurants);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "An error occurred while fetching restaurants.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching restaurants.");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurantById(int id)
        {
            try
            {
                var restaurant = await restaurantRepo.GetByIdAsync(id);
                if (restaurant == null)
                {
                    logger.LogWarning($"Restaurant with ID {id} not found.");
                    return NotFound();
                }
                return Ok(restaurant);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"An error occurred while fetching restaurant with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while fetching restaurant with ID {id}.");
            }
            
        }
    }
}
