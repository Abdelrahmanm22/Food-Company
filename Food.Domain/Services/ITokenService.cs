using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace Food.Domain.Services
{
    public interface ITokenService
    {
        // function to create token
        public Task<string> CreateTokenAsync(AppUser user,UserManager<AppUser> userManager);
    }
}
