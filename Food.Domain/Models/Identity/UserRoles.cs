using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food.Domain.Models.Identity
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
        public static bool TryNormalize(string? role, out string normalizedRole)
        {
            normalizedRole = string.Empty;
            if(string.IsNullOrWhiteSpace(role)) return false;

            if(string.Equals(role,Admin,StringComparison.OrdinalIgnoreCase))
            {
                normalizedRole = Admin;
                return true;
            }

            if(string.Equals(role, Employee, StringComparison.OrdinalIgnoreCase))
            {
                normalizedRole = Employee;
                return true;
            }
            return false;
        }
    }
}
