
using System.Threading.Tasks;
using Food.APIs.Errors;
using Food.APIs.Helpers;
using Food.APIs.Middlewares;
using Food.Domain.Models.Identity;
using Food.Domain.Repositories;
using Food.Repository;
using Food.Repository.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Food.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()                    // Keep console for development 
                .WriteTo.File(
                    path: "Logs/log-.txt",            // Daily file name pattern
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,       // Keep last 30 days
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB per file
                    rollOnFileSizeLimit: true,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog(); //Use Serilog as the main logging system for the entire ASP.NET Core application.

            #region Configure Services
            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions
                    .Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()); //to convert enum to string in json response instead of int value..
                });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<FoodContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddAutoMapper(typeof(MappingProfiles));

            #region validation error
            builder.Services.Configure<ApiBehaviorOptions>(Options =>
            {
                Options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState
                                                .Where(p => p.Value.Errors.Count() > 0)
                                                .SelectMany(p => p.Value.Errors)
                                                .Select(e => e.ErrorMessage)
                                                .ToList();

                    var ValidationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };
                    return new BadRequestObjectResult(ValidationErrorResponse);
                };
            });
            #endregion
            builder.Services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<FoodContext>();
            builder.Services.AddAuthentication(); 
            #endregion
            var app = builder.Build();


            #region Update-Database-on-Startup
            using var Scope = app.Services.CreateScope();
            var Services = Scope.ServiceProvider;
            try
            {
                var DbContext = Services.GetRequiredService<FoodContext>();
                await DbContext.Database.MigrateAsync(); //Udpate-Database on Startup

                #region Data Seeding
                var roleManager = Services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = Services.GetRequiredService<UserManager<AppUser>>();
                await AppIdentityDbContextSeed.SeedUserAsync(userManager, roleManager);
                await FoodContextSeed.SeedAsync(DbContext);
                #endregion
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while applying database migrations.");
            }
            #endregion

            #region Configure the Http request pipeline.

            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMiddleware<ExceptionMiddleware>();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStatusCodePagesWithReExecute("/errors/{0}"); //redirect to EndPointNotFound Controller when user access endpoint not found..
            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseStaticFiles();


            app.MapControllers();
            #endregion
            app.Run();
        }
    }
}
