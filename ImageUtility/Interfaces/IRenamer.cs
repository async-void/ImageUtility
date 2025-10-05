using ImageUtility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUtility.Interfaces
{
    public interface IRenamer
    {
        Task<Result<string, string>> RenameFilesAsync(IEnumerable<string> sourcePaths, string destinationDir, bool copyFiles, string? pattern = null, IEnumerable<string>? renameStrings = null);
    }
}
