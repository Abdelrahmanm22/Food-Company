using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.DTOs;
using Food.Domain.Models.Identity;
using Food.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Food.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration configuration;

        public TokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<string> CreateAccessTokenAsync(AppUser user, UserManager<AppUser> userManager)
        {
            //Payload
            // 1. Private Claims [User - Defined]
            var AuthClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.GivenName,user.UserName),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("AspNetCore.Identity.SecurityStamp",user.SecurityStamp)
            };
            var UserRoles = await userManager.GetRolesAsync(user);
            foreach (var Role in UserRoles)
            {
                AuthClaims.Add(new Claim(ClaimTypes.Role, Role));
            }
            var AuthKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
            var Token = new JwtSecurityToken
                (
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddMinutes(double.Parse(configuration["JWT:DurationMinutes"])),
                    claims: AuthClaims,
                    signingCredentials: new SigningCredentials(AuthKey, SecurityAlgorithms.HmacSha256Signature)
                );
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }

        public RefreshToken CreateRefreshToken()
        {
            return new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)), // cryptographically secure random token
                ExpiresAt = DateTime.UtcNow.AddDays(10), // set expiration time for refresh token (e.g., 10 days)
                CreatedAt = DateTime.UtcNow // set creation time for refresh token
            };
        }
        public async Task<TokenResultDto> RefreshTokenAsync(string refreshToken, UserManager<AppUser> userManager)
        {
            var user = userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            if(user is null) return null;

            var existingToken = user.RefreshTokens.Single(t => t.Token == refreshToken);
            if (!existingToken.IsActive) return null;

            existingToken.RevokedAt = DateTime.UtcNow; // Revoke the existing refresh token

            var newRefreshToken = CreateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken); // Add the new refresh token to the user's collection

            user.RefreshTokens
                .Where(t=>!t.IsActive && t.CreatedAt < DateTime.UtcNow.AddDays(-10)) // Remove old inactive tokens (e.g., older than 10 days)
                .ToList()
                .ForEach(t=>user.RefreshTokens.Remove(t));
            await userManager.UpdateAsync(user); // Update the user to save changes to the database
            return new TokenResultDto()
            {
                UserName = user.UserName,
                Email = user.Email,
                AccessToken = await CreateAccessTokenAsync(user, userManager),
                RefreshToken = newRefreshToken.Token
            };
        }
        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, UserManager<AppUser> userManager)
        {
            var user = userManager.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == refreshToken));

            if (user is null) return false;

            var existingToken = user.RefreshTokens.Single(t => t.Token == refreshToken);

            if (!existingToken.IsActive) return false;

            existingToken.RevokedAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            return true;
        }
    }
}
