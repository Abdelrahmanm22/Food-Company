using System.Security.Claims;
using AutoMapper;
using Food.APIs.DTOs;
using Food.APIs.Errors;
using Food.Domain;
using Food.Domain.Models;
using Food.Domain.Models.Identity;
using Food.Domain.Services;
using Food.Domain.Specifications.SessionSpec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    [Authorize]
    public class SessionController : APIBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ISessionService _sessionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SessionController(UserManager<AppUser> userManager, ISessionService sessionService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _sessionService = sessionService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return null;
            return await _userManager.FindByEmailAsync(email);
        }

        // GET api/Session  — list sessions (filterable by status / restaurantId)
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SessionToReturnDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllSessions([FromQuery] SessionSpecParams specParams)
        {
            var spec = new SessionWithDetailsSpec(specParams);
            var sessions = await _unitOfWork.Repository<Session>().GetAllAsync(spec);
            var mapped = _mapper.Map<IEnumerable<Session>, IEnumerable<SessionToReturnDto>>(sessions);
            return Ok(mapped);
        }

        // GET api/Session/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SessionToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSessionById(int id)
        {
            var spec = new SessionWithDetailsSpec(id);
            var session = await _unitOfWork.Repository<Session>().GetByIdAsync(spec);
            if (session == null) return NotFound(new ApiErrorResponse(404, "Session not found."));
            var mapped = _mapper.Map<Session, SessionToReturnDto>(session);
            return Ok(mapped);
        }

        // POST api/Session  — host creates a new session
        [HttpPost]
        [ProducesResponseType(typeof(SessionToReturnDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionDto model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new ApiErrorResponse(401));
            try
            {
                var session = await _sessionService.CreateSessionAsync(user.Id, model.RestaurantId, model.Notes);
                var spec = new SessionWithDetailsSpec(session.Id);
                var fullSession = await _unitOfWork.Repository<Session>().GetByIdAsync(spec);
                var mapped = _mapper.Map<Session, SessionToReturnDto>(fullSession!);
                return CreatedAtAction(nameof(GetSessionById), new { id = session.Id }, mapped);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        // POST api/Session/{id}/join  — employee joins with at least one item
        [HttpPost("{id}/join")]
        [ProducesResponseType(typeof(SessionJoinToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JoinSession(int id, [FromBody] JoinSessionDto model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new ApiErrorResponse(401));
            try
            {
                var cartItems = model.Items.Select(i => new CartItem
                {
                    ItemId = i.ItemId,
                    Quantity = i.Quantity
                }).ToList();

                var join = await _sessionService.JoinSessionAsync(id, user.Id, cartItems);
                var mapped = _mapper.Map<SessionJoin, SessionJoinToReturnDto>(join);
                return Ok(mapped);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        // DELETE api/Session/{id}/leave  — participant removes themselves + clears Redis cart
        [HttpDelete("{id}/leave")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LeaveSession(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new ApiErrorResponse(401));
            try
            {
                await _sessionService.LeaveSessionAsync(id, user.Id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }

        // PUT api/Session/{id}/close  — host stops new joins (Open → Closed)
        [HttpPut("{id}/close")]
        [ProducesResponseType(typeof(SessionToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CloseSession(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new ApiErrorResponse(401));
            try
            {
                var session = await _sessionService.CloseSessionAsync(id, user.Id);
                var spec = new SessionWithDetailsSpec(session.Id);
                var fullSession = await _unitOfWork.Repository<Session>().GetByIdAsync(spec);
                var mapped = _mapper.Map<Session, SessionToReturnDto>(fullSession!);
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

        // PUT api/Session/{id}/cancel  — host cancels the session (must be Open)
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(SessionToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CancelSession(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new ApiErrorResponse(401));
            try
            {
                var session = await _sessionService.CancelSessionAsync(id, user.Id);
                var spec = new SessionWithDetailsSpec(session.Id);
                var fullSession = await _unitOfWork.Repository<Session>().GetByIdAsync(spec);
                var mapped = _mapper.Map<Session, SessionToReturnDto>(fullSession!);
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
    }
}
