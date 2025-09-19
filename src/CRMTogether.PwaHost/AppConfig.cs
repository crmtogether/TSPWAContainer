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

        public static string ConfigDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRMTogether", "PwaHost");
        public static string ConfigPath => Path.Combine(ConfigDir, "config.json");

        public static AppConfig LoadDefault()
        {
            try
            {
                Directory.CreateDirectory(ConfigDir);
                if (File.Exists(ConfigPath))
                {
                    var text = File.ReadAllText(ConfigPath);
                    var cfg = JsonSerializer.Deserialize<AppConfig>(text) ?? new AppConfig();
                    if (cfg.WatchedFolders == null) cfg.WatchedFolders = new List<string>();
                    if (cfg.WatchedFolders.Count == 0)
                    {
                        var dl = GetDownloadsPath();
                        if (!string.IsNullOrWhiteSpace(dl)) cfg.WatchedFolders.Add(dl);
                    }
                    if (string.IsNullOrWhiteSpace(cfg.StartupUrl)) cfg.StartupUrl = "https://crmtogether.com/univex-app-home/";
                    return cfg;
                }
            }
            catch { }
            var c = new AppConfig();
            var downloads = GetDownloadsPath();
            if (!string.IsNullOrWhiteSpace(downloads)) c.WatchedFolders.Add(downloads);
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

        private static string GetDownloadsPath()
        {
            try
            {
                var user = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var dl = Path.Combine(user ?? "", "Downloads");
                if (!string.IsNullOrWhiteSpace(dl) && Directory.Exists(dl)) return dl;
            }
            catch { }
            return "";
        }
    }

    public class JsScriptEntry
    {
        public string Name { get; set; }
        public string File { get; set; }
    }
}
