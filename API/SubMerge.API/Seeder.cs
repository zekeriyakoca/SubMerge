using SubMerge.API.Repositories;

namespace SubMerge.API
{
    public static class Seeder
    {
        public static Task Seed(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IDataInitializer, BlobStorageRepository>();

            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetServices<IDataInitializer>().ToList().ForEach(initializer => initializer.Initialize().GetAwaiter().GetResult());

            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IDataInitializer));
            services.Remove(serviceDescriptor);
            return Task.CompletedTask;
        }
    }
}
