namespace Core.Azure.KeyVault
{
    public class AzureKeyVaultSettings
    {
        public string VaultBaseUrl { get; set; }
        public string ApplicationId { get; set; }
        public string ClientSecret { get; set; }
        public string CertificateThumbprint { get; set; }
    }
}