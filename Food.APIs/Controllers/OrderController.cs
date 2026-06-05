using System.Security.Claims;
using AutoMapper;
using Food.APIs.DTOs;
using Food.APIs.Errors;
using Food.Domain;
using Food.Domain.Models;
using Food.Domain.Models.Identity;
using Food.Domain.Services;
using Food.Domain.Specifications.OrderSpec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    [Authorize]
    public class OrderController : APIBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public OrderController(IOrderService orderService, IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _orderService = orderService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;
            return await _userManager.FindByEmailAsync(email);
        }

        // POST api/Order/{sessionId}/confirm
        // Host confirms the order: reads Redis carts → creates Order+OrderDetails in DB → notifies participants
        [HttpPost("{sessionId}/confirm")]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmOrder(int sessionId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new ApiErrorResponse(401));
            try
            {
                var order = await _orderService.ConfirmOrderAsync(sessionId, user.Id);
                var mapped = _mapper.Map<Order, OrderToReturnDto>(order);
                return CreatedAtAction(nameof(GetOrderById), new { orderId = order.Id }, mapped);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(403, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        // PUT api/Order/{orderId}/status
        // Host updates the order status (Confirmed → Preparing → Delivered / Cancelled)
        [HttpPut("{orderId}/status")]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new ApiErrorResponse(401));
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(orderId, model.Status, user.Id);
                var mapped = _mapper.Map<Order, OrderToReturnDto>(order);
                return Ok(mapped);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(403, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        // GET api/Order/{orderId}
        // Get a specific order with full details (all participants' items)
        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var spec = new OrderWithDetailsSpec(orderId);
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(spec);
            if (order == null) return NotFound(new ApiErrorResponse(404, "Order not found."));
            var mapped = _mapper.Map<Order, OrderToReturnDto>(order);
            return Ok(mapped);
        }

        // GET api/Order/session/{sessionId}
        // Get the order associated with a specific session
        [HttpGet("session/{sessionId}")]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderBySessionId(int sessionId)
        {
            var spec = new OrderWithDetailsSpec(sessionId, isSessionId: true);
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(spec);
            if (order == null) return NotFound(new ApiErrorResponse(404, $"No order found for session {sessionId}."));
            var mapped = _mapper.Map<Order, OrderToReturnDto>(order);
            return Ok(mapped);
        }
    }
}
