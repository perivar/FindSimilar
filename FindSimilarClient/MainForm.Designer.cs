namespace FindSimilar
{
	partial class MainForm
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
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.findSimilarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ResetBtn = new System.Windows.Forms.Button();
			this.DistanceTypeCombo = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// dataGridView1
			// 
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
									| System.Windows.Forms.AnchorStyles.Left) 
									| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
			this.dataGridView1.Location = new System.Drawing.Point(12, 41);
			this.dataGridView1.MultiSelect = false;
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView1.Size = new System.Drawing.Size(654, 278);
			this.dataGridView1.TabIndex = 1;
			this.dataGridView1.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridView1CellMouseDown);
			this.dataGridView1.SelectionChanged += new System.EventHandler(this.DataGridView1_SelectionChanged);
			this.dataGridView1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DataGridView1KeyPress);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.findSimilarToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(153, 48);
			// 
			// findSimilarToolStripMenuItem
			// 
			this.findSimilarToolStripMenuItem.Name = "findSimilarToolStripMenuItem";
			this.findSimilarToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.findSimilarToolStripMenuItem.Text = "Find Similar";
			this.findSimilarToolStripMenuItem.Click += new System.EventHandler(this.FindSimilarToolStripMenuItemClick);
			// 
			// ResetBtn
			// 
			this.ResetBtn.Location = new System.Drawing.Point(12, 12);
			this.ResetBtn.Name = "ResetBtn";
			this.ResetBtn.Size = new System.Drawing.Size(75, 23);
			this.ResetBtn.TabIndex = 2;
			this.ResetBtn.Text = "Reset";
			this.ResetBtn.UseVisualStyleBackColor = true;
			this.ResetBtn.Click += new System.EventHandler(this.ResetBtnClick);
			// 
			// DistanceTypeCombo
			// 
			this.DistanceTypeCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DistanceTypeCombo.FormattingEnabled = true;
			this.DistanceTypeCombo.Location = new System.Drawing.Point(516, 14);
			this.DistanceTypeCombo.Name = "DistanceTypeCombo";
			this.DistanceTypeCombo.Size = new System.Drawing.Size(150, 21);
			this.DistanceTypeCombo.TabIndex = 3;
			this.DistanceTypeCombo.SelectedIndexChanged += new System.EventHandler(this.DistanceTypeComboSelectedIndexChanged);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(678, 331);
			this.Controls.Add(this.DistanceTypeCombo);
			this.Controls.Add(this.ResetBtn);
			this.Controls.Add(this.dataGridView1);
			this.Name = "MainForm";
			this.Text = "FindSimilarClient";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.ComboBox DistanceTypeCombo;
		private System.Windows.Forms.Button ResetBtn;
		private System.Windows.Forms.ToolStripMenuItem findSimilarToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.DataGridView dataGridView1;
	}
}
