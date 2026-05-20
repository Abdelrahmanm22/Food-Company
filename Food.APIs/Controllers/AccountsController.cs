using Food.APIs.DTOs;
using Food.APIs.Errors;
using Food.Domain.Models.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    public class AccountsController : APIBaseController
    {
        private readonly UserManager<AppUser> userManager;

        public AccountsController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
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
                Token = "This is a token for " + User.UserName
            };
            return ReturnedUser;
        }
    }
}
