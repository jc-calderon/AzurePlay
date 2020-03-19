using Microsoft.Extensions.Configuration;

namespace Core.Configurations
{
    public class Configuration
    {
        public static IConfigurationRoot ConfigRoot { get; set; }

        public static void SetConfiguration(string basePath, string path)
        {
            ConfigRoot = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(path, optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        }
    }
}