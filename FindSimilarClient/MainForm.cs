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
	/// Description of FindSimilar GUI Client.
	/// </summary>
	public partial class MainForm : Form
	{
		//IEnumerable<string> filesAll;
		private AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence;
		private Db db = null;
		private SoundPlayer player;
		private string selectedFilePath = null;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
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
			
			/*
			string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects";
			string[] extensions = { "*.mp3", "*.wma", "*.mp4", "*.wav", "*.ogg" };
			filesAll = IOUtils.GetFiles(path, extensions, SearchOption.AllDirectories);
			Debug.WriteLine("Found {0} files in scan directory.", filesAll.Count());
			
			this.dataGridView1.Columns.Add("Path", "Path");
			this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			
			this.dataGridView1.Columns.Add("Play", "Play");
			this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			
			foreach (string filePath in filesAll) {
				this.dataGridView1.Rows.Add(filePath);
			}
			 */
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
		
		void FindSimilarToolStripMenuItemClick(object sender, EventArgs e)
		{
			Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.SCMS;
			//Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.MandelEllis;
			
			//AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence;

			int numToTake = 100;
			double percentage = 0.5;
			
			if (dataGridView1.SelectedRows[0].Cells[0].Value != null) {
				string queryPath = (string) dataGridView1.SelectedRows[0].Cells[1].Value;
				int queryId = (int) dataGridView1.SelectedRows[0].Cells[0].Value;
				int[] seedTrackIds = new int[] { queryId };
				
				// Clear all rows
				this.dataGridView1.Rows.Clear();

				// Add the one we are querying with
				this.dataGridView1.Rows.Add(queryId, queryPath, 0);
				
				// Add the found similar tracks
				var similarTracks = Mir.SimilarTracks(seedTrackIds, seedTrackIds, db, analysisMethod, numToTake, percentage, distanceType);
				foreach (var entry in similarTracks)
				{
					this.dataGridView1.Rows.Add(entry.Key.Key, entry.Key.Value, entry.Value);
				}
			}
		}
		
		private void DataGridView1_SelectionChanged(object sender, EventArgs e)
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
				if (e.ColumnIndex >= 0) {
					dataGridView1.CurrentCell = null;
					dataGridView1.CurrentCell = dataGridView1[e.ColumnIndex, e.RowIndex];
				}
			}
		}
		
		void ResetBtnClick(object sender, EventArgs e)
		{
			ReadAllTracks();
		}
		
		void DistanceTypeComboSelectedIndexChanged(object sender, EventArgs e)
		{
			Enum.TryParse<AudioFeature.DistanceType>(DistanceTypeCombo.SelectedValue.ToString(), out distanceType);
		}
		
		void DataGridView1KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Space) {
				PlaySelected();
			}
		}
		
		void MainFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if (player != null) player.Dispose();
		}
		
	}
}
