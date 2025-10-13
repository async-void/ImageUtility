using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarfBuzzSharp;
using ImageUtility.Interfaces;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Material.Icons;
using SukiUI.MessageBox;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


namespace ImageUtility.Features.Renamer
{
    public partial class RenamerViewModel : ViewModelBase
    {
        private readonly MainWindow _mWindow;
        private readonly IRenamer _renameService;
        private readonly ISukiToastManager _toastManager;

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

        [ObservableProperty]
        private bool _isLoadingDirectories;

        [ObservableProperty]
        private bool _copyFiles;

        private List<string> filesList = [];

        public RenamerViewModel(MainWindow mWindow, IRenamer renameService, ISukiToastManager toastManager) : base("Renamer", MaterialIconKind.Rename, 2)
        {
            _mWindow = mWindow;
            _renameService = renameService;
            _toastManager = toastManager;
            CopyFiles = true;
        }

        [RelayCommand(CanExecute = nameof(CanRename))]
        private async Task Rename()
        {
            StatusMessage = "Renaming Files... Please Wait";
            IsLoading = true;
            IEnumerable<string> files = Directory.EnumerateFiles(SourceDir!);

            var result = await _renameService.RenameFilesAsync([.. files], DestinationDir!, CopyFiles, Pattern, filesList);
            var message = result.Match(
                    ok => $"SUCCESS: {ok}",
                    err => $"FAILURE: {err}"
                );

            IsLoading = false;

            if ( OpenOnComplete && result.IsOk)
            {
                try
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
                catch (Exception ex)
                {
                    _toastManager.CreateToast()
                          .WithContent($"Unable to open {DestinationDir} Directory")
                          .OfType(NotificationType.Warning)
                          .Dismiss().After(TimeSpan.FromSeconds(5))
                          .Queue();
                }
            }
            _toastManager.CreateToast()
                .WithContent($"{message}")
                .OfType(NotificationType.Success)
                .Dismiss().After(TimeSpan.FromSeconds(5))
                .Queue();
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
            var startLoc = await topLevel!.StorageProvider.TryGetWellKnownFolderAsync(Avalonia.Platform.Storage.WellKnownFolder.Documents);
            IsLoadingDirectories = true;
            StatusMessage = "Loading Source Files";

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
            else
            {
                _toastManager.CreateToast()
                    .WithTitle("Warning")
                    .WithContent("Source Directory not selected")
                    .OfType(NotificationType.Warning)
                    .Dismiss().After(TimeSpan.FromSeconds(5))
                    .Queue();
            }
            IsLoadingDirectories = false;
            StatusMessage = string.Empty;
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
        private async Task LoadExternal()
        {
            StatusMessage = "Reading Filename's...";
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
                using StreamReader reader = new(result[0].Path.LocalPath);
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    filesList.Add(line);
                }

            }

            IsLoading = false;
        }

        public bool CanRename() => !string.IsNullOrEmpty(Pattern) || !string.IsNullOrEmpty(SourceDir) && !string.IsNullOrEmpty(DestinationDir);
        public bool CanClear() => !string.IsNullOrEmpty(Pattern) || !string.IsNullOrEmpty(SourceDir) && !string.IsNullOrEmpty(DestinationDir);
    }
}
