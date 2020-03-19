using Refit;

namespace AzurePlay.Functions.MediaManager.Models
{
    public class TMDbMoviesParamaters
    {
        [AliasAs("api_key")]
        public string ApiKey { get; set; }

        [AliasAs("query")]
        public string Query { get; set; }

        [AliasAs("year")]
        public int? Year { get; set; }

        [AliasAs("language")]
        public string Language { get; set; } = "es";
    }
}