using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageUtility.Enums;
using ImageUtility.Extensions;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using ImageUtility.Services;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.MessageBox;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;
using Path = System.IO.Path;

namespace ImageUtility.Features.Converting
{
    public partial class ConverterViewModel : ViewModelBase
    {
        private readonly MainWindow _mWindow;
        private readonly ISukiToastManager _toastManager;
        private readonly ConversionService _converterService;
        private readonly IFileUtilities _fileUtilities;
        private readonly IJsonData _dataService;

        [ObservableProperty]
        private bool _isBusy;
        [ObservableProperty]
        private string? _statusMessage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConvertCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
        [Required]
        private string? _sourceDir;
        [ObservableProperty]
        [Required]
        [NotifyCanExecuteChangedFor(nameof(ConvertCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
        private string? _destinationDir;
        [ObservableProperty]
        private ImageType _selectedFileType;
        [ObservableProperty]
        private int _selectedIndex;
        [ObservableProperty]
        private string? _curFileSize;
        [ObservableProperty]
        private string? _fileCount;
        [ObservableProperty]
        private bool _copyFiles;
        [ObservableProperty]
        private bool _openOnCompletion;
        [ObservableProperty]
        private bool _isQualityEnabled = true;
        [ObservableProperty]
        private int _quality = 85;

        public Array FileTypes => Enum.GetValues<ImageType>();

        public ConverterViewModel(MainWindow mWindow, ConversionService converterService, IFileUtilities fileUtilities,
                            ISukiToastManager toastManager, IJsonData dataService) : base("Converter", MaterialIconKind.ImageEdit, 4)
        {
            _mWindow = mWindow;
            _toastManager = toastManager;
            _converterService = converterService;
            _fileUtilities = fileUtilities;
            CopyFiles = true;
            _dataService = dataService;
        }

        [RelayCommand(CanExecute = nameof(CanConvert))]
        private async Task Convert()
        {
            var errors = new List<string>();
            IsBusy = true;
            var isOk = false;
            StatusMessage = "Converting images... this may take a moment";
            var imageType = SelectedFileType;
            var processedCount = 0;

            var imageExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                            { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".webp", ".avif" };
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = false
            };

            IEnumerable<string> files = Directory.EnumerateFiles(SourceDir!, "*.*", options);
            int fileCount = files.Count();
            var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "FFMPEG", "ffmpeg", "bin", "ffmpeg.exe");
            foreach (string img in files)
            {
                processedCount++;
                int percent = processedCount * 100 / fileCount;
                var fileName = Path.GetFileName(img);
                var newFilePath = Path.Combine(DestinationDir!, Path.GetFileNameWithoutExtension(img));
                var ext = $".{SelectedFileType.ToExtensionString()}";
                var newFileName = $"{newFilePath}{ext}";
                StatusMessage = $"Converting {percent}% complete.";
                try
                { 
                    switch (SelectedFileType)
                    {
                        case ImageType.AVIF:
                            IsQualityEnabled = false;
                            _fileUtilities.ConvertToAvif(ffmpegPath, img, newFileName);
                            isOk = true;
                            break;
                        case ImageType.PNG:
                            IsQualityEnabled = false;
                            _fileUtilities.ConvertToPng(ffmpegPath, img, newFileName);
                            isOk = true;
                            break;
                        case ImageType.JPEG:
                            IsQualityEnabled = false;
                            _fileUtilities.ConvertToJpg(ffmpegPath, img, newFileName);
                            isOk = true;
                            break;
                        case ImageType.WEBP:
                            IsQualityEnabled = false;
                            _fileUtilities.ConvertToWebp(ffmpegPath, img, newFileName);
                            isOk = true;
                            break;
                        default:
                            StatusMessage = "un-known file type";
                            isOk = false;
                            break;
                    }
                   
                 }
                catch(Exception ex) 
                {
                    errors.Add($"{ex.Message}");
                    continue;
                }
            }

            await Task.Delay(300);
            StatusMessage = "Conversion complete!";
            IsBusy = false;

            var userStat = new Day()
            {
                Date = DateTime.Now,
                Stats = new UserStats()
                {
                    Converter = new ConverterStats()
                    {
                        Total = fileCount,
                        Success = fileCount,
                        Fail = 0
                    }
                }
            };
            await _dataService.InsertDailyStatsAsync(userStat);

            if (OpenOnCompletion && isOk)
            {
                try
                {
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = DestinationDir,
                            UseShellExecute = true,
                            Verb = "open"
                        }
                    };
                    p.Start();
                }
                catch (Exception ex)
                {
                    _toastManager.CreateToast()
                        .WithTitle("Error")
                        .WithContent($"Failed to open destination folder\r\n{ex.Message}")
                        .OfType(NotificationType.Error)
                        .Dismiss().After(TimeSpan.FromSeconds(5))
                        .Queue();
                }
            }

           
            if (errors.Any())
            {
                var errMsg = string.Join("\r\n", errors);
                _toastManager.CreateToast()
                    .WithTitle("Error")
                    .WithContent($"Some images failed to convert\r\n{errMsg}")
                    .OfType(NotificationType.Error)
                    .Dismiss().After(TimeSpan.FromSeconds(10))
                    .Queue();
            }
            else
            {
                _toastManager.CreateToast()
                            .WithTitle($"SUCCESS: all files converted successfully.")
                            .OfType(NotificationType.Success)
                            .Dismiss().After(TimeSpan.FromSeconds(5))
                            .Queue();
            }
                

        }

