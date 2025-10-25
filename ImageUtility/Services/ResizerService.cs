using CommunityToolkit.Mvvm.Messaging;
using ImageUtility.Common;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using Size = SixLabors.ImageSharp.Size;

namespace ImageUtility.Services
{
    public class ResizerService(ILogger<ResizerService> logger, IMessenger messenger) : IResizer
    {

        private ILogger<ResizerService> Logger { get; } = logger;
        public async Task<Result<string, string>> ResizeImagesAsync(IEnumerable<string> sourcePaths, string destinationDir, int width, int height, string resizeMode, bool maintainAspectRatio, bool copyFiles)
        {
            var cts = new CancellationTokenSource();
            var errors = new List<string>();
            var files = sourcePaths.ToList();
            int completed = 0;
            int fileCount = sourcePaths.Count();
            int lastPercent = -1;
            var progress = new Progress<int>(percent => messenger.Send(new ProgressMessage(percent)));
            var options = new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cts.Token };

            await Parallel.ForEachAsync(sourcePaths, options, async (source, token) =>
            {
                try
                {
                    using Image img = await Image.LoadAsync(source);
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
                    img.Save(Path.Combine(destinationDir, Path.GetFileName(source)));
                    if (!copyFiles)
                    {
                        try
                        {
                            File.Delete(source);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Failed to delete original file {source}: {ex.Message}");
                            Logger.LogError(ex, "Failed to delete original file {FilePath}", source);
                        }
                    }
                    int current = Interlocked.Increment(ref completed);
                    int percent = current * 100 / fileCount;
                    int prev;
                    do
                    {
                        prev = Volatile.Read(ref lastPercent);
                        if (prev == percent) break;
                    } while (Interlocked.CompareExchange(ref lastPercent, percent, prev) != prev);

                    if (prev != percent) ((IProgress<int>)progress).Report(percent);
                    }
                catch(UnknownImageFormatException ex) { errors.Add(ex.Message); }
                catch(Exception ex) { errors.Add(ex.Message); }
            });

            return Result<string, string>.Ok("Resizing completed successfully.");

        }
    }
}
