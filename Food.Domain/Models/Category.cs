using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food.Domain.Models
{
    public class Category : BaseModel
    {
        public string Name { get; set; }
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        public ICollection<Item> Items { get; set; }
    }
}
