using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;
using Food.Domain.Repositories;
using Food.Repository.Data;
using Microsoft.EntityFrameworkCore;

namespace Food.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseModel
    {
        private readonly FoodContext dbContext;

        public GenericRepository(FoodContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await dbContext.Set<T>().FirstAsync(r => r.Id == id);
        }
    }
}
