using AzurePlay.Common.Models;
using AzurePlay.Functions.MediaManager.Models;
using Refit;
using System.Threading.Tasks;

namespace AzurePlay.Functions.MediaManager.Services
{
    public interface ITMDbApiService
    {
        [Get("/search/movie")]
        Task<TMDbMoviesSearch> GetMovies(TMDbMoviesParamaters parameters);

        [Get("/movie/{id}")]
        Task<TMDbMovie> GetMovie(int id, TMDbMoviesParamaters parameters);
    }
}