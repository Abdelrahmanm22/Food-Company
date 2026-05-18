using AutoMapper;
using Food.APIs.DTOs;
using Food.Domain.Models;

namespace Food.APIs.Helpers
{
    public class ItemPictureUrlResolver : IValueResolver<Item, ItemToReturnDto, string>
    {
        private readonly IConfiguration configuration;

        public ItemPictureUrlResolver(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public string Resolve(Item source, ItemToReturnDto destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.ImageUrl))
            {
                return $"{configuration["ApiBaseURL"]}{source.ImageUrl}";
            }
            return string.Empty;
        }
    }
}
