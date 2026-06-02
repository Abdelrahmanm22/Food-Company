using System.ComponentModel.DataAnnotations;

namespace Food.APIs.DTOs
{
    public class CartItemDto
    {
        [Required]
        public int ItemId { get; set; }
        [Required]
        [Range(1,100, ErrorMessage = "Quantity must be between 1 and 100.")]
        public int Quantity { get; set; }
    }
}
