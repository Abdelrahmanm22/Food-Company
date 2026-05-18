using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;

namespace Food.Domain.Specifications
{
    public class BaseSpecifications<T> : ISpecifications<T> where T : BaseModel
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; } = new List<Expression<Func<T, object>>>(); // instead of repeating this line in both constructors, we can initialize it here directly. This way, it will be initialized regardless of which constructor is used.

        //Get All
        public BaseSpecifications()
        {
            //Includes = new List<Expression<Func<T, object>>>();
        }
        //Get By Id
        public BaseSpecifications(Expression<Func<T, bool>> cr)
        {
            Criteria = cr;
            //Includes = new List<Expression<Func<T, object>>>();
        }
    }
}
