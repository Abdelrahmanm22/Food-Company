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
            CreateMap<Item, ItemToReturnDto>()
                .ForMember(d => d.Category, O => O.MapFrom(s => s.Category.Name))
                .ForMember(d => d.ImageUrl, O => O.MapFrom<ItemPictureUrlResolver>()); 
        }
    }
}
