using System;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace CRMTogether.PwaHost
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            Text = "About CRMTogether PWA Host";
            StartPosition = FormStartPosition.CenterParent;
            Width = 520;
            Height = 280;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var asm = Assembly.GetExecutingAssembly();
            var name = asm.GetName();
            string ver = name.Version?.ToString() ?? "n/a";
            string webviewVer = "";
            try { webviewVer = CoreWebView2Environment.GetAvailableBrowserVersionString(); } catch { webviewVer = "unknown"; }

            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                AutoSize = false,
                Text = 
$@"CRMTogether PWA Host

Version: {ver}
Assembly: {name.Name}

WebView2 Runtime: {webviewVer}

© CRMTogether"
            };
            var ok = new Button { Text = "OK", Dock = DockStyle.Bottom, Height = 36 };
            ok.Click += (s,e) => Close();
            Controls.Add(lbl);
            Controls.Add(ok);
        }
    }
}
