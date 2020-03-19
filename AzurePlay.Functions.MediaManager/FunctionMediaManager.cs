using AzurePlay.Common;
using AzurePlay.Common.Models;
using AzurePlay.Common.Services;
using AzurePlay.Functions.MediaManager.Models;
using AzurePlay.Functions.MediaManager.Services;
using Core.Azure.KeyVault;
using Core.Azure.MediaServices;
using Core.Azure.Storage;
using log4net;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzurePlay.Functions.MediaManager
{
    public class FunctionMediaManager
    {
        private readonly string tmdbImagesUrl = $"http://image.tmdb.org/t/p";
        private readonly ILog _log;
        private readonly IDataAccessService _dataAccessService;
        private readonly ITMDbApiService _tMDbApiService;
        private readonly ISignalRService _signalRService;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly IAzureStorageService _azureStorageService;
        private readonly IAzureMediaServices _azureMediaServices;

        public FunctionMediaManager(ILog log,
            IDataAccessService dataAccessService,
            ITMDbApiService tMDbApiService,
            ISignalRService signalRService,
            IAzureKeyVaultService azureKeyVaultService,
            IAzureStorageService azureStorageService,
            IAzureMediaServices azureMediaServices)
        {
            _log = log;
            _dataAccessService = dataAccessService;
            _tMDbApiService = tMDbApiService;
            _signalRService = signalRService;
            _azureKeyVaultService = azureKeyVaultService;
            _azureStorageService = azureStorageService;
            _azureMediaServices = azureMediaServices;
        }

        [FunctionName("FunctionMediaManager")]
        public async Task Run([BlobTrigger("movies/{filename}", Connection = "AzureWebJobsStorage")]Stream myBlob, string filename, ILogger log)
        {
            try
            {
                _log.Info("Get movie info from TMDb");
                var tmdbMovie = this.GetTMDbMovie(filename);
                _log.Info($"Movie found: {tmdbMovie.Title}");

                _log.Info("Downloading images from TMDb to Azure Storage");
                var urlBackdropPath = DownloadImagesToAzureStorage("backdrops", tmdbMovie.BackdropPath);
                var urlPosterPath = DownloadImagesToAzureStorage("posters", tmdbMovie.PosterPath);

                _log.Info("Mapping");
                var movie = tmdbMovie.MapTo<APMovie>();
                movie.UrlBackdropPath = urlBackdropPath;
                movie.UrlPosterPath = urlPosterPath;
                movie.GenresJson = JsonConvert.SerializeObject(movie.Genres);

                _log.Info("Preparing movie with Azure Media Services");
                var urls = await _azureMediaServices.ProcessMovieAsync(myBlob, filename);
                movie.UrlVideo = urls.LastOrDefault();

                _log.Info("Saving to DB");
                movie.AddedDate = new DateTime();
                movie.Id = _dataAccessService.Insert(movie);

                _log.Info("Sending update info to SignalR");
                await _signalRService.UpdateMovie(movie);
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
        }

        private string DownloadImagesToAzureStorage(string container, string filename)
        {
            if (filename == null)
            {
                return "";
            }

            var imageSize = TmdbImageResolution.W780.ToString().ToLower();
            filename = filename.TrimStart('/');
            string url = "";
            using (var client = new WebClient())
            {
                using (var stream = new MemoryStream(client.DownloadData($"{tmdbImagesUrl}/{imageSize}/{filename}")))
                {
                    url = _azureStorageService.UploadFromStream(stream, container, $"{imageSize}_{filename}");
                };
            }

            return url;
        }

        private TMDbMovie GetTMDbMovie(string filename)
        {
            int? year;
            var potentialTitle = this.GetMediaTitle(Path.GetFileNameWithoutExtension(filename), out year);
            var parameters = new TMDbMoviesParamaters
            {
                ApiKey = _azureKeyVaultService.GetSecret(AzureKeyVaultKeys.TMDbApiKey),
                Query = potentialTitle,
                Year = year,
            };

            var tmdbMoviesSearch = Task.Run(() => _tMDbApiService.GetMovies(parameters)).ConfigureAwait(false).GetAwaiter().GetResult();
            var tmdbMovieSearch = tmdbMoviesSearch.Results.FirstOrDefault();
            var tmdbMovie = Task.Run(() => _tMDbApiService.GetMovie(tmdbMovieSearch.Id, parameters)).ConfigureAwait(false).GetAwaiter().GetResult();

            return tmdbMovie;
        }

        private string GetMediaTitle(string potentialTitle, out int? year)
        {
            year = null;
            Regex regex = new Regex(@"\(\d{4}\)");
            Match match = regex.Match(potentialTitle);
            if (match.Success)
            {
                year = int.Parse(match.Value.Trim('(', ')'));
            }

            regex = new Regex(@"[^(]+\(");
            match = regex.Match(potentialTitle);
            string title;
            if (match.Success)
            {
                title = match.Value;
            }
            else
            {
                title = potentialTitle;
            }

            Regex rgx = new Regex(@"[\s|.|_|\(]");
            title = rgx.Replace(title, " ").Trim();

            return title;
        }
    }
}