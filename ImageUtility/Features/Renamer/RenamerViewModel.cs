using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageUtility.ViewModels;
using Material.Icons;
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
        }

        [RelayCommand]
        private void SetDestinationDirectory()
        {
        }

        [RelayCommand]
        private void LoadExternal()
        {
        }

        public bool CanRename() => !string.IsNullOrEmpty(Pattern) && !string.IsNullOrEmpty(SourceDir) && !string.IsNullOrEmpty(DestinationDir);
    }
}
