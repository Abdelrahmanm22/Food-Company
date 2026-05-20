using Food.APIs.DTOs;
using Food.APIs.Errors;
using Food.Domain.Models.Identity;
using Food.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<UserDto>> Register(RegisterDto model)
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

            var ReturnedUser = new UserDto()
            {
                UserName = User.UserName,
                Email = User.Email,
                Token = await tokenService.CreateTokenAsync(User, userManager)
            };
            return ReturnedUser;
        } 
        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var User = await userManager.FindByEmailAsync(model.Email);
            if (User is null) return Unauthorized(new ApiErrorResponse(401, "Invalid Email or Password"));
            var Result = await signInManager.CheckPasswordSignInAsync(User, model.Password, false);
            if(!Result.Succeeded) return Unauthorized(new ApiErrorResponse(401, "Invalid Email or Password"));
            var ReturnedUser = new UserDto()
            {
                UserName = User.UserName,
                Email = User.Email,
                Token = await tokenService.CreateTokenAsync(User, userManager)
            };
            return ReturnedUser;
        }

    }
}
