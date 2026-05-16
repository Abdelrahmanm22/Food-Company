
using System.Threading.Tasks;
using Food.Repository.Data;
using Microsoft.EntityFrameworkCore;

namespace Food.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure Services
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<FoodContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            #endregion
            var app = builder.Build();


            #region Update-Database-on-Startup
            using var Scope = app.Services.CreateScope();
            var Services = Scope.ServiceProvider;
            var DbContext = Services.GetRequiredService<FoodContext>();
            await DbContext.Database.MigrateAsync();
            #endregion

            #region Configure the Http request pipeline.

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            #endregion
            app.Run();
        }
    }
}
