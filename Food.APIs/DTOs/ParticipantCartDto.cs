namespace Food.APIs.DTOs
{
    public class ParticipantCartDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public SessionCartToReturnDto Cart { get; set; }
    }
}
