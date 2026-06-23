/*
 * Win-Restore
 * MainForm.Designer.cs
 * Copyright (c) 2026 Arthur Taft. All Rights Reserved.
*/
namespace win_restore
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            label1 = new Label();
            cboDrive = new ComboBox();
            lblDestination = new Label();
            groupBox1 = new GroupBox();
            btnRemovePath = new Button();
            btnAddPath = new Button();
            clbLocations = new CheckedListBox();
            groupBox2 = new GroupBox();
            chkSizeCheck = new CheckBox();
            btnStart = new Button();
            btnExit = new Button();
            btnDifferentUser = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(127, 15);
            label1.TabIndex = 0;
            label1.Text = "Source Backup Drive:";
            // 
            // cboDrive
            // 
            cboDrive.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDrive.FormattingEnabled = true;
            cboDrive.Location = new Point(12, 35);
            cboDrive.Name = "cboDrive";
            cboDrive.Size = new Size(555, 23);
            cboDrive.TabIndex = 1;
            // 
            // lblDestination
            // 
            lblDestination.AutoSize = true;
            lblDestination.ForeColor = Color.Gray;
            lblDestination.Location = new Point(12, 65);
            lblDestination.Name = "lblDestination";
            lblDestination.Size = new Size(178, 15);
            lblDestination.TabIndex = 2;
            lblDestination.Text = "Restoring to: C:\\Users\\username";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnRemovePath);
            groupBox1.Controls.Add(btnAddPath);
            groupBox1.Controls.Add(clbLocations);
            groupBox1.Location = new Point(12, 90);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(370, 330);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Locations to Restore";
            // 
            // btnRemovePath
            // 
            btnRemovePath.Location = new Point(208, 291);
            btnRemovePath.Name = "btnRemovePath";
            btnRemovePath.Size = new Size(151, 28);
            btnRemovePath.TabIndex = 2;
            btnRemovePath.Text = "- Remove Selected";
            btnRemovePath.UseVisualStyleBackColor = true;
            btnRemovePath.Click += btnRemovePath_Click;
            // 
            // btnAddPath
            // 
            btnAddPath.Location = new Point(10, 291);
            btnAddPath.Name = "btnAddPath";
            btnAddPath.Size = new Size(151, 28);
            btnAddPath.TabIndex = 1;
            btnAddPath.Text = "+ Add Custom Path";
            btnAddPath.UseVisualStyleBackColor = true;
            btnAddPath.Click += btnAddPath_Click;
            // 
            // clbLocations
            // 
            clbLocations.CheckOnClick = true;
            clbLocations.FormattingEnabled = true;
            clbLocations.Location = new Point(10, 25);
            clbLocations.Name = "clbLocations";
            clbLocations.Size = new Size(348, 256);
            clbLocations.TabIndex = 0;
            clbLocations.SelectedIndexChanged += clbLocations_SelectedIndexChanged;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(chkSizeCheck);
            groupBox2.Location = new Point(395, 90);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(172, 310);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Options";
            // 
            // chkSizeCheck
            // 
            chkSizeCheck.AutoSize = true;
            chkSizeCheck.Checked = true;
            chkSizeCheck.CheckState = CheckState.Checked;
            chkSizeCheck.Location = new Point(12, 30);
            chkSizeCheck.Name = "chkSizeCheck";
            chkSizeCheck.Size = new Size(137, 19);
            chkSizeCheck.TabIndex = 0;
            chkSizeCheck.Text = "Post-Copy Size Audit";
            chkSizeCheck.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            btnStart.BackColor = Color.SteelBlue;
            btnStart.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStart.ForeColor = Color.White;
            btnStart.Location = new Point(12, 435);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(460, 40);
            btnStart.TabIndex = 5;
            btnStart.Text = "Start Restore";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // btnExit
            // 
            btnExit.Location = new Point(485, 435);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(82, 40);
            btnExit.TabIndex = 6;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // btnDifferentUser
            // 
            btnDifferentUser.FlatStyle = FlatStyle.System;
            btnDifferentUser.Location = new Point(337, 62);
            btnDifferentUser.Name = "btnDifferentUser";
            btnDifferentUser.Size = new Size(230, 28);
            btnDifferentUser.TabIndex = 7;
            btnDifferentUser.Text = "Restore a different user";
            btnDifferentUser.UseVisualStyleBackColor = true;
            btnDifferentUser.Click += btnDifferentUser_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(584, 491);
            Controls.Add(btnDifferentUser);
            Controls.Add(btnExit);
            Controls.Add(btnStart);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(lblDestination);
            Controls.Add(cboDrive);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Backup Restore Utility";
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox cboDrive;
        private Label lblDestination;
        private GroupBox groupBox1;
        private CheckedListBox clbLocations;
        private GroupBox groupBox2;
        private CheckBox chkSizeCheck;
        private Button btnStart;
        private Button btnExit;
        private Button btnRemovePath;
        private Button btnAddPath;
        private Button btnDifferentUser;
    }
}
