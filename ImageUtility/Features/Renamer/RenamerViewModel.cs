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
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        private string _sourceDir;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        private string _destinationDir;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
        private string _pattern;

        public RenamerViewModel() : base("Renamer", MaterialIconKind.Rename, 2)
        {
        }

        [RelayCommand(CanExecute = nameof(CanRename))]
        private void Rename()
        {

        }

        public bool CanRename() => !string.IsNullOrEmpty(Pattern) && !string.IsNullOrEmpty(SourceDir) && !string.IsNullOrEmpty(DestinationDir);
    }
}
