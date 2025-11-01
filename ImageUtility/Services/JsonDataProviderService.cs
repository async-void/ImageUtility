using ImageUtility.Interfaces;
using ImageUtility.Models;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ImageUtility.Services
{
    public class JsonDataProviderService : IJsonData
    {
        private readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Json", "userstats.json");

        public async Task<Result<bool, string>> InsertDailyStatsAsync(Day newStats)
        {
            UserStatsHistory history;
            
            if (File.Exists(_path))
            {
                string json = await File.ReadAllTextAsync(_path);
                history = JsonSerializer.Deserialize<UserStatsHistory>(json) ?? new UserStatsHistory();
            }
            else
            {
                history = new UserStatsHistory();
            }

            history.Days ??= new List<Day>();

            var existing = history.Days.FirstOrDefault(d => d != null && d.Date.Date == newStats.Date.Date);
            if (existing != null)
            {
                existing.Stats ??= new UserStats();

                if (newStats.Stats?.Renamer != null)
                {
                    existing.Stats.Renamer ??= new RenamerStats();
                    existing.Stats.Renamer.Total += newStats.Stats.Renamer.Total;
                    existing.Stats.Renamer.Success += newStats.Stats.Renamer.Success;
                }

                if (newStats.Stats?.Resizer != null)
                {
                    existing.Stats.Resizer ??= new ResizerStats();
                    existing.Stats.Resizer.Total += newStats.Stats.Resizer.Total;
                    existing.Stats.Resizer.Success += newStats.Stats.Resizer.Success;
                }

                if (newStats.Stats?.Converter != null)
                {
                    existing.Stats.Converter ??= new ConverterStats();
                    existing.Stats.Converter.Total += newStats.Stats.Converter.Total;
                    existing.Stats.Converter.Success += newStats.Stats.Converter.Success;
                }

            }
            else
            {
                history.Days.Add(newStats);
            }

            try
            {
                await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(history, _jsonOptions));
                return Result<bool, string>.Ok(true);
            }
            catch (Exception ex)
            {
                return Result<bool, string>.Err($"unable to create or update json file.\r\n{ex.Message}");
            }


        }

        public async Task<Result<UserStatsHistory, string>> LoadStatsAsync()
        {
           
            if (string.IsNullOrEmpty(_path))
               throw new ArgumentNullException(nameof(_path));

            if (!File.Exists(_path))
            {
                throw new FileNotFoundException("file not found");
            }

            try
            {
                var json = await File.ReadAllTextAsync(_path);
                var history = JsonSerializer.Deserialize<UserStatsHistory>(json, _jsonOptions);

                if (history is null)
                {
                    return Result<UserStatsHistory, string>.Err("Failed to deserialize JSON into UserStatsHistory.");
                }
                
                return Result<UserStatsHistory, string>.Ok(history);
            }
            catch(Exception ex)
            {
                var test = "";
                return Result<UserStatsHistory, string>.Err($"Error: {ex.Message}");
            } 
        }


        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
        };


    }
}
