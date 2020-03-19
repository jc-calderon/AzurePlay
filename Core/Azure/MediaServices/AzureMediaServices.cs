using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.Storage.Blob;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Azure.MediaServices
{
    public class AzureMediaServices : IAzureMediaServices
    {
        private const string AdaptiveStreamingTransformName = "MyTransformWithAdaptiveStreamingPreset";
        public AzureMediaServicesSettings AzureMediaServicesSettings { get; set; }

        public AzureMediaServices(AzureMediaServicesSettings azureMediaServicesSettings)
        {
            this.AzureMediaServicesSettings = azureMediaServicesSettings;
        }

        public async Task<IList<string>> ProcessMovieAsync(Stream stream, string inputMP4FileName)
        {
            IList<string> urls = new List<string>();
            IAzureMediaServicesClient client = await CreateMediaServicesClientAsync();
            client.LongRunningOperationRetryTimeout = 2;

            string uniqueness = Guid.NewGuid().ToString("N");
            string jobName = $"job-{uniqueness}";
            string locatorName = $"locator-{uniqueness}";
            string outputAssetName = $"output-{uniqueness}";
            string inputAssetName = $"input-{uniqueness}";
            // Ensure that you have the desired encoding Transform. This is really a one time setup operation.
            Transform transform = await GetOrCreateTransformAsync(client, this.AzureMediaServicesSettings.ResourceGroup, this.AzureMediaServicesSettings.MediaServiceAccountName, AdaptiveStreamingTransformName);

            await CreateInputAssetAsync(client, this.AzureMediaServicesSettings.ResourceGroup, this.AzureMediaServicesSettings.MediaServiceAccountName, inputAssetName, inputMP4FileName, stream);
            Asset outputAsset = await CreateOutputAssetAsync(client, this.AzureMediaServicesSettings.ResourceGroup, this.AzureMediaServicesSettings.MediaServiceAccountName, outputAssetName);
            Job job = await SubmitJobAsync(client, this.AzureMediaServicesSettings.ResourceGroup, this.AzureMediaServicesSettings.MediaServiceAccountName, AdaptiveStreamingTransformName, jobName, inputAssetName, outputAsset.Name);
            job = await WaitForJobToFinishAsync(client, this.AzureMediaServicesSettings.ResourceGroup, this.AzureMediaServicesSettings.MediaServiceAccountName, AdaptiveStreamingTransformName, jobName);
            if (job.State == JobState.Finished)
            {
                StreamingLocator locator = await CreateStreamingLocatorAsync(client, this.AzureMediaServicesSettings.ResourceGroup, this.AzureMediaServicesSettings.MediaServiceAccountName, outputAsset.Name, locatorName);
                urls = await GetStreamingUrlsAsync(client, this.AzureMediaServicesSettings.ResourceGroup, this.AzureMediaServicesSettings.MediaServiceAccountName, locator.Name);
            }

            return urls;
        }

        private async Task<ServiceClientCredentials> GetCredentialsAsync()
        {
            ClientCredential clientCredential = new ClientCredential(this.AzureMediaServicesSettings.AadClientId, this.AzureMediaServicesSettings.AadSecret);
            return await ApplicationTokenProvider.LoginSilentAsync(this.AzureMediaServicesSettings.AadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);
        }

        private async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync()
        {
            var credentials = await GetCredentialsAsync();
            return new AzureMediaServicesClient(this.AzureMediaServicesSettings.ArmEndpoint, credentials)
            {
                SubscriptionId = this.AzureMediaServicesSettings.SubscriptionId,
            };
        }

        private async Task<Asset> CreateInputAssetAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string assetName, string fileToUpload, Stream stream)
        {
            Asset asset = await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, new Asset());
            var response = await client.Assets.ListContainerSasAsync(resourceGroupName, accountName, assetName, permissions: AssetContainerPermission.ReadWrite, expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());
            var sasUri = new Uri(response.AssetContainerSasUrls.First());
            CloudBlobContainer container = new CloudBlobContainer(sasUri);
            var blob = container.GetBlockBlobReference(Path.GetFileName(fileToUpload));
            await blob.UploadFromStreamAsync(stream);

            return asset;
        }

        private async Task<Asset> CreateOutputAssetAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string assetName)
        {
            Asset outputAsset = await client.Assets.GetAsync(resourceGroupName, accountName, assetName);
            Asset asset = new Asset();
            string outputAssetName = assetName;

            if (outputAsset != null)
            {
                string uniqueness = $"-{Guid.NewGuid().ToString("N")}";
                outputAssetName += uniqueness;

                Console.WriteLine("Warning – found an existing Asset with name = " + assetName);
                Console.WriteLine("Creating an Asset with this name instead: " + outputAssetName);
            }

            return await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, outputAssetName, asset);
        }

        private static async Task<Job> SubmitJobAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string transformName, string jobName, string inputAssetName, string outputAssetName)
        {
            JobInput jobInput = new JobInputAsset(assetName: inputAssetName);
            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAssetName),
            };

            Job job = await client.Jobs.CreateAsync(resourceGroupName, accountName, transformName, jobName,
                new Job
                {
                    Input = jobInput,
                    Outputs = jobOutputs,
                });

            return job;
        }

        private static async Task<Job> WaitForJobToFinishAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string transformName, string jobName)
        {
            const int SleepIntervalMs = 60 * 1000;
            Job job = null;
            do
            {
                job = await client.Jobs.GetAsync(resourceGroupName, accountName, transformName, jobName);

                Console.WriteLine($"Job is '{job.State}'.");
                for (int i = 0; i < job.Outputs.Count; i++)
                {
                    JobOutput output = job.Outputs[i];
                    Console.Write($"\tJobOutput[{i}] is '{output.State}'.");
                    if (output.State == JobState.Processing)
                    {
                        Console.Write($"  Progress: '{output.Progress}'.");
                    }

                    Console.WriteLine();
                }

                if (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled)
                {
                    await Task.Delay(SleepIntervalMs);
                }
            }
            while (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled);

            return job;
        }

        private async Task<StreamingLocator> CreateStreamingLocatorAsync(IAzureMediaServicesClient client, string resourceGroup, string accountName, string assetName, string locatorName)
        {
            StreamingLocator locator = await client.StreamingLocators.CreateAsync(resourceGroup, accountName, locatorName,
                new StreamingLocator
                {
                    AssetName = assetName,
                    StreamingPolicyName = PredefinedStreamingPolicy.ClearStreamingOnly
                });

            return locator;
        }

        private async Task<IList<string>> GetStreamingUrlsAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string locatorName)
        {
            const string DefaultStreamingEndpointName = "default";
            IList<string> streamingUrls = new List<string>();

            StreamingEndpoint streamingEndpoint = await client.StreamingEndpoints.GetAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);
            if (streamingEndpoint != null)
            {
                if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
                {
                    await client.StreamingEndpoints.StartAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);
                }
            }

            ListPathsResponse paths = await client.StreamingLocators.ListPathsAsync(resourceGroupName, accountName, locatorName);
            foreach (StreamingPath path in paths.StreamingPaths)
            {
                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = "https";
                uriBuilder.Host = streamingEndpoint.HostName;

                uriBuilder.Path = path.Paths[0];
                streamingUrls.Add(uriBuilder.ToString());
            }

            return streamingUrls;
        }

        /// <summary>
        /// If the specified transform exists, get that transform.
        /// If the it does not exist, creates a new transform with the specified output.
        /// In this case, the output is set to encode a video using one of the built-in encoding presets.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <returns></returns>
        // <EnsureTransformExists>
        private static async Task<Transform> GetOrCreateTransformAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName)
        {
            // Does a Transform already exist with the desired name? Assume that an existing Transform with the desired name
            // also uses the same recipe or Preset for processing content.
            Transform transform = await client.Transforms.GetAsync(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                // You need to specify what you want it to produce as an output
                TransformOutput[] output = new TransformOutput[]
                {
                    new TransformOutput
                    {
                        // The preset for the Transform is set to one of Media Services built-in sample presets.
                        // You can  customize the encoding settings by changing this to use "StandardEncoderPreset" class.
                        Preset = new BuiltInStandardEncoderPreset()
                        {
                            // This sample uses the built-in encoding preset for Adaptive Bitrate Streaming.
                            PresetName = EncoderNamedPreset.AdaptiveStreaming
                        }
                    }
                };

                // Create the Transform with the output defined above
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, output);
            }

            return transform;
        }
    }
}