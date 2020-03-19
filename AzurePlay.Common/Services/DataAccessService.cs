using AzurePlay.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AzurePlay.Common.Services
{
    public class DataAccessService : IDataAccessService
    {
        public string ConnectionString { get; set; }

        public DataAccessService(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public List<APMovie> GetAll()
        {
            List<APMovie> movies = new List<APMovie>();
            using (var conn = new SqlConnection(this.ConnectionString))
            {
                conn.Open();
                var command = new SqlCommand("SELECT * FROM Movies", conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        APMovie movie = new APMovie
                        {
                            Id = int.Parse(reader[nameof(movie.Id)].ToString()),
                            Title = reader[nameof(movie.Title)].ToString(),
                            Overview = reader[nameof(movie.Overview)].ToString(),
                            ReleaseDate = DateTime.Parse(reader[nameof(movie.ReleaseDate)].ToString()),
                            AddedDate = DateTime.Parse(reader[nameof(movie.AddedDate)].ToString()),
                            GenresJson = reader[nameof(movie.GenresJson)].ToString(),
                            OriginalTitle = reader[nameof(movie.OriginalTitle)].ToString(),
                            OriginalLanguage = reader[nameof(movie.OriginalLanguage)].ToString(),
                            UrlBackdropPath = reader[nameof(movie.UrlBackdropPath)].ToString(),
                            UrlPosterPath = reader[nameof(movie.UrlPosterPath)].ToString(),
                            UrlVideo = reader[nameof(movie.UrlVideo)].ToString(),
                            Runtime = int.Parse(reader[nameof(movie.Runtime)].ToString())
                        };

                        movie.Genres = JsonConvert.DeserializeObject<APGenre[]>(movie.GenresJson);
                        movies.Add(movie);
                    }
                }

                return movies;
            }
        }

        public int Insert(APMovie movie)
        {
            using (var conn = new SqlConnection(this.ConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Movies (Title, Overview, ReleaseDate, AddedDate, GenresJson, OriginalTitle, OriginalLanguage, UrlBackdropPath, UrlPosterPath, UrlVideo, RunTime)" +
                                         "OUTPUT INSERTED.ID VALUES (@Title, @Overview, @ReleaseDate, @AddedDate, @GenresJson, @OriginalTitle, @OriginalLanguage, @UrlBackdropPath, @UrlPosterPath, @UrlVideo, @RunTime); SELECT SCOPE_IDENTITY()", conn);

                cmd.Parameters.AddWithValue("@Title", movie.Title ?? string.Empty);
                cmd.Parameters.AddWithValue("@Overview", movie.Overview ?? string.Empty);
                cmd.Parameters.AddWithValue("@ReleaseDate", movie.ReleaseDate ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@AddedDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@GenresJson", movie.GenresJson ?? string.Empty);
                cmd.Parameters.AddWithValue("@OriginalTitle", movie.OriginalTitle ?? string.Empty);
                cmd.Parameters.AddWithValue("@OriginalLanguage", movie.OriginalLanguage ?? string.Empty);
                cmd.Parameters.AddWithValue("@UrlBackdropPath", movie.UrlBackdropPath ?? string.Empty);
                cmd.Parameters.AddWithValue("@UrlPosterPath", movie.UrlPosterPath ?? string.Empty);
                cmd.Parameters.AddWithValue("@UrlVideo", movie.UrlVideo ?? string.Empty);
                cmd.Parameters.AddWithValue("@Runtime", movie.Runtime ?? 0);

                return (int)cmd.ExecuteScalar();
            }
        }
    }
}