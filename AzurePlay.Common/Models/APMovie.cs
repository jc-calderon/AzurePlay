using AutoMapper;
using System;

namespace AzurePlay.Common.Models
{
    [AutoMap(typeof(TMDbMovie))]
    public class APMovie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool ShowOverview { get; set; }
        public string Overview { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime AddedDate { get; set; }
        public string OriginalTitle { get; set; }
        public string OriginalLanguage { get; set; }
        public int? Runtime { get; set; }
        public string UrlPosterPath { get; set; }
        public string UrlBackdropPath { get; set; }
        public string UrlVideo { get; set; } = "";
        public APGenre[] Genres { get; set; }
        public string GenresJson { get; set; }
    }

    [AutoMap(typeof(TMDbGenre))]
    public class APGenre
    {
        public string Name { get; set; }
    }
}