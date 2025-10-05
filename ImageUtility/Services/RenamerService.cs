using ImageUtility.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Services
{
    public class RenamerService : IRenamer
    {
        public Task<bool> RenameFilesAsync(List<string> sourceFiles, string destinationDir, string? pattern = null, List<string>? extFileNamingList = null)
        {
            throw new NotImplementedException();
        }
    }
}
