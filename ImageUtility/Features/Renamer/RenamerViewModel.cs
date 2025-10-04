using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Material.Icons;
using SukiUI.MessageBox;
using System.ComponentModel.DataAnnotations;

namespace ImageUtility.Features.Renamer
{
    public partial class RenamerViewModel : ViewModelBase
    {
        [ObservableProperty]
        [Required]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        private string _sourceDir;

        [ObservableProperty]
        [Required]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        private string _destinationDir;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        private string? _pattern;

        [ObservableProperty]
        private bool _useExternalFile;

        [ObservableProperty]
        private bool _openOnComplete;

        public RenamerViewModel() : base("Renamer", MaterialIconKind.Rename, 2)
        {
        }

        [RelayCommand(CanExecute = nameof(CanRename))]
        private void Rename()
        {

        }

        [RelayCommand]
        private void SetSourceDirectory()
        {
            
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow);

            // Start async operation to open the dialog.
            var files = topLevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());
            
        }

        [RelayCommand]
        private void SetDestinationDirectory()
        {
        }

        [RelayCommand]
        private void LoadExternal()
        {
            UseExternalFile = true;
        }

        public bool CanRename() => !string.IsNullOrEmpty(Pattern) && !string.IsNullOrEmpty(SourceDir) && !string.IsNullOrEmpty(DestinationDir);
    }
}