        [RelayCommand(CanExecute = nameof(CanClear))]
        private void Clear()
        {
            SourceDir = string.Empty;
            DestinationDir = string.Empty;
            StatusMessage = string.Empty;
            OpenOnCompletion = false;
            CopyFiles = true;
            IsBusy = false;
        }

        [RelayCommand]
        private async Task SetDestinationDirectory()
        {
            var topLevel = TopLevel.GetTopLevel(_mWindow);
            var startLoc = await topLevel!.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
            var options = new FolderPickerOpenOptions
            {
                Title = "Select a Folder",
                SuggestedStartLocation = startLoc,
                AllowMultiple = false
            };
            var result = await topLevel.StorageProvider.OpenFolderPickerAsync(options);
            if (result is { Count: > 0 })
            {
                DestinationDir = result[0].Path.LocalPath;
                
            }
            else
            {
                _toastManager.CreateToast()
                    .WithTitle("Warning")
                    .WithContent("Destination Directory not selected")
                    .OfType(NotificationType.Warning)
                    .Dismiss().After(TimeSpan.FromSeconds(5))
                    .Queue();
            }
        }

        [RelayCommand]
        private async Task SetSourceDirectory()
        {
            var toplevel = TopLevel.GetTopLevel(_mWindow);
            var startLoc = await toplevel!.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
            IsBusy = true;
            StatusMessage = "Enumerating Source files... \r\nthis may take a moment";
            var options = new FolderPickerOpenOptions
            {
                Title = "Select a Folder",
                SuggestedStartLocation = startLoc,
                AllowMultiple = false
            };
            var result = await toplevel.StorageProvider.OpenFolderPickerAsync(options);
            if (result is { Count: > 0 })
            {
                SourceDir = result[0].Path.LocalPath;
                FileCount = Directory.GetFiles(SourceDir ?? string.Empty, "*.*", SearchOption.AllDirectories).Length.ToString();
                
                IsBusy = false;
                StatusMessage = string.Empty;
            }
            else
            {
                IsBusy = false;
                StatusMessage = string.Empty;
                _toastManager.CreateToast()
                    .WithTitle("Warning")
                    .WithContent("Source Directory not selected")
                    .OfType(NotificationType.Warning)
                    .Dismiss().After(TimeSpan.FromSeconds(5))
                    .Queue();

            }

        }

        partial void OnSelectedFileTypeChanged(ImageType value)
        {
            if (value == ImageType.AVIF || value == ImageType.PNG)
                IsQualityEnabled = false;
            else
                IsQualityEnabled = true;
        }

        public bool CanConvert() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir);
        public bool CanClear() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir);
    }
}
