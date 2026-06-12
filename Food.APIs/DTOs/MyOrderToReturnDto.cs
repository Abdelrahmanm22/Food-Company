namespace Food.APIs.DTOs
{
    public class MyOrderToReturnDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DeliveryCost { get; set; }
        public decimal DeliveryCostPerPerson { get; set; }
        public decimal MyItemsTotal { get; set; }
        public decimal MyGrandTotal { get; set; }
        public int SessionId { get; set; }
        public string RestaurantName { get; set; }
        public string HostUserName { get; set; }
        public List<OrderDetailToReturnDto> MyItems { get; set; } = new List<OrderDetailToReturnDto>();
    }
}
