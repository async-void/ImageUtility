using ImageUtility.Enums;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using NeoSolve.ImageSharp.AVIF;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUtility.Converters
{
    public class AvifConverter : IImageConverter
    {
        public ImageType SupportedType => ImageType.AVIF;

        public async Task<Result<Stream, string>> ConvertAsync(Stream input, CancellationToken cancellationToken = default)
        {
            if (input is null) return Result<Stream, string>.Err("input is null");
            if (!input.CanRead) return Result<Stream, string>.Err("input is not readable");

            var output = new MemoryStream(capacity: 64 * 1024);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var image = await Image.LoadAsync(input, cancellationToken);
                using var normalized = image.CloneAs<Rgb48>();
                normalized.Metadata.IccProfile = null;
              
                var encoder = new AVIFEncoder
                {
                    SkipMetadata = true,
                    CQLevel = 23,
                };

                await normalized.SaveAsync(output, encoder, cancellationToken);

                output.Position = 0;
                return Result<Stream, string>.Ok(output);
            }
            catch (OperationCanceledException)
            {
                output.Dispose();
                return Result<Stream, string>.Err("operation was cancelled");
            }
            catch (Exception ex)
            {
                output.Dispose();
                return Result<Stream, string>.Err($"Failed to convert image to AVIF format\r\n{ex.Message}");
            }

        }

        //public static async Task<Result<int, string>> ConvertToAvifAsync(string ffmpegPath, string inputPath, string outputPath, string encoderArgs = "-c:v libaom-av1 -crf 30 -b:v 0 -speed 4")
        //{
        //    if (!File.Exists(ffmpegPath)) throw new FileNotFoundException("ffmpeg not found", ffmpegPath);
        //    if (!File.Exists(inputPath)) throw new FileNotFoundException("input not found", inputPath);

        //    var psi = new ProcessStartInfo
        //    {
        //        FileName = ffmpegPath,
        //        Arguments = $"-hide_banner -y -i \"{inputPath}\" {encoderArgs} \"{outputPath}\"",
        //        UseShellExecute = false,
        //        CreateNoWindow = true,
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true
        //    };

        //    using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };

        //    proc.Start();

        //    var stdOut = proc.StandardOutput.ReadToEndAsync();
        //    var stdErr = proc.StandardError.ReadToEndAsync();

        //    await Task.WhenAll(stdOut, stdErr);

        //    proc.WaitForExit();

        //    var exitCode = proc.ExitCode;

        //    if (exitCode != 0)
        //    {
        //        var err = stdErr.Result;
        //        return Result<int, string>.Err($"ffmpeg exited {exitCode}: {err}");
        //    }
        //    return Result<int, string>.Ok(exitCode);
        //}


        //public static async Task BatchConvertAsync(string ffmpegPath, string inputDir, string outputDir, string encoderArgs = "-c:v libaom-av1 -crf 30 -b:v 0 -speed 4")
        //{
        //    Directory.CreateDirectory(outputDir);
        //    var files = Directory.EnumerateFiles(inputDir, "*.*")
        //        .Where(f => new[] { ".png", ".jpg", ".jpeg", ".tiff", ".webp", ".bmp" }
        //        .Contains(Path.GetExtension(f).ToLowerInvariant()));

        //    foreach (var file in files)
        //    {
        //        var outFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(file) + ".avif");
        //        await ConvertToAvifAsync(ffmpegPath, file, outFile, encoderArgs);
        //    }
        //}

    }
}
