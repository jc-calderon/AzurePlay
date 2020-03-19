using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Azure.MediaServices
{
    public interface IAzureMediaServices
    {
        Task<IList<string>> ProcessMovieAsync(Stream stream, string inputMP4FileName);
    }
}