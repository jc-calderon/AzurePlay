using AzurePlay.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzurePlay.Web.Models
{
    public class Movies
    {
        public IEnumerable<APMovie> All { get; set; }
        public IEnumerable<APMovie> RecentlyAdded { get; set; }
    }
}