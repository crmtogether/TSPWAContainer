using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace CRMTogether.PwaHost
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            Text = TranslationManager.GetString("about.title");
            StartPosition = FormStartPosition.CenterParent;
            Width = 520;
            Height = 280;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            
            // Set the form icon
            try
            {
                Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, "images", "crmtogethericon.ico"));
            }
            catch (Exception ex)
            {
                // Silently handle icon loading errors
                System.Diagnostics.Debug.WriteLine($"Error loading icon: {ex.Message}");
            }

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
                Font = new Font("Segoe UI", 9F),
                Text = TranslationManager.GetString("about.content", ver, name.Name, webviewVer)
            };
            var ok = new Button { 
                Text = TranslationManager.GetString("about.ok"), 
                Dock = DockStyle.Bottom, 
                Height = 36,
                Font = new Font("Segoe UI", 9F)
            };
            ok.Click += (s,e) => Close();
            Controls.Add(lbl);
            Controls.Add(ok);
        }
    }
}
