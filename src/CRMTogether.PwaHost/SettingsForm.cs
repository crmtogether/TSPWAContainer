using System;
using System.Drawing;
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
        private bool _isSaving = false;

        public SettingsForm()
        {
            Text = TranslationManager.GetString("settings.title");
            Width = 700;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(15);
            
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

            // Create main container with proper spacing
            var mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(0)
            };

            // Configure rows
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Startup URL section
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Folders label
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // List box
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Buttons

            // Startup URL section
            var startupUrlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 10)
            };

            _lblStartupUrl = new Label { 
                Text = TranslationManager.GetString("settings.startup_url_label"), 
                Dock = DockStyle.Top, 
                Height = 20,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51)
            };
            
            _txtStartupUrl = new TextBox { 
                Dock = DockStyle.Top, 
                Height = 25,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 5, 0, 0)
            };

            startupUrlPanel.Controls.Add(_txtStartupUrl);
            startupUrlPanel.Controls.Add(_lblStartupUrl);

            // Folders label
            _lbl = new Label { 
                Text = TranslationManager.GetString("settings.folders_label"), 
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 10, 0, 10)
            };

            // List box with padding
            var listPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 10)
            };

            _list = new ListBox { 
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                BorderStyle = BorderStyle.FixedSingle
            };

            listPanel.Controls.Add(_list);

            // Buttons panel
            var buttonPanel = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                FlowDirection = FlowDirection.RightToLeft, 
                Padding = new Padding(0, 10, 0, 0),
                WrapContents = false
            };

            _btnAdd = new Button { 
                Text = TranslationManager.GetString("settings.add_folder"), 
                Width = 120,
                Height = 35,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(5, 0, 0, 0)
            };
            _btnRemove = new Button { 
                Text = TranslationManager.GetString("settings.remove_selected"), 
                Width = 140,
                Height = 35,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(5, 0, 0, 0)
            };
            _btnClose = new Button { 
                Text = TranslationManager.GetString("settings.close"), 
                Width = 100,
                Height = 35,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(5, 0, 0, 0)
            };

            buttonPanel.Controls.AddRange(new Control[] { _btnClose, _btnRemove, _btnAdd });

            // Add all panels to main container
            mainContainer.Controls.Add(startupUrlPanel, 0, 0);
            mainContainer.Controls.Add(_lbl, 0, 1);
            mainContainer.Controls.Add(listPanel, 0, 2);
            mainContainer.Controls.Add(buttonPanel, 0, 3);

            Controls.Add(mainContainer);

            Load += (s,e) => {
                _list.Items.Clear();
                foreach (var f in Program.Config.WatchedFolders) _list.Items.Add(f);
                _txtStartupUrl.Text = Program.Config.StartupUrl ?? "https://crmtogether.com/univex-app-home/";
            };

            _btnAdd.Click += (s,e) => {
                using (var dlg = new FolderBrowserDialog())
                {
                    dlg.Description = TranslationManager.GetString("settings.select_folder_description");
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

            _btnClose.Click += (s,e) => { 
                try 
                { 
                    SaveAndClose(); 
                } 
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"Error in close button click: {ex.Message}");
                    // Try to close the form even if SaveAndClose fails
                    try { Close(); } catch { }
                } 
            };
            FormClosing += (s,e) => { 
                try 
                { 
                    SaveAndClose(); 
                } 
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"Error in FormClosing: {ex.Message}");
                    // Don't prevent closing even if SaveAndClose fails
                } 
            };
        }

        private void SaveAndClose()
        {
            try
            {
                // Prevent multiple simultaneous calls
                if (_isSaving)
                {
                    return;
                }
                _isSaving = true;

                // Ensure we're on the UI thread
                if (InvokeRequired)
                {
                    Invoke(new Action(SaveAndClose));
                    return;
                }

                // Check if form is already disposed or disposing
                if (IsDisposed || Disposing)
                {
                    return;
                }

                // Update watched folders
                Program.Config.WatchedFolders.Clear();
                foreach (var it in _list.Items) 
                {
                    if (it is string folder && !string.IsNullOrWhiteSpace(folder))
                    {
                        Program.Config.WatchedFolders.Add(folder);
                    }
                }
                
                // Save startup URL
                Program.Config.StartupUrl = _txtStartupUrl.Text?.Trim();
                if (string.IsNullOrWhiteSpace(Program.Config.StartupUrl))
                {
                    Program.Config.StartupUrl = "https://crmtogether.com/univex-app-home/";
                }
                
                // Save configuration
                Program.Config.Save();
                
                // Close the form safely
                if (!IsDisposed && !Disposing)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                // Log the error and show a user-friendly message
                System.Diagnostics.Debug.WriteLine($"Error in SaveAndClose: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Try to close the form even if saving failed
                try
                {
                    if (!IsDisposed && !Disposing)
                    {
                        Close();
                    }
                }
                catch (Exception closeEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error closing form: {closeEx.Message}");
                }
            }
            finally
            {
                _isSaving = false;
            }
        }
    }
}
