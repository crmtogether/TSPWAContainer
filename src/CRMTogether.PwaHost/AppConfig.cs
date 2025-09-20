using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CRMTogether.PwaHost
{
    public class AppConfig
    {
        public List<string> WatchedFolders { get; set; } = new List<string>();
        public string JsScriptsRoot { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRMTogether", "PwaHost", "scripts");
        public List<JsScriptEntry> JsScripts { get; set; } = new List<JsScriptEntry>();
        public string StartupUrl { get; set; } = "https://crmtogether.com/univex-app-home/";
        public string LastUrl { get; set; } = "";
        public string ProcessingFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRMTogether", "PwaHost", "processing");
        public string ProcessedFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRMTogether", "PwaHost", "processed");

        public static string ConfigDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRMTogether", "PwaHost");
        public static string ConfigPath => Path.Combine(ConfigDir, "config.json");

        public static AppConfig LoadDefault()
        {
            try
            {
                LogDebug($"Loading config from: {ConfigPath}");
                Directory.CreateDirectory(ConfigDir);
                if (File.Exists(ConfigPath))
                {
                    var text = File.ReadAllText(ConfigPath);
                    LogDebug($"Config file content: {text}");
                    var cfg = JsonSerializer.Deserialize<AppConfig>(text) ?? new AppConfig();
                    if (cfg.WatchedFolders == null) cfg.WatchedFolders = new List<string>();
                    if (cfg.WatchedFolders.Count == 0)
                    {
                        var dl = GetDownloadsPath();
                        LogDebug($"No watched folders found, adding downloads path: {dl}");
                        if (!string.IsNullOrWhiteSpace(dl)) cfg.WatchedFolders.Add(dl);
                    }
                    if (string.IsNullOrWhiteSpace(cfg.StartupUrl)) cfg.StartupUrl = "https://crmtogether.com/univex-app-home/";
                    cfg.EnsureProcessingFolders();
                    LogDebug($"Loaded config with {cfg.WatchedFolders.Count} watched folders");
                    return cfg;
                }
                else
                {
                    LogDebug("Config file does not exist, creating default");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error loading config: {ex.Message}");
            }
            var c = new AppConfig();
            var downloads = GetDownloadsPath();
            LogDebug($"Creating default config with downloads path: {downloads}");
            if (!string.IsNullOrWhiteSpace(downloads)) c.WatchedFolders.Add(downloads);
            c.EnsureProcessingFolders();
            return c;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(ConfigDir);
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }

        public void EnsureProcessingFolders()
        {
            try
            {
                Directory.CreateDirectory(ProcessingFolder);
                Directory.CreateDirectory(ProcessedFolder);
                LogDebug($"Ensured processing folders exist: {ProcessingFolder}, {ProcessedFolder}");
            }
            catch (Exception ex)
            {
                LogDebug($"Error creating processing folders: {ex.Message}");
            }
        }

        private static string GetDownloadsPath()
        {
            try
            {
                var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var dl = Path.Combine(user ?? "", "Downloads");
                LogDebug($"User profile: {user}");
                LogDebug($"Downloads path: {dl}");
                LogDebug($"Downloads directory exists: {Directory.Exists(dl)}");
                if (!string.IsNullOrWhiteSpace(dl) && Directory.Exists(dl)) return dl;
            }
            catch (Exception ex)
            {
                LogDebug($"Error getting downloads path: {ex.Message}");
            }
            return "";
        }

        private static void LogDebug(string message)
        {
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                         "CRMTogether", "PwaHost", "debug.log");
                var logDir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
                File.AppendAllText(logPath, logEntry);
                
                // Also write to debug output
                System.Diagnostics.Debug.WriteLine(message);
            }
            catch { }
        }
    }

    public class JsScriptEntry
    {
        public string Name { get; set; }
        public string File { get; set; }
    }
}
