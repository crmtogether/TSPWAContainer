using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace CRMTogether.PwaHost
{
    public static class TranslationManager
    {
        private static Dictionary<string, string> _translations = new Dictionary<string, string>();
        private static string _currentLanguage = "en-US";
        private static readonly Dictionary<string, string> _languageMappings = new Dictionary<string, string>
        {
            // English variants
            { "en", "en-US" },
            { "en-US", "en-US" },
            { "en-GB", "en-GB" },
            { "en-AU", "en-US" },
            { "en-CA", "en-US" },
            
            // German variants
            { "de", "de-DE" },
            { "de-DE", "de-DE" },
            { "de-AT", "de-DE" },
            { "de-CH", "de-DE" },
            
            // French variants
            { "fr", "fr-FR" },
            { "fr-FR", "fr-FR" },
            { "fr-CA", "fr-FR" },
            { "fr-CH", "fr-FR" },
            { "fr-BE", "fr-FR" }
        };

        public static string CurrentLanguage => _currentLanguage;

        public static void Initialize()
        {
            try
            {
                // Detect system language
                var systemLanguage = CultureInfo.CurrentCulture.Name;
                _currentLanguage = GetSupportedLanguage(systemLanguage);
                
                // Load translations
                LoadTranslations(_currentLanguage);
                
                LogDebug($"TranslationManager initialized with language: {_currentLanguage} (detected: {systemLanguage})");
            }
            catch (Exception ex)
            {
                LogDebug($"Error initializing TranslationManager: {ex.Message}");
                // Fallback to English
                _currentLanguage = "en-US";
                LoadTranslations(_currentLanguage);
            }
        }

        private static string GetSupportedLanguage(string systemLanguage)
        {
            // Try exact match first
            if (_languageMappings.ContainsKey(systemLanguage))
            {
                return _languageMappings[systemLanguage];
            }
            
            // Try language code only (e.g., "en" from "en-US")
            var languageCode = systemLanguage.Split('-')[0];
            if (_languageMappings.ContainsKey(languageCode))
            {
                return _languageMappings[languageCode];
            }
            
            // Default to US English
            return "en-US";
        }

        private static void LoadTranslations(string language)
        {
            try
            {
                _translations.Clear();
                
                // Get the directory where the executable is located
                var appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var translationsPath = Path.Combine(appDirectory, "translations", $"{language}.json");
                
                if (File.Exists(translationsPath))
                {
                    var jsonContent = File.ReadAllText(translationsPath);
                    var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                    
                    if (translations != null)
                    {
                        _translations = translations;
                        LogDebug($"Loaded {_translations.Count} translations for language: {language}");
                    }
                }
                else
                {
                    LogDebug($"Translation file not found: {translationsPath}");
                    // Load default English translations
                    LoadDefaultTranslations();
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error loading translations for {language}: {ex.Message}");
                LoadDefaultTranslations();
            }
        }

        private static void LoadDefaultTranslations()
        {
            // Fallback to hardcoded English translations
            _translations = new Dictionary<string, string>
            {
                // Application
                { "app.title", "CRM Together ContextAI" },
                { "app.ready", "Ready" },
                
                // Menu items
                { "menu.settings", "Settings" },
                { "menu.monitored_folders", "Monitored Folders..." },
                { "menu.about", "About..." },
                { "menu.test", "Test" },
                { "menu.test_email", "Test changeSelectedEmail" },
                { "menu.clipboard", "Clipboard" },
                { "menu.toggle_clipboard", "Toggle Clipboard Monitoring" },
                { "menu.test_clipboard", "Test Clipboard Check" },
                
                // Settings form
                { "settings.title", "Settings" },
                { "settings.startup_url_label", "Startup URL (URL to load when the app starts):" },
                { "settings.folders_label", "Folders to monitor for new files" },
                { "settings.add_folder", "Add Folder..." },
                { "settings.remove_selected", "Remove Selected" },
                { "settings.close", "Close" },
                { "settings.select_folder_description", "Select a folder to monitor" },
                
                // About form
                { "about.title", "About CRM Together ContextAI" },
                { "about.content", "CRM Together ContextAI\n\nVersion: {0}\nAssembly: {1}\n\nWebView2 Runtime: {2}\n\n© CRMTogether" },
                { "about.ok", "OK" },
                
                // Clipboard monitoring
                { "clipboard.enabled", "Clipboard monitoring enabled" },
                { "clipboard.disabled", "Clipboard monitoring disabled" },
                { "clipboard.event_based", "Clipboard monitoring enabled (event-based)" },
                { "clipboard.testing", "Testing clipboard check..." },
                { "clipboard.no_text", "Clipboard does not contain text" },
                { "clipboard.contains", "Clipboard contains: {0}..." },
                
                // Content detection
                { "content.email_detected", "Email detected from {0}: {1}" },
                { "content.phone_detected", "Phone number detected from {0}: {1}" },
                { "content.website_detected", "Website detected from {0}: {1}" },
                { "content.address_detected", "Postal address detected from {0}: {1}" },
                { "content.text_detected", "Text detected from {0}: {1}" },
                { "content.no_recognizable", "No recognizable content found in {0} content" },
                
                // Processing
                { "processing.email", "Processing email value from {0}: {1}" },
                { "processing.phone", "Processing phone number from {0}: {1}" },
                { "processing.website", "Processing website from {0}: {1}" },
                { "processing.address", "Processing postal address from {0}: {1}" },
                { "processing.text", "Processing text from {0}: {1}" },
                
                // Status messages
                { "status.error", "Error: {0}" },
                { "status.critical_error", "Critical Error: {0}" },
                { "status.webview_init_failed", "WebView2 initialization failed: {0}" },
                { "status.webview_config_failed", "WebView2 configuration failed: {0}" },
                { "status.webview_process_failed", "WebView2 process failed: {0}" },
                
                // File processing
                { "file.processing", "Processing {0} file: {1}" },
                { "file.processed_successfully", "{0} file processed successfully! {1}" },
                { "file.empty", "{0} file is empty or contains no {1}" },
                
                // Errors
                { "error.generic", "An unexpected error occurred: {0}" },
                { "error.critical", "A critical error occurred: {0}" },
                { "error.webview", "WebView2 process failed: {0}" },
                { "error.clipboard", "Error in clipboard test: {0}" },
                { "error.processing", "Error processing {0} value: {1}" }
            };
        }

        public static string GetString(string key, params object[] args)
        {
            try
            {
                if (_translations.TryGetValue(key, out string value))
                {
                    if (args != null && args.Length > 0)
                    {
                        return string.Format(value, args);
                    }
                    return value;
                }
                
                // Fallback to key if translation not found
                LogDebug($"Translation key not found: {key}");
                return key;
            }
            catch (Exception ex)
            {
                LogDebug($"Error getting translation for key '{key}': {ex.Message}");
                return key;
            }
        }

        public static void SetLanguage(string language)
        {
            if (_languageMappings.ContainsKey(language))
            {
                _currentLanguage = _languageMappings[language];
                LoadTranslations(_currentLanguage);
                LogDebug($"Language changed to: {_currentLanguage}");
            }
        }

        public static string[] GetSupportedLanguages()
        {
            return _languageMappings.Values.Distinct().ToArray();
        }

        private static void LogDebug(string message)
        {
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                         "CRMTogether", "PwaHost", "debug.log");
                var logDir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
                
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [TranslationManager] {message}{Environment.NewLine}";
                File.AppendAllText(logPath, logEntry);
                
                System.Diagnostics.Debug.WriteLine($"[TranslationManager] {message}");
            }
            catch { }
        }
    }
}
