using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Food.Domain.Specifications
{
    public class SpecificationEvalutor<T> where T : BaseModel
    {
        //Function To build Query
        /// [ _dbContext.Products.Where(P => P.Id == id).Include(P => P.ProductType).Include(P => P.ProductBrand); ]
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecifications<T> spec)
        {
            var query = inputQuery; //_dbContext.Set<T>()
            if (spec.Criteria is not null)
            {
                query = query.Where(spec.Criteria); // _dbContext.Set<T>().Where(P => P.Id == id)
            }
            query = spec.Includes.Aggregate(query, (CurrentQuery, IncludeExpression) => CurrentQuery.Include(IncludeExpression)); ///_dbContext.Products.Where(P => P.Id == id).Include(P => P.ProductType).Include(P => P.ProductBrand);
            return query;
        }
    }
}
