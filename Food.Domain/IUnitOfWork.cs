using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;
using Food.Domain.Repositories;

namespace Food.Domain
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseModel; // Generic method to get repository for any entity type
        Task<int> CompleteAsync();
    }
}
