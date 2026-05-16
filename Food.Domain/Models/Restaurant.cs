using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food.Domain.Models
{
    public class Restaurant : BaseModel
    {
        public string Name{ get; set; }
        public string Address{ get; set; }
        public decimal DefaultDeliveryCost { get; set; }
        public ICollection<Category> Categories { get; set; }
    }
}
