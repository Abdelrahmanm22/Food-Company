using AutoMapper;
using Food.APIs.DTOs;
using Food.Domain.Models;

namespace Food.APIs.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Restaurant, RestaurantToReturnDto>();
        }
    }
}
