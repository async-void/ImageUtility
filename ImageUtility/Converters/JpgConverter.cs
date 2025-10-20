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
    public class JpgConverter : IImageConverter
    {
        public ImageType SupportedType => ImageType.JPEG;

        public async Task<Result<Stream, string>> ConvertAsync(Stream input, int quality, CancellationToken cancellationToken = default)
        {
            if (input is null) return Result<Stream, string>.Err("input stream is null");
            var output = new MemoryStream();

            try
            {
                using var image = await Image.LoadAsync(input, cancellationToken);
                using var normalized = image.CloneAs<Rgba32>();

                var encoder = new JpegEncoder
                {
                    Quality = quality,
                    ColorType = JpegEncodingColor.Rgb,
                    SkipMetadata = false,
                    Interleaved = true,
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
