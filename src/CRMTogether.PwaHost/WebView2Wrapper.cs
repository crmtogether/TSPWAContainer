using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;

namespace CRMTogether.PwaHost
{
    public class WebView2Wrapper : Microsoft.Web.WebView2.WinForms.WebView2
    {
        public WebView2Wrapper()
        {
        }

        public async Task AsyncInitializeWebView()
        {
            LogInfo("AsyncInitializeWebView start", true);

            string wv_options = "--disable-web-security --allow-insecure-localhost --allow-running-insecure-content --ignore-certificate-errors --ignore-ssl-errors --ignore-certificate-errors-spki-list --disable-certificate-verification --ignore-urlfetcher-cert-requests --disable-features=msForceBrowserSignIn --enable-features=InsecurePrivateNetworkRequestsAllowed --user-data-dir --disable-features=VizDisplayCompositor --disable-background-timer-throttling --disable-backgrounding-occluded-windows --disable-renderer-backgrounding --disable-features=TranslateUI --disable-ipc-flooding-protection --disable-hang-monitor --disable-prompt-on-repost --disable-domain-reliability --disable-component-extensions-with-background-pages --disable-background-networking --disable-sync --disable-default-apps --disable-extensions --disable-plugins --disable-translate --disable-logging --disable-gpu-logging --silent-debugger-extension-api --disable-gpu-sandbox --no-sandbox --disable-setuid-sandbox --disable-dev-shm-usage --disable-gpu --disable-software-rasterizer --disable-gpu-rasterization --disable-2d-canvas-clip-aa --disable-3d-apis --disable-accelerated-2d-canvas --disable-accelerated-jpeg-decoding --disable-accelerated-mjpeg-decode --disable-accelerated-video-decode --disable-gpu-compositing --disable-gpu-memory-buffer-video-frames --disable-zero-browsers-open-for-tests";
            
            var options = new CoreWebView2EnvironmentOptions(wv_options);
            var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                        "CRMTogether", "PwaHost", "WebView2UserData");
            Directory.CreateDirectory(dataPath);
            
            var env = await CoreWebView2Environment.CreateAsync(null, dataPath, options);
            await EnsureCoreWebView2Async(env);
            LogInfo("AsyncInitializeWebView end", true);
        }

        /// <summary>
        /// to wait asynchronously to initialize WebView2.CoreWebView2
        /// </summary>
        public void WaitForInitialization()
        {
            LogInfo("WebView2.CoreWebView2 WaitForInitialization", true);
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();

            while (CoreWebView2 == null)
            {
                System.Threading.Thread.Sleep(100);
                System.Windows.Forms.Application.DoEvents();
                //failsafe
                if (sw.ElapsedMilliseconds > 10000)
                {
                    LogInfo("WebView2.CoreWebView2 WaitForInitialization taking too long....", true);
                    break;
                }
            }

            if (CoreWebView2 != null)
            {
                CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                sw.Stop();
                LogInfo(string.Format("WebView2.CoreWebView2 is successfully initialized in {0} milliseconds", sw.ElapsedMilliseconds), true);
            }
        }

        public async void EnsureCoreWebView2Async(string sender)
        {
            LogInfo("EnsureCoreWebView2Async start:" + sender, true);
            lock (this)
            {
                LogInfo("EnsureCoreWebView2Async inside lock..." + sender, true);
                AsyncInitializeWebView();
                WaitForInitialization();
            }
            LogInfo("EnsureCoreWebView2Async end:" + sender, true);
        }

        #region logging
        private bool EnableDebugging()
        {
            return true; // You can make this configurable
        }

        public void LogInfo(string val, bool isForce = false)
        {
            if (EnableDebugging() || isForce)
            {
                System.Diagnostics.Debug.WriteLine($"WebView2: {val}");
            }
        }
        #endregion
    }
}
