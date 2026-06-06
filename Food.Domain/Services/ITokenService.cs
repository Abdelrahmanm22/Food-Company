using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.DTOs;
using Food.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace Food.Domain.Services
{
    public interface ITokenService
    {
        // function to create token
        public Task<string> CreateAccessTokenAsync(AppUser user,UserManager<AppUser> userManager);
        RefreshToken CreateRefreshToken();
        Task<TokenResultDto> RefreshTokenAsync(string refreshToken, UserManager<AppUser> userManager);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, UserManager<AppUser> userManager);
    }
}
