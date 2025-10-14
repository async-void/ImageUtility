using ImageUtility.Interfaces;
using ImageUtility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Utilities
{
    public class FileUtilities : IFileUtilities
    {
        public async Task<Result<string, string>> GetFileExtAsync(IEnumerable<string> files)
        {
            throw new NotImplementedException();
        }
    }
}
