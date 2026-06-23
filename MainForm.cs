/*
 * Win-Restore
 * MainForm.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using win_restore.Helpers;
using win_restore.Models;

namespace win_restore
{
    public partial class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
        private const uint BCM_SETSHIELD = 0x160C;

        private string _targetUser = Environment.UserName;
        private List<BackupItem> _restoreItems = new List<BackupItem>();
        private Dictionary<string, DriveInfo> _driveMapping = new Dictionary<string, DriveInfo>();

        // The root folder on the backup drive e.g. "D:\john"
        private string BackupRoot =>
            cboDrive.SelectedItem == null ? string.Empty :
            Path.Combine(
                _driveMapping[cboDrive.SelectedItem.ToString()].RootDirectory.FullName.TrimEnd('\\'),
                _targetUser);

        // Always restores back to C:\Users\username
        private string RestoreDestination =>
            Path.Combine(@"C:\Users", _targetUser);

        public MainForm(string[] args)
        {
            InitializeComponent();

            btnDifferentUser.FlatStyle = FlatStyle.System;
            SendMessage(btnDifferentUser.Handle, BCM_SETSHIELD, 0, 1);

            cboDrive.SelectedIndexChanged += (s, e) => LoadRestoreLocations();

            LoadDrives();

            lblDestination.Text = $"Restoring to: {RestoreDestination}";

            if (args.Contains("--pickuser") && RestoreEngine.IsElevated())
            {
                ShowUserPicker();
            }
        }

        private void LoadDrives()
        {
            _driveMapping = RestoreEngine.GetDrives();

            cboDrive.Items.Clear();
            foreach (var displayText in _driveMapping.Keys)
                cboDrive.Items.Add(displayText);

            if (cboDrive.Items.Count > 0)
                cboDrive.SelectedIndex = 0;
        }

        private void LoadRestoreLocations()
        {
            clbLocations.Items.Clear();

            if (string.IsNullOrEmpty(BackupRoot))
                return;

            _restoreItems = RestoreEngine.BuildRestoreItems(BackupRoot, RestoreDestination);

            if (_restoreItems.Count == 0)
            {
                // No backup found on this drive for this user
                MessageBox.Show(
                    $"No backup found for user '{Environment.UserName}' on this drive.\n\n" +
                    $"Expected folder: {BackupRoot}",
                    "No Backup Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            foreach (var item in _restoreItems)
            {
                int index = clbLocations.Items.Add(item.Name);
                clbLocations.SetItemChecked(index, item.Enabled);
            }
        }

        private void ShowUserPicker()
        {
            var users = RestoreEngine.GetUserProfiles();

            if (users.Count == 0)
            {
                MessageBox.Show("No user profiles found.", "No Users",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var picker = new UserPickerForm(users, _targetUser))
            {
                if (picker.ShowDialog() == DialogResult.OK)
                {
                    _targetUser = picker.SelectedUser;
                    Text = $"Backup Restore Utility — Restoring: {_targetUser}";
                    lblDestination.Text = $"Restoring to: {RestoreDestination}";
                    LoadRestoreLocations(); // rescan the drive for this user's backup
                }
            }
        }

        private void clbLocations_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void clbLocations_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            _restoreItems[e.Index].Enabled = (e.NewValue == CheckState.Checked);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int selectedCount = _restoreItems.Count(i => i.Enabled);

            if (selectedCount == 0)
            {
                MessageBox.Show(
                    "Please select at least one location to restore.",
                    "No Locations Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Warn the user before overwriting — this is destructive
            var confirm = MessageBox.Show(
                $"This will overwrite existing files in:\n{RestoreDestination}\n\n" +
                $"{selectedCount} location(s) will be restored. Continue?",
                "Confirm Restore",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            var progressForm = new ProgressForm(_restoreItems, chkSizeCheck.Checked);
            progressForm.ShowDialog();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnAddPath_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder to restore";
                dialog.UseDescriptionForTitle = true;
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() != DialogResult.OK) return;

                string selectedPath = dialog.SelectedPath;
                string folderName = Path.GetFileName(selectedPath);

                if (_restoreItems.Any(i => i.Src.Equals(selectedPath, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show(
                        "That folder is already in the list.",
                        "Duplicate Path",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string displayName = $"[Custom] {folderName}";

                var newItem = new BackupItem
                {
                    Name = displayName,
                    Src = selectedPath,
                    Dest = Path.Combine(RestoreDestination, folderName), // restores to C:\Users\username\FolderName
                    Enabled = true
                };

                _restoreItems.Add(newItem);
                int index = clbLocations.Items.Add(displayName);
                clbLocations.SetItemChecked(index, true);
            }
        }

        private void btnRemovePath_Click(object sender, EventArgs e)
        {
            int selected = clbLocations.SelectedIndex;

            if (selected == -1)
            {
                MessageBox.Show(
                    "Please select a location from the list first.",
                    "Nothing Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!_restoreItems[selected].Name.StartsWith("[Custom]"))
            {
                MessageBox.Show(
                    "Only custom paths can be removed.\n" +
                    "Use the checkbox to disable built-in locations instead.",
                    "Cannot Remove",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _restoreItems.RemoveAt(selected);
            clbLocations.Items.RemoveAt(selected);
        }

        private void btnDifferentUser_Click(object sender, EventArgs e)
        {
            if (RestoreEngine.IsElevated())
            {
                ShowUserPicker();
                return;
            }

            if (RestoreEngine.RelaunchElevated("--pickuser"))
            {
                Application.Exit();
            }
        }
    }
}