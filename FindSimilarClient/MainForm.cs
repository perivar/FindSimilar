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
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		IEnumerable<string> filesAll;
		bool rowSelected = false;
		Db db = null;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			this.dataGridView1.Columns.Add("Id", "Id");
			this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

			this.dataGridView1.Columns.Add("Path", "Path");
			this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			
			this.dataGridView1.Columns.Add("Duration", "Duration");
			this.dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

			db = new Db();
			ResetView();
			
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

		private void ResetView() {
			
			// Clear all rows
			this.dataGridView1.Rows.Clear();
			
			Dictionary<string, KeyValuePair<int, long>> filesProcessed = db.GetTracks();
			Console.Out.WriteLine("Database contains {0} processed files.", filesProcessed.Count);
			
			foreach (string filePath in filesProcessed.Keys) {
				this.dataGridView1.Rows.Add(filesProcessed[filePath].Key, filePath, filesProcessed[filePath].Value);
			}
		}
		
		private void Play(string filePath) {
			//string[] asioDevices = SoundPlayer.GetAsioDriverNames();
			//SoundPlayer player = SoundPlayer.GetAsioInstance(asioDevices[0], 250);
			SoundPlayer player = SoundPlayer.GetWaveOutInstance();
			
			float[] audioData = Mirage.AudioFileReader.Decode(filePath, Analyzer.SAMPLING_RATE, Analyzer.SECONDS_TO_ANALYZE);
			if (audioData == null || audioData.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return;
			}
			
			SoundProvider provicer = new SoundProvider(Analyzer.SAMPLING_RATE, audioData, 2);
			
			//player.OpenFile(filePath);
			player.OpenSampleProvider(provicer);
			if (player.CanPlay) {
				player.Play();
			} else {
				Debug.WriteLine("Failed playing ...");
			}
		}
		
		private void DataGridView1_SelectionChanged(object sender, EventArgs e)
		{
			if (rowSelected) {
				DataGridView dgv = (DataGridView)sender;

				// User selected WHOLE ROW (by clicking in the margin)
				// or if SelectionMode = DataGridViewSelectionMode.FullRowSelect;
				if (dgv.SelectedRows.Count> 0) {
					if (dgv.SelectedRows[0].Cells[1].Value != null) {
						string filePath = dgv.SelectedRows[0].Cells[1].Value.ToString();
						Play(filePath);
					}
				}
			}
			rowSelected = true;
		}
		
		void FindSimilarToolStripMenuItemClick(object sender, EventArgs e)
		{
			Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.SCMS;
			//Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.MandelEllis;
			
			AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence;

			int numToTake = 100;
			double percentage = 0.5;
			
			if (dataGridView1.SelectedRows[0].Cells[0].Value != null) {
				int queryId = (int) dataGridView1.SelectedRows[0].Cells[0].Value;
				int[] seedTrackIds = new int[] { queryId };
				
				// Clear all rows
				this.dataGridView1.Rows.Clear();

				var similarTracks = Mir.SimilarTracks(seedTrackIds, seedTrackIds, db, analysisMethod, numToTake, percentage, distanceType);
				foreach (var entry in similarTracks)
				{
					this.dataGridView1.Rows.Add(entry.Key.Key, entry.Key.Value, entry.Value);
				}
			}
		}
		
		void DataGridView1CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				dataGridView1.CurrentCell = dataGridView1[e.ColumnIndex, e.RowIndex];
			}
		}
		
		void ResetBtnClick(object sender, EventArgs e)
		{
			ResetView();
		}
	}
}
