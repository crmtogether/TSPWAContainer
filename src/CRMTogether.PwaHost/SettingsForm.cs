using System;
using System.Linq;
using System.Windows.Forms;

namespace CRMTogether.PwaHost
{
    public class SettingsForm : Form
    {
        private ListBox _list;
        private Button _btnAdd, _btnRemove, _btnClose;
        private Label _lbl, _lblStartupUrl;
        private TextBox _txtStartupUrl;

        public SettingsForm()
        {
            Text = "Settings";
            Width = 640;
            Height = 480;
            StartPosition = FormStartPosition.CenterParent;

            // Startup URL section
            _lblStartupUrl = new Label { 
                Text = "Startup URL (URL to load when the app starts):", 
                Dock = DockStyle.Top, 
                Height = 20, 
                Padding = new Padding(10, 10, 10, 0) 
            };
            
            _txtStartupUrl = new TextBox { 
                Dock = DockStyle.Top, 
                Height = 25, 
                Margin = new Padding(10, 0, 10, 10)
            };

            _lbl = new Label { 
                Text = "Folders to monitor for new files (.eml handled specially):", 
                Dock = DockStyle.Top, 
                Height = 30, 
                Padding = new Padding(10, 10, 10, 0) 
            };

            _list = new ListBox { Dock = DockStyle.Fill };
            _btnAdd = new Button { Text = "Add Folder...", Width = 110 };
            _btnRemove = new Button { Text = "Remove Selected", Width = 130 };
            _btnClose = new Button { Text = "Close", Width = 90 };

            var panel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10) };
            panel.Controls.AddRange(new Control[] { _btnClose, _btnRemove, _btnAdd });

            Controls.Add(_list);
            Controls.Add(panel);
            Controls.Add(_lbl);
            Controls.Add(_txtStartupUrl);
            Controls.Add(_lblStartupUrl);

            Load += (s,e) => {
                _list.Items.Clear();
                foreach (var f in Program.Config.WatchedFolders) _list.Items.Add(f);
                _txtStartupUrl.Text = Program.Config.StartupUrl ?? "https://crmtogether.com/univex-app-home/";
            };

            _btnAdd.Click += (s,e) => {
                using (var dlg = new FolderBrowserDialog())
                {
                    dlg.Description = "Select a folder to monitor";
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        if (!_list.Items.Cast<string>().Any(x => string.Equals(x, dlg.SelectedPath, System.StringComparison.OrdinalIgnoreCase)))
                        {
                            _list.Items.Add(dlg.SelectedPath);
                        }
                    }
                }
            };

            _btnRemove.Click += (s,e) => {
                var sel = _list.SelectedItem as string;
                if (sel != null) _list.Items.Remove(sel);
            };

            _btnClose.Click += (s,e) => { SaveAndClose(); };
            FormClosing += (s,e) => { SaveAndClose(); };
        }

        private void SaveAndClose()
        {
            Program.Config.WatchedFolders.Clear();
            foreach (var it in _list.Items) Program.Config.WatchedFolders.Add(it as string);
            
            // Save startup URL
            Program.Config.StartupUrl = _txtStartupUrl.Text?.Trim();
            if (string.IsNullOrWhiteSpace(Program.Config.StartupUrl))
            {
                Program.Config.StartupUrl = "https://crmtogether.com/univex-app-home/";
            }
            
            Program.Config.Save();
        }
    }
}
