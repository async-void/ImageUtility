using ImageUtility.Interfaces;
using ImageUtility.Models;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Size = SixLabors.ImageSharp.Size;

namespace ImageUtility.Services
{
    public class ResizerService(ILogger<ResizerService> logger) : IResizer
    {

        private ILogger<ResizerService> Logger { get; } = logger;
        public async Task<Result<string, string>> ResizeImagesAsync(IEnumerable<string> sourcePaths, string destinationDir, int width, int height, string resizeMode, bool maintainAspectRatio, bool copyFiles)
        {
            var errors = new List<string>();
            var files = sourcePaths.ToList();
            for (int i = 0; i < sourcePaths.Count(); i++)
            {
                using Image img = await Image.LoadAsync(files[i]);
                int srcW = img.Width;
                int srcH = img.Height;

                int destW = width > 0 ? width : (int)Math.Round(srcW * (height / (double)srcH));
                int destH = height > 0 ? height : (int)Math.Round(srcH * (width / (double)srcW));

                // Calculate scale factor to keep aspect ratio and fit within box
                double widthRatio = destW / (double)srcW;
                double heightRatio = destH / (double)srcH;
                double scale = Math.Min(widthRatio, heightRatio);

                var finalSize = new Size(
                         Math.Max(1, (int)Math.Round(img.Width * scale)),
                         Math.Max(1, (int)Math.Round(img.Height * scale))
                );

                var resizeOptions = new ResizeOptions
                {
                    Size = finalSize,
                    Mode = resizeMode.ToLower() switch
                    {
                        "stretch" => ResizeMode.Stretch,
                        "crop" => ResizeMode.Crop,
                        "fill" => ResizeMode.BoxPad,
                        "pad" => ResizeMode.Pad,
                        "max" => ResizeMode.Max,
                        "min" => ResizeMode.Min,
                        _ => ResizeMode.Max
                    },
                    Position = AnchorPositionMode.Center,
                    Sampler = KnownResamplers.Lanczos3,
                    Compand = true
                };

                img.Mutate(x => x.Resize(resizeOptions));
                img.Save(Path.Combine(destinationDir, Path.GetFileName(files[i])));
                if (copyFiles)
                {
                    try
                    {
                        File.Delete(files[i]);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to delete original file {files[i]}: {ex.Message}");
                        Logger.LogError(ex, "Failed to delete original file {FilePath}", files[i]);
                    }
                }
            }
           
            return Result<string, string>.Ok("Resizing completed successfully.");
        }
    }
}
