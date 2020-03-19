using AzurePlay.Common.Models;
using Refit;
using System.Threading.Tasks;

namespace AzurePlay.Functions.MediaManager.Services
{
    public interface ISignalRService
    {
        [Post("/api/movies")]
        Task UpdateMovie([Body] APMovie movie);
    }
}