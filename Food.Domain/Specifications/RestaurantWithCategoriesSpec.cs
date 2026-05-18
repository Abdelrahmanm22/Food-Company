using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Specifications
{
    public class RestaurantWithCategoriesSpec : BaseSpecifications<Restaurant>
    {
        public RestaurantWithCategoriesSpec() : base()
        {
            Includes.Add(R => R.Categories); // Include the Categories navigation property when retrieving Restaurant entities.
        }
        
        public RestaurantWithCategoriesSpec(int id): base(R => R.Id == id)
        {
            Includes.Add(R => R.Categories);
        }
    }
}
