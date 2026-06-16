
using System.Text;
using System.Threading.Tasks;
using Food.APIs.Errors;
using Food.APIs.Helpers;
using Food.APIs.Middlewares;
using Food.Domain;
using Food.Domain.Models.Identity;
using Food.Domain.Repositories;
using Food.Domain.Services;
using Food.Repository;
using Food.Repository.Data;
using Food.Service;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;

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
                    // Prevent circular reference errors (e.g. Restaurant → Categories → Restaurant)
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<FoodContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            // Hangfire — background job processing
            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddHangfireServer();
            builder.Services.AddSingleton<IConnectionMultiplexer>(Options =>
            {
                var Connection = builder.Configuration.GetConnectionString("RedisConnection");
                return ConnectionMultiplexer.Connect(Connection);
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
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ISessionService, SessionService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IRedisCartService, RedisCartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddIdentity<AppUser, IdentityRole>() 
                .AddEntityFrameworkStores<FoodContext>();
            builder.Services.AddAuthentication(Options =>
            {
                Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(Options =>
                {
                    Options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["JWT:ValidAudience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                    };
                }); //UserManager / SigninManager / RoleManager
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:8080")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            builder.Services.AddScoped<ITokenService, TokenService>();
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
                await FoodContextSeed.SeedAsync(DbContext);
                await AppIdentityDbContextSeed.SeedUserAsync(userManager, roleManager);
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
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHangfireDashboard("/hangfire");
            app.UseStaticFiles();


            app.MapControllers();
            #endregion
            app.Run();
        }
    }
}
