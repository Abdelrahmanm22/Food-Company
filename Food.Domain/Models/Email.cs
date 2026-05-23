using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food.Domain.Models
{
    public class Email : BaseModel
    {
        public int Id { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
