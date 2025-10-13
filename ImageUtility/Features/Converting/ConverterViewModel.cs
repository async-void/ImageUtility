using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Material.Icons;
using SukiUI.Toasts;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ImageUtility.Features.Converting
{
    public partial class ConverterViewModel : ViewModelBase
    {
        private readonly MainWindow _mWindow;
        private readonly ISukiToastManager _toastManager;

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
        private string? _selectedFileType;
        [ObservableProperty]
        private bool _copyFiles;

        public ObservableCollection<string> FileTypes { get; } = ["PNG", "JPG", "JPEG", "WEBP", "AVIF", "BMP"];

        public ConverterViewModel(MainWindow mWindow, ISukiToastManager toastManager) : base("Converter", MaterialIconKind.ImageEdit, 4)
        {
            _mWindow = mWindow;
            _toastManager = toastManager;
            CopyFiles = true;
        }

        [RelayCommand(CanExecute = nameof(CanConvert))]
        private async Task Convert()
        {
            IsBusy = true;
            StatusMessage = "Converting images... this may take a moment";
            await Task.Delay(2000);
            StatusMessage = "Conversion complete!";
            IsBusy = false;
        }

        [RelayCommand(CanExecute = nameof(CanClear))]
        private void Clear()
        {
            SourceDir = string.Empty;
            DestinationDir = string.Empty;
            StatusMessage = string.Empty;
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

        public bool CanConvert() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir);
        public bool CanClear() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir);
    }
}
