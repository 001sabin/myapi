using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using myapi.Data;
using myapi.Model;

namespace myapi.ExtensionMethods
{
    public static class IdentityServiceExtension
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {

            // 1. Setup AuthDbContext for PostgreSQL 
            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("IdentityConnection")));

            // 2. Setup Identity Systems
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password requirements (Conceptual)
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AuthDbContext>() // Tell Identity to use AuthDbContext
            .AddDefaultTokenProviders();

            return services;

        }
    }
}
