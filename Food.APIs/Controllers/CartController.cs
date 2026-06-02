using System.Security.Claims;
using AutoMapper;
using Food.APIs.DTOs;
using Food.APIs.Errors;
using Food.Domain;
using Food.Domain.Enums.Session;
using Food.Domain.Models;
using Food.Domain.Models.Identity;
using Food.Domain.Services;
using Food.Domain.Specifications;
using Food.Domain.Specifications.SessionSpec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    [Authorize]
    public class CartController : APIBaseController
    {
        private readonly IRedisCartService _cartService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public CartController(IRedisCartService cartService,IMapper mapper,UserManager<AppUser> userManager,IUnitOfWork unitOfWork)
        {
            _cartService = cartService;
            _mapper = mapper;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }
        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;
            return await _userManager.FindByEmailAsync(email);
        }
        //Get current user's cart for a session
        [HttpGet("{sessionId}")]
        [ProducesResponseType(typeof(SessionCartToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCart(int sessionId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new ApiErrorResponse(401));

            var cartKey = $"cart:{sessionId}:{user.Id}";
            var cart = await _cartService.GetCartAsync(cartKey);
            if(cart == null) return NotFound(new ApiErrorResponse(404, "Cart not found. Make sure you have joined the session."));
            var cartDto = _mapper.Map<SessionCart, SessionCartToReturnDto>(cart);
            return Ok(cartDto);
        }
        //Update current user's cart items (≥1 item required)
        [HttpPut("{sessionId}")]
        [ProducesResponseType(typeof(SessionCartToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCart(int sessionId, [FromBody] UpdateCartDto model)
        {
            var user = await GetCurrentUserAsync();
            if(user == null) return Unauthorized(new ApiErrorResponse(401));

            var sessionSpec = new BaseSpecifications<Session>(s => s.Id == sessionId);
            var session = await _unitOfWork.Repository<Session>().GetByIdAsync(sessionSpec);
            if(session == null) return NotFound(new ApiErrorResponse(404, "Session not found"));
            if(session.Status!= SessionStatus.Open) return BadRequest(new ApiErrorResponse(400, "Session is not open"));

            var joinSpec = new SessionJoinSpec(sessionId, user.Id);
            var joinRecord = await _unitOfWork.Repository<SessionJoin>().GetByIdAsync(joinSpec);
            if (joinRecord == null)
            {
                return BadRequest(new ApiErrorResponse(400, "You must join the session before updating your cart."));
            }

            // Validate that all item IDs in the request exist in the database and belong to the session's restaurant
            var itemIds = model.Items.Select(i => i.ItemId).ToList();
            var itemSpec = new BaseSpecifications<Item>(i => itemIds.Contains(i.Id));
            itemSpec.Includes.Add(i => i.Category);
            var dbItemsList = await _unitOfWork.Repository<Item>().GetAllAsync(itemSpec);
            var dbItems = dbItemsList.ToList();

            if (dbItems.Count != itemIds.Distinct().Count())
            {
                return BadRequest(new ApiErrorResponse(400, "One or more items were not found."));
            }
            // Check that each item belongs to the restaurant and is available
            foreach (var dbItem in dbItems)
            {
                if (dbItem.Category == null || dbItem.Category.RestaurantId != session.RestaurantId)
                {
                    return BadRequest(new ApiErrorResponse(400, $"Item '{dbItem.Name}' does not belong to this restaurant."));
                }
                if (!dbItem.IsAvailable)
                {
                    return BadRequest(new ApiErrorResponse(400, $"Item '{dbItem.Name}' is not currently available."));
                }
            }

            var cartItems = new List<CartItem>();
            foreach(var item in model.Items)
            {
                var dbItem = dbItems.First(i => i.Id == item.ItemId);
                cartItems.Add(new CartItem
                {
                    ItemId = item.ItemId,
                    ItemName = dbItem.Name,
                    Price = dbItem.Price,
                    Quantity = item.Quantity
                });
            }
            var cart = new SessionCart
            {
                Id = $"cart:{sessionId}:{user.Id}",
                Items = cartItems
            };
            
            var updatedCart = await _cartService.UpdateCartAsync(cart);
            var updatedCartDto = _mapper.Map<SessionCart, SessionCartToReturnDto>(updatedCart!);
            return Ok(updatedCartDto);
        }

    }
}
