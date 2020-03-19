namespace Core.Azure.KeyVault
{
    public interface IAzureKeyVaultService
    {
        AzureKeyVaultSettings AzureKeyVaultSettings { get; set; }

        string GetSecret(string secretName);
    }
}