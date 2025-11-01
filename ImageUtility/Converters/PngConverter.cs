using ImageUtility.Enums;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUtility.Converters
{
    public class PngConverter : IImageConverter
    {
        public ImageType SupportedType => ImageType.PNG;

        public async Task<Result<Stream, string>> ConvertAsync(Stream input, int quality, CancellationToken cancellationToken = default)
        {

            if (input is null) return Result<Stream, string>.Err("input stream is null");
            var output = new MemoryStream();

            try
            {
                using var image = await Image.LoadAsync(input, cancellationToken);
                using var normalized = image.CloneAs<Rgba32>();

                var encoder = new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    ColorType = PngColorType.RgbWithAlpha,
                    TransparentColorMode = PngTransparentColorMode.Preserve,
                    SkipMetadata = false,
                    FilterMethod = PngFilterMethod.Adaptive,
                };

                await normalized.SaveAsync(output, encoder, cancellationToken);
                output.Position = 0;
                return Result<Stream, string>.Ok(output);
            }
            catch (Exception ex)
            {
                output.Dispose();
                return Result<Stream, string>.Err($"Failed to convert image to PNG format\r\n{ex.Message}");
            }

        }

    }
}
