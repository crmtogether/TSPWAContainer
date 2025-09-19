using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CRMTogether.PwaHost
{
    internal class IpcWindow : System.Windows.Forms.Form
    {
        private readonly Action<string> _onMessage;
        private readonly string _title;

        public IpcWindow(string title, Action<string> onMessage)
        {
            _title = title;
            _onMessage = onMessage ?? (_ => {});
            this.Text = _title;
            this.ShowInTaskbar = false;
            this.Opacity = 1.0; // Make it fully visible so FindWindow can find it
            this.Width = 1;
            this.Height = 1;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(-32000, -32000);
            this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            System.Diagnostics.Debug.WriteLine($"IpcWindow constructor: Title='{_title}', Text='{this.Text}'");
        }

        public void EnsureHandleCreated()
        {
            System.Diagnostics.Debug.WriteLine($"IpcWindow.EnsureHandleCreated: IsHandleCreated={this.IsHandleCreated}, Title='{this.Text}'");
            if (!this.IsHandleCreated) 
            {
                this.CreateControl();
                System.Diagnostics.Debug.WriteLine($"IpcWindow.EnsureHandleCreated: After CreateControl, IsHandleCreated={this.IsHandleCreated}, Handle={this.Handle}");
            }
            // Ensure the window is shown so FindWindow can find it
            this.Show();
            System.Diagnostics.Debug.WriteLine($"IpcWindow.EnsureHandleCreated: After Show, Visible={this.Visible}, Handle={this.Handle}");
        }

        public static string BuildPayloadFromArgs(string[] args)
        {
            if (args != null && args.Length == 1 && args[0].StartsWith("crmtog", StringComparison.OrdinalIgnoreCase))
                return args[0];
            foreach (var a in args ?? Array.Empty<string>())
            {
                if (a.StartsWith("--url=", StringComparison.OrdinalIgnoreCase)) return "URL|" + a.Substring(6).Trim();
            }
            return "ACTIVATE";
        }

        public static bool ForwardToExistingInstance(string windowTitle, string payload)
        {
            System.Diagnostics.Debug.WriteLine($"ForwardToExistingInstance: Looking for window '{windowTitle}' with payload '{payload}'");
            
            IntPtr targetWindow = IntPtr.Zero;
            
            // Use EnumWindows to find the window by title
            EnumWindows((hWnd, lParam) => {
                var sb = new System.Text.StringBuilder(256);
                GetWindowText(hWnd, sb, 256);
                string title = sb.ToString();
                if (title == windowTitle)
                {
                    targetWindow = hWnd;
                    System.Diagnostics.Debug.WriteLine($"Found target window: {title} (Handle: {hWnd})");
                    return false; // Stop enumeration
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);
            
            if (targetWindow == IntPtr.Zero) 
            {
                System.Diagnostics.Debug.WriteLine("No existing window found");
                return false;
            }
            System.Diagnostics.Debug.WriteLine($"Sending message to existing window: {payload}");
            SendCopyDataString(targetWindow, payload ?? "ACTIVATE");
            return true;
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_COPYDATA)
            {
                try
                {
                    var cds = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                    string s = PtrToStringUniSafe(cds.lpData, cds.cbData);
                    System.Diagnostics.Debug.WriteLine($"IpcWindow received WM_COPYDATA: {s}");
                    _onMessage?.Invoke(s);
                    m.Result = new IntPtr(1);
                    return;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in WndProc: {ex.Message}");
                }
            }
            base.WndProc(ref m);
        }

        private static string PtrToStringUniSafe(IntPtr ptr, int bytes)
        {
            if (ptr == IntPtr.Zero || bytes <= 0) return string.Empty;
            int chars = bytes / 2;
            return Marshal.PtrToStringUni(ptr, chars)?.TrimEnd('\0') ?? string.Empty;
        }

        private static void SendCopyDataString(IntPtr hwnd, string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s + "\0");
            var cds = new COPYDATASTRUCT
            {
                dwData = IntPtr.Zero,
                cbData = bytes.Length,
            };
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                cds.lpData = ptr;
                SendMessage(hwnd, WM_COPYDATA, IntPtr.Zero, ref cds);
            }
            finally
            {
                if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
            }
        }

        private const int WM_COPYDATA = 0x004A;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }
    }
}
