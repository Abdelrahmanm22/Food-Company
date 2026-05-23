using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Specifications.SessionSpec
{
    public class SessionWithDetailsSpec : BaseSpecifications<Session>
    {
        // Constructor for getting a single session by ID with all details
        public SessionWithDetailsSpec(int id) : base(s => s.Id == id)
        {
            Includes.Add(s => s.Restaurant);
            Includes.Add(s => s.HostUser);
            Includes.Add(s => s.SessionJoins);
            Includes.Add(s => s.Order);
            Includes.Add(s => s.Order.OrderDetails);
            IncludeStrings.Add("SessionJoins.User");
            IncludeStrings.Add("Order.OrderDetails.Item");
            IncludeStrings.Add("Order.OrderDetails.User");
        }
    }
}
