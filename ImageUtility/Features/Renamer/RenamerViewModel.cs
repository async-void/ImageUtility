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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace ImageUtility.Features.Renamer
{
    public partial class RenamerViewModel : ViewModelBase
    {
        private readonly MainWindow _mWindow;
        [ObservableProperty]
        [Required]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
        private string? _sourceDir;

        [ObservableProperty]
        [Required]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
        private string? _destinationDir;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        [NotifyCanExecuteChangedFor(nameof(ClearCommand))]
        private string? _pattern;

        [ObservableProperty]
        private string? _statusMessage;

        [ObservableProperty]
        private bool _useExternalFile;

        [ObservableProperty]
        private bool _openOnComplete;

        [ObservableProperty]
        private bool _isLoading;

        private List<string>? filesList = new List<string>();
        public RenamerViewModel(MainWindow mWindow) : base("Renamer", MaterialIconKind.Rename, 2)
        {
            _mWindow = mWindow;
        }

        [RelayCommand(CanExecute = nameof(CanRename))]
        private void Rename()
        {
            StatusMessage = "Renaming Files... Please Wait";
            IsLoading = true;
        }
        [RelayCommand(CanExecute = nameof(CanClear))]
        private void Clear()
        {
            Pattern = string.Empty;
            SourceDir = string.Empty;
            DestinationDir = string.Empty;
            UseExternalFile = false;
            OpenOnComplete = false;
        }

        [RelayCommand]
        private async Task SetSourceDirectory()
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
                SourceDir = result[0].Path.LocalPath;
            }
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
        }

        [RelayCommand]
        private async Task LoadExternal()
        {
            StatusMessage = "Loading Files...";
            IsLoading = true;
            var topLevel = TopLevel.GetTopLevel(_mWindow);
            var startLoc = await topLevel!.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

            var options = new FilePickerOpenOptions
            {
                Title = "Select a File",
                SuggestedStartLocation = startLoc,
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Text Files")
                    {
                        Patterns = new List<string> { "*.txt" },
                        AppleUniformTypeIdentifiers = new List<string> { "public.plain-text" }
                    }
                }
            };
            var result = await topLevel.StorageProvider.OpenFilePickerAsync(options);
           
            if (result is { Count: > 0 })
            {
                var lines = await File.ReadAllLinesAsync(result[0].Path.LocalPath);
                
                foreach (var line in lines)
                {
                    filesList?.Add(line);
                }
               
            }

            IsLoading = false;
        }

        public bool CanRename() => !string.IsNullOrEmpty(Pattern) || !string.IsNullOrEmpty(SourceDir) && !string.IsNullOrEmpty(DestinationDir);
        public bool CanClear() => !string.IsNullOrEmpty(Pattern) || !string.IsNullOrEmpty(SourceDir) && !string.IsNullOrEmpty(DestinationDir);
    }
}
