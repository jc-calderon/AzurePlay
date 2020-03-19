using Core.Azure.KeyVault;
using Core.Azure.MediaServices;

namespace Core.Azure
{
    public class AzureModuleSettings
    {
        public AzureKeyVaultSettings AzureKeyVaultSettings { get; set; }
        public AzureMediaServicesSettings AzureMediaServicesSettings { get; set; }
        public string AzureStorageConnectionString { get; set; }
    }
}