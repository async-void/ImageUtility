using CommunityToolkit.Mvvm.Messaging;
using ImageUtility.Common;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUtility.Services
{
    public class RenamerService(ILogger<RenamerService> logger, IMessenger messenger) : IRenamer
    {
        private readonly ILogger<RenamerService> _logger = logger;

        #region RENAME FILES
        public async Task<Result<string, string>> RenameFilesAsync(IEnumerable<string> sourcePaths, string destinationDir, bool copyFiles, string? pattern = null, IEnumerable<string>? renameStrings = null)
        {
            var cts = new CancellationTokenSource();
            //FindExact(sourcePaths, renameStrings);
            if (pattern is { } p)
            {
                int completed = 0;
                int fileCount = sourcePaths.Count();
                int lastPercent = -1;
                var progress = new Progress<int>(percent => messenger.Send(new ProgressMessage(percent)));
                var options = new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cts.Token };
                await Parallel.ForEachAsync(sourcePaths, options, async (sourcePath, token) =>
                {
                    int current = Interlocked.Increment(ref completed);
                    string destinationPath = GetNewNameWithPattern(sourcePath, destinationDir, p, current);
                    await CopyOrMoveAsync(sourcePath, destinationPath, copyFiles, token); 
                    int percent = current * 100 / fileCount;
                    int prev;
                    do
                    {
                        prev = Volatile.Read(ref lastPercent);
                        if (prev == percent) break;
                    } while (Interlocked.CompareExchange(ref lastPercent, percent, prev) != prev);

                    if (prev != percent) ((IProgress<int>)progress).Report(percent);

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
                int completed = 0;
                int fileCount = sourcePaths.Count();
                int lastPercent = -1;
                var progress = new Progress<int>(percent => messenger.Send(new ProgressMessage(percent)));
                var options = new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = cts.Token };
                await Parallel.ForEachAsync(renamePairs, cts.Token, async (pair, token) =>
                {
                    string destinationPath = GetNewNameWithRenameStrings(pair.source, destinationDir, pair.rename);
                    await CopyOrMoveAsync(pair.source, destinationPath, copyFiles, token);
                    int current = Interlocked.Increment(ref completed);
                    int percent = current * 100 / fileCount;
                    int prev;
                    do
                    {
                        prev = Volatile.Read(ref lastPercent);
                        if (prev == percent) break;
                    } while (Interlocked.CompareExchange(ref lastPercent, percent, prev) != prev);

                    if (prev != percent) ((IProgress<int>)progress).Report(percent);
                });
                return Result<string, string>.Ok("Successfully renamed all files");
            }

            return Result<string, string>.Err("Something went wrong, unable to rename files!");

        }
        #endregion

        private static string GetNewName(string sourcePath, string destDir)
        {
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var ext = Path.GetExtension(sourcePath);
            return Path.Combine(destDir, $"{fileName}{ext}");
        }

        private static string GetNewNameWithPattern(string sourcePath, string destDir, string pattern, int increment)
        {
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var ext = Path.GetExtension(sourcePath);
            return Path.Combine(destDir, $"{pattern}_{increment}{ext}");
        }

        private static string GetNewNameWithRenameStrings(string sourcePath, string destinationPath, string renameString)
        {
            var parts = renameString.Split("|", StringSplitOptions.RemoveEmptyEntries);
            var fileName = Path.GetFileNameWithoutExtension(parts[1]);
            var ext = Path.GetExtension(sourcePath);
            return Path.Combine(Path.GetDirectoryName(destinationPath)!, $"{fileName}{ext}");
        }

        private async Task CopyOrMoveAsync(string source, string destination, bool copy, CancellationToken token)
        {
            using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
            using var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
            try
            {
                await sourceStream.CopyToAsync(destinationStream, bufferSize: 81920);
                sourceStream.Dispose();
            }
            catch(OperationCanceledException)
            {
                try
                {
                    File.Delete(destination);
                }
                catch { }
                return;
            }

            if (!copy && !token.IsCancellationRequested)
            {
                File.Delete(source);
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

