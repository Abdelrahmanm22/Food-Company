namespace Food.APIs.DTOs
{
    public class RestaurantToReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal DefaultDeliveryCost { get; set; }
        // Using DTO instead of domain model to avoid circular reference (Restaurant → Category → Restaurant)
        public ICollection<CategoryToReturnDto> Categories { get; set; } = new List<CategoryToReturnDto>();
    }

    public class CategoryToReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
