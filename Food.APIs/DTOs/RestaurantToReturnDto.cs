using Food.Domain.Models;

namespace Food.APIs.DTOs
{
    public class RestaurantToReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal DefaultDeliveryCost { get; set; }
        public ICollection<Category> Categories { get; set; }
    }
}
