using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Reflection;

namespace CRMTogether.PwaHost
{
    public class AppConfig
    {
        public List<string> WatchedFolders { get; set; } = new List<string>();
        public string JsScriptsRoot { get; set; } = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "CRMTogether", "PwaHost", "scripts");
        public List<JsScriptEntry> JsScripts { get; set; } = new List<JsScriptEntry>();
        public string StartupUrl { get; set; } = "https://crmtogether.com/univex-app-home/";
        public string LastUrl { get; set; } = "";
        public string ProcessingFolder { get; set; } = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "CRMTogether", "PwaHost", "processing");
        public string ProcessedFolder { get; set; } = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "CRMTogether", "PwaHost", "processed");
        
        // Environment-specific properties
        public string Environment { get; set; } = "Default";
        public string AppName { get; set; } = "ContextAgent";
        public string AppDescription { get; set; } = "WinForms WebView2 host with clipboard monitoring, custom URI handling, and multi-language support";
        public string ProtocolHandler { get; set; } = "crmtog";
        public BuildInfo BuildInfo { get; set; } = new BuildInfo();
        
        // Feature toggles
        public bool ShowTestMenu { get; set; } = true;
        public bool ShowClipboardMenu { get; set; } = false;
        public bool EnableFolderMonitoring { get; set; } = true;

        public static string ConfigDir => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "CRMTogether", "PwaHost");
        public static string ConfigPath => Path.Combine(ConfigDir, "config.json");

        public static AppConfig LoadDefault()
        {
            try
            {
                // First, load environment-specific configuration
                var environmentConfig = LoadEnvironmentConfig();
                
                LogDebug($"Loading config from: {ConfigPath}");
                Directory.CreateDirectory(ConfigDir);
                if (File.Exists(ConfigPath))
                {
                    var text = File.ReadAllText(ConfigPath);
                    LogDebug($"Config file content: {text}");
                    var cfg = JsonSerializer.Deserialize<AppConfig>(text) ?? new AppConfig();
                    
                    // Apply environment-specific overrides
                    ApplyEnvironmentConfig(cfg, environmentConfig);
                    
                    if (cfg.WatchedFolders == null) cfg.WatchedFolders = new List<string>();
                    if (cfg.WatchedFolders.Count == 0)
                    {
                        var dl = GetDownloadsPath();
                        LogDebug($"No watched folders found, adding downloads path: {dl}");
                        if (!string.IsNullOrWhiteSpace(dl)) cfg.WatchedFolders.Add(dl);
                    }
                    if (string.IsNullOrWhiteSpace(cfg.StartupUrl)) cfg.StartupUrl = environmentConfig?.StartupUrl ?? "https://crmtogether.com/univex-app-home/";
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
            var defaultEnvConfig = LoadEnvironmentConfig();
            ApplyEnvironmentConfig(c, defaultEnvConfig);
            
            // Ensure startup URL is set from environment config
            if (defaultEnvConfig != null && !string.IsNullOrWhiteSpace(defaultEnvConfig.StartupUrl))
            {
                c.StartupUrl = defaultEnvConfig.StartupUrl;
            }
            
            var downloads = GetDownloadsPath();
            LogDebug($"Creating default config with downloads path: {downloads}");
            if (!string.IsNullOrWhiteSpace(downloads)) c.WatchedFolders.Add(downloads);
            c.EnsureProcessingFolders();
            return c;
        }

        private static EnvironmentConfig LoadEnvironmentConfig()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyDir = Path.GetDirectoryName(assembly.Location);
                
                // Determine environment from multiple sources (in order of priority):
                // 1. Build constants (MSBuild)
                // 2. Environment variable
                // 3. Command line arguments
                // 4. Default
                string environment = "Default";
                
                // Check build constants first
#if SAGE100_BUILD
                environment = "Sage100";
                LogDebug("Environment detected from build constants: Sage100");
#else
                // Check environment variable
                var envVar = System.Environment.GetEnvironmentVariable("Environment");
                if (!string.IsNullOrWhiteSpace(envVar))
                {
                    environment = envVar;
                    LogDebug($"Environment detected from environment variable: {environment}");
                }
                else
                {
                    // Check command line arguments
                    var args = System.Environment.GetCommandLineArgs();
                    for (int i = 0; i < args.Length - 1; i++)
                    {
                        if (args[i].Equals("--environment", StringComparison.OrdinalIgnoreCase) ||
                            args[i].Equals("-e", StringComparison.OrdinalIgnoreCase))
                        {
                            environment = args[i + 1];
                            LogDebug($"Environment detected from command line: {environment}");
                            break;
                        }
                    }
                }
#endif
                
                var configPath = Path.Combine(assemblyDir, "config", $"{environment.ToLower()}.json");
                LogDebug($"Loading environment config from: {configPath}");
                
                if (File.Exists(configPath))
                {
                    var text = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<EnvironmentConfig>(text);
                    LogDebug($"Loaded environment config for: {config?.Environment}");
                    return config;
                }
                else
                {
                    LogDebug($"Environment config file not found: {configPath}");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error loading environment config: {ex.Message}");
            }
            return null;
        }

        private static void ApplyEnvironmentConfig(AppConfig appConfig, EnvironmentConfig environmentConfig)
        {
            if (environmentConfig != null)
            {
                appConfig.Environment = environmentConfig.Environment;
                appConfig.AppName = environmentConfig.AppName;
                appConfig.AppDescription = environmentConfig.AppDescription;
                appConfig.ProtocolHandler = environmentConfig.ProtocolHandler;
                appConfig.BuildInfo = environmentConfig.BuildInfo ?? new BuildInfo();
                
                // Apply feature toggles
                appConfig.ShowTestMenu = environmentConfig.ShowTestMenu;
                appConfig.ShowClipboardMenu = environmentConfig.ShowClipboardMenu;
                appConfig.EnableFolderMonitoring = environmentConfig.EnableFolderMonitoring;
                
                // Always use the environment-specific startup URL
                appConfig.StartupUrl = environmentConfig.StartupUrl;
                
                LogDebug($"Applied environment config: {environmentConfig.Environment}");
            }
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
                var user = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
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
                var logPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), 
                                         "CRMTogether", "PwaHost", "debug.log");
                var logDir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{System.Environment.NewLine}";
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

    public class EnvironmentConfig
    {
        public string Environment { get; set; }
        public string StartupUrl { get; set; }
        public string AppName { get; set; }
        public string AppDescription { get; set; }
        public string ProtocolHandler { get; set; }
        public BuildInfo BuildInfo { get; set; }
        
        // Feature toggles
        public bool ShowTestMenu { get; set; } = true;
        public bool ShowClipboardMenu { get; set; } = false;
        public bool EnableFolderMonitoring { get; set; } = true;
    }


    public class BuildInfo
    {
        public string Version { get; set; } = "2.0.0";
        public string BuildTag { get; set; } = "Default";
        public string TargetUrl { get; set; }
    }
}
