using Newtonsoft.Json;
using System;

namespace AzurePlay.Common.Models
{
    public class TMDbMovieSearch
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}