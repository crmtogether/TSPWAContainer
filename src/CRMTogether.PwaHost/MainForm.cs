using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace CRMTogether.PwaHost
{
    public class MainForm : Form
    {
        private WebView2Wrapper _webView;
        private bool _initialized;
        private readonly Dictionary<string, string> _extraHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private MenuStrip _menu;
        private string _pendingUrl;

        private static readonly string[] AllowedPrefixes = new[] {
            "https://",
            "http://localhost",
            "file:///"
        };

        public string InitialUrl { get; set; }
        public WebView2Wrapper WebView => _webView;

        public MainForm()
        {
            Text = "CRMTogether PWA Host";
            Width = 420;
            Height = 0;
            StartPosition = FormStartPosition.Manual;
            AllowDrop = true;

            _menu = BuildMenu();
            this.MainMenuStrip = _menu;

            // Create a table layout panel to properly manage the layout
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(0)
            };

            // Configure the rows
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Menu row
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // WebView row

            // Create a panel for the menu
            var menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            _menu.Dock = DockStyle.Fill;
            menuPanel.Controls.Add(_menu);

            // Create a panel with padding for the WebView
            var webViewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5) // 5 pixels padding on all sides
            };

            _webView = new WebView2Wrapper { Dock = DockStyle.Fill };
            webViewPanel.Controls.Add(_webView);
            
            // Add panels to the table layout
            tableLayout.Controls.Add(menuPanel, 0, 0);
            tableLayout.Controls.Add(webViewPanel, 0, 1);
            
            // Add the table layout to the form
            Controls.Add(tableLayout);

            this.ResumeLayout(performLayout: true);

            Load += async (_, __) => await InitializeAsync();
            DragEnter += OnDragEnter;
            DragDrop += OnDragDrop;
        }

        private MenuStrip BuildMenu()
        {
            var ms = new MenuStrip();
            var settings = new ToolStripMenuItem("Settings");
            var mFolders = new ToolStripMenuItem("Monitored Folders...");
            mFolders.Click += (s,e) => {
                using (var dlg = new SettingsForm())
                {
                    dlg.ShowDialog(this);
                    StartWatchers();
                }
            };
            var mAbout = new ToolStripMenuItem("About...");
            mAbout.Click += (s,e) => { using (var a = new AboutForm()) a.ShowDialog(this); };

            settings.DropDownItems.Add(mFolders);
            settings.DropDownItems.Add(new ToolStripSeparator());
            settings.DropDownItems.Add(mAbout);
            ms.Items.Add(settings);

            // Add Test menu
            var test = new ToolStripMenuItem("Test");
            var mTestEmail = new ToolStripMenuItem("Test changeSelectedEmail");
            mTestEmail.Click += async (s, e) => await TestChangeSelectedEmail();
            test.DropDownItems.Add(mTestEmail);
            ms.Items.Add(test);

            return ms;
        }

        private async Task InitializeAsync()
        {
            if (_initialized) return;

            var wa = Screen.FromControl(this).WorkingArea;
            int width = 390;
            int height = (int)(wa.Height * 0.90);
            this.Size = new Size(width, height);
            this.Location = new Point(wa.Right - this.Width, wa.Top);

            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                        "CRMTogether", "PwaHost", "WebView2UserData");
            Directory.CreateDirectory(dataPath);

            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: dataPath);
            await _webView.EnsureCoreWebView2Async(env);

            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = true;
            _webView.CoreWebView2.Settings.IsZoomControlEnabled = true;

            // Inject PWA host object into the web page
            await InjectPwaHostObject();

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
                catch { }
            };

            // Re-inject PWA host object when navigation completes
            _webView.CoreWebView2.NavigationCompleted += async (s, e) =>
            {
                if (e.IsSuccess)
                {
                    await InjectPwaHostObject();
                }
            };

            _initialized = true;

            if (!string.IsNullOrWhiteSpace(_pendingUrl)) { var u = _pendingUrl; _pendingUrl = null; SafeNavigate(u); }
            else if (!string.IsNullOrWhiteSpace(InitialUrl)) { SafeNavigate(InitialUrl); }

            StartWatchers();
        }

        private void StartWatchers()
        {
            try
            {
                foreach (var w in _watchers) { try { w.Dispose(); } catch { } }
                _watchers.Clear();

                foreach (var folder in Program.Config.WatchedFolders)
                {
                    if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) continue;
                    var fsw = new FileSystemWatcher(folder);
                    fsw.IncludeSubdirectories = false;
                    fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                    fsw.Created += (s, e) => OnFileAppeared(e.FullPath);
                    fsw.EnableRaisingEvents = true;
                    _watchers.Add(fsw);
                }
            }
            catch { }
        }

        private void OnFileAppeared(string path)
        {
            try
            {
                if (string.Equals(Path.GetExtension(path), ".eml", StringComparison.OrdinalIgnoreCase))
                {
                    this.BeginInvoke((Action)(async () =>
                    {
                        await ProcessEmlFileWithRetry(path);
                    }));
                }
            }
            catch { }
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
                    if (string.Equals(Path.GetExtension(f), ".eml", StringComparison.OrdinalIgnoreCase))
                    {
                        _ = ProcessEmlFile(f); // Fire and forget async call
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
                return null;

            try
            {
                var result = await _webView.ExecuteScriptFunctionAsync(functionName, args);
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calling browser function {functionName}: {ex.Message}");
                return null;
            }
        }

        public async Task<T> CallBrowserFunctionAsync<T>(string functionName, params object[] args)
        {
            if (_webView?.CoreWebView2 == null || string.IsNullOrWhiteSpace(functionName)) 
                return default(T);

            try
            {
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
                System.Diagnostics.Debug.WriteLine($"Error calling browser function {functionName}: {ex.Message}");
                return default(T);
            }
        }

        private async Task InjectPwaHostObject()
        {
            if (_webView?.CoreWebView2 == null) return;

            try
            {
                // Use AddHostObjectToScript to inject the C# object directly
                _webView.CoreWebView2.AddHostObjectToScript("pwa", new PwaHostObject(this));
                
                // Add a simple script to make the object available as window.pwa
                await _webView.CoreWebView2.ExecuteScriptAsync(@"
                    window.pwa = chrome.webview.hostObjects.pwa;
                    console.log('PWA Host object injected successfully');
                ");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error injecting PWA host object: {ex.Message}");
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
                        MessageBox.Show(this, $"Error processing EML file after {maxRetries} attempts: {ex.Message}", "EML Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                
                // Call the browser function
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(emailObject);
                var result = await CallBrowserFunctionAsync("changeSelectedEmail", json);
                
                if (result != null)
                {
                    MessageBox.Show(this, $"EML processed successfully!\nSubject: {info.Subject}\nFrom: {info.From}\nResult: {result}", "EML Processed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, $"EML processed but changeSelectedEmail function not found\nSubject: {info.Subject}\nFrom: {info.From}", "EML Processed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error processing EML file: {ex.Message}", "EML Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    addresses = (object)null,
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

                // Call the browser function
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(emailObject);
                var result = await CallBrowserFunctionAsync("changeSelectedEmail", json);
                
                if (result != null)
                {
                    MessageBox.Show(this, $"changeSelectedEmail called successfully!\nResult: {result}", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, "changeSelectedEmail function not found or returned null", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error calling changeSelectedEmail: {ex.Message}", "Test Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
