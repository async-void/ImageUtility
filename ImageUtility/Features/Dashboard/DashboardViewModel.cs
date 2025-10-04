using ImageUtility.ViewModels;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Features.Dashboard
{
    public partial class DashboardViewModel : ViewModelBase
    {
        public DashboardViewModel() : base("Dashboard", MaterialIconKind.ViewDashboard, 1)
        {
        }
    }
}
