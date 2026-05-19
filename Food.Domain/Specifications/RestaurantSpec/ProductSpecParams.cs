using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Enums.Restaurant;

namespace Food.Domain.Specifications.RestaurantSpec
{
    public class ProductSpecParams
    {
        public RestaurantSort? Sort { get; set; }
        private int pageSize;

        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value > 10 ? 10 : value; }
        }
        private int pageIndex = 1;

        public int PageIndex
        {
            get { return pageIndex; }
            set { pageIndex = value; }
        }

    }
}
