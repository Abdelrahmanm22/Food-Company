using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Enums.Restaurant;
using Food.Domain.Models;

namespace Food.Domain.Specifications.RestaurantSpec
{
    public class RestaurantWithCategoriesSpec : BaseSpecifications<Restaurant>
    {
        public RestaurantWithCategoriesSpec(ProductSpecParams Params) : base()
        {
            Includes.Add(R => R.Categories); // Include the Categories navigation property when retrieving Restaurant entities.
            if(Params.Sort.HasValue)
            {
                switch (Params.Sort)
                {
                    case RestaurantSort.NameAsc:
                        SetOrderBy(R => R.Name);
                        break;
                    case RestaurantSort.NameDesc:
                        SetOrderByDesc(R => R.Name);
                        break;
                    default:
                        SetOrderBy(R => R.Id);
                        break;
                }
            }
            #region Pagination
            // if we have 100 products 
            // and size = 10 
            // and page index = 5
            // so => skip = (5 - 1) * 10 = 40 and take = 10
            ApplyPagination((Params.PageIndex - 1) * Params.PageSize, Params.PageSize);
            #endregion
        }

        public RestaurantWithCategoriesSpec(int id): base(R => R.Id == id)
        {
            Includes.Add(R => R.Categories);
        }
    }
}
