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
            IncludeStrings.Add("SessionJoins.User");
            IncludeStrings.Add("Order.OrderDetails");
            IncludeStrings.Add("Order.OrderDetails.Item");
            IncludeStrings.Add("Order.OrderDetails.User");
        }
        // Constructor for listing/filtering sessions
        public SessionWithDetailsSpec(SessionSpecParams specParams)
            : base(s=>
                (!specParams.Status.HasValue || s.Status == specParams.Status.Value) &&
                (!specParams.RestaurantId.HasValue || s.RestaurantId == specParams.RestaurantId.Value)
            )
        {
            Includes.Add(s => s.Restaurant);
            Includes.Add(s => s.HostUser);
            Includes.Add(s => s.SessionJoins);
            IncludeStrings.Add("SessionJoins.User");

            SetOrderByDesc(s => s.StartDate);

            ApplyPagination((specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);
        }
    }
}
