using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Enums.Session;

namespace Food.Domain.Specifications.SessionSpec
{
    public class SessionSpecParams
    {
        public SessionStatus? Status { get; set; }
        public int? RestaurantId { get; set; }
        private int pageSize = 10;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value > 20 ? 20 : value; }
        }

        public int PageIndex { get; set; } = 1;
    }
}
