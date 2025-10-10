using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageUtility.Enums;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Features.Resizer
{
    public partial class ResizerViewModel : ViewModelBase
    {
        private readonly MainWindow _mWindow;
        private readonly ISukiToastManager _toastManager;
        private readonly ISukiDialogManager _dialogManager;

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
        private bool _maintainAspectRatio;
        [ObservableProperty]
        private bool _openOnCompletion;
        [ObservableProperty]
        private string? _statusMessage;
        [ObservableProperty]
        private bool _isBusy;
        [ObservableProperty]
        private bool _removeOriginal;
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
        private string? _selectedResizeMode;
        [ObservableProperty]
        private string? _selectedFileType;

        public ObservableCollection<string> ResizeModes { get; } = ["Stretch", "Crop", "Fill", "Pad", "Max", "Min"];
        public ObservableCollection<string> FileTypes { get; } = ["All", "PNG", "JPG", "JPEG", "BMP", "GIF", "TIFF"];

        public ResizerViewModel(MainWindow mWindow, ISukiToastManager toastManager, ISukiDialogManager dialogManager) : base("Resizer", MaterialIconKind.Resize, 3)
        {
            _mWindow = mWindow;
            _toastManager = toastManager;
            _dialogManager = dialogManager;
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
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
            }
        }

        [RelayCommand]
        private async Task SetSourceDirectory()
        {
            var toplevel = TopLevel.GetTopLevel(_mWindow);
            var startLoc = await toplevel!.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
            IsBusy = true;
            StatusMessage = "Enumerating Source.files...";
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
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();

            }
            
        }

        [RelayCommand(CanExecute = nameof(CanResize))]
        private async Task Resize()
        {
            IsBusy = true;
            StatusMessage = "Resizing images...";
            var files = Directory.GetFiles(SourceDir!, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
                .ToList();
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
    }
}
