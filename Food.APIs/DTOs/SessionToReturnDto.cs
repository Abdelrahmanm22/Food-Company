namespace Food.APIs.DTOs
{
    public class SessionToReturnDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public decimal DeliveryCost { get; set; }
        public string? Notes { get; set; }

        public string HostUserId { get; set; }
        public string HostUserName { get; set; }

        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }

        public ICollection<SessionJoinToReturnDto> Participants { get; set; } = new List<SessionJoinToReturnDto>();
    }
}
