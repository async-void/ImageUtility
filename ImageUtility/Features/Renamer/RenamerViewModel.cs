
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImageUtility.Common;
using ImageUtility.Interfaces;
using ImageUtility.Messaging;
using ImageUtility.Models;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Markdown.Avalonia;
using Material.Icons;
using Microsoft.Extensions.Logging;
using SukiUI.Controls;
using SukiUI.MessageBox;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace ImageUtility.Features.Renamer
{
    public partial class RenamerViewModel :  ViewModelBase, IRecipient<ProgressMessage>
    {
        private readonly MainWindow _mWindow;
        private readonly IRenamer _renameService;
        private readonly ISukiToastManager _toastManager;
        private readonly IJsonData _dataService;
        private readonly ILogger<RenamerViewModel> _logger;
        private readonly IMessenger _messenger;

        [ObservableProperty] private bool _useAlternativeHeaderStyle = true;
        [ObservableProperty] private bool _showHeaderContentSeparator = true;
        [ObservableProperty] private bool _useNativeWindow;

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
        [ObservableProperty]
        private int _progress;

        private List<string> filesList = [];


        public RenamerViewModel(MainWindow mWindow, IRenamer renameService, ISukiToastManager toastManager, 
            IJsonData dataService, ILogger<RenamerViewModel> logger, IMessenger messenger) : base("Renamer", MaterialIconKind.Rename, 2)
        {
            _mWindow = mWindow;
            _renameService = renameService;
            _dataService = dataService;
            _toastManager = toastManager;
            _logger = logger;
            _messenger = messenger;
            _messenger.Register(this);
            CopyFiles = true;
        }

        [RelayCommand(CanExecute = nameof(CanRename))]
        private async Task Rename()
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
            StatusMessage = "Renaming Files... Please Wait";
            IsLoading = true;
            IEnumerable<string> files = [.. Directory.EnumerateFiles(SourceDir!)];
            var fileCount = files.Count();
            var result = await _renameService.RenameFilesAsync([.. files], DestinationDir!, CopyFiles, Pattern, filesList);
            var message = result.Match(
                    ok => $"SUCCESS: {ok}",
                    err => $"FAILURE: {err}"
                );

            var userStat = new Day()
            {
                Date = DateTime.Now,
                Stats = new UserStats()
                {
                    Renamer = new RenamerStats()
                    {
                        Total = files.Count(),
                        Success = files.Count(),
                        Fail = 0
                    }
                }
            };

            await _dataService.InsertDailyStatsAsync(userStat);

            _messenger.Send(new UserDataActivityMessage(
                new UserDataPayload()
                {
                    RenameCount = fileCount
                }));

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
                    _logger.LogError(ex, "Failed to open destination directory after renaming.");
                    _toastManager.CreateToast()
                          .WithContent($"Unable to open {DestinationDir} Directory")
                          .OfType(NotificationType.Warning)
                          .Dismiss().After(TimeSpan.FromSeconds(5))
                          .Queue();
                }
            }
            _logger.LogInformation("Renaming completed with message: {Message}", message);
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
                FileTypeFilter =
                [
                    new("Text Files")
                    {
                        Patterns = ["*.txt"],
                        AppleUniformTypeIdentifiers = ["public.plain-text"]
                    }
                ]
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

        [RelayCommand]
        private async Task ShowHelpDialog()
        {
            var okButton = SukiMessageBoxButtonsFactory.CreateButton(SukiMessageBoxResult.OK);
            var result = await SukiMessageBox.ShowDialog(new SukiMessageBoxHost
            {
                IconPresetSize = 48,
                IconPreset = SukiMessageBoxIcons.Information,
                UseAlternativeHeaderStyle = UseAlternativeHeaderStyle,
                ShowHeaderContentSeparator = ShowHeaderContentSeparator,
                Header = "Renamer Help",
                Margin = new Thickness(10),
                Content = new MarkdownScrollViewer()
                {
                    Markdown = """
                           ## Using External Text File:

                           - if the "use external file" option is selected, the application will read filenames from a user-provided text file.
                           - "load external file" button will be enabled.
                           - clicking the button will open a file dialog where you can navigate to the naming file.
                           - naming file should be a plain text file (.txt) with oldfilename|newfilename pattern on a new line.
                           - the naming file will be loaded into memory and used for renaming files.
                           - pattern and numbering options will be disabled to avoid conflicts.

                           ## Pattern:

                           - the new filename example "renamed_".
                           - default pattern is "renamed_" if not specified.

                           ## Numbering:
                            - add a starting number after the renaming pattern.
                            - the numbering will increment by 1 for each file renamed.
                            - default numbering starts at 1 if not specified.

                           ## Copy Files:

                           - When enabled, the application will copy files from the source directory to the destination directory with the new names.

                           ## Open On Completion:

                           - When enabled, this will open the destination directory when file processing is complete

                           For any feedback or support, please reach out to our support team at support@example.com.
                           """
                },
                
                ActionButtonsSource = [okButton],
            }, new SukiMessageBoxOptions
            {
                UseNativeWindow = UseNativeWindow,
            });

            if (result is SukiMessageBoxResult.OK)
            {
                
            }
        }
        public bool CanRename() => !string.IsNullOrEmpty(Pattern) || !string.IsNullOrEmpty(SourceDir) && !string.IsNullOrEmpty(DestinationDir);
        public bool CanClear() => !string.IsNullOrEmpty(Pattern) || !string.IsNullOrEmpty(SourceDir) && !string.IsNullOrEmpty(DestinationDir);

        public void Receive(ProgressMessage message)
        {
            Progress = message.Value;
            StatusMessage = $"Renaming {Progress}% complete...";
        }
    }
}
