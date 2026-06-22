using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using win_restore.Helpers;
using win_restore.Models;

namespace win_restore
{
    public class ProgressForm : Form
    {
        // Controls
        private Label _lblStatus;
        private ProgressBar _progressBar;
        private Label _lblCurrentFile;
        private ListView _lstItems;
        private Button _btnClose;
        private Button _btnToggleLog;

        // Data
        private readonly List<BackupItem> _items;
        private readonly bool _sizeCheckEnabled;
        private LogForm _logForm;

        public ProgressForm(List<BackupItem> items, bool sizeCheckEnabled)
        {
            _items = items;
            _sizeCheckEnabled = sizeCheckEnabled;

            SetupForm();
            Load += ProgressForm_Load;
        }

        private void SetupForm()
        {
            Text = "Restore In Progress";
            Size = new Size(660, 500);
            MinimumSize = new Size(500, 380);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            _lblStatus = new Label
            {
                Text = "Preparing restore...",
                Dock = DockStyle.Top,
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(8, 0, 0, 0)
            };

            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 22,
                Style = ProgressBarStyle.Continuous
            };

            _lblCurrentFile = new Label
            {
                Text = "",
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 8),
                ForeColor = Color.Gray,
                Padding = new Padding(8, 0, 0, 0)
            };

            _lstItems = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9)
            };
            _lstItems.Columns.Add("Location", 450);
            _lstItems.Columns.Add("Status", 130);

            foreach (var item in _items)
            {
                var row = new ListViewItem(item.Name);
                row.SubItems.Add(item.Enabled ? "Waiting" : "Skipped");
                row.ForeColor = item.Enabled ? Color.Black : Color.Gray;
                _lstItems.Items.Add(row);
            }

            // Bottom panel
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };

            _btnToggleLog = new Button { Text = "View Log", Size = new Size(90, 35) };
            _btnToggleLog.Click += (s, e) =>
            {
                if (_logForm != null && !_logForm.IsDisposed)
                {
                    _logForm.Focus();
                    return;
                }
                _logForm = new LogForm();
                _logForm.Show(this);
            };

            _btnClose = new Button
            {
                Text = "Close",
                Size = new Size(100, 35),
                Enabled = false
            };
            _btnClose.Click += (s, e) => Close();

            bottomPanel.Controls.Add(_btnToggleLog);
            bottomPanel.Controls.Add(_btnClose);
            bottomPanel.Resize += (s, e) =>
            {
                _btnClose.Left = (bottomPanel.Width - _btnClose.Width) / 2;
                _btnClose.Top = (bottomPanel.Height - _btnClose.Height) / 2;
                _btnToggleLog.Left = bottomPanel.Width - _btnToggleLog.Width - 10;
                _btnToggleLog.Top = (bottomPanel.Height - _btnToggleLog.Height) / 2;
            };

            Controls.Add(_lstItems);
            Controls.Add(_lblCurrentFile);
            Controls.Add(_progressBar);
            Controls.Add(_lblStatus);
            Controls.Add(bottomPanel);
        }

        private async void ProgressForm_Load(object sender, EventArgs e)
        {
            var progress = new Progress<CopyProgress>(UpdateUI);

            // Note: no excluded files parameter — restore doesn't need it
            var auditLogs = await Task.Run(() =>
                RestoreEngine.RunRestore(_items, progress));

            _lblStatus.Text = "✓ Restore complete!";
            _lblStatus.ForeColor = Color.DarkGreen;
            _lblCurrentFile.Text = "";

            if (_sizeCheckEnabled && auditLogs.Count > 0)
            {
                _lblStatus.Text = "Auditing transfer logs...";
                var failures = await Task.Run(() => RestoreEngine.VerifyLogs(auditLogs));
                _lblStatus.Text = "✓ Restore complete!";
                _lblStatus.ForeColor = Color.DarkGreen;
                new AuditForm(failures).ShowDialog(this);
            }

            _btnClose.Enabled = true;
        }

        private void UpdateUI(CopyProgress p)
        {
            _progressBar.Maximum = p.TotalItems;
            _progressBar.Value = Math.Min(p.ItemIndex, p.TotalItems);

            if (p.Status == "Copying")
                _lblStatus.Text = $"Restoring: {p.ItemName}";
            else if (p.Status == "Complete")
                _lblStatus.Text = $"Completed: {p.ItemName}";

            if (!string.IsNullOrEmpty(p.CurrentFile))
                _lblCurrentFile.Text = p.CurrentFile;

            foreach (ListViewItem row in _lstItems.Items)
            {
                if (row.Text != p.ItemName) continue;

                row.SubItems[1].Text = p.Status;
                row.ForeColor = p.Status switch
                {
                    "Complete" => Color.DarkGreen,
                    "Copying" => Color.DarkBlue,
                    "Skipped" => Color.Gray,
                    "Not Found" => Color.DarkOrange,
                    _ => Color.Black
                };
                row.EnsureVisible();
                break;
            }
        }
    }
}