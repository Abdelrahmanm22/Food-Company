using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain;
using Food.Domain.Models;
using Food.Domain.Repositories;
using Food.Repository.Data;

namespace Food.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FoodContext dbContext;
        private Hashtable _repositories;

        public UnitOfWork(FoodContext dbContext)
        {
            this.dbContext = dbContext;
            _repositories = new Hashtable();
        }
        public async Task<int> CompleteAsync()
        {
            return await dbContext.SaveChangesAsync();
        }

        public ValueTask DisposeAsync()
        {
            return dbContext.DisposeAsync();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseModel
        {
            var type = typeof(TEntity).Name; // Get the name of the entity type (e.g., "Category", "Item", etc.)
            if (!_repositories.ContainsKey(type))
            {
                var Repo = new GenericRepository<TEntity>(dbContext); // Create a new instance of the generic repository for the specified entity type.
                _repositories.Add(type, Repo); // Add the repository instance to the hashtable with the entity type name as the key.
            }
            return _repositories[type] as IGenericRepository<TEntity>; // Return the repository instance from the hashtable, cast to the appropriate type.
        }
    }
}
