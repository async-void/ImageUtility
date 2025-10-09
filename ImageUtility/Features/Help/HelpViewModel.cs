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
        public HelpViewModel() : base("Help", MaterialIconKind.Help, 4)
        {
        }
    }
}
