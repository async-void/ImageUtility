using CommunityToolkit.Mvvm.Input;
using ImageUtility.Interfaces;
using ImageUtility.Models;
using ImageUtility.ViewModels;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Features.Help
{
    public partial class HelpViewModel : ViewModelBase
    {
        private readonly IHelpProvider _helpProvider;
        public HelpViewModel(IHelpProvider helpProvider) : base("Help", MaterialIconKind.Help, 6)
        {
            _helpProvider = helpProvider;
        }

        public List<HelpRoot> HelpContents { get;  private set; } = new List<HelpRoot>();

        [RelayCommand]
        private async Task LoadHelp()
        {
            Result<HelpRoot, string> HelpContents = await _helpProvider.GetHelpContentAsync();
           
            var test = "";
        }
    }
}
