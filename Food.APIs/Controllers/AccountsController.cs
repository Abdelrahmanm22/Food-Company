using System.Security.Claims;
using Food.APIs.DTOs;
using Food.APIs.Errors;
using Food.Domain.Models.Identity;
using Food.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Food.APIs.Controllers
{
    public class AccountsController : APIBaseController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ITokenService tokenService;

        public AccountsController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,ITokenService tokenService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.tokenService = tokenService;
        }
        [HttpPost("Register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto model)
        {
            var User = new AppUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                DepartmentId = model.DepartmentId ?? null
            };
            var Result = await userManager.CreateAsync(User,model.Password);
            if(!Result.Succeeded) return BadRequest( new ApiValidationErrorResponse() { Errors = Result.Errors.Select(e => e.Description).ToList() } );

            var refreshToken = tokenService.CreateRefreshToken();
            User.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(User);

            var ReturnedUser = new AuthResponseDto()
            {
                UserName = User.UserName,
                Email = User.Email,
                AccessToken = await tokenService.CreateAccessTokenAsync(User, userManager),
                RefreshToken = refreshToken.Token
            };
            return ReturnedUser;
        } 
        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto model)
        {
            var User = await userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Email == model.Email);
            if (User is null) return Unauthorized(new ApiErrorResponse(401, "Invalid Email or Password"));
            var Result = await signInManager.CheckPasswordSignInAsync(User, model.Password, false);
            if(!Result.Succeeded) return Unauthorized(new ApiErrorResponse(401, "Invalid Email or Password"));

            var refreshToken = tokenService.CreateRefreshToken();
            User.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(User);

            var ReturnedUser = new AuthResponseDto()
            {
                UserName = User.UserName,
                Email = User.Email,
                AccessToken = await tokenService.CreateAccessTokenAsync(User, userManager),
                RefreshToken = refreshToken.Token
            };
            return ReturnedUser;
        }
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout(RefreshTokenRequestDto model)
        {
            var revoked = await tokenService.RevokeRefreshTokenAsync(model.RefreshToken, userManager);
            if(!revoked) return BadRequest(new ApiErrorResponse(400, "Invalid Refresh Token"));
            return Ok(new { Message = "Logged out Successfully" });
        }
        [HttpPost("Refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenRequestDto model)
        {
            var result = await tokenService.RefreshTokenAsync(model.RefreshToken, userManager);
            if(result is null) return BadRequest(new ApiErrorResponse(400, "Invalid Refresh Token"));
            return Ok(new AuthResponseDto()
            {
                UserName = result.UserName,
                Email = result.Email,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized(new ApiErrorResponse(401));

            var user = await userManager.Users
                .Include(u=>u.Department)
                .SingleOrDefaultAsync(u => u.Email == email);
            if (user is null) return Unauthorized(new ApiErrorResponse(401));

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DepartmentName = user.Department?.Name
            });
        }
    }
}
