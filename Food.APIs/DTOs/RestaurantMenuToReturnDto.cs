namespace Food.APIs.DTOs
{
    public class RestaurantMenuToReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal DefaultDeliveryCost { get; set; }
        public List<CategoryWithItemsDto> Categories { get; set; } = new List<CategoryWithItemsDto>();
    }
    public class CategoryWithItemsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ItemToReturnDto> Items { get; set; } = new List<ItemToReturnDto>();
    }
}
