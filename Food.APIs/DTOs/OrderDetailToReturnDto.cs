namespace Food.APIs.DTOs
{
    public class OrderDetailToReturnDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
