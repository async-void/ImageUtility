using ImageUtility.Enums;
using ImageUtility.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUtility.Interfaces
{
    public interface IImageConverter
    {
        ImageType SupportedType { get; }
        Task<Result<Stream, string>> ConvertAsync(Stream stream, int quality = 85, CancellationToken cancellationToken = default);

    }
}
