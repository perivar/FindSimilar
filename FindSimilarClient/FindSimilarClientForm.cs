using System;
using System.Drawing;
using System.Windows.Forms;

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;

using CommonUtils;

using FindSimilar.AudioProxies;
//using CommonUtils.Audio.NAudio;

using Mirage;
using Comirva.Audio.Feature;

namespace FindSimilar
{
	/// <summary>
	/// FindSimilarClientForm
	/// </summary>
	public partial class FindSimilarClientForm : Form
	{
		// Static Variables
		private static Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.SCMS;
		//private static Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.MandelEllis;
		
		private static int NUM_TO_TAKE = 100;
		private static double PERCENTAGE = 0.8; // 1.0 = disabled
		
		// Instance Variables
		private AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence;
		private Db db = null;
		private IAudio player = null;
		private string selectedFilePath = null;
		
		public FindSimilarClientForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// Constructor code after the InitializeComponent() call.
			//
			this.DistanceTypeCombo.DataSource = Enum.GetValues(typeof(AudioFeature.DistanceType));
			
			this.dataGridView1.Columns.Add("Id", "Id");
			this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

			this.dataGridView1.Columns.Add("Path", "Path");
			this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			
			this.dataGridView1.Columns.Add("Duration", "Duration");
			this.dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

			db = new Db();
			
			ReadAllTracks();
		}
		
		private void ReadAllTracks() {
			
			// Clear all rows
			this.dataGridView1.Rows.Clear();
			
			Dictionary<string, KeyValuePair<int, long>> filesProcessed = db.GetTracks();
			Console.Out.WriteLine("Database contains {0} processed files.", filesProcessed.Count);
			
			int counter = 0;
			foreach (string filePath in filesProcessed.Keys) {
				this.dataGridView1.Rows.Add(filesProcessed[filePath].Key, filePath, filesProcessed[filePath].Value);
				if (counter == NUM_TO_TAKE) break;
				counter++;
			}
		}
		
		private void Play(string filePath) {
			
			// return if play is auto play is disabled
			if (!autoPlayCheckBox.Checked) return;
			
			player = BassProxy.Instance;
			if (player != null) {
				player.Stop();
				player.OpenFile(filePath);
				if (player.CanPlay) {
					player.Play();
				} else {
					Debug.WriteLine("Failed playing using Un4Seen Bass, trying to use mplayer ...");

					float[] audioData = Mirage.AudioFileReader.Decode(filePath, Analyzer.SAMPLING_RATE, Analyzer.SECONDS_TO_ANALYZE);
					if (audioData != null && audioData.Length > 0) {
						player = NAudioProxy.GetWaveOutInstance();
						if (player != null) {
							NAudioFloatArrayProvider provicer = new NAudioFloatArrayProvider(Analyzer.SAMPLING_RATE, audioData, 2);
							((NAudioProxy) player).OpenSampleProvider(provicer);
							if (player.CanPlay) {
								player.Play();
							} else {
								MessageBox.Show("Could not play file!", "Error playing file", MessageBoxButtons.OK, MessageBoxIcon.Error);
							}
						}
					}
				}
			}
		}

		private void PlaySelected() {
			if (player != null) {
				Play(selectedFilePath);
			}
		}
		
