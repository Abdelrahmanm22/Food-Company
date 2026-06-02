using System.ComponentModel.DataAnnotations;

namespace Food.APIs.DTOs
{
    public class UpdateCartDto
    {
        [Required]
        [MinLength(1,ErrorMessage ="Cart must have at least one item")]
        public List<CartItemDto> Items { get; set; }
    }
}
