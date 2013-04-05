using System;
using System.Drawing;
using System.Windows.Forms;

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;

using NAudio.Wave;

using CommonUtils;
using CommonUtils.Audio.NAudio;

using Mirage;
using Comirva.Audio.Feature;

namespace FindSimilar
{
	/// <summary>
	/// FindSimilarClientForm
	/// </summary>
	public partial class FindSimilarClientForm : Form
	{
		Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.SCMS;
		//Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.MandelEllis;
		private static int numToTake = 100;
		private static double percentage = 0.8; // 1.0 = disabled
		
		private AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence;
		private Db db = null;
		private SoundPlayer player;
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
			player = GetSoundPlayer();
			
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
		
		private SoundPlayer GetSoundPlayer() {
			
			//string[] asioDevices = SoundPlayer.GetAsioDriverNames();
			//return SoundPlayer.GetAsioInstance(asioDevices[0], 250);
			return SoundPlayer.GetWaveOutInstance();
		}
		
		private void ReadAllTracks() {
			
			// Clear all rows
			this.dataGridView1.Rows.Clear();
			
			Dictionary<string, KeyValuePair<int, long>> filesProcessed = db.GetTracks();
			Console.Out.WriteLine("Database contains {0} processed files.", filesProcessed.Count);
			
			foreach (string filePath in filesProcessed.Keys) {
				this.dataGridView1.Rows.Add(filesProcessed[filePath].Key, filePath, filesProcessed[filePath].Value);
			}
		}
		
		private void Play(string filePath) {
			
			// return if play is auto play is disabled
			if (!autoPlayCheckBox.Checked) return;
			
			float[] audioData = Mirage.AudioFileReader.Decode(filePath, Analyzer.SAMPLING_RATE, Analyzer.SECONDS_TO_ANALYZE);
			if (audioData == null || audioData.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return;
			}
			
			player = GetSoundPlayer();
			SoundProvider provicer = new SoundProvider(Analyzer.SAMPLING_RATE, audioData, 2);
			//player.OpenFile(filePath);
			player.OpenSampleProvider(provicer);
			if (player.CanPlay) {
				player.Play();
			} else {
				Debug.WriteLine("Failed playing ...");
			}
		}

		private void PlaySelected() {
			if (player != null) {
				player.Stop();
				Play(selectedFilePath);
			}
		}
		
		void TabPage1DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
		}
		
		void TabPage1DragDrop(object sender, DragEventArgs e)
		{
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
		}
		
		void AudioFileQueryBtnClick(object sender, EventArgs e)
		{
			// convert extension string array to open file dialog filter
			//openFileDialog.Filter = "Audio Files(*.wav;*.mp3)|*.wav;*.mp3|All files (*.*)|*.*";
			string filter = string.Join(";", Mir.extensionsWithStar);
			openFileDialog.Filter = String.Format("Audio Files({0})|{0}|All files (*.*)|*.*", filter);
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
		
		void FindSimilarToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (dataGridView1.SelectedRows[0].Cells[0].Value != null) {
				//string queryPath = (string) dataGridView1.SelectedRows[0].Cells[1].Value;
				int queryId = (int) dataGridView1.SelectedRows[0].Cells[0].Value;
				FindById(queryId);
			}
		}
		
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
			}
		}
		
		private void FindByFilePath(string queryPath) {
			if (queryPath != "") {
				FileInfo fi = new FileInfo(queryPath);
				if (fi.Exists) {
					
					// Clear all rows
					this.dataGridView1.Rows.Clear();

					// Add the one we are querying with
					this.dataGridView1.Rows.Add(-1, queryPath, 0);
					
					// Add the found similar tracks
					var similarTracks = Mir.SimilarTracks(queryPath, db, analysisMethod, numToTake, percentage, distanceType);
					foreach (var entry in similarTracks)
					{
						this.dataGridView1.Rows.Add(entry.Key.Key, entry.Key.Value, entry.Value);
					}
				} else {
					MessageBox.Show("File does not exist!");
				}

				// reset
				selectedFilePath = null;
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
					var similarTracks = Mir.SimilarTracks(seedTrackIds, seedTrackIds, db, analysisMethod, numToTake, percentage, distanceType);
					foreach (var entry in similarTracks)
					{
						this.dataGridView1.Rows.Add(entry.Key.Key, entry.Key.Value, entry.Value);
					}
				} else {
					MessageBox.Show("File-id does not exist!");
				}
			}
			
			// reset
			selectedFilePath = null;
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