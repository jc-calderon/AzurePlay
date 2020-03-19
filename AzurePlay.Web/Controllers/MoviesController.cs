using AzurePlay.Common.Models;
using AzurePlay.Common.Services;
using AzurePlay.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using log4net;

namespace AzurePlay.Web.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly ILog _log;

        public MoviesController(IDataAccessService dataAccessService, ILog log)
        {
            _dataAccessService = dataAccessService;
            _log = log;
        }

        [HttpGet]
        [Route("")]
        public ActionResult<Movies> GetAll()
        {
            Movies movies = new Movies();
            try
            {
                var allMovies = _dataAccessService.GetAll();
                movies = new Movies
                {
                    All = allMovies,
                    RecentlyAdded = allMovies.OrderByDescending(m => m.AddedDate).Take(2)
                };
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }

            return movies;
        }
    }
}