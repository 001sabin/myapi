namespace myapi.ExtensionMethods
{
    public static class RedisServiceExtension
    {
        public static IServiceCollection AddRedisServiceExtension(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("RedisConnection");
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
            return services;
        }
    }
}
