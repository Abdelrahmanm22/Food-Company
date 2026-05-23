using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models.Identity;

namespace Food.Domain.Models
{
    public class SessionJoin : BaseModel
    {
        public int SessionId { get; set; }
        public Session Session { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
