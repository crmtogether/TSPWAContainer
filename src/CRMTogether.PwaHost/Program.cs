using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CRMTogether.PwaHost
{
    internal static class Program
    {
        internal static MainForm MainFormInstance;
        internal static AppConfig Config;
        internal static IpcWindow Ipc;

        private const string IPC_WINDOW_TITLE = "CRMTogetherPwaHostIPC";

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool created;
            using var singleton = new Mutex(true, @"Global\CRMTogether.PwaHost.Singleton", out created);
            if (!created)
            {
                string payload = IpcWindow.BuildPayloadFromArgs(args);
                IpcWindow.ForwardToExistingInstance(IPC_WINDOW_TITLE, payload);
                return;
            }

            string initialUrl = null;
            bool regUri = false;
            foreach (var a in args ?? Array.Empty<string>())
            {
                if (a.StartsWith("--url=", StringComparison.OrdinalIgnoreCase)) initialUrl = a.Substring(6).Trim();
                else if (a.Equals("/RegUri", StringComparison.OrdinalIgnoreCase)) regUri = true;
            }

            if (regUri || !UriRegistrar.IsRegistered("crmtog")) { UriRegistrar.RegisterUserProtocol("crmtog"); }

            Config = AppConfig.LoadDefault();

            // Use command line URL if provided, otherwise use last URL, then startup URL
            if (string.IsNullOrWhiteSpace(initialUrl))
            {
                if (!string.IsNullOrWhiteSpace(Config.LastUrl))
                {
                    initialUrl = Config.LastUrl;
                }
                else
                {
                    initialUrl = Config.StartupUrl;
                }
            }

            MainFormInstance = new MainForm { InitialUrl = initialUrl };

            Ipc = new IpcWindow(IPC_WINDOW_TITLE, HandleIpcMessage);
            Ipc.EnsureHandleCreated();

            if (args != null && args.Length == 1 && args[0].StartsWith("crmtog", StringComparison.OrdinalIgnoreCase))
            {
                try { UriCommandDispatcher.Dispatch(args[0], MainFormInstance); } catch { }
            }

            MainFormInstance.Show();

            Application.Run(MainFormInstance);
        }

        private static void HandleIpcMessage(string msg)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"IPC Message received: {msg}");
                
                if (string.IsNullOrWhiteSpace(msg)) { MainFormInstance?.BringToFront(); return; }

                if (msg.StartsWith("crmtog:", StringComparison.OrdinalIgnoreCase) ||
                    msg.StartsWith("crmtog://", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Dispatching URI command: {msg}");
                    UriCommandDispatcher.Dispatch(msg, MainFormInstance);
                    MainFormInstance?.BringToFront();
                }
                else if (msg.StartsWith("URL|", StringComparison.OrdinalIgnoreCase))
                {
                    var url = msg.Substring(4);
                    System.Diagnostics.Debug.WriteLine($"Navigating to URL: {url}");
                    MainFormInstance?.Navigate(url);
                    MainFormInstance?.BringToFront();
                }
                else if (msg.Equals("ACTIVATE", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine("Activating window");
                    MainFormInstance?.BringToFront();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Unknown message type: {msg}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling IPC message: {ex.Message}");
            }
        }
    }
}
