using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Enums.Session;
using Food.Domain.Models.Identity;

namespace Food.Domain.Models
{
    public class Session : BaseModel
    {
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Open;
        public decimal DeliveryCost { get; set; }
        public string? Notes { get; set; }
        public string HostUserId { get; set; }
        public AppUser HostUser { get; set; }
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

        public ICollection<SessionJoin> SessionJoins { get; set; } = new List<SessionJoin>();
        public Order? Order { get; set; }
    }
}
