using AzurePlay.Common.Models;
using System.Collections.Generic;

namespace AzurePlay.Common.Services
{
    public interface IDataAccessService
    {
        List<APMovie> GetAll();

        int Insert(APMovie movie);
    }
}