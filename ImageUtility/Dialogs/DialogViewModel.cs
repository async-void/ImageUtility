using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;

namespace ImageUtility.Dialogs
{
    public partial class DialogViewModel(ISukiDialog dialog) : ObservableObject
    {
        [RelayCommand]
        private void CloseDialog()
        {
            dialog.Dismiss();
        }
    }
}
