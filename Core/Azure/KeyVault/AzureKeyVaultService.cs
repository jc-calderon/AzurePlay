using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;

namespace Core.Azure.KeyVault
{
    public class AzureKeyVaultService : IAzureKeyVaultService
    {
        public AzureKeyVaultSettings AzureKeyVaultSettings { get; set; }

        public AzureKeyVaultService(AzureKeyVaultSettings azureKeyVaultSettings)
        {
            this.AzureKeyVaultSettings = azureKeyVaultSettings;
        }

        public string GetSecret(string secretName)
        {
            using (var keyVaultClient = new KeyVaultClient(this.AuthenticationCallback))
            {
                var secret = Task.Run(() => keyVaultClient.GetSecretAsync(this.AzureKeyVaultSettings.VaultBaseUrl, secretName)).ConfigureAwait(false).GetAwaiter().GetResult();
                return secret.Value;
            }
        }

        private async Task<string> AuthenticationCallback(string authority, string resource, string scope)
        {
            var adCredential = new ClientCredential(this.AzureKeyVaultSettings.ApplicationId, this.AzureKeyVaultSettings.ClientSecret);
            var authenticationContext = new AuthenticationContext(authority, null);
            return (await authenticationContext.AcquireTokenAsync(resource, adCredential)).AccessToken;
        }
    }
}