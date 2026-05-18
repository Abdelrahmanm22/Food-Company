using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;
using Food.Domain.Specifications;

namespace Food.Domain.Repositories
{
    public interface IGenericRepository<T> where T : BaseModel
    {
        Task<IEnumerable<T>> GetAllAsync(ISpecifications<T> Spec);
        Task<T> GetByIdAsync(ISpecifications<T> Spec);
    }
}
