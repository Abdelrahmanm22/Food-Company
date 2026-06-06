namespace Food.APIs.DTOs
{
    public class AuthResponseDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
