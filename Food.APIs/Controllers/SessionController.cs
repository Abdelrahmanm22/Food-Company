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

        public SessionController(UserManager<AppUser> userManager,ISessionService sessionService,IUnitOfWork unitOfWork,IMapper mapper)
        {
            _userManager = userManager;
            _sessionService = sessionService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if(string.IsNullOrEmpty(email)) return null;
            return await _userManager.FindByEmailAsync(email);
        }
        [HttpPost]
        [ProducesResponseType(typeof(SessionToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSession([FromForm] CreateSessionDto model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized(new ApiErrorResponse(401));
            try
            {
                var session = await _sessionService.CreateSessionAsync(user.Id, model.RestaurantId, model.Notes);
                var spec = new SessionWithDetailsSpec(session.Id);
                var fullSession = await _unitOfWork.Repository<Session>().GetByIdAsync(spec);
                var mapped = _mapper.Map<Session, SessionToReturnDto>(fullSession!);
                return Ok(mapped);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorResponse(400, ex.Message));
            }
        }
    }
}
