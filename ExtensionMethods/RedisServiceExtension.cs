namespace myapi.ExtensionMethods
{
    public static class RedisServiceExtension
    {
        public static IServiceCollection AddRedisServiceExtension(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("RedisConnection");

            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new InvalidOperationException("Redis Connection string 'RedisConnection' not found in configuration.");
            }
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "EmployeeAPI_"; // this line puts EmployeeAPI as prefix to all the keys stored in redis cache
            });
            return services;
        }
    }
}
