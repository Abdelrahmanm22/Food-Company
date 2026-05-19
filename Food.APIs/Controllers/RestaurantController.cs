using AutoMapper;
using Food.APIs.DTOs;
using Food.APIs.Errors;
using Food.Domain.Models;
using Food.Domain.Repositories;
using Food.Domain.Specifications.RestaurantSpec;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    public class RestaurantController : APIBaseController
    {
        private readonly IGenericRepository<Restaurant> restaurantRepo;
        private readonly ILogger<RestaurantController> logger;
        private readonly IMapper mapper;

        public RestaurantController(IGenericRepository<Restaurant> restaurantRepo, ILogger<RestaurantController> logger, IMapper mapper)
        {
            this.restaurantRepo = restaurantRepo;
            this.logger = logger;
            this.mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RestaurantToReturnDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRestaurants([FromQuery]ProductSpecParams Params)
        {
            var Spec = new RestaurantWithCategoriesSpec(Params);
            var restaurants = await restaurantRepo.GetAllAsync(Spec);
            var restaurantDtos = mapper.Map<IEnumerable<Restaurant>, IEnumerable<RestaurantToReturnDto>>(restaurants);
            return Ok(restaurantDtos);

        }
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RestaurantToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRestaurantById(int id)
        {
            var Spec = new RestaurantWithCategoriesSpec(id);
            var restaurant = await restaurantRepo.GetByIdAsync(Spec);
            if (restaurant == null)
            {
                logger.LogWarning($"Restaurant with ID {id} not found.");
                return NotFound(new ApiErrorResponse(404, $"Restaurant with ID {id} not found."));
            }
            var restaurantDto = mapper.Map<Restaurant, RestaurantToReturnDto>(restaurant);
            return Ok(restaurantDto);
        }
    }
}
