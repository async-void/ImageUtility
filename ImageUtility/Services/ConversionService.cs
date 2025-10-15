using ImageUtility.Enums;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUtility.Services
{
    public class ConversionService
    {
        private readonly Dictionary<ImageType, IImageConverter> _converters;

        public ConversionService(IEnumerable<IImageConverter> converters)
        {
            _converters = converters.ToDictionary(c => c.SupportedType);
        }

        public async Task<Result<Stream, string>> ConvertAsync(ImageType type, Stream input, CancellationToken cancellationToken = default)
        {
            if (!_converters.TryGetValue(type, out IImageConverter? converter))
            {
                throw new NotSupportedException($"No converter registered for {type}");
            }

            var result = await converter.ConvertAsync(input, cancellationToken);
            return Result<Stream, string>.Ok(result.Value);
        }

    }
}
