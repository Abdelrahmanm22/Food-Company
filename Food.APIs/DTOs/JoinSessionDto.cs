using System.ComponentModel.DataAnnotations;

namespace Food.APIs.DTOs
{
    public class JoinSessionDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "You must add at least one item to join")]
        public List<CartItemDto> Items { get; set; }
    }
}
