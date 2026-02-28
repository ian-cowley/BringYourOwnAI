using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Interfaces;
using BringYourOwnAI.Core.Models;

namespace BringYourOwnAI.Core.Services
{
    public class LocalSettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public LocalSettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directory = Path.Combine(appData, "BringYourOwnAI");
            _settingsFilePath = Path.Combine(directory, "settings.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task<Settings> LoadAsync()
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new Settings();
            }

            try
            {
                string json;
                using (var reader = new StreamReader(_settingsFilePath))
                {
                    json = await reader.ReadToEndAsync();
                }
                return JsonSerializer.Deserialize<Settings>(json, _jsonOptions) ?? new Settings();
            }
            catch
            {
                return new Settings();
            }
        }

        public async Task SaveAsync(Settings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                using (var writer = new StreamWriter(_settingsFilePath))
                {
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception ex)
            {
                // In a production app, log this
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex}");
            }
        }
    }
}
