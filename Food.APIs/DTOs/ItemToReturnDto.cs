using Food.Domain.Models;

namespace Food.APIs.DTOs
{
    public class ItemToReturnDto 
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
    }
}
