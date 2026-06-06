using System.ComponentModel.DataAnnotations;

namespace Food.APIs.DTOs
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
