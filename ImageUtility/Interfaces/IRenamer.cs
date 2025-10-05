using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Interfaces
{
    public interface IRenamer
    {
        Task<bool> RenameFilesAsync(List<string> sourceFiles, string destinationDir, string? pattern  = null, List<string>? extFileNamingList = null);
    }
}
