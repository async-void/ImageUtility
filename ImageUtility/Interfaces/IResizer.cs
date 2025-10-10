using ImageUtility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Interfaces
{
    public interface IResizer
    {
        Task<Result<string, string>> ResizeImagesAsync(IEnumerable<string> sourcePaths, string destinationDir, int width, int height, string resizeMode, bool maintainAspectRatio, bool removeOriginal);
    }
}
