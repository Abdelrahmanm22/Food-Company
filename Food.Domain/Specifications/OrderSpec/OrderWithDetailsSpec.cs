using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Specifications.OrderSpec
{
    public class OrderWithDetailsSpec : BaseSpecifications<Order>
    {
        public OrderWithDetailsSpec(int id) : base(o => o.Id == id)
        {
            AddCommonIncludes();
        }

        public OrderWithDetailsSpec(int sessionId, bool isSessionId) : base(o => o.SessionId == sessionId)
        {
            AddCommonIncludes();
        }

        private void AddCommonIncludes()
        {
            Includes.Add(o => o.Session);
            Includes.Add(o => o.OrderDetails);
            Includes.Add(o => o.User);
            IncludeStrings.Add("Session.Restaurant");
            IncludeStrings.Add("Session.HostUser");
            IncludeStrings.Add("Session.SessionJoins");
            IncludeStrings.Add("OrderDetails.Item");
            IncludeStrings.Add("OrderDetails.User");
        }
    }
}
