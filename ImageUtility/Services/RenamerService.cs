using ImageUtility.Interfaces;
using ImageUtility.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUtility.Services
{
    public class RenamerService(ILogger<RenamerService> logger) : IRenamer
    {
        private readonly ILogger<RenamerService> _logger = logger;
        public async Task<Result<string, string>> RenameFilesAsync(IEnumerable<string> sourcePaths, string destinationDir, bool copyFiles, string? pattern = null, IEnumerable<string>? renameStrings = null)
        {
            var cts = new CancellationTokenSource();
            //FindExact(sourcePaths, renameStrings);
            if (pattern is { } p)
            {
                await Parallel.ForEachAsync(sourcePaths, cts.Token, async (sourcePath, token) =>
                {
                    string destinationPath = GetNewNameWithPattern(sourcePath, destinationDir, p);
                    await CopyOrMoveAsync(sourcePath, destinationPath, copyFiles);
                });
                return Result<string, string>.Ok("Successfully renamed all files");
            }
            if (renameStrings is { } rStrings)
            {
                var rList = rStrings.ToList();
                var sList = sourcePaths.ToList();

                if (rList.Count != sList.Count)
                    return Result<string, string>.Err("filename count does not match source file count.");

                var renamePairs = sList.Zip(rList, (source, rename) => new { source, rename });
                
                await Parallel.ForEachAsync(renamePairs, cts.Token, async (pair, token) =>
                {
                    string destinationPath = GetNewNameWithRenameStrings(pair.source, destinationDir, pair.rename);
                    await CopyOrMoveAsync(pair.source, destinationPath, copyFiles);
                });
                return Result<string, string>.Ok("Successfully renamed all files");
            }

            return Result<string, string>.Err("Something went wrong, unable to rename files!");

        }

        private static string GetNewNameWithPattern(string sourcePath, string destDir, string pattern)
        {
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var ext = Path.GetExtension(sourcePath);
            return Path.Combine(destDir, $"{fileName}_{pattern}{ext}");
        }

        private static string GetNewNameWithRenameStrings(string sourcePath, string destinationPath, string renameString)
        {
            var parts = renameString.Split("|", StringSplitOptions.RemoveEmptyEntries);
            var fileName = Path.GetFileNameWithoutExtension(parts[1]);
            var ext = Path.GetExtension(sourcePath);
            return Path.Combine(Path.GetDirectoryName(destinationPath)!, $"{fileName}{ext}");
        }

        private async Task CopyOrMoveAsync(string source, string destination, bool copy)
        {
            using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
            using var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
            try
            {
                await sourceStream.CopyToAsync(destinationStream);
                sourceStream.Dispose();
                if (!copy)
                    File.Delete(source);
            }
            catch (Exception ex)
            {
                _logger.LogError(message: $"Failed to {(copy ? "copy" : "move")} file from {source} to {destination}", ex);
            }

        }

        public void FindExact(IEnumerable<string> sourcePaths, IEnumerable<string>? renameStrings)
        {
            for (var i = 0; i < sourcePaths.Count(); i++)
            {
                var line = Path.GetFileName(sourcePaths.ElementAt(i));
                var parts = renameStrings?.ElementAt(i).Split("|", StringSplitOptions.RemoveEmptyEntries);
                var line2 = Path.GetFileName(parts[0]);
                if (line != line2)
                {
                    Console.WriteLine($"Error found: {line}");
                }
            }
        }
    }
}

