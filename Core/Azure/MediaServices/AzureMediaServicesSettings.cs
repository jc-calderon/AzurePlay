using System;

namespace Core.Azure.MediaServices
{
    public class AzureMediaServicesSettings
    {
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string MediaServiceAccountName { get; set; }
        public string AadTenantId { get; set; }
        public string AadClientId { get; set; }
        public string AadSecret { get; set; }
        public Uri ArmAadAudience { get; set; } = new Uri("https://management.core.windows.net/");
        public Uri AadEndpoint { get; set; } = new Uri("https://login.microsoftonline.com");
        public Uri ArmEndpoint { get; set; } = new Uri("https://management.azure.com/");
        public string Region { get; set; } = "East US";
    }
}