using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
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
        private string? _sourceDir;
        [ObservableProperty]
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

        public ResizerViewModel(MainWindow mWindow, ISukiToastManager toastManager, ISukiDialogManager dialogManager) : base("Resizer", MaterialIconKind.Resize, 3)
        {
            _mWindow = mWindow;
            _toastManager = toastManager;
            _dialogManager = dialogManager;
        }

        [RelayCommand(CanExecute = nameof(CanResize))]
        private async Task Resize()
        {

        }

        public bool CanResize() => !string.IsNullOrWhiteSpace(SourceDir) && !string.IsNullOrWhiteSpace(DestinationDir) && Width > 0 && Height > 0;
    }
}
