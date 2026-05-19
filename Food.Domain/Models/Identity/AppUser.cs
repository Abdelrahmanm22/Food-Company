using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Food.Domain.Models.Identity
{
    public class AppUser : IdentityUser
    {
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
