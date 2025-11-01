﻿using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImageUtility.Common;
using ImageUtility.Enums;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.MessageBox;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Features.Resizer
{
    public partial class ResizerViewModel : ViewModelBase, IRecipient<ProgressMessage>
    {
        private readonly MainWindow _mWindow;
        private readonly IResizer _resizerService;
        private readonly ISukiToastManager _toastManager;
        private readonly ISukiDialogManager _dialogManager;
        private readonly IMessenger _messenger;
        private readonly IJsonData _dataService;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
        [Required]
        private string? _sourceDir;
        [ObservableProperty]
        [Required]
        [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
        private string? _destinationDir;
        [ObservableProperty]
        private int _width;
        [ObservableProperty]
        private int _height;
        [ObservableProperty]
        private int _progress;
        [ObservableProperty]
        private bool _maintainAspectRatio;
        [ObservableProperty]
        private bool _openOnCompletion;
        [ObservableProperty]
        private string? _statusMessage;
        [ObservableProperty]
        private bool _isBusy;
        [ObservableProperty]
        private bool _copyFiles;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
        private string? _selectedResizeMode;
        [ObservableProperty]
        private string? _selectedFileType;

        public ObservableCollection<string> ResizeModes { get; } = ["Stretch", "Crop", "Fill", "Pad", "Max", "Min"];
        public ObservableCollection<string> FileTypes { get; } = ["All", "PNG", "JPG", "JPEG", "BMP", "GIF", "TIFF", "WEBP"];

        public ResizerViewModel(MainWindow mWindow, ISukiToastManager toastManager, ISukiDialogManager dialogManager,
            IResizer resizerService, IMessenger messenger, IJsonData dataService) : base("Resizer", MaterialIconKind.Resize, 3)
        {
            _mWindow = mWindow;
            _resizerService = resizerService;
            _toastManager = toastManager;
            _dialogManager = dialogManager;
            _messenger = messenger;
            _messenger.Register(this);
            CopyFiles = true;
            _dataService = dataService;
        }

        [RelayCommand]
        private async Task SetDestinationDirectory()
        {
            IsBusy = true;
            StatusMessage = "Setting Destination Directory...";
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
                IsBusy = false;
                StatusMessage = string.Empty;
            }
            else
            {
                IsBusy = false;
                StatusMessage = string.Empty;
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

        [RelayCommand(CanExecute = nameof(CanResize))]
        private async Task Resize()
        {
            IsBusy = true;
            StatusMessage = "Resizing images... this may take a moment";
           
            var options = new EnumerationOptions { RecurseSubdirectories = false };
            IEnumerable<string> files = [.. Directory.EnumerateFiles(SourceDir!, "*", options)];

            bool hasAvif = files.Any(f =>
                    string.Equals(Path.GetExtension(f), ".avif", StringComparison.OrdinalIgnoreCase));

            if (hasAvif)
            {
                var msgBox = new SukiMessageBoxHost
                {
                    ActionButtonsPreset = SukiMessageBoxButtons.OK,
                    IconPreset = SukiMessageBoxIcons.Error,
                    Header = "Unsupported File Type",
                    Content = "avif file types are not supported.\r\nto resize avif files you need to:\r\n1. convert all avif to png\r\n2. resize\r\n3. convert back to avif"
                };
                await SukiMessageBox.ShowDialog(msgBox);
                IsBusy = false;
                StatusMessage = string.Empty;
                return;
            }

            var result = await _resizerService.ResizeImagesAsync(files, DestinationDir!, Width, Height, SelectedResizeMode!, MaintainAspectRatio, CopyFiles);

            IsBusy = false;
            StatusMessage = string.Empty;

            var message = result.Match(
                    ok => $"SUCCESS: {ok}",
                    err => $"FAILURE: {err}"
                );

            var userStat = new Day()
            {
                Date = DateTime.Now,
                Stats = new UserStats()
                {
                    Resizer = new ResizerStats()
                    {
                        Total = files.Count(),
                        Success = files.Count(),
                        Fail = 0
                    }
                }
            };
            await _dataService.InsertDailyStatsAsync(userStat);

            var notificationType = result.IsOk ? NotificationType.Success : NotificationType.Error;

            if (OpenOnCompletion && result.IsOk)
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo()
                {
                    FileName = DestinationDir,
                    UseShellExecute = true,
                    Verb = "open"
                };
                p.Start();
            }
            else
            {
                StatusMessage = string.Empty;
            }

                _toastManager.CreateToast()
                             .WithTitle($"{message}")
                             .OfType(notificationType)
                             .Dismiss().After(TimeSpan.FromSeconds(5))
                             .Queue();
        }

        [RelayCommand(CanExecute = nameof(CanClear))]
        private void Clear()
        {
            SourceDir = string.Empty;
            DestinationDir = string.Empty;
            Width = 64;
            Height = 64;
            StatusMessage = string.Empty;
            IsBusy = false;
            MaintainAspectRatio = false;
            OpenOnCompletion = false;
        }

        public bool CanResize() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir) && !string.IsNullOrWhiteSpace(SelectedResizeMode);
        public bool CanClear() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir);

        public void Receive(ProgressMessage message)
        {
            Progress = message.Value;
            StatusMessage = $"Processing {Progress}% complete.";
        }
    }
}