		#region Drag and Drop
		void TabPage1DragEnter(object sender, DragEventArgs e)
		{
			/*
			foreach ( var item in e.Data.GetFormats() ) {
				MessageBox.Show( item );
			}
			 */

			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				e.Effect = DragDropEffects.Copy;
			} else if (e.Data.GetDataPresent(DataFormats.Text)) {
				e.Effect = DragDropEffects.Copy;
			}
		}
		
		void TabPage1DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (string inputFilePath in files) {
					string fileExtension = Path.GetExtension(inputFilePath);
					int pos = Array.IndexOf(Mir.extensions, fileExtension);
					if (pos >- 1)
					{
						AudioFileQueryTextBox.Text = inputFilePath;
						break;
					}
				}
			} else if (e.Data.GetDataPresent(DataFormats.Text)) {
				string droppedText = (string)e.Data.GetData(DataFormats.Text);
				AudioFileQueryTextBox.Text = droppedText;
			}
		}
		#endregion
		
		void AudioFileQueryBtnClick(object sender, EventArgs e)
		{
			// convert extension string array to open file dialog filter
			//openFileDialog.Filter = "Audio Files(*.wav;*.mp3)|*.wav;*.mp3|All files (*.*)|*.*";
			//string filter = string.Join(";", Mir.extensionsWithStar);
			//filter = String.Format("Audio Files({0})|{0}|All files (*.*)|*.*", filter);
			string filter = "All supported Audio Files|*.wav;*.ogg;*.mp1;*.m1a;*.mp2;*.m2a;*.mpa;*.mus;*.mp3;*.mpg;*.mpeg;*.mp3pro;*.aif;*.aiff;*.bwf;*.wma;*.wmv;*.aac;*.adts;*.mp4;*.m4a;*.m4b;*.mod;*.mdz;*.mo3;*.s3m;*.s3z;*.xm;*.xmz;*.it;*.itz;*.umx;*.mtm;*.flac;*.fla;*.oga;*.ogg;*.aac;*.m4a;*.m4b;*.mp4;*.mpc;*.mp+;*.mpp;*.ac3;*.wma;*.ape;*.mac|WAVE Audio|*.wav|Ogg Vorbis|*.ogg|MPEG Layer 1|*.mp1;*.m1a|MPEG Layer 2|*.mp2;*.m2a;*.mpa;*.mus|MPEG Layer 3|*.mp3;*.mpg;*.mpeg;*.mp3pro|Audio IFF|*.aif;*.aiff|Broadcast Wave|*.bwf|Windows Media Audio|*.wma;*.wmv|Advanced Audio Codec|*.aac;*.adts|MPEG 4 Audio|*.mp4;*.m4a;*.m4b|MOD Music|*.mod;*.mdz|MO3 Music|*.mo3|S3M Music|*.s3m;*.s3z|XM Music|*.xm;*.xmz|IT Music|*.it;*.itz;*.umx|MTM Music|*.mtm|Free Lossless Audio Codec|*.flac;*.fla|Free Lossless Audio Codec (Ogg)|*.oga;*.ogg|Advanced Audio Coding|*.aac|Advanced Audio Coding MPEG-4|*.m4a;*.m4b;*.mp4|Musepack|*.mpc;*.mp+;*.mpp|Dolby Digital AC-3|*.ac3|Windows Media Audio|*.wma|Monkey's Audio|*.ape;*.mac";
			openFileDialog.Filter = filter;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				AudioFileQueryTextBox.Text = openFileDialog.FileName;
			}
		}
		
		void DistanceTypeComboSelectedValueChanged(object sender, EventArgs e)
		{
			Enum.TryParse<AudioFeature.DistanceType>(DistanceTypeCombo.SelectedValue.ToString(), out distanceType);
		}
		
		void FindSimilarClientFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if (player != null) player.Dispose();
		}
		
		#region DataGridView Navigation
		void DataGridView1SelectionChanged(object sender, EventArgs e)
		{
			// on first load the selectedfilepath is null
			bool doPlay = true;
			if (selectedFilePath == null) {
				doPlay = false;
			}
			
			DataGridView dgv = (DataGridView)sender;

			// User selected WHOLE ROW (by clicking in the margin)
			// or if SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			if (dgv.SelectedRows.Count> 0) {
				if (dgv.SelectedRows[0].Cells[1].Value != null) {
					selectedFilePath = dgv.SelectedRows[0].Cells[1].Value.ToString();
					if (doPlay) Play(selectedFilePath);
				}
			}
		}
		
		void DataGridView1CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right) {
				dataGridView1.CurrentCell = dataGridView1[e.ColumnIndex, e.RowIndex];
			} else if (e.Button == MouseButtons.Left) {
				if (e.ColumnIndex >= 0 && e.RowIndex >= 0) {
					dataGridView1.CurrentCell = null;
					dataGridView1.CurrentCell = dataGridView1[e.ColumnIndex, e.RowIndex];
				}
			}
		}
		
		void DataGridView1KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Space) {
				PlaySelected();
			}
		}
		
		void DataGridView1MouseDown(object sender, MouseEventArgs e)
		{
			// The DoDragDrop method of a control is used to start a drag and drop operation.
			// We call it from MouseDown event of the DataGridView.
			// The first parameter is the data that we want to send in drag and drop operation.
			// Here we are sending selected rows of the DataGridView.
			// The second parameter is a DragDropEffects enumeration that provides the drag and drop operation effect.
			// The cursor style changes accordingly while the drag and drop is being performed.
			// Possible values are DragDropEffects.All, DragDropEffects.Copy, DragDropEffects.Link, DragDropEffects.Move,
			// DragDropEffects.None and DragDropEffects.Scroll.
			if (dataGridView1.SelectedRows[0].Cells[1].Value != null) {
				string data = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
				dataGridView1.DoDragDrop(data, DragDropEffects.Copy);
			}
		}
		#endregion
		
		#region ToolStripMenu Clicks
		void FindSimilarToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (dataGridView1.SelectedRows[0].Cells[0].Value != null) {
				//string queryPath = (string) dataGridView1.SelectedRows[0].Cells[1].Value;
				int queryId = (int) dataGridView1.SelectedRows[0].Cells[0].Value;
				FindById(queryId);
			}
		}
		
		void OpenFileLocationToolStripMenuItemClick(object sender, EventArgs e)
		{
			string path = Path.GetDirectoryName(selectedFilePath);
			System.Diagnostics.Process.Start(path);
		}
		#endregion
		
		void ResetBtnClick(object sender, EventArgs e)
		{
			ReadAllTracks();
		}
		
		void GoBtnClick(object sender, EventArgs e)
		{
			if (tabControl1.SelectedTab == tabControl1.TabPages["tabFileSearch"])
			{
				string queryPath = AudioFileQueryTextBox.Text;
				FindByFilePath(queryPath);
			} else if (tabControl1.SelectedTab == tabControl1.TabPages["tabIdSearch"]) {
				int queryId = -1;
				int.TryParse(QueryIdTextBox.Text, out queryId);
				FindById(queryId);
			} else if (tabControl1.SelectedTab == tabControl1.TabPages["tabStringSearch"]) {
				string queryString = QueryStringTextBox.Text;
				FindByString(queryString);
			}
		}
		
		#region Find methods
		private void FindByFilePath(string queryPath) {
			if (queryPath != "") {
				FileInfo fi = new FileInfo(queryPath);
				if (fi.Exists) {
					
					// Clear all rows
					this.dataGridView1.Rows.Clear();

					// Add the one we are querying with
					this.dataGridView1.Rows.Add(-1, queryPath, 0);
					
					// Add the found similar tracks
					var similarTracks = Mir.SimilarTracks(queryPath, db, analysisMethod, NUM_TO_TAKE, PERCENTAGE, distanceType);
					foreach (var entry in similarTracks)
					{
						this.dataGridView1.Rows.Add(entry.Key.Key, entry.Key.Value, entry.Value);
					}
				} else {
					MessageBox.Show("File does not exist!");
				}

				// reset
				//selectedFilePath = null;
			}
		}
		
		private void FindById(int queryId) {
			if (queryId != -1) {
				int[] seedTrackIds = new int[] { queryId };
				
				// Clear all rows
				this.dataGridView1.Rows.Clear();

				// Add the one we are querying with
				AudioFeature m1 = db.GetTrack(queryId, analysisMethod);
				
				if (m1 != null) {
					this.dataGridView1.Rows.Add(queryId, m1.Name, 0);
					
					// Add the found similar tracks
					var similarTracks = Mir.SimilarTracks(seedTrackIds, seedTrackIds, db, analysisMethod, NUM_TO_TAKE, PERCENTAGE, distanceType);
					foreach (var entry in similarTracks)
					{
						this.dataGridView1.Rows.Add(entry.Key.Key, entry.Key.Value, entry.Value);
					}
				} else {
					MessageBox.Show("File-id does not exist!");
				}
			}
			
			// reset
			//selectedFilePath = null;
		}
		
		private void FindByString(string queryString) {

			if (queryString != "") {
				
				// Clear all rows
				this.dataGridView1.Rows.Clear();

				// search for tracks
				string whereClause = string.Format("WHERE name like '%{0}%'", queryString);
				Dictionary<string, KeyValuePair<int, long>> filesFound = db.GetTracks(whereClause);
				Console.Out.WriteLine("Database contains {0} files that matches the query '{1}'.", filesFound.Count, queryString);
				
				int counter = 0;
				foreach (string filePath in filesFound.Keys) {
					this.dataGridView1.Rows.Add(filesFound[filePath].Key, filePath, filesFound[filePath].Value);
					if (counter == NUM_TO_TAKE) break;
					counter++;
				}
			}
			
			// reset
			//selectedFilePath = null;
		}
		
		void QueryIdTextBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Enter) {
				int queryId = -1;
				int.TryParse(QueryIdTextBox.Text, out queryId);
				FindById(queryId);
			}
		}
		
		void AudioFileQueryTextBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Enter) {
				string queryPath = AudioFileQueryTextBox.Text;
				FindByFilePath(queryPath);
			}
		}
		
		void QueryStringTextBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Enter) {
				string queryString = QueryStringTextBox.Text;
				FindByString(queryString);
			}
		}
		#endregion

		void AutoPlayCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (!autoPlayCheckBox.Checked) {
				if (player != null) {
					player.Stop();
				}
			}
		}
	}
}