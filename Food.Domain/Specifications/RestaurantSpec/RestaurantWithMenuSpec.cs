using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Specifications.RestaurantSpec
{
    public class RestaurantWithMenuSpec : BaseSpecifications<Restaurant>
    {
        public RestaurantWithMenuSpec(int id): base(r => r.Id == id)
        {
            Includes.Add(r => r.Categories);
            IncludeStrings.Add("Categories.Items");
        }
    }
}
