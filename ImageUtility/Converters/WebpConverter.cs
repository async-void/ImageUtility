using ImageUtility.Enums;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUtility.Converters
{
    public class WebpConverter : IImageConverter
    {
        public ImageType SupportedType => ImageType.WEBP;

        public async Task<Result<Stream, string>> ConvertAsync(Stream input, CancellationToken cancellationToken = default)
        {
            if (input is null) return Result<Stream, string>.Err("input stream is null");
            var output = new MemoryStream();

            try
            {
                using var image = await Image.LoadAsync(input, cancellationToken);
                using var normalized = image.CloneAs<Rgba32>();

                var encoder = new WebpEncoder
                {
                    Quality = 85,
                    FileFormat = WebpFileFormatType.Lossless,
                    Method = WebpEncodingMethod.BestQuality,
                    NearLossless = true,
                    EntropyPasses = 5
                };

                await normalized.SaveAsync(output, encoder, cancellationToken);
                output.Position = 0;
                return Result<Stream, string>.Ok(output);
            }
            catch (Exception ex)
            {
                output.Dispose();
                return Result<Stream, string>.Err($"Failed to convert image to JPG format\r\n{ex.Message}");
            }
        }
    }
}
