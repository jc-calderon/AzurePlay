using Core.Azure.KeyVault;
using Core.Azure.MediaServices;
using Core.Azure.Storage;
using Core.DI;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Azure
{
    public class AzureModule : IModule
    {
        private readonly AzureModuleSettings _azureModuleSettings;

        public AzureModule(AzureModuleSettings azureModuleSettings)
        {
            this._azureModuleSettings = azureModuleSettings;
        }

        public void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IAzureKeyVaultService>(x => new AzureKeyVaultService(_azureModuleSettings.AzureKeyVaultSettings));
            serviceCollection.AddSingleton<IAzureMediaServices>(x => new AzureMediaServices(_azureModuleSettings.AzureMediaServicesSettings));
            serviceCollection.AddSingleton<IAzureStorageService>(x => new AzureStorageService(_azureModuleSettings.AzureStorageConnectionString));
        }
    }
}