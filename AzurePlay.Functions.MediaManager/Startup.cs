using AzurePlay.Common.Models;
using AzurePlay.Common.Services;
using AzurePlay.Functions.MediaManager;
using AzurePlay.Functions.MediaManager.Services;
using Core.Azure;
using Core.Azure.KeyVault;
using Core.Azure.MediaServices;
using Core.Configurations;
using Core.DI;
using Core.Logging;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;
using System;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzurePlay.Functions.MediaManager
{
    public class Startup : FunctionsStartup
    {
        private IAzureKeyVaultService _azureKeyVaultService;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            IServiceCollection serviceCollection = builder.Services;
            Configuration.SetConfiguration(Environment.CurrentDirectory, "local.settings.json");
            this.ConfigureAzureKeyVault();

            var azureModuleSettings = this.GetAzureModuleSettings();
            serviceCollection.RegisterModule(new AzureModule(azureModuleSettings));
            serviceCollection.RegisterModule(new LoggingModule(new LogglySettings { LogglyCustomerToken = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.LogglyCustomerToken), LogglyTag = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.LogglyTag) }));
            var tmdb = RestService.For<ITMDbApiService>("http://api.themoviedb.org/3");
            serviceCollection.AddSingleton(tmdb);
            var signalRService = RestService.For<ISignalRService>("https://<your domain>.azurewebsites.net");
            serviceCollection.AddSingleton(signalRService);
            serviceCollection.AddSingleton<IDataAccessService>(x => new DataAccessService(_azureKeyVaultService.GetSecret(AzureKeyVaultKeys.DbConnectionString)));
            serviceCollection.BuildAppServiceProvider();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter() }
            };
        }

        private void ConfigureAzureKeyVault()
        {
            _azureKeyVaultService = new AzureKeyVaultService(this.GetAzureKeyVaultSettings());
        }

        private AzureKeyVaultSettings GetAzureKeyVaultSettings()
        {
            return new AzureKeyVaultSettings
            {
                VaultBaseUrl = Configuration.ConfigRoot["KeyVaultBaseUrl"],
                ApplicationId = Configuration.ConfigRoot["KeyVaultApplicationId"],
                ClientSecret = Configuration.ConfigRoot["KeyVaultClientSecret"],
                CertificateThumbprint = Configuration.ConfigRoot["KeyVaultCertificateThumbprint"]
            };
        }

        private AzureModuleSettings GetAzureModuleSettings()
        {
            return new AzureModuleSettings
            {
                AzureKeyVaultSettings = this.GetAzureKeyVaultSettings(),
                AzureMediaServicesSettings = new AzureMediaServicesSettings
                {
                    AadTenantId = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.AadTenantId),
                    AadClientId = Configuration.ConfigRoot["KeyVaultApplicationId"],
                    AadSecret = Configuration.ConfigRoot["KeyVaultClientSecret"],
                    MediaServiceAccountName = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.MediaServiceAccountName),
                    Region = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.Region),
                    ResourceGroup = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.ResourceGroup),
                    SubscriptionId = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.SubscriptionId)
                },
                AzureStorageConnectionString = Configuration.ConfigRoot["AzureWebJobsStorage"],
            };
        }
    }
}