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
        [Required]
        private string? _sourceDir;
        [ObservableProperty]
        [Required]
        [NotifyCanExecuteChangedFor(nameof(ResizeCommand))]
        private string? _destinationDir;
        [ObservableProperty]
        private int _width;
        [ObservableProperty]
        private int _height;
        [ObservableProperty]
        private bool _maintainAspectRatio;
        [ObservableProperty]
        private string? _statusMessage;
        [ObservableProperty]
        private bool _isLoading;
        [ObservableProperty]
        private bool _isBusy;
        [ObservableProperty]
        private string? _selectedResizeMode;

        public ObservableCollection<string> ResizeModes { get; } = ["Stretch", "Crop", "Fill", "Pad", "Max", "Min"];

        public ResizerViewModel(MainWindow mWindow, ISukiToastManager toastManager, ISukiDialogManager dialogManager) : base("Resizer", MaterialIconKind.Resize, 3)
        {
            _mWindow = mWindow;
            _toastManager = toastManager;
            _dialogManager = dialogManager;
        }

        [RelayCommand(CanExecute = nameof(CanResize))]
        private async Task Resize()
        {
            IsBusy = true;
            StatusMessage = "Resizing images...";
        }

        [RelayCommand(CanExecute = nameof(CanClear))]
        private void Clear()
        {
            SourceDir = string.Empty;
            DestinationDir = string.Empty;
            Width = 0;
            Height = 0;
            StatusMessage = string.Empty;
            IsLoading = false;
        }

        public bool CanResize() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir);
        public bool CanClear() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir);
    }
}
