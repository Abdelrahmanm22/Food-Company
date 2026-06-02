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
            CreateMap<Department, DepartmentToReturnDto>();
            CreateMap<SessionJoin, SessionJoinToReturnDto>()
                .ForMember(d => d.UserName, O => O.MapFrom(s => s.User.UserName));
            CreateMap<Session, SessionToReturnDto>()
                .ForMember(d => d.HostUserName, O => O.MapFrom(s => s.HostUser.UserName))
                .ForMember(d => d.RestaurantName, O => O.MapFrom(s => s.Restaurant.Name))
                .ForMember(d => d.Participants, O => O.MapFrom(s => s.SessionJoins));

            //Restaurant menue mapping
            CreateMap<Restaurant, RestaurantMenuToReturnDto>();
            CreateMap<Category, CategoryWithItemsDto>();
        }
    }
}
