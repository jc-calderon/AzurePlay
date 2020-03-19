using Microsoft.Extensions.DependencyInjection;

namespace Core.DI
{
    public interface IModule
    {
        void Register(IServiceCollection serviceCollection);
    }
}