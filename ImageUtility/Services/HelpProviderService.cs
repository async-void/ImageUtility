using ImageUtility.Interfaces;
using ImageUtility.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageUtility.Services
{
    public class HelpProviderService : IHelpProvider
    {
        public async Task<Result<HelpRoot, string>> GetHelpContentAsync()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Json", "help.json");
            if (!File.Exists(path))
                return Result<HelpRoot, string>.Err("Help content not found.");
            var json = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<HelpRoot>(json);
            
            if (data is { } rootData)
                return Result<HelpRoot, string>.Ok(rootData);
            return Result<HelpRoot, string>.Err("Failed to parse help content.");
        }
    }
}
