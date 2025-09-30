using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace CRMTogether.PwaHost
{
    public class MainForm : Form
    {
        // Windows API declarations for clipboard monitoring and text selection
        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardOwner();

        [DllImport("user32.dll")]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("user32.dll")]
        private static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern IntPtr AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private const int WM_GETTEXT = 0x000D;
        private const int WM_GETTEXTLENGTH = 0x000E;
        private const uint CF_TEXT = 1;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;
        private const int VK_C = 0x43;
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        // Regex patterns for different content types
        private static readonly Regex EmailRegex = new Regex(
            @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly Regex PhoneRegex = new Regex(
            @"(\+?1[-.\s]?)?(\(?[0-9]{3}\)?[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}|[0-9]{3}[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}|\+?[0-9]{1,4}[-.\s]?[0-9]{1,4}[-.\s]?[0-9]{1,4}[-.\s]?[0-9]{1,9})(?![0-9])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly Regex WebsiteRegex = new Regex(
            @"(https?://[^\s]+|www\.[^\s]+|[a-zA-Z0-9][a-zA-Z0-9-]{1,61}[a-zA-Z0-9]?\.[a-zA-Z]{2,})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // Clipboard monitoring
        private string _lastClipboardContent = "";
        private bool _clipboardMonitoringEnabled = true;
        private ToolStripMenuItem _toggleClipboardMenuItem;
        public DateTime _lastClipboardCheck = DateTime.MinValue;
        private const int CLIPBOARD_DEBOUNCE_MS = 500; // Prevent rapid-fire events
        private WebView2Wrapper _webView;
        private bool _initialized;
        private readonly Dictionary<string, string> _extraHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private MenuStrip _menu;
        private string _pendingUrl;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private readonly Dictionary<string, string> _contextParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private object _lastEmailObject = null;

        private static readonly string[] AllowedPrefixes = new[] {
            "https://",
            "http://localhost",
            "http://",
            "file:///"
        };

        public string InitialUrl { get; set; }
        public WebView2Wrapper WebView => _webView;

        public MainForm()
        {
            // Initialize translation framework first
            TranslationManager.Initialize();
            
            // Disable clipboard monitoring if clipboard menu is hidden
            if (!Program.Config.ShowClipboardMenu)
            {
                _clipboardMonitoringEnabled = false;
            }
            
            Text = TranslationManager.GetString("app.title");
            Width = 420;
            Height = 600; // Set a reasonable initial height instead of 0
            StartPosition = FormStartPosition.Manual;
            AllowDrop = true;
            
            // Set the form icon
            try
            {
                Icon = new Icon(Path.Combine(Application.StartupPath, "images", "crmtogethericon.ico"));
            }
            catch (Exception ex)
            {
                LogDebug($"Error loading icon: {ex.Message}");
            }

            // Add global exception handling
            Application.ThreadException += (sender, e) =>
            {
                LogDebug($"Unhandled thread exception: {e.Exception.Message}");
                LogDebug($"Stack trace: {e.Exception.StackTrace}");
                SetStatusMessage($"Error: {e.Exception.Message}");
                // MessageBox.Show(this, $"An unexpected error occurred:\n{e.Exception.Message}\n\nThe application will continue running.", "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                LogDebug($"Unhandled domain exception: {ex?.Message}");
                LogDebug($"Stack trace: {ex?.StackTrace}");
                SetStatusMessage($"Critical Error: {ex?.Message}");
                // MessageBox.Show(this, $"A critical error occurred:\n{ex?.Message}\n\nThe application may need to be restarted.", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            _menu = BuildMenu();
            this.MainMenuStrip = _menu;

            // Initialize clipboard monitoring only if enabled
            if (Program.Config.ShowClipboardMenu)
            {
                InitializeClipboardMonitoring();
            }
            else
            {
                LogDebug("Clipboard monitoring disabled for this environment");
            }

            // Create a table layout panel to properly manage the layout
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(0)
            };

            // Configure the rows - use fixed sizes for menu and status to prevent excessive sizing
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Menu row - fixed at 30px
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // WebView row
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); // Status row - fixed at 22px

            // Create a panel for the menu
            var menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            _menu.Dock = DockStyle.Fill;
            menuPanel.Controls.Add(_menu);

            // Create a panel with minimal padding for the WebView
            var webViewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(1) // Minimal padding to prevent sizing issues
            };

            _webView = new WebView2Wrapper { Dock = DockStyle.Fill };
            webViewPanel.Controls.Add(_webView);

            // Create status strip
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel(TranslationManager.GetString("app.ready"));
            _statusStrip.Items.Add(_statusLabel);
            
            // Add panels to the table layout
            tableLayout.Controls.Add(menuPanel, 0, 0);
            tableLayout.Controls.Add(webViewPanel, 0, 1);
            tableLayout.Controls.Add(_statusStrip, 0, 2);
            
            // Add the table layout to the form
            Controls.Add(tableLayout);

            this.ResumeLayout(performLayout: true);

            // Handle form resize to ensure proper WebView sizing
            this.Resize += OnFormResize;
            this.Load += async (_, __) => await InitializeAsync();
            DragEnter += OnDragEnter;
            DragDrop += OnDragDrop;
        }

        private void OnFormResize(object sender, EventArgs e)
        {
            try
            {
                // Force layout update to ensure WebView is properly sized
                if (_webView != null && _webView.IsHandleCreated)
                {
                    _webView.Invalidate();
                    _webView.Update();
                }
                
                // Log resize for debugging
                LogDebug($"Form resized to: {this.Size}");
            }
            catch (Exception ex)
            {
                LogDebug($"Error in OnFormResize: {ex.Message}");
            }
        }

        private void EnsureWebViewProperSize()
        {
            try
            {
                if (_webView != null && _webView.IsHandleCreated)
                {
                    // Force a layout update
                    this.PerformLayout();
                    
                    // Ensure WebView fills its container properly
                    _webView.Dock = DockStyle.Fill;
                    _webView.Invalidate();
                    _webView.Update();
                    
                    LogDebug($"WebView size ensured: {_webView.Size}");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error in EnsureWebViewProperSize: {ex.Message}");
            }
        }

        private void LogMenuDimensions()
        {
            try
            {
                LogDebug($"Form size: {this.Size}");
                LogDebug($"Menu size: {_menu?.Size}");
                LogDebug($"Menu height: {_menu?.Height}");
                LogDebug($"Status strip size: {_statusStrip?.Size}");
                LogDebug($"Status strip height: {_statusStrip?.Height}");
                
                // Log TableLayoutPanel row heights
                if (Controls.Count > 0 && Controls[0] is TableLayoutPanel tableLayout)
                {
                    for (int i = 0; i < tableLayout.RowCount; i++)
                    {
                        var rowStyle = tableLayout.RowStyles[i];
                        LogDebug($"Row {i}: SizeType={rowStyle.SizeType}, Height={rowStyle.Height}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error in LogMenuDimensions: {ex.Message}");
            }
        }

        private MenuStrip BuildMenu()
        {
            var ms = new MenuStrip();
            var settings = new ToolStripMenuItem(TranslationManager.GetString("menu.settings"));
            var mAbout = new ToolStripMenuItem(TranslationManager.GetString("menu.about"));
            mAbout.Click += (s,e) => { using (var a = new AboutForm()) a.ShowDialog(this); };

            // Add Monitored folders menu item only if folder monitoring is enabled
            if (Program.Config.EnableFolderMonitoring)
            {
                var mFolders = new ToolStripMenuItem(TranslationManager.GetString("menu.monitored_folders"));
                mFolders.Click += (s,e) => {
                    using (var dlg = new SettingsForm())
                    {
                        dlg.ShowDialog(this);
                        StartWatchers();
                    }
                };
                settings.DropDownItems.Add(mFolders);
                settings.DropDownItems.Add(new ToolStripSeparator());
            }
            
            settings.DropDownItems.Add(mAbout);
            ms.Items.Add(settings);

            // Add Test menu (conditionally)
            if (Program.Config.ShowTestMenu)
            {
                var test = new ToolStripMenuItem(TranslationManager.GetString("menu.test"));
                var mTestEmail = new ToolStripMenuItem(TranslationManager.GetString("menu.test_email"));
                mTestEmail.Click += async (s, e) => await TestChangeSelectedEmail();
                test.DropDownItems.Add(mTestEmail);
                ms.Items.Add(test);
            }

            // Add Clipboard menu (conditionally)
            ToolStripMenuItem mToggleClipboard = null;
            if (Program.Config.ShowClipboardMenu)
            {
                var clipboard = new ToolStripMenuItem(TranslationManager.GetString("menu.clipboard"));
                mToggleClipboard = new ToolStripMenuItem(TranslationManager.GetString("menu.toggle_clipboard"));
                mToggleClipboard.Click += (s, e) => ToggleClipboardMonitoring();
                clipboard.DropDownItems.Add(mToggleClipboard);
                
                var mTestClipboard = new ToolStripMenuItem(TranslationManager.GetString("menu.test_clipboard"));
                mTestClipboard.Click += (s, e) => {
                    LogDebug(TranslationManager.GetString("debug.menu_clicked"));
                    TestClipboardCheck();
                };
                clipboard.DropDownItems.Add(mTestClipboard);
                
                ms.Items.Add(clipboard);
            }

            // Store reference to toggle menu item for updating text (only if clipboard menu is shown)
            if (Program.Config.ShowClipboardMenu && mToggleClipboard != null)
            {
                _toggleClipboardMenuItem = mToggleClipboard;
                UpdateClipboardMenuText();
            }

            return ms;
        }

        private async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                var wa = Screen.FromControl(this).WorkingArea;
                int width = 390;
                
                // Calculate height using fixed menu and status bar heights
                int menuHeight = 30; // Fixed menu height
                int statusHeight = 22; // Fixed status height
                int availableHeight = (int)(wa.Height * 0.90);
                int webViewHeight = availableHeight - menuHeight - statusHeight;
                
                // Ensure minimum height
                int totalHeight = Math.Max(400, menuHeight + webViewHeight + statusHeight);
                
                this.Size = new Size(width, totalHeight);
                this.Location = new Point(wa.Right - this.Width, wa.Top);

                var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                            "CRMTogether", "PwaHost", "WebView2UserData");
                Directory.CreateDirectory(dataPath);

                LogDebug("Creating WebView2 environment...");
                var env = await CoreWebView2Environment.CreateAsync(userDataFolder: dataPath);
                LogDebug("WebView2 environment created successfully");

                LogDebug("Initializing WebView2...");
                await _webView.EnsureCoreWebView2Async(env);
                LogDebug("WebView2 initialized successfully");
                
                // Ensure WebView is properly sized after initialization
                await Task.Delay(100); // Small delay to ensure WebView is ready
                EnsureWebViewProperSize();
                
                // Log menu dimensions for debugging
                LogMenuDimensions();
            }
            catch (Exception ex)
            {
                LogDebug($"Error during WebView2 initialization: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
                SetStatusMessage($"WebView2 initialization failed: {ex.Message}");
                // MessageBox.Show(this, $"Failed to initialize WebView2: {ex.Message}\n\nPlease try restarting the application.", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                LogDebug("Configuring WebView2 settings...");
                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = true;
                _webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
                LogDebug("WebView2 settings configured successfully");

                // Inject PWA host object into the web page
                LogDebug("Injecting PWA host object...");
                await InjectPwaHostObject();
                LogDebug("PWA host object injected successfully");

                LogDebug("Setting up WebView2 event handlers...");
                _webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                _webView.CoreWebView2.WebResourceRequested += (s, e) =>
                {
                    try
                    {
                        foreach (var kv in _extraHeaders)
                        {
                            try { e.Request.Headers.SetHeader(kv.Key, kv.Value); } catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogDebug($"Error in WebResourceRequested handler: {ex.Message}");
                    }
                };

                // Re-inject PWA host object when navigation completes
                _webView.CoreWebView2.NavigationCompleted += async (s, e) =>
                {
                    try
                    {
                        LogDebug($"Navigation completed. Success: {e.IsSuccess}");
                        if (e.IsSuccess)
                        {
                            await InjectPwaHostObject();
                            // Save the current URL as the last visited URL
                            SaveCurrentUrl();
                        }
                        else
                        {
                            LogDebug($"Navigation failed: {e.WebErrorStatus}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogDebug($"Error in NavigationCompleted handler: {ex.Message}");
                    }
                };

                // Add error event handlers
                _webView.CoreWebView2.ProcessFailed += (s, e) =>
                {
                    LogDebug($"WebView2 process failed: {e.ProcessFailedKind}, ExitCode: {e.ExitCode}");
                    SetStatusMessage($"WebView2 process failed: {e.ProcessFailedKind}");
                    // MessageBox.Show(this, $"WebView2 process failed: {e.ProcessFailedKind}\n\nThe application may need to be restarted.", "WebView2 Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };

                LogDebug("WebView2 event handlers set up successfully");
            }
            catch (Exception ex)
            {
                LogDebug($"Error setting up WebView2: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
                SetStatusMessage($"WebView2 configuration failed: {ex.Message}");
                // MessageBox.Show(this, $"Failed to configure WebView2: {ex.Message}", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _initialized = true;

            if (!string.IsNullOrWhiteSpace(_pendingUrl)) { var u = _pendingUrl; _pendingUrl = null; SafeNavigate(u); }
            else if (!string.IsNullOrWhiteSpace(InitialUrl)) { SafeNavigate(InitialUrl); }

            StartWatchers();
        }

        private void StartWatchers()
        {
            try
            {
                LogDebug($"StartWatchers called. Current watchers count: {_watchers.Count}");
                
                foreach (var w in _watchers) { try { w.Dispose(); } catch { } }
                _watchers.Clear();

                // Check if folder monitoring is enabled
                if (!Program.Config.EnableFolderMonitoring)
                {
                    LogDebug("Folder monitoring is disabled for this environment, skipping watcher creation");
                    return;
                }

                LogDebug($"WatchedFolders count: {Program.Config.WatchedFolders.Count}");
                foreach (var folder in Program.Config.WatchedFolders)
                {
                    LogDebug($"Processing folder: '{folder}'");
                    if (string.IsNullOrWhiteSpace(folder))
                    {
                        LogDebug("Folder is null or whitespace, skipping");
                        continue;
                    }
                    if (!Directory.Exists(folder))
                    {
                        LogDebug($"Directory does not exist: '{folder}', skipping");
                        continue;
                    }
                    
                    var fsw = new FileSystemWatcher(folder);
                    fsw.IncludeSubdirectories = false;
                    fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                    fsw.Created += (s, e) => OnFileAppeared(e.FullPath);
                    fsw.Renamed += (s, e) => OnFileAppeared(e.FullPath);
                    fsw.EnableRaisingEvents = true;
                    _watchers.Add(fsw);
                    
                    LogDebug($"Created FileSystemWatcher for: '{folder}', Enabled: {fsw.EnableRaisingEvents}");
                }
                
                LogDebug($"Total watchers created: {_watchers.Count}");
            }
            catch (Exception ex)
            {
                LogDebug($"Error in StartWatchers: {ex.Message}");
            }
        }

        private void OnFileAppeared(string path)
        {
            try
            {
                LogDebug($"OnFileAppeared called for: '{path}'");
                var extension = Path.GetExtension(path);
                LogDebug($"File extension: '{extension}'");
                
                // Skip .tmp files - these are temporary files created during downloads
                if (path.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                {
                    LogDebug("Skipping .tmp file - temporary download file, waiting for final rename");
                    return;
                }
                
                if (string.Equals(extension, ".eml", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".phone", StringComparison.OrdinalIgnoreCase))
                {
                    LogDebug($"Copying {extension} file to processing folder");
                    CopyFileToProcessingFolder(path);
                }
                else
                {
                    LogDebug($"File extension '{extension}' not supported");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error in OnFileAppeared: {ex.Message}");
            }
        }

        private void CopyFileToProcessingFolder(string sourcePath)
        {
            try
            {
                // Ensure processing folder exists
                Program.Config.EnsureProcessingFolders();
                
                // Generate unique filename to avoid conflicts
                var fileName = Path.GetFileName(sourcePath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var extension = Path.GetExtension(fileName);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var uniqueFileName = $"{nameWithoutExt}_{timestamp}{extension}";
                var processingPath = Path.Combine(Program.Config.ProcessingFolder, uniqueFileName);
                
                LogDebug($"Copying file from '{sourcePath}' to '{processingPath}'");
                
                // Copy the file to processing folder
                File.Copy(sourcePath, processingPath, overwrite: true);
                
                LogDebug($"File copied successfully to processing folder");
                
                // Process the copied file
                this.BeginInvoke((Action)(async () =>
                {
                    await ProcessFileFromProcessingFolder(processingPath);
                }));
            }
            catch (Exception ex)
            {
                LogDebug($"Error copying file to processing folder: {ex.Message}");
            }
        }

        private async Task ProcessFileFromProcessingFolder(string processingPath)
        {
            try
            {
                var extension = Path.GetExtension(processingPath);
                LogDebug($"Processing file from processing folder: {processingPath}");
                
                if (string.Equals(extension, ".eml", StringComparison.OrdinalIgnoreCase))
                {
                    await ProcessEmlFileWithRetry(processingPath);
                }
                else if (string.Equals(extension, ".phone", StringComparison.OrdinalIgnoreCase))
                {
                    await ProcessPhoneFileWithRetry(processingPath);
                }
                
                // Move file to processed folder after successful processing
                MoveFileToProcessedFolder(processingPath);
            }
            catch (Exception ex)
            {
                LogDebug($"Error processing file from processing folder: {ex.Message}");
                // Move file to processed folder even if processing failed
                MoveFileToProcessedFolder(processingPath);
            }
        }

        private void MoveFileToProcessedFolder(string processingPath)
        {
            try
            {
                var fileName = Path.GetFileName(processingPath);
                var processedPath = Path.Combine(Program.Config.ProcessedFolder, fileName);
                
                LogDebug($"Moving file from '{processingPath}' to '{processedPath}'");
                
                // Move the file to processed folder
                File.Move(processingPath, processedPath);
                
                LogDebug($"File moved successfully to processed folder");
            }
            catch (Exception ex)
            {
                LogDebug($"Error moving file to processed folder: {ex.Message}");
                // If move fails, try to delete the processing file to clean up
                try
                {
                    if (File.Exists(processingPath))
                    {
                        File.Delete(processingPath);
                        LogDebug("Cleaned up processing file after failed move");
                    }
                }
                catch (Exception deleteEx)
                {
                    LogDebug($"Error cleaning up processing file: {deleteEx.Message}");
                }
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var f in files)
                {
                    var extension = Path.GetExtension(f);
                    if (string.Equals(extension, ".eml", StringComparison.OrdinalIgnoreCase))
                    {
                        _ = ProcessEmlFile(f); // Fire and forget async call
                    }
                    else if (string.Equals(extension, ".phone", StringComparison.OrdinalIgnoreCase))
                    {
                        _ = ProcessPhoneFile(f); // Fire and forget async call
                    }
                    else
                    {
                        var uri = new Uri(f).AbsoluteUri;
                        SafeNavigate(uri);
                    }
                }
            }
            catch { }
        }

        public void Navigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (File.Exists(url)) { try { url = new Uri(Path.GetFullPath(url)).AbsoluteUri; } catch {} }
            if (!_initialized || _webView?.CoreWebView2 == null) { _pendingUrl = url; return; }
            SafeNavigate(url);
        }

        private void SafeNavigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            if (!IsAllowed(url)) return;
            try { _webView.CoreWebView2?.Navigate(url); } catch { }
        }

        private bool IsAllowed(string url)
        {
            foreach (var p in AllowedPrefixes)
            {
                if (url.StartsWith(p, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public void Reload() => _webView.CoreWebView2?.Reload();
        public void GoBack() { if (_webView.CoreWebView2?.CanGoBack == true) _webView.CoreWebView2.GoBack(); }
        public void ExecuteScriptAsync(string js) { if (string.IsNullOrWhiteSpace(js)) return; _ = _webView?.CoreWebView2?.ExecuteScriptAsync(js); }
        public string ExecuteScriptWithResultBlocking(string js)
        {
            if (string.IsNullOrWhiteSpace(js) || _webView?.CoreWebView2 == null) return string.Empty;
            try
            {
                var raw = _webView.CoreWebView2.ExecuteScriptAsync(js).GetAwaiter().GetResult();
                try { return System.Text.Json.JsonSerializer.Deserialize<string>(raw) ?? raw; } catch { return raw; }
            }
            catch { return string.Empty; }
        }
        public string GetCurrentUrl() => _webView.CoreWebView2?.Source ?? string.Empty;
        
        private void SaveCurrentUrl()
        {
            try
            {
                var currentUrl = GetCurrentUrl();
                if (!string.IsNullOrWhiteSpace(currentUrl) && IsAllowed(currentUrl))
                {
                    Program.Config.LastUrl = currentUrl;
                    Program.Config.Save();
                    LogDebug($"Saved last URL: {currentUrl}");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error saving current URL: {ex.Message}");
            }
        }
        public string GetTitle()
        {
            try { var t = _webView.CoreWebView2?.DocumentTitle; if (!string.IsNullOrEmpty(t)) return t; } catch { }
            return ExecuteScriptWithResultBlocking("document.title");
        }
        public string GetHtmlBlocking() => ExecuteScriptWithResultBlocking("document.documentElement.outerHTML");
        public bool PrintToPdfBlocking(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || _webView?.CoreWebView2 == null) return false;
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var settings = _webView.CoreWebView2.Environment.CreatePrintSettings();
                settings.Orientation = CoreWebView2PrintOrientation.Portrait;
                settings.ShouldPrintBackgrounds = true;
                settings.ShouldPrintHeaderAndFooter = false;
                return _webView.CoreWebView2.PrintToPdfAsync(filePath, settings).GetAwaiter().GetResult();
            }
            catch { return false; }
        }
        public void SetZoom(double factor) { if (factor <= 0.1 || factor > 5.0) return; try { _webView.ZoomFactor = factor; } catch { } }
        public double GetZoom() { try { return _webView.ZoomFactor; } catch { return 1.0; } }
        public void AddHeader(string name, string value) { if (string.IsNullOrWhiteSpace(name)) return; _extraHeaders[name] = value ?? string.Empty; }
        public void RemoveHeader(string name) { if (string.IsNullOrWhiteSpace(name)) return; _extraHeaders.Remove(name); }
        public void ClearHeaders() => _extraHeaders.Clear();
        public void NavigateWithRequest(string url, string method, string headers, string bodyUtf8)
        {
            if (_webView?.CoreWebView2 == null || string.IsNullOrWhiteSpace(url)) return;
            if (!IsAllowed(url)) return;
            try
            {
                var bytes = string.IsNullOrEmpty(bodyUtf8) ? null : new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(bodyUtf8));
                var req = _webView.CoreWebView2.Environment.CreateWebResourceRequest(
                    url,
                    string.IsNullOrWhiteSpace(method) ? "GET" : method,
                    bytes,
                    headers ?? string.Empty
                );
                _webView.CoreWebView2.NavigateWithWebResourceRequest(req);
            }
            catch { }
        }
        public void SetUserAgent(string userAgent) { if (_webView?.CoreWebView2 == null) return; try { _webView.CoreWebView2.Settings.UserAgent = userAgent; } catch { } }
        public string GetUserAgent() { try { return _webView.CoreWebView2?.Settings?.UserAgent ?? string.Empty; } catch { return string.Empty; } }
        
        public string GetEmailData(string sender)
        {
            try
            {
                if (_lastEmailObject == null)
                {
                    LogDebug("No email data available - no email object has been set yet");
                    return string.Empty;
                }

                // Serialize the stored email object to JSON
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(_lastEmailObject);
                LogDebug($"Retrieved email data for sender: {sender}");
                return json;
            }
            catch (Exception ex)
            {
                LogDebug($"Error retrieving email data: {ex.Message}");
                return string.Empty;
            }
        }

        public void SetLastEmailObject(object emailObject)
        {
            _lastEmailObject = emailObject;
            LogDebug("Email object stored for later retrieval");
        }
        
        public void addParam(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _contextParams[name] = value ?? string.Empty;
            LogDebug($"Added context parameter: {name} = {value}");
        }

        public void clearParams()
        {
            _contextParams.Clear();
            LogDebug("Cleared all context parameters");
        }

        public string getParams()
        {
            var paramList = new List<string>();
            foreach (var param in _contextParams)
            {
                paramList.Add($"{param.Key}={param.Value}");
            }
            return string.Join(", ", paramList);
        }

        private string GetContextParam(string key)
        {
            return _contextParams.TryGetValue(key, out string value) ? value : "";
        }

        public void ProcessContextValue(string value, string source, string type = "email")
        {
            try
            {
                LogDebug($"ProcessContextValue called with value: {value}, source: {source}, type: {type}");
                SetStatusMessage(TranslationManager.GetString("processing.email", source, value));
                
                // Store the value as a context parameter
                addParam("emailAddress", value);
                addParam("source", source);
                addParam("contentType", type);
                
                // Create a context entity for the email
                OpenEntity("email", value);
            }
            catch (Exception ex)
            {
                LogDebug($"Error processing context value: {ex.Message}");
                SetStatusMessage(TranslationManager.GetString("error.processing", "context", ex.Message));
            }
        }

        public void ProcessPhoneValue(string value, string source)
        {
            try
            {
                LogDebug($"ProcessPhoneValue called with value: {value}, source: {source}");
                SetStatusMessage(TranslationManager.GetString("processing.phone", source, value));
                
                // Store the phone number as a context parameter
                addParam("phoneNumber", value);
                addParam("source", source);
                addParam("contentType", "phone");

                // Create a phone entity
                OpenEntity("phone", value);
            }
            catch (Exception ex)
            {
                LogDebug($"Error processing phone value: {ex.Message}");
                SetStatusMessage(TranslationManager.GetString("error.phone_processing", ex.Message));
            }
        }

        public void ProcessWebsiteValue(string value, string source)
        {
            try
            {
                LogDebug($"ProcessWebsiteValue called with value: {value}, source: {source}");
                SetStatusMessage(TranslationManager.GetString("processing.website", source, value));
                
                // Store the website as a context parameter
                addParam("website", value);
                addParam("source", source);
                addParam("contentType", "website");
                
                // Create a website entity
                OpenEntity("website", value);
            }
            catch (Exception ex)
            {
                LogDebug($"Error processing website value: {ex.Message}");
                SetStatusMessage(TranslationManager.GetString("error.website_processing", ex.Message));
            }
        }

        public void ProcessTextValue(string value, string source)
        {
            try
            {
                LogDebug($"ProcessTextValue called with value: {value}, source: {source}");
                
                // Check if this is multiline text that might contain contact information
                if (value.Contains("\n") || value.Contains("\r"))
                {
                    LogDebug("Multiline text detected, analyzing for contact information");
                    ProcessMultilineContactInfo(value, source);
                }
                else
                {
                    // Single line text - treat as regular text
                    SetStatusMessage(TranslationManager.GetString("processing.text", source, value));
                    
                    // Store the text as a context parameter
                    addParam("textValue", value);
                    addParam("source", source);
                    addParam("contentType", "text");
                    
                    // Create a text entity (could be name, company, etc.)
                    OpenEntity("text", value);
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error processing text value: {ex.Message}");
                SetStatusMessage(TranslationManager.GetString("error.text_processing", ex.Message));
            }
        }

        private void ProcessMultilineContactInfo(string value, string source)
        {
            try
            {
                LogDebug("Processing multiline text for contact information");
                
                // Extract different types of contact information
                var emailMatches = EmailRegex.Matches(value);
                var phoneMatches = PhoneRegex.Matches(value);
                var websiteMatches = WebsiteRegex.Matches(value);
                
                LogDebug($"Found {emailMatches.Count} emails, {phoneMatches.Count} phones, {websiteMatches.Count} websites in multiline text");
                
                // Create a comprehensive contact object
                var contactInfo = new
                {
                    originalText = value.Trim(),
                    companyName = ExtractCompanyName(value),
                    emails = emailMatches.Cast<Match>().Select(m => m.Value).ToArray(),
                    phoneNumbers = phoneMatches.Cast<Match>().Select(m => m.Value).ToArray(),
                    websites = websiteMatches.Cast<Match>().Select(m => m.Value).ToArray(),
                    address = ExtractAddress(value),
                    source = source,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    hasEmail = emailMatches.Count > 0,
                    hasPhone = phoneMatches.Count > 0,
                    hasWebsite = websiteMatches.Count > 0,
                    hasAddress = !string.IsNullOrWhiteSpace(ExtractAddress(value))
                };
                
                // Store all the extracted information
                addParam("contactInfo", Newtonsoft.Json.JsonConvert.SerializeObject(contactInfo));
                addParam("companyName", contactInfo.companyName);
                addParam("emails", string.Join(";", contactInfo.emails));
                addParam("phoneNumbers", string.Join(";", contactInfo.phoneNumbers));
                addParam("websites", string.Join(";", contactInfo.websites));
                addParam("address", contactInfo.address);
                addParam("source", source);
                addParam("contentType", "contact");
                
                // Create status message based on what was found
                var foundItems = new List<string>();
                if (contactInfo.hasEmail) foundItems.Add($"{contactInfo.emails.Length} email(s)");
                if (contactInfo.hasPhone) foundItems.Add($"{contactInfo.phoneNumbers.Length} phone(s)");
                if (contactInfo.hasWebsite) foundItems.Add($"{contactInfo.websites.Length} website(s)");
                if (contactInfo.hasAddress) foundItems.Add("address");
                
                var statusMessage = foundItems.Count > 0 
                    ? $"Contact info detected: {string.Join(", ", foundItems)}"
                    : "Multiline text processed (no contact info detected)";
                
                SetStatusMessage(statusMessage);
                LogDebug($"Contact processing result: {statusMessage}");
                
                // Create a contact entity
                OpenEntity("contact", contactInfo.companyName ?? "Unknown Company");
            }
            catch (Exception ex)
            {
                LogDebug($"Error processing multiline contact info: {ex.Message}");
                SetStatusMessage($"Error processing contact information: {ex.Message}");
            }
        }

        private string ExtractCompanyName(string text)
        {
            try
            {
                // Get the first non-empty line, which is often the company name
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    var firstLine = lines[0].Trim();
                    // If the first line looks like a company name (not an email, phone, or website)
                    if (!EmailRegex.IsMatch(firstLine) && 
                        !PhoneRegex.IsMatch(firstLine) && 
                        !WebsiteRegex.IsMatch(firstLine) &&
                        !firstLine.Contains("@") &&
                        firstLine.Length > 2)
                    {
                        return firstLine;
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error extracting company name: {ex.Message}");
            }
            return null;
        }

        private string ExtractAddress(string text)
        {
            try
            {
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var addressLines = new List<string>();
                
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    // Skip if it's an email, phone, or website
                    if (EmailRegex.IsMatch(trimmedLine) || 
                        PhoneRegex.IsMatch(trimmedLine) || 
                        WebsiteRegex.IsMatch(trimmedLine))
                    {
                        continue;
                    }
                    
                    // Skip if it's likely a company name (first line)
                    if (addressLines.Count == 0 && lines.Length > 2)
                    {
                        continue;
                    }
                    
                    // Look for address-like patterns (contains postal codes, street indicators, etc.)
                    if (trimmedLine.Length > 5 && 
                        (trimmedLine.Contains(",") || 
                         trimmedLine.Contains("Street") || 
                         trimmedLine.Contains("Road") || 
                         trimmedLine.Contains("Avenue") || 
                         trimmedLine.Contains("Lane") || 
                         trimmedLine.Contains("Drive") || 
                         trimmedLine.Contains("UK") || 
                         trimmedLine.Contains("USA") || 
                         trimmedLine.Contains("US") ||
                         System.Text.RegularExpressions.Regex.IsMatch(trimmedLine, @"\b[A-Z]{1,2}\d{1,2}[A-Z]?\s?\d[A-Z]{2}\b") || // UK postal code
                         System.Text.RegularExpressions.Regex.IsMatch(trimmedLine, @"\b\d{5}(-\d{4})?\b"))) // US ZIP code
                    {
                        addressLines.Add(trimmedLine);
                    }
                }
                
                return addressLines.Count > 0 ? string.Join(", ", addressLines) : null;
            }
            catch (Exception ex)
            {
                LogDebug($"Error extracting address: {ex.Message}");
                return null;
            }
        }

        public void ProcessAddressValue(string value, string source)
        {
            try
            {
                LogDebug($"ProcessAddressValue called with value: '{value}', source: {source}");
                LogDebug($"About to call SetStatusMessage for address processing");
                SetStatusMessage(TranslationManager.GetString("processing.address", source, value));
                LogDebug($"SetStatusMessage called successfully for address");
                
                // Store the address as a context parameter
                addParam("address", value);
                addParam("source", source);
                addParam("contentType", "address");
                
                // Create an address entity
                OpenEntity("address", value);
                LogDebug($"ProcessAddressValue completed successfully");
            }
            catch (Exception ex)
            {
                LogDebug($"Error processing address value: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
                SetStatusMessage(TranslationManager.GetString("error.address_processing", ex.Message));
            }
        }

        private bool IsEmailAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return EmailRegex.IsMatch(value);
        }

        public void OpenEntity(string entityType, string entityId)
        {
            try
            {
                LogDebug($"OpenEntity called with type: {entityType}, id: {entityId}");
                SetStatusMessage($"Opening {entityType} with ID: {entityId}");
                
                // Build a minimal EML-like JSON object using stored context parameters
                //customMessage is our own property for searching on 3rd party codes...codeOrId is the id of the entity or a code parsed on the server side
                var customMessageData = new Dictionary<string, object>
                {
                    ["entity"] = entityType,
                    ["codeOrId"] = entityId
                };
                
                // Add all stored context parameters to customMessage
                foreach (var param in _contextParams)
                {
                    customMessageData[param.Key] = param.Value;
                }
                
                var emailObject = new
                {
                    customMessage = customMessageData,
                    from = new
                    {
                        emailAddress = GetContextParam("emailAddress"),
                        displayName = GetContextParam("name") ?? GetContextParam("emailAddress"),
                        type = (string)null
                    },
                    replyto = (string)null,
                    fullName = GetContextParam("name") ?? GetContextParam("emailAddress"),
                    phoneNumbers = new [] {
                        new {
                            type = "Mobile",
                            number = GetContextParam("phoneNumber")
                        }
                    },
                    to = new[]
                    {
                        new
                        {
                            emailAddress = GetContextParam("emailAddress"),
                            displayName = GetContextParam("ContactName") ?? GetContextParam("emailAddress"),
                            type = "Business"
                        }
                    },
                    cc = new object[0],
                    bcc = new object[0],
                    subject = "",
                    body = "",
                    htmlBody = (string)null,
                    attachments = (object)null,
                    entryid = $"ENTITY_{entityType}_{entityId}",
                    urls = new object[0],
                    addresses = new [] {
                        new {
                            address = GetContextParam("address")
                        }
                    },
                    sentItem = false,
                    receivedDateTime = new
                    {
                        year = DateTime.Now.Year,
                        month = DateTime.Now.Month,
                        day = DateTime.Now.Day,
                        hour = DateTime.Now.Hour,
                        minute = DateTime.Now.Minute,
                        second = DateTime.Now.Second,
                        raw = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                        rawutc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        TZ = new
                        {
                            StandardName = TimeZoneInfo.Local.StandardName,
                            DaylightName = TimeZoneInfo.Local.DaylightName
                        }
                    },
                    sentDateTime = new
                    {
                        year = DateTime.Now.Year,
                        month = DateTime.Now.Month,
                        day = DateTime.Now.Day,
                        hour = DateTime.Now.Hour,
                        minute = DateTime.Now.Minute,
                        second = DateTime.Now.Second,
                        raw = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                        rawutc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        TZ = new
                        {
                            StandardName = TimeZoneInfo.Local.StandardName,
                            DaylightName = TimeZoneInfo.Local.DaylightName
                        }
                    },
                    companies = (object)null
                };

                // Store the email object for later retrieval
                _lastEmailObject = emailObject;

                // Serialize to JSON
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(emailObject);

                // Call the browser function as in the EML test function
                var _ = CallBrowserFunctionAsync("changeSelectedEmail", json);
                
                LogDebug($"OpenEntity placeholder completed for {entityType}:{entityId}");
            }
            catch (Exception ex)
            {
                LogDebug($"Error in OpenEntity: {ex.Message}");
                SetStatusMessage($"Error opening {entityType}: {ex.Message}");
            }
        }

        // Additional methods for PwaHostObject
        public void SetSize(int width, int height)
        {
            try { Size = new System.Drawing.Size(width, height); } catch { }
        }

        public void ExecuteScript(string js)
        {
            if (_webView?.CoreWebView2 == null || string.IsNullOrWhiteSpace(js)) return;
            try { _webView.CoreWebView2.ExecuteScriptAsync(js); } catch { }
        }

        public async Task<object> CallBrowserFunctionAsync(string functionName, params object[] args)
        {
            if (_webView?.CoreWebView2 == null || string.IsNullOrWhiteSpace(functionName)) 
            {
                LogDebug($"Cannot call browser function {functionName}: WebView2 is null or function name is empty");
                return null;
            }

            try
            {
                LogDebug($"Calling browser function: {functionName}");
                var result = await _webView.ExecuteScriptFunctionAsync(functionName, args);
                LogDebug($"Browser function {functionName} returned: {result}");
                return result;
            }
            catch (Exception ex)
            {
                LogDebug($"Error calling browser function {functionName}: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<T> CallBrowserFunctionAsync<T>(string functionName, params object[] args)
        {
            if (_webView?.CoreWebView2 == null || string.IsNullOrWhiteSpace(functionName)) 
            {
                LogDebug($"Cannot call browser function {functionName}: WebView2 is null or function name is empty");
                return default(T);
            }

            try
            {
                LogDebug($"Calling browser function: {functionName} (generic)");
                var result = await _webView.ExecuteScriptFunctionAsync(functionName, args);
                
                // Try to deserialize the result to the specified type
                if (result != null)
                {
                    var jsonString = result.ToString();
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)jsonString;
                    }
                    
                    try
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
                    }
                    catch
                    {
                        // If JSON deserialization fails, try direct conversion
                        return (T)Convert.ChangeType(result, typeof(T));
                    }
                }
                
                return default(T);
            }
            catch (Exception ex)
            {
                LogDebug($"Error calling browser function {functionName}: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
                return default(T);
            }
        }

        private async Task InjectPwaHostObject()
        {
            if (_webView?.CoreWebView2 == null) 
            {
                LogDebug("Cannot inject PWA host object: WebView2 is null");
                return;
            }

            try
            {
                LogDebug("Injecting PWA host object...");
                // Use AddHostObjectToScript to inject the C# object directly
                _webView.CoreWebView2.AddHostObjectToScript("pwa", new PwaHostObject(this));
                
                // Add a more robust script to make the object available as window.pwa
                await _webView.CoreWebView2.ExecuteScriptAsync(@"
                    try {
                        // Store the host object reference
                        const hostObject = chrome.webview.hostObjects.pwa;
                        console.log('Host object retrieved:', hostObject);
                        console.log('Host object type:', typeof hostObject);
                        
                        // Create a wrapper that handles async calls properly
                        window.pwa = {
                            // Simple synchronous methods
                            test: () => hostObject.Test(),
                            getCurrentTime: () => hostObject.GetCurrentTime(),
                            getVersion: () => hostObject.GetVersion(),
                            
                            // Async methods that return promises
                            getEmailData: (sender) => Promise.resolve(hostObject.getEmailData(sender)),
                            getHomePage: () => Promise.resolve(hostObject.GetHomePage()),
                            setHomePage: (url) => Promise.resolve(hostObject.SetHomePage(url)),
                            navigate: (url) => Promise.resolve(hostObject.Navigate(url)),
                            reload: () => Promise.resolve(hostObject.Reload()),
                            goBack: () => Promise.resolve(hostObject.GoBack()),
                            getCurrentUrl: () => Promise.resolve(hostObject.GetCurrentUrl()),
                            getTitle: () => Promise.resolve(hostObject.GetTitle()),
                            bringToFront: () => Promise.resolve(hostObject.BringToFront()),
                            setSize: (width, height) => Promise.resolve(hostObject.SetSize(width, height)),
                            executeScript: (js) => Promise.resolve(hostObject.ExecuteScript(js)),
                            log: (message) => Promise.resolve(hostObject.Log(message)),
                            testChangeSelectedEmail: () => hostObject.TestChangeSelectedEmail()
                        };
                        
                        console.log('PWA wrapper created successfully');
                        console.log('PWA object methods:', Object.keys(window.pwa));
                        
                        // Test the simple methods
                        try {
                            const testResult = window.pwa.test();
                            console.log('PWA test method successful:', testResult);
                        } catch (error) {
                            console.error('PWA test method failed:', error);
                        }
                        
                    } catch (error) {
                        console.error('Error setting up PWA object:', error);
                        console.error('Error details:', error.message, error.stack);
                    }
                ");
                LogDebug("PWA host object injected successfully");
            }
            catch (Exception ex)
            {
                LogDebug($"Error injecting PWA host object: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
                // Don't throw the exception, just log it
            }
        }

        public new void BringToFront()
        {
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
            TopMost = true;
            TopMost = false;
        }

        public void RegisterScript(string name, string filePath)
        {
            var root = Program.Config.JsScriptsRoot;
            try { Directory.CreateDirectory(root); } catch { }
            string p = filePath;
            if (!Path.IsPathRooted(p)) p = Path.Combine(root, p);
            Program.Config.JsScripts.RemoveAll(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            Program.Config.JsScripts.Add(new JsScriptEntry { Name = name, File = p });
            Program.Config.Save();
        }

        public void LogDebug(string message)
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

        private void SetStatusMessage(string message)
        {
            try
            {
                if (_statusLabel != null)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => _statusLabel.Text = message));
                    }
                    else
                    {
                        _statusLabel.Text = message;
                    }
                }
            }
            catch { }
        }

        public string[] ListScripts()
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (var s in Program.Config.JsScripts) list.Add(s.Name);
            return list.ToArray();
        }

        public bool RunScriptByName(string name, string args)
        {
            var s = Program.Config.JsScripts.Find(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (s == null) return false;
            return RunScriptFile(s.File, args);
        }

        public bool RunScriptFile(string filePath, string args)
        {
            try
            {
                string cscript = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\cscript.exe");
                if (!File.Exists(cscript)) return false;
                var psi = new ProcessStartInfo
                {
                    FileName = cscript,
                    Arguments = $"//E:JScript //nologo \"{filePath}\" {args ?? ""}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var p = Process.Start(psi);
                return p != null;
            }
            catch { return false; }
        }

        private async Task ProcessEmlFileWithRetry(string filePath)
        {
            const int maxRetries = 5;
            const int delayMs = 1000; // 1 second delay between retries
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Check if file is accessible
                    if (IsFileAccessible(filePath))
                    {
                        await ProcessEmlFile(filePath);
                        return; // Success, exit retry loop
                    }
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        SetStatusMessage($"Error processing EML file after {maxRetries} attempts: {ex.Message}");
                        return;
                    }
                }
                
                // Wait before next attempt
                if (attempt < maxRetries)
                {
                    await Task.Delay(delayMs);
                }
            }
        }

        private bool IsFileAccessible(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task ProcessEmlFile(string filePath)
        {
            try
            {
                var info = EmlParser.Parse(filePath);
                var emailObject = ConvertEmlToEmailObject(info, filePath);
                
                // Store the email object for later retrieval
                _lastEmailObject = emailObject;
                
                // Call the browser function
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(emailObject);
                var result = await CallBrowserFunctionAsync("changeSelectedEmail", json);
                
                if (result != null)
                {
                    SetStatusMessage(TranslationManager.GetString("file.eml_processed", info.Subject, info.From));
                }
                else
                {
                    SetStatusMessage($"EML processed but changeSelectedEmail function not found. Subject: {info.Subject}, From: {info.From}");
                }
            }
            catch (Exception ex)
            {
                SetStatusMessage($"Error processing EML file: {ex.Message}");
            }
        }

        private async Task ProcessPhoneFileWithRetry(string filePath)
        {
            const int maxRetries = 5;
            const int delayMs = 1000; // 1 second delay between retries
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Check if file is accessible
                    if (IsFileAccessible(filePath))
                    {
                        await ProcessPhoneFile(filePath);
                        return; // Success, exit retry loop
                    }
                }
                catch (Exception ex)
                {
                    if (attempt == maxRetries)
                    {
                        SetStatusMessage($"Error processing phone file after {maxRetries} attempts: {ex.Message}");
                        return;
                    }
                }
                
                // Wait before next attempt
                if (attempt < maxRetries)
                {
                    await Task.Delay(delayMs);
                }
            }
        }

        private async Task ProcessPhoneFile(string filePath)
        {
            try
            {
                SetStatusMessage(TranslationManager.GetString("file.processing", "phone", Path.GetFileName(filePath)));
                // Read the phone number from the file
                var phoneNumber = File.ReadAllText(filePath);
                phoneNumber = phoneNumber.Trim(); // Remove any whitespace
                
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    SetStatusMessage(TranslationManager.GetString("file.empty", "Phone", "phone number"));
                    return;
                }
                
                // Create a phone object similar to the email object
                var phoneObject = new
                {
                    phoneNumber = phoneNumber,
                    source = "file",
                    fileName = Path.GetFileName(filePath),
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                // Call the browser function using the same method as EML files
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(phoneObject);
                var result = await CallBrowserFunctionAsync("changeSelectedPhone", json);
                
                if (result != null)
                {
                    SetStatusMessage(TranslationManager.GetString("file.phone_processed", phoneNumber));
                }
                else
                {
                    SetStatusMessage($"Phone file processed but changeSelectedPhone function not found. Phone: {phoneNumber}");
                }
            }
            catch (Exception ex)
            {
                SetStatusMessage($"Error processing phone file: {ex.Message}");
            }
        }

        private object ConvertEmlToEmailObject(EmlInfo info, string filePath)
        {
            // Parse email addresses
            var fromAddress = ParseEmailAddress(info.From);
            var toAddresses = ParseEmailAddresses(info.To);
            
            // Use file creation time as received time, or email date if available
            var receivedTime = info.Date?.DateTime ?? File.GetCreationTime(filePath);
            var sentTime = info.Date?.DateTime ?? receivedTime;

            return new
            {
                from = fromAddress,
                replyto = (string)null,
                fullName = (fromAddress as dynamic)?.displayName ?? "",
                phoneNumbers = new object[0],
                to = toAddresses,
                cc = new object[0],
                bcc = new object[0],
                subject = info.Subject ?? "",
                body = info.HasText ? "Email body content available" : "",
                htmlBody = info.HasHtml ? "HTML content available" : (string)null,
                attachments = (object)null,
                entryid = $"EML_{Path.GetFileName(filePath)}_{DateTime.Now.Ticks}",
                urls = new object[0],
                addresses = (object)null,
                sentItem = false,
                receivedDateTime = new
                {
                    year = receivedTime.Year,
                    month = receivedTime.Month,
                    day = receivedTime.Day,
                    hour = receivedTime.Hour,
                    minute = receivedTime.Minute,
                    second = receivedTime.Second,
                    raw = receivedTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                    rawutc = receivedTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    TZ = new
                    {
                        StandardName = TimeZoneInfo.Local.StandardName,
                        DaylightName = TimeZoneInfo.Local.DaylightName
                    }
                },
                sentDateTime = new
                {
                    year = sentTime.Year,
                    month = sentTime.Month,
                    day = sentTime.Day,
                    hour = sentTime.Hour,
                    minute = sentTime.Minute,
                    second = sentTime.Second,
                    raw = sentTime.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                    rawutc = sentTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    TZ = new
                    {
                        StandardName = TimeZoneInfo.Local.StandardName,
                        DaylightName = TimeZoneInfo.Local.DaylightName
                    }
                },
                companies = (object)null
            };
        }

        private object ParseEmailAddress(string emailString)
        {
            if (string.IsNullOrWhiteSpace(emailString)) return null;
            
            // Simple email parsing - extract name and email
            var match = System.Text.RegularExpressions.Regex.Match(emailString, @"(.+?)\s*<(.+?)>|(.+)");
            if (match.Success)
            {
                var displayName = match.Groups[1].Value.Trim();
                var emailAddress = match.Groups[2].Value.Trim();
                
                if (string.IsNullOrEmpty(emailAddress))
                {
                    emailAddress = match.Groups[3].Value.Trim();
                    displayName = "";
                }
                
                return new
                {
                    displayName = displayName,
                    emailAddress = emailAddress,
                    type = (string)null
                };
            }
            
            return new
            {
                displayName = "",
                emailAddress = emailString.Trim(),
                type = (string)null
            };
        }

        private object[] ParseEmailAddresses(string emailString)
        {
            if (string.IsNullOrWhiteSpace(emailString)) return new object[0];
            
            // Split by comma and parse each address
            var addresses = emailString.Split(',');
            var result = new List<object>();
            
            foreach (var addr in addresses)
            {
                var parsed = ParseEmailAddress(addr.Trim());
                if (parsed != null) result.Add(parsed);
            }
            
            return result.ToArray();
        }

        private async Task TestChangeSelectedEmail()
        {
            try
            {
                // Create a sample email object matching the JSON structure
                var emailObject = new
                {
                    from = new
                    {
                        displayName = "Majella O'Connor",
                        emailAddress = "majella@crmtogether.com",
                        type = (string)null
                    },
                    replyto = (string)null,
                    fullName = "Majella O'Connor",
                    phoneNumbers = new object[0],
                    to = new[]
                    {
                        new
                        {
                            displayName = "marc@crmtogether.com",
                            emailAddress = "marc@crmtogether.com",
                            type = (string)null
                        }
                    },
                    cc = new object[0],
                    bcc = new object[0],
                    subject = "RE: names",
                    body = "email body here",
                    htmlBody = (string)null,
                    attachments = (object)null,
                    entryid = "000000008D29C22795ED0F43AE13B26188F01E9A0700774E149E6B7EAB44AD9FF58B53D5D33C00000000010C0000774E149E6B7EAB44AD9FF58B53D5D33C000781D207C90000",
                    urls = new object[0],
                    addresses = new[]
                    {
                        new
                        {
                            fullAddress = "3015 Lake Drive, Citywest Business Campus, Citywest, Dublin 24, D24DKP4, Ireland",
                            address1 = "3015 Lake Drive",
                            address2 = "Citywest Business Campus",
                            address3 = "Citywest",
                            address4 = "Dublin 24",
                            city = "Dublin",
                            state = "Dublin",
                            zip = "D24DKP4",
                            country = "Ireland",
                            type = "Business"
                        }
                    },
                    sentItem = false,
                    receivedDateTime = new
                    {
                        year = 2025,
                        month = 9,
                        day = 19,
                        hour = 14,
                        minute = 21,
                        second = 42,
                        raw = "2025-09-19T14:21:42.505",
                        rawutc = "2025-09-19T13:21:42.505Z",
                        TZ = new
                        {
                            StandardName = "GMT Standard Time",
                            DaylightName = "GMT Summer Time"
                        }
                    },
                    sentDateTime = new
                    {
                        year = 2025,
                        month = 9,
                        day = 19,
                        hour = 14,
                        minute = 21,
                        second = 38,
                        raw = "2025-09-19T14:21:38",
                        rawutc = "2025-09-19T13:21:38Z",
                        TZ = new
                        {
                            StandardName = "GMT Standard Time",
                            DaylightName = "GMT Summer Time"
                        }
                    },
                    companies = (object)null
                };

                // Store the email object for later retrieval
                _lastEmailObject = emailObject;

                // Call the browser function
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(emailObject);
                var result = await CallBrowserFunctionAsync("changeSelectedEmail", json);
                
                if (result != null)
                {
                    //MessageBox.Show(this, $"changeSelectedEmail called successfully!\nResult: {result}", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    SetStatusMessage("changeSelectedEmail function not found or returned null");
                }
            }
            catch (Exception ex)
            {
                SetStatusMessage($"Error calling changeSelectedEmail: {ex.Message}");
            }
        }

        #region Clipboard and Text Selection Monitoring

        private void InitializeClipboardMonitoring()
        {
            try
            {
                // Remove the aggressive timer-based polling
                // _clipboardTimer = new System.Threading.Timer(CheckClipboardChanges, null, 200, 200);
                
                // Set up global keyboard hook for text selection monitoring
                _hookID = SetHook(_proc);
                
                // Add clipboard format listener for real-time notifications
                AddClipboardFormatListener(this.Handle);
                
                LogDebug("Clipboard monitoring initialized with Windows event notifications");
                SetStatusMessage(TranslationManager.GetString("clipboard.event_based"));
            }
            catch (Exception ex)
            {
                LogDebug($"Error initializing clipboard monitoring: {ex.Message}");
                SetStatusMessage(TranslationManager.GetString("status.clipboard_init_failed"));
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                // Check for Ctrl+C combination
                if (vkCode == VK_C && (GetAsyncKeyState(VK_LCONTROL) < 0 || GetAsyncKeyState(VK_RCONTROL) < 0))
                {
                    // Log the keyboard event
                    if (Program.MainFormInstance != null)
                    {
                        Program.MainFormInstance.LogDebug("Ctrl+C detected via keyboard hook");
                        
                        // Check debouncing
                        var now = DateTime.Now;
                        if ((now - Program.MainFormInstance._lastClipboardCheck).TotalMilliseconds > CLIPBOARD_DEBOUNCE_MS)
                        {
                            Program.MainFormInstance._lastClipboardCheck = now;
                            
                            // Small delay to allow clipboard to update, then check for content
                            Task.Delay(300).ContinueWith(_ => 
                            {
                                if (Program.MainFormInstance != null)
                                {
                                    Program.MainFormInstance.LogDebug("Checking clipboard after Ctrl+C");
                                    Program.MainFormInstance.CheckClipboardChanges(null);
                                }
                            });
                        }
                        else
                        {
                            Program.MainFormInstance.LogDebug("Ctrl+C ignored (debounced)");
                        }
                    }
                }
            }
            
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void CheckClipboardChanges(object state, bool forceCheck = false)
        {
            if (!_clipboardMonitoringEnabled && !forceCheck) 
            {
                LogDebug("Clipboard monitoring is disabled, skipping check");
                return;
            }

            try
            {
                // Use Invoke to ensure we're on the UI thread when accessing clipboard
                if (InvokeRequired)
                {
                    Invoke(new Action(() => CheckClipboardChanges(state, forceCheck)));
                    return;
                }

                // Try to open clipboard with a timeout
                bool clipboardOpened = false;
                try
                {
                    // Try to open clipboard with a short timeout
                    var startTime = DateTime.Now;
                    while (!clipboardOpened && (DateTime.Now - startTime).TotalMilliseconds < 100)
                    {
                        try
                        {
                            Clipboard.GetText(); // This will throw if clipboard is busy
                            clipboardOpened = true;
                        }
                        catch
                        {
                            Thread.Sleep(10); // Wait 10ms and try again
                        }
                    }
                }
                catch
                {
                    LogDebug("Clipboard is busy or inaccessible, skipping this check");
                    return;
                }

                if (Clipboard.ContainsText())
                {
                    var currentContent = Clipboard.GetText();
                    LogDebug($"Clipboard contains text: '{currentContent}' (length: {currentContent?.Length ?? 0})");
                    
                    if (!string.IsNullOrWhiteSpace(currentContent) && currentContent != _lastClipboardContent)
                    {
                        LogDebug($"New clipboard content detected: '{currentContent}'");
                        _lastClipboardContent = currentContent;
                        ProcessClipboardContent(currentContent, "clipboard");
                    }
                    else if (currentContent == _lastClipboardContent)
                    {
                        LogDebug("Clipboard content unchanged, skipping processing");
                    }
                }
                else
                {
                    LogDebug("Clipboard does not contain text");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error checking clipboard: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
            }
        }

        private void ProcessClipboardContent(string content, string source)
        {
            try
            {
                LogDebug($"Processing content from {source}: '{content}'");
                LogDebug($"Content length: {content?.Length ?? 0}");
                LogDebug($"Content contains newlines: {content?.Contains("\n") ?? false} or {content?.Contains("\r") ?? false}");
                
                // Check for different types of content in order of priority
                var emailMatches = EmailRegex.Matches(content);
                var phoneMatches = PhoneRegex.Matches(content);
                var websiteMatches = WebsiteRegex.Matches(content);
                
                LogDebug($"Found {emailMatches.Count} email, {phoneMatches.Count} phone, {websiteMatches.Count} website matches");
                
                // Debug: Show what each regex found
                if (emailMatches.Count > 0)
                {
                    LogDebug($"Email matches: {string.Join(", ", emailMatches.Cast<Match>().Select(m => m.Value))}");
                }
                if (phoneMatches.Count > 0)
                {
                    LogDebug($"Phone matches: {string.Join(", ", phoneMatches.Cast<Match>().Select(m => m.Value))}");
                }
                if (websiteMatches.Count > 0)
                {
                    LogDebug($"Website matches: {string.Join(", ", websiteMatches.Cast<Match>().Select(m => m.Value))}");
                }
                
                string detectedValue = null;
                string contentType = null;
                string uri = null;
                
                // Priority: Email > Address (newlines) > Phone > Website > Generic Text
                if (emailMatches.Count > 0)
                {
                    detectedValue = emailMatches[0].Value;
                    contentType = "email";
                    uri = $"crmtog://context?value={Uri.EscapeDataString(detectedValue)}&source={source}&type={contentType}";
                }
                else if (content.Contains("\n") || content.Contains("\r"))
                {
                    // Text with newlines - likely a postal address (check this before phone to avoid false positives)
                    LogDebug($"Address detected: content contains newlines");
                    detectedValue = content.Trim();
                    contentType = "address";
                    uri = $"crmtog://address?value={Uri.EscapeDataString(detectedValue)}&source={source}";
                }
                else if (phoneMatches.Count > 0)
                {
                    detectedValue = phoneMatches[0].Value;
                    contentType = "phone";
                    uri = $"crmtog://phone?value={Uri.EscapeDataString(detectedValue)}&source={source}";
                }
                else if (websiteMatches.Count > 0)
                {
                    detectedValue = websiteMatches[0].Value;
                    contentType = "website";
                    uri = $"crmtog://website?value={Uri.EscapeDataString(detectedValue)}&source={source}";
                }
                else if (!string.IsNullOrWhiteSpace(content.Trim()))
                {
                    // Generic text (names, company names, etc.)
                    detectedValue = content.Trim();
                    contentType = "text";
                    uri = $"crmtog://text?value={Uri.EscapeDataString(detectedValue)}&source={source}";
                }
                
                if (!string.IsNullOrEmpty(detectedValue))
                {
                    LogDebug($"{contentType} detected from {source}: {detectedValue}");
                    LogDebug($"Dispatching URI: {uri}");
                    UriCommandDispatcher.Dispatch(uri, this);
                    SetStatusMessage(TranslationManager.GetString($"content.{contentType}_detected", source, detectedValue));
                }
                else
                {
                    LogDebug($"No recognizable content found in {source} content");
                    SetStatusMessage(TranslationManager.GetString("content.no_recognizable", source));
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error processing {source} content: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
            }
        }

        private void ToggleClipboardMonitoring()
        {
            _clipboardMonitoringEnabled = !_clipboardMonitoringEnabled;
            var status = _clipboardMonitoringEnabled ? 
                TranslationManager.GetString("clipboard.enabled") : 
                TranslationManager.GetString("clipboard.disabled");
            SetStatusMessage(status);
            LogDebug($"Clipboard monitoring {status}");
            UpdateClipboardMenuText();
        }

        private void UpdateClipboardMenuText()
        {
            if (_toggleClipboardMenuItem != null)
            {
                var status = _clipboardMonitoringEnabled ? 
                    TranslationManager.GetString("clipboard.disabled") : 
                    TranslationManager.GetString("clipboard.enabled");
                _toggleClipboardMenuItem.Text = status;
            }
        }

        private void TestClipboardCheck()
        {
            try
            {
                LogDebug("Manual clipboard check triggered");
                SetStatusMessage(TranslationManager.GetString("clipboard.testing"));
                LogDebug($"Clipboard monitoring enabled: {_clipboardMonitoringEnabled}");
                
                // Check if clipboard has any content at all
                if (Clipboard.ContainsText())
                {
                    var clipboardText = Clipboard.GetText();
                    LogDebug($"Clipboard contains text: '{clipboardText}' (length: {clipboardText?.Length ?? 0})");
                    SetStatusMessage($"Clipboard contains: {clipboardText?.Substring(0, Math.Min(50, clipboardText?.Length ?? 0))}...");
                }
                else
                {
                    LogDebug("Clipboard does not contain text");
                    SetStatusMessage("Clipboard does not contain text");
                }
                
                if (!_clipboardMonitoringEnabled)
                {
                    LogDebug("Clipboard monitoring is disabled, but manual test should still work");
                }
                
                CheckClipboardChanges(null, forceCheck: true);
                LogDebug("Manual clipboard check completed");
            }
            catch (Exception ex)
            {
                LogDebug($"Error in TestClipboardCheck: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
                SetStatusMessage($"Error in clipboard test: {ex.Message}");
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Only process clipboard events if monitoring is enabled
            if (_clipboardMonitoringEnabled)
            {
                // Handle clipboard change notifications
                if (m.Msg == WM_CLIPBOARDUPDATE)
                {
                    // Debounce rapid clipboard changes
                    var now = DateTime.Now;
                    if ((now - _lastClipboardCheck).TotalMilliseconds > CLIPBOARD_DEBOUNCE_MS)
                    {
                        _lastClipboardCheck = now;
                        LogDebug("Clipboard change notification received (debounced)");
                        // Small delay to allow clipboard to update
                        Task.Delay(100).ContinueWith(_ => CheckClipboardChanges(null));
                    }
                    else
                    {
                        LogDebug("Clipboard change notification ignored (debounced)");
                    }
                }
                // Monitor for text selection changes using Windows messages
                else if (m.Msg == 0x0100) // WM_KEYDOWN
                {
                    // Check if Ctrl+C or Ctrl+A was pressed (common copy/select operations)
                    if (m.WParam.ToInt32() == 0x43 && Control.ModifierKeys == Keys.Control) // Ctrl+C
                    {
                        LogDebug("Ctrl+C detected in WndProc");
                        // Small delay to allow clipboard to update
                        Task.Delay(100).ContinueWith(_ => CheckClipboardChanges(null));
                    }
                }
            }
            
            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // _clipboardTimer?.Dispose(); // No longer using timer
                if (_hookID != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookID);
                    _hookID = IntPtr.Zero;
                }
                // Remove clipboard format listener
                try
                {
                    RemoveClipboardFormatListener(this.Handle);
                }
                catch { }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
