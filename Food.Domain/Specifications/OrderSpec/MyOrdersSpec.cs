using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Specifications.OrderSpec
{
    public class MyOrdersSpec : BaseSpecifications<Order>
    {
        public MyOrdersSpec(string userId) : base(o =>
            o.OrderDetails.Any(od => od.UserId == userId) ||
            o.Session.HostUserId == userId)
        {
            AddCommonIncludes();
            SetOrderByDesc(o => o.OrderDate);
        }

        private void AddCommonIncludes()
        {
            Includes.Add(o => o.Session);
            Includes.Add(o => o.OrderDetails);
            IncludeStrings.Add("Session.Restaurant");
            IncludeStrings.Add("Session.HostUser");
            IncludeStrings.Add("Session.SessionJoins");
            IncludeStrings.Add("OrderDetails.Item");
            IncludeStrings.Add("OrderDetails.User");
        }
    }
}
