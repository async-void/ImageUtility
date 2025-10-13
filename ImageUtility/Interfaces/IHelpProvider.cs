using ImageUtility.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Interfaces
{
    public interface IHelpProvider
    {
        Task<Result<HelpRoot, string>> GetHelpContentAsync();
    }
}
