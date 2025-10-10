using ImageUtility.Interfaces;
using ImageUtility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Services
{
    public class ResizerService : IResizer
    {
        public Task<Result<string, string>> ResizeImagesAsync(IEnumerable<string> sourcePaths, string destinationDir, int width, int height, string resizeMode, bool maintainAspectRatio, bool removeOriginal)
        {
            throw new NotImplementedException();
        }
    }
}
