using Microsoft.Extensions.DependencyInjection;

namespace Core.DI
{
    public static class ContainerBuilder
    {
        public static ServiceProvider ServiceProvider { get; set; }

        public static void RegisterModule(this IServiceCollection serviceCollection, IModule module)
        {
            module.Register(serviceCollection);
        }

        public static ServiceProvider BuildAppServiceProvider(this IServiceCollection serviceCollection)
        {
            ServiceProvider = serviceCollection.BuildServiceProvider();
            return ServiceProvider;
        }
    }
}