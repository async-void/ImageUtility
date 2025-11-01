using ImageUtility.Interfaces;
using ImageUtility.Models;
using LibHeifSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Utilities
{
    public class FileUtilities : IFileUtilities
    {
        public async Task<Result<string, string>> GetFileExtAsync(IEnumerable<string> files)
        {
            throw new NotImplementedException();
        }

        public int ConvertToAvif(string ffmpegPath, string inputPath, string outputPath, int crf = 30, int cpuUsed = 4)
        {
            var args = new StringBuilder();
            args.Append("-y ");
            args.Append($"-i \"{inputPath}\" ");
            args.Append("-c:v libaom-av1 ");
            args.Append($"-crf {crf} ");
            args.Append($"-cpu-used {cpuUsed} ");
            args.Append("-pix_fmt yuv420p ");
            args.Append("-still-picture 1 ");   // important for single-image AVIF
            args.Append("-f avif ");            // force AVIF container
            args.Append($"\"{outputPath}\"");

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi };
            proc.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            proc.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            return proc.ExitCode;
        }

        public int ConvertToPng(string ffmpegPath, string input, string output)
        {
            var args = new StringBuilder();
            args.Append("-y ");
            args.Append($"-i \"{input}\" ");
            args.Append($"\"{output}\"");

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi };
            proc.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            proc.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            return proc.ExitCode;
        }

        public int ConvertToJpg(string ffmpegPath, string input, string output)
        {
            var args = new StringBuilder();
            args.Append("-y ");
            args.Append($"-i \"{input}\" ");
            args.Append("-q:v 2 ");
            args.Append($"\"{output}\"");

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi };
            proc.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            proc.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            return proc.ExitCode;
        }

        public int ConvertToWebp(string ffmpegPath, string input, string output)
        {
            var args = new StringBuilder();
            args.Append("-y "); 
            args.Append($"-i \"{input}\" "); 
            args.Append("-qscale 80 "); 
            args.Append($"\"{output}\""); 

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi };
            proc.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            proc.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            return proc.ExitCode;
        }
    }
}
