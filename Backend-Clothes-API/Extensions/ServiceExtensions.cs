using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend_Clothes_API.Data;
using Backend_Clothes_API.Helpers;
using Backend_Clothes_API.Repositories;
using Backend_Clothes_API.Repositories.InterfaceRepo;
using Backend_Clothes_API.Services;

namespace Backend_Clothes_API.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Add database context with PostgreSQL
        /// </summary>
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        /// <summary>
        /// Add JWT and Cookie authentication
        /// </summary>
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"];

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwtSettings["Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    // Read token from cookies as well
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.AccessDeniedPath = "/access-denied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                });

            return services;
        }

        /// <summary>
        /// Add CORS with flexible configuration
        /// </summary>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOriginsConfig = configuration.GetSection("Cors:AllowedOrigins").Get<string>() ?? "*";
            var corsOrigins = allowedOriginsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries);

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .WithOrigins(corsOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });

                options.AddPolicy("AllowLocalhost", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return services;
        }

        /// <summary>
        /// Add application services (repositories, services, helpers)
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICartRepository, CartRepository>();

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICartService, CartService>();

            // Helpers
            services.AddScoped<JwtHelper>();

            return services;
        }

        /// <summary>
        /// Run database migrations
        /// </summary>
        public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

                try
                {
                    logger.LogInformation("Attempting to run database migrations on Supabase...");
                    await db.Database.MigrateAsync();
                    logger.LogInformation("Database migration completed successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database. Ensure the database host is accessible and credentials are correct.");
                }
            }
        }
    }
}
