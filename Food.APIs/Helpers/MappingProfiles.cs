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

            //Cart mapping
            CreateMap<SessionCart, SessionCartToReturnDto>()
                .ForMember(d => d.Total, o => o.MapFrom(s => s.Items.Sum(it => it.Price * it.Quantity)));
            CreateMap<CartItem, CartItemToReturnDto>()
                .ForMember(d => d.SubTotal, o => o.MapFrom(s => s.Price * s.Quantity));

            // Order mapping
            CreateMap<Order, OrderToReturnDto>()
                .ForMember(d => d.RestaurantName, o => o.MapFrom(s => s.Session.Restaurant.Name))
                .ForMember(d => d.HostUserName, o => o.MapFrom(s => s.Session.HostUser.UserName))
                .ForMember(d => d.DeliveryCostPerPerson, o => o.MapFrom(s => s.Session.SessionJoins.Count > 0 ? s.DeliveryCost / s.Session.SessionJoins.Count : 0));
            CreateMap<OrderDetail,OrderDetailToReturnDto>()
                .ForMember(d => d.ItemName, o => o.MapFrom(s => s.Item.Name))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.UserName : ""))
                .ForMember(d => d.SubTotal, o => o.MapFrom(s => s.Price * s.Quantity));
        }
    }
}
