using ImageUtility.Enums;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUtility.Converters
{
    public class JpgConverter : IImageConverter
    {
        public ImageType SupportedType => ImageType.JPG;

        public async Task<Result<Stream, string>> ConvertAsync(Stream input, CancellationToken cancellationToken = default)
        {
            using var image = await Image.LoadAsync(input, cancellationToken);
            var output = new MemoryStream();
            var encoder = new JpegEncoder();

            try
            {
                await image.SaveAsync(output, encoder, cancellationToken);
                output.Position = 0;
                return Result<Stream, string>.Ok(output);
            }
            catch (Exception ex)
            {
                return Result<Stream, string>.Err($"Failed to convert image to JPG format\r\n{ex.Message}");
            }
        }
    }
}
