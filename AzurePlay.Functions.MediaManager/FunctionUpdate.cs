using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;

namespace AzurePlay.Functions.MediaManager
{
    public static class FunctionUpdate
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
             [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
             [SignalRConnectionInfo(HubName = "ap")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("movies")]
        public static Task SendNotification(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] object request,
            [SignalR(HubName = "ap")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "newMovie",
                    Arguments = new[] { request }
                });
        }
    }
}