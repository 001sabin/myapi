using Microsoft.EntityFrameworkCore;

namespace myapi.ExtensionMethods
{
    public static class SqlServiceExtension
    {
        public static IServiceCollection AddSqlServiceExtension(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<Data.AppDbContext>(options =>
                options.UseSqlServer(connectionString));
            return services;
        }
    }
}
