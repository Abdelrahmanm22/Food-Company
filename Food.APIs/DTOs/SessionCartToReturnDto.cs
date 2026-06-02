namespace Food.APIs.DTOs
{
    public class SessionCartToReturnDto
    {
        public string Id { get; set; }
        public List<CartItemToReturnDto> Items { get; set; } = new List<CartItemToReturnDto>();
        public decimal Total { get; set; }
    }
    public class CartItemToReturnDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }
}
