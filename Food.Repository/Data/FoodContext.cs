using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Food.Domain.Models;
using Food.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Food.Repository.Data
{
    public class FoodContext : IdentityDbContext<AppUser>
    {
        public FoodContext(DbContextOptions<FoodContext> options):base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<SessionJoin> SessionJoins { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    }
}
