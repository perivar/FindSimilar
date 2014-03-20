namespace FindSimilar
{
	partial class FindSimilarClientForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabFileSearch = new System.Windows.Forms.TabPage();
			this.AudioFilePlayBtn = new System.Windows.Forms.Button();
			this.AudioFileQueryBtn = new System.Windows.Forms.Button();
			this.AudioFileQueryTextBox = new System.Windows.Forms.TextBox();
			this.FileQueryLabel = new System.Windows.Forms.Label();
			this.tabIdSearch = new System.Windows.Forms.TabPage();
			this.QueryIdTextBox = new System.Windows.Forms.TextBox();
			this.QueryIdLabel = new System.Windows.Forms.Label();
			this.tabStringSearch = new System.Windows.Forms.TabPage();
			this.QueryStringTextBox = new System.Windows.Forms.TextBox();
			this.QueryStringLabel = new System.Windows.Forms.Label();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.findSimilarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.dumpDebugInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.DistanceTypeCombo = new System.Windows.Forms.ComboBox();
			this.GoBtn = new System.Windows.Forms.Button();
			this.ResetBtn = new System.Windows.Forms.Button();
			this.autoPlayCheckBox = new System.Windows.Forms.CheckBox();
			this.IgnoreFileLengthCheckBox = new System.Windows.Forms.CheckBox();
			this.versionLabel = new System.Windows.Forms.Label();
			this.version = new System.Windows.Forms.Label();
			this.rbScms = new System.Windows.Forms.RadioButton();
			this.rbSoundfingerprinting = new System.Windows.Forms.RadioButton();
			this.txtFilterResults = new System.Windows.Forms.TextBox();
			this.lblFilterResults = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabFileSearch.SuspendLayout();
			this.tabIdSearch.SuspendLayout();
			this.tabStringSearch.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabFileSearch);
			this.tabControl1.Controls.Add(this.tabIdSearch);
			this.tabControl1.Controls.Add(this.tabStringSearch);
			this.tabControl1.Location = new System.Drawing.Point(12, 29);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(676, 55);
			this.tabControl1.TabIndex = 0;
			// 
			// tabFileSearch
			// 
			this.tabFileSearch.AllowDrop = true;
			this.tabFileSearch.Controls.Add(this.AudioFilePlayBtn);
			this.tabFileSearch.Controls.Add(this.AudioFileQueryBtn);
			this.tabFileSearch.Controls.Add(this.AudioFileQueryTextBox);
			this.tabFileSearch.Controls.Add(this.FileQueryLabel);
			this.tabFileSearch.Location = new System.Drawing.Point(4, 22);
			this.tabFileSearch.Name = "tabFileSearch";
			this.tabFileSearch.Padding = new System.Windows.Forms.Padding(3);
			this.tabFileSearch.Size = new System.Drawing.Size(668, 29);
			this.tabFileSearch.TabIndex = 0;
			this.tabFileSearch.Text = "Find using audio";
			this.tabFileSearch.UseVisualStyleBackColor = true;
			this.tabFileSearch.DragDrop += new System.Windows.Forms.DragEventHandler(this.TabPage1DragDrop);
			this.tabFileSearch.DragEnter += new System.Windows.Forms.DragEventHandler(this.TabPage1DragEnter);
			// 
			// AudioFilePlayBtn
			// 
			this.AudioFilePlayBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AudioFilePlayBtn.Location = new System.Drawing.Point(627, 2);
			this.AudioFilePlayBtn.Name = "AudioFilePlayBtn";
			this.AudioFilePlayBtn.Size = new System.Drawing.Size(38, 23);
			this.AudioFilePlayBtn.TabIndex = 3;
			this.AudioFilePlayBtn.Text = "Play";
			this.AudioFilePlayBtn.UseVisualStyleBackColor = true;
			this.AudioFilePlayBtn.Click += new System.EventHandler(this.AudioFilePlayBtnClick);
			// 
			// AudioFileQueryBtn
			// 
			this.AudioFileQueryBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AudioFileQueryBtn.Location = new System.Drawing.Point(543, 2);
			this.AudioFileQueryBtn.Name = "AudioFileQueryBtn";
			this.AudioFileQueryBtn.Size = new System.Drawing.Size(66, 23);
			this.AudioFileQueryBtn.TabIndex = 2;
			this.AudioFileQueryBtn.Text = "Browse";
			this.AudioFileQueryBtn.UseVisualStyleBackColor = true;
			this.AudioFileQueryBtn.Click += new System.EventHandler(this.AudioFileQueryBtnClick);
			// 
			// AudioFileQueryTextBox
			// 
			this.AudioFileQueryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.AudioFileQueryTextBox.Location = new System.Drawing.Point(68, 4);
			this.AudioFileQueryTextBox.Name = "AudioFileQueryTextBox";
			this.AudioFileQueryTextBox.Size = new System.Drawing.Size(469, 20);
			this.AudioFileQueryTextBox.TabIndex = 1;
			this.AudioFileQueryTextBox.Text = "Browse or Drag Audio File here";
			this.AudioFileQueryTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AudioFileQueryTextBoxKeyPress);
			// 
			// FileQueryLabel
			// 
			this.FileQueryLabel.Location = new System.Drawing.Point(7, 7);
			this.FileQueryLabel.Name = "FileQueryLabel";
			this.FileQueryLabel.Size = new System.Drawing.Size(100, 23);
			this.FileQueryLabel.TabIndex = 0;
			this.FileQueryLabel.Text = "Audio File:";
			// 
			// tabIdSearch
			// 
			this.tabIdSearch.Controls.Add(this.QueryIdTextBox);
			this.tabIdSearch.Controls.Add(this.QueryIdLabel);
			this.tabIdSearch.Location = new System.Drawing.Point(4, 22);
			this.tabIdSearch.Name = "tabIdSearch";
			this.tabIdSearch.Padding = new System.Windows.Forms.Padding(3);
			this.tabIdSearch.Size = new System.Drawing.Size(668, 29);
			this.tabIdSearch.TabIndex = 1;
			this.tabIdSearch.Text = "Find using Id";
			this.tabIdSearch.UseVisualStyleBackColor = true;
			// 
			// QueryIdTextBox
			// 
			this.QueryIdTextBox.Location = new System.Drawing.Point(150, 5);
			this.QueryIdTextBox.Name = "QueryIdTextBox";
			this.QueryIdTextBox.Size = new System.Drawing.Size(95, 20);
			this.QueryIdTextBox.TabIndex = 3;
			this.QueryIdTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.QueryIdTextBoxKeyPress);
			// 
			// QueryIdLabel
			// 
			this.QueryIdLabel.Location = new System.Drawing.Point(7, 8);
			this.QueryIdLabel.Name = "QueryIdLabel";
			this.QueryIdLabel.Size = new System.Drawing.Size(137, 23);
			this.QueryIdLabel.TabIndex = 2;
			this.QueryIdLabel.Text = "Type in file-Id to search for:";
			// 
			// tabStringSearch
			// 
			this.tabStringSearch.Controls.Add(this.QueryStringTextBox);
			this.tabStringSearch.Controls.Add(this.QueryStringLabel);
			this.tabStringSearch.Location = new System.Drawing.Point(4, 22);
			this.tabStringSearch.Name = "tabStringSearch";
			this.tabStringSearch.Padding = new System.Windows.Forms.Padding(3);
			this.tabStringSearch.Size = new System.Drawing.Size(668, 29);
			this.tabStringSearch.TabIndex = 2;
			this.tabStringSearch.Text = "Find using string";
			this.tabStringSearch.UseVisualStyleBackColor = true;
			// 
			// QueryStringTextBox
			// 
			this.QueryStringTextBox.Location = new System.Drawing.Point(150, 5);
			this.QueryStringTextBox.Name = "QueryStringTextBox";
			this.QueryStringTextBox.Size = new System.Drawing.Size(184, 20);
			this.QueryStringTextBox.TabIndex = 5;
			this.QueryStringTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.QueryStringTextBoxKeyPress);
			// 
			// QueryStringLabel
			// 
			this.QueryStringLabel.Location = new System.Drawing.Point(7, 8);
			this.QueryStringLabel.Name = "QueryStringLabel";
			this.QueryStringLabel.Size = new System.Drawing.Size(137, 23);
			this.QueryStringLabel.TabIndex = 4;
			this.QueryStringLabel.Text = "Type in text to search for:";
			// 
			// dataGridView1
			// 
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
			this.dataGridView1.Location = new System.Drawing.Point(12, 114);
			this.dataGridView1.MultiSelect = false;
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView1.Size = new System.Drawing.Size(676, 269);
			this.dataGridView1.TabIndex = 3;
			this.dataGridView1.SelectionChanged += new System.EventHandler(this.DataGridView1SelectionChanged);
			this.dataGridView1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DataGridView1KeyPress);
			this.dataGridView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView1MouseDown);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.findSimilarToolStripMenuItem,
									this.openFileLocationToolStripMenuItem,
									this.dumpDebugInfoToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(169, 70);
			// 
			// findSimilarToolStripMenuItem
			// 
			this.findSimilarToolStripMenuItem.Name = "findSimilarToolStripMenuItem";
			this.findSimilarToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			this.findSimilarToolStripMenuItem.Text = "Find Similar";
			this.findSimilarToolStripMenuItem.Click += new System.EventHandler(this.FindSimilarToolStripMenuItemClick);
			// 
			// openFileLocationToolStripMenuItem
			// 
			this.openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
			this.openFileLocationToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			this.openFileLocationToolStripMenuItem.Text = "Open file location";
			this.openFileLocationToolStripMenuItem.Click += new System.EventHandler(this.OpenFileLocationToolStripMenuItemClick);
			// 
			// dumpDebugInfoToolStripMenuItem
			// 
			this.dumpDebugInfoToolStripMenuItem.Name = "dumpDebugInfoToolStripMenuItem";
			this.dumpDebugInfoToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			this.dumpDebugInfoToolStripMenuItem.Text = "Dump debug info";
			this.dumpDebugInfoToolStripMenuItem.Click += new System.EventHandler(this.DumpDebugInfoToolStripMenuItemClick);
			// 
			// DistanceTypeCombo
			// 
			this.DistanceTypeCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DistanceTypeCombo.FormattingEnabled = true;
			this.DistanceTypeCombo.Location = new System.Drawing.Point(538, 89);
			this.DistanceTypeCombo.Name = "DistanceTypeCombo";
			this.DistanceTypeCombo.Size = new System.Drawing.Size(150, 21);
			this.DistanceTypeCombo.TabIndex = 4;
			this.DistanceTypeCombo.SelectedValueChanged += new System.EventHandler(this.DistanceTypeComboSelectedValueChanged);
			// 
			// GoBtn
			// 
			this.GoBtn.Location = new System.Drawing.Point(12, 87);
			this.GoBtn.Name = "GoBtn";
			this.GoBtn.Size = new System.Drawing.Size(123, 23);
			this.GoBtn.TabIndex = 5;
			this.GoBtn.Text = "Go!";
			this.GoBtn.UseVisualStyleBackColor = true;
			this.GoBtn.Click += new System.EventHandler(this.GoBtnClick);
			// 
			// ResetBtn
			// 
			this.ResetBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ResetBtn.Location = new System.Drawing.Point(613, 12);
			this.ResetBtn.Name = "ResetBtn";
			this.ResetBtn.Size = new System.Drawing.Size(75, 23);
			this.ResetBtn.TabIndex = 6;
			this.ResetBtn.Text = "Reset";
			this.ResetBtn.UseVisualStyleBackColor = true;
			this.ResetBtn.Click += new System.EventHandler(this.ResetBtnClick);
			// 
			// autoPlayCheckBox
			// 
			this.autoPlayCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.autoPlayCheckBox.Checked = true;
			this.autoPlayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoPlayCheckBox.Location = new System.Drawing.Point(535, 12);
			this.autoPlayCheckBox.Name = "autoPlayCheckBox";
			this.autoPlayCheckBox.Size = new System.Drawing.Size(75, 24);
			this.autoPlayCheckBox.TabIndex = 7;
			this.autoPlayCheckBox.Text = "Auto Play";
			this.autoPlayCheckBox.UseVisualStyleBackColor = true;
			this.autoPlayCheckBox.CheckedChanged += new System.EventHandler(this.AutoPlayCheckBoxCheckedChanged);
			// 
			// IgnoreFileLengthCheckBox
			// 
			this.IgnoreFileLengthCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.IgnoreFileLengthCheckBox.Location = new System.Drawing.Point(413, 88);
			this.IgnoreFileLengthCheckBox.Name = "IgnoreFileLengthCheckBox";
			this.IgnoreFileLengthCheckBox.Size = new System.Drawing.Size(119, 24);
			this.IgnoreFileLengthCheckBox.TabIndex = 8;
			this.IgnoreFileLengthCheckBox.Text = "Ignore File Length";
			this.IgnoreFileLengthCheckBox.UseVisualStyleBackColor = true;
			this.IgnoreFileLengthCheckBox.CheckedChanged += new System.EventHandler(this.IgnoreFileLengthCheckedChanged);
			// 
			// versionLabel
			// 
			this.versionLabel.Location = new System.Drawing.Point(12, 6);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(46, 17);
			this.versionLabel.TabIndex = 9;
			this.versionLabel.Text = "Version:";
			// 
			// version
			// 
			this.version.Location = new System.Drawing.Point(64, 6);
			this.version.Name = "version";
			this.version.Size = new System.Drawing.Size(102, 17);
			this.version.TabIndex = 10;
			this.version.Text = "version_number";
			// 
			// rbScms
			// 
			this.rbScms.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.rbScms.Location = new System.Drawing.Point(244, 0);
			this.rbScms.Name = "rbScms";
			this.rbScms.Size = new System.Drawing.Size(58, 24);
			this.rbScms.TabIndex = 11;
			this.rbScms.Text = "Scms";
			this.rbScms.UseVisualStyleBackColor = true;
			this.rbScms.CheckedChanged += new System.EventHandler(this.RbScmsCheckedChanged);
			// 
			// rbSoundfingerprinting
			// 
			this.rbSoundfingerprinting.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.rbSoundfingerprinting.Checked = true;
			this.rbSoundfingerprinting.Location = new System.Drawing.Point(299, 0);
			this.rbSoundfingerprinting.Name = "rbSoundfingerprinting";
			this.rbSoundfingerprinting.Size = new System.Drawing.Size(120, 24);
			this.rbSoundfingerprinting.TabIndex = 12;
			this.rbSoundfingerprinting.TabStop = true;
			this.rbSoundfingerprinting.Text = "Soundfingerprinting";
			this.rbSoundfingerprinting.UseVisualStyleBackColor = true;
			this.rbSoundfingerprinting.CheckedChanged += new System.EventHandler(this.RbSoundfingerprintingCheckedChanged);
			// 
			// txtFilterResults
			// 
			this.txtFilterResults.Location = new System.Drawing.Point(219, 90);
			this.txtFilterResults.Name = "txtFilterResults";
			this.txtFilterResults.Size = new System.Drawing.Size(186, 20);
			this.txtFilterResults.TabIndex = 13;
			this.txtFilterResults.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtFilterResultsKeyPress);
			// 
			// lblFilterResults
			// 
			this.lblFilterResults.Location = new System.Drawing.Point(141, 93);
			this.lblFilterResults.Name = "lblFilterResults";
			this.lblFilterResults.Size = new System.Drawing.Size(81, 19);
			this.lblFilterResults.TabIndex = 14;
			this.lblFilterResults.Text = "Filter Results:";
			// 
			// FindSimilarClientForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(700, 395);
			this.ContextMenuStrip = this.contextMenuStrip1;
			this.Controls.Add(this.lblFilterResults);
			this.Controls.Add(this.txtFilterResults);
			this.Controls.Add(this.rbSoundfingerprinting);
			this.Controls.Add(this.rbScms);
			this.Controls.Add(this.version);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.IgnoreFileLengthCheckBox);
			this.Controls.Add(this.autoPlayCheckBox);
			this.Controls.Add(this.ResetBtn);
			this.Controls.Add(this.GoBtn);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.DistanceTypeCombo);
			this.Controls.Add(this.tabControl1);
			this.Name = "FindSimilarClientForm";
			this.Text = "Find Similar Client";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindSimilarClientFormFormClosing);
			this.tabControl1.ResumeLayout(false);
			this.tabFileSearch.ResumeLayout(false);
			this.tabFileSearch.PerformLayout();
			this.tabIdSearch.ResumeLayout(false);
			this.tabIdSearch.PerformLayout();
			this.tabStringSearch.ResumeLayout(false);
			this.tabStringSearch.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Label lblFilterResults;
		private System.Windows.Forms.TextBox txtFilterResults;
		private System.Windows.Forms.RadioButton rbSoundfingerprinting;
		private System.Windows.Forms.RadioButton rbScms;
		private System.Windows.Forms.Button AudioFilePlayBtn;
		private System.Windows.Forms.ToolStripMenuItem dumpDebugInfoToolStripMenuItem;
		private System.Windows.Forms.Label version;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.CheckBox IgnoreFileLengthCheckBox;
		private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
		private System.Windows.Forms.Label QueryStringLabel;
		private System.Windows.Forms.TextBox QueryStringTextBox;
		private System.Windows.Forms.TabPage tabStringSearch;
		private System.Windows.Forms.CheckBox autoPlayCheckBox;
		private System.Windows.Forms.Button ResetBtn;
		private System.Windows.Forms.Button GoBtn;
		private System.Windows.Forms.ToolStripMenuItem findSimilarToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ComboBox DistanceTypeCombo;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.Label QueryIdLabel;
		private System.Windows.Forms.TextBox QueryIdTextBox;
		private System.Windows.Forms.Label FileQueryLabel;
		private System.Windows.Forms.TextBox AudioFileQueryTextBox;
		private System.Windows.Forms.Button AudioFileQueryBtn;
		private System.Windows.Forms.TabPage tabIdSearch;
		private System.Windows.Forms.TabPage tabFileSearch;
		private System.Windows.Forms.TabControl tabControl1;				

	}
}
