using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRMTogether.PwaHost
{
    /// <summary>
    /// Host object that provides PWA functionality to the web page
    /// </summary>
    [System.Runtime.InteropServices.ComVisible(true)]
    public class PwaHostObject
    {
        private readonly MainForm _mainForm;

        public PwaHostObject(MainForm mainForm)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
        }

        // Home page management
        public string SetHomePage(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL is required", nameof(url));

            try
            {
                Program.Config.StartupUrl = url;
                Program.Config.Save();
                return $"Home page set to: {url}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set home page: {ex.Message}");
            }
        }

        public string GetHomePage()
        {
            return Program.Config?.StartupUrl ?? "https://crmtogether.com/univex-app-home/";
        }

        // Navigation
        public string Navigate(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL is required", nameof(url));

            try
            {
                _mainForm.Invoke(new Action(() => _mainForm.Navigate(url)));
                return $"Navigating to: {url}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to navigate: {ex.Message}");
            }
        }

        public string Reload()
        {
            try
            {
                _mainForm.Invoke(new Action(() => _mainForm.Reload()));
                return "Page reloaded";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to reload: {ex.Message}");
            }
        }

        public string GoBack()
        {
            try
            {
                _mainForm.Invoke(new Action(() => _mainForm.GoBack()));
                return "Went back";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to go back: {ex.Message}");
            }
        }

        public string GetCurrentUrl()
        {
            try
            {
                return _mainForm.GetCurrentUrl();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get current URL: {ex.Message}");
            }
        }

        public string GetTitle()
        {
            try
            {
                return _mainForm.GetTitle();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get title: {ex.Message}");
            }
        }

        // Window controls
        public string BringToFront()
        {
            try
            {
                _mainForm.Invoke(new Action(() => _mainForm.BringToFront()));
                return "Brought to front";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to bring to front: {ex.Message}");
            }
        }

        public string SetSize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive");

            try
            {
                _mainForm.Invoke(new Action(() => _mainForm.SetSize(width, height)));
                return $"Window size set to {width}x{height}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set size: {ex.Message}");
            }
        }

        public string GetVersion()
        {
            return "1.0.0.0";
        }

        // JavaScript execution
        public string ExecuteScript(string js)
        {
            if (string.IsNullOrWhiteSpace(js))
                throw new ArgumentException("JavaScript code is required", nameof(js));

            try
            {
                _mainForm.Invoke(new Action(() => _mainForm.ExecuteScript(js)));
                return "Script executed";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute script: {ex.Message}");
            }
        }

        // Test changeSelectedEmail function
        public async Task<string> TestChangeSelectedEmail()
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
                var result = await _mainForm.CallBrowserFunctionAsync("changeSelectedEmail", json);
                
                if (result != null)
                {
                    return $"changeSelectedEmail called successfully! Result: {result}";
                }
                else
                {
                    return "changeSelectedEmail function not found or returned null";
                }
            }
            catch (Exception ex)
            {
                return $"Error calling changeSelectedEmail: {ex.Message}";
            }
        }

        // Logging
        public string Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"PWA: {message}");
            return $"Logged: {message}";
        }
    }
}