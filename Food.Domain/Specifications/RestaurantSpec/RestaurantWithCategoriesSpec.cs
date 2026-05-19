using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Specifications.RestaurantSpec
{
    public class RestaurantWithCategoriesSpec : BaseSpecifications<Restaurant>
    {
        public RestaurantWithCategoriesSpec(string? Sort) : base()
        {
            Includes.Add(R => R.Categories); // Include the Categories navigation property when retrieving Restaurant entities.
            if(!string.IsNullOrEmpty(Sort))
            {
                switch (Sort)
                {
                    case "NameAsc":
                        SetOrderBy(R => R.Name);
                        break;
                    case "NameDesc":
                        SetOrderByDesc(R => R.Name);
                        break;
                    default:
                        SetOrderBy(R => R.Id);
                        break;
                }
            }
        }
        
        public RestaurantWithCategoriesSpec(int id): base(R => R.Id == id)
        {
            Includes.Add(R => R.Categories);
        }
    }
}
