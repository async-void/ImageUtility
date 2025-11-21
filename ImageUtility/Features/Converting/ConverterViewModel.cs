using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImageUtility.Enums;
using ImageUtility.Extensions;
using ImageUtility.Interfaces;
using ImageUtility.Messaging;
using ImageUtility.Models;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Material.Icons;
using Microsoft.Extensions.Logging;
using SukiUI.Controls;
using SukiUI.MessageBox;
using SukiUI.Toasts;
using Path = System.IO.Path;

namespace ImageUtility.Features.Converting
{
    public partial class ConverterViewModel : ViewModelBase
    {
        private readonly MainWindow _mWindow;
        private readonly ISukiToastManager _toastManager;
        private readonly IMessenger _messenger;
        private readonly IFileUtilities _fileUtilities;
        private readonly IJsonData _dataService;
        private readonly ILogger<ConverterViewModel> _logger;

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

        public ConverterViewModel(MainWindow mWindow, IFileUtilities fileUtilities,
                            ISukiToastManager toastManager, IJsonData dataService, IMessenger messenger, ILogger<ConverterViewModel> logger) : base("Converter", MaterialIconKind.ImageEdit, 1)
        {
            _mWindow = mWindow;
            _toastManager = toastManager;
           
            _fileUtilities = fileUtilities;
            _messenger = messenger;
            _dataService = dataService;
            _logger = logger;
            CopyFiles = true;
        }

        [RelayCommand(CanExecute = nameof(CanConvert))]
        private async Task Convert()
        {
            if (SourceDir!.Equals(DestinationDir, StringComparison.OrdinalIgnoreCase))
            {
                var msgBox = new SukiMessageBoxHost
                {
                    ActionButtonsPreset = SukiMessageBoxButtons.OK,
                    ShowHeaderContentSeparator = true,
                    IconPreset = SukiMessageBoxIcons.Information,
                    Header = "Image Utility ",
                    Content = "Source Directory and Destination Directory cannot be the same directory.\r\nPlease choose a different destination directory."
                };
                await SukiMessageBox.ShowDialog(msgBox);
                DestinationDir = string.Empty;
                return;
            }
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
                RecurseSubdirectories = false,
                IgnoreInaccessible = false,
                ReturnSpecialDirectories = false
            };

            IEnumerable<string> files = Directory.EnumerateFiles(SourceDir!, "*.*", options)
                                                 .Where(f => imageExts.Contains(Path.GetExtension(f)));
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
                await Task.Delay(100);
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
                            IsQualityEnabled = true;
                            _fileUtilities.ConvertToJpg(ffmpegPath, img, newFileName);
                            isOk = true;
                            break;
                        case ImageType.WEBP:
                            IsQualityEnabled = true;
                            _fileUtilities.ConvertToWebp(ffmpegPath, img, newFileName, Quality);
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
           
            _messenger.Send(new UserDataActivityMessage(
                new UserDataPayload()
                {
                    ConversionCount = fileCount
                }));

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
                _logger.LogError("Some images failed to convert: {errors}", string.Join("; ", errors));
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
                _logger.LogInformation("{type} Conversion completed successfully: {files} total files converted", SelectedFileType, fileCount);
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
            IsQualityEnabled = value is not (ImageType.AVIF or ImageType.PNG);
        }

        public bool CanConvert() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir);
        public bool CanClear() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir);
    }
}
