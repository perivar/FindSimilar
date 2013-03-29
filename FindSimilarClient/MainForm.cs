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

namespace FindSimilar
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		IEnumerable<string> filesAll;
		bool rowSelected = false;
		
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			Db db = new Db();
			Dictionary<string, int> filesProcessed = db.GetTracks();
			Console.Out.WriteLine("Database contains {0} processed files.", filesProcessed.Count);
			
			this.dataGridView1.Columns.Add("Id", "Id");
			this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

			this.dataGridView1.Columns.Add("Path", "Path");
			this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;			
			
			foreach (string filePath in filesProcessed.Keys) {
				this.dataGridView1.Rows.Add(filesProcessed[filePath], filePath);
			}

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
		
		private void Play(string filePath) {
			//MessageBox.Show(filePath);

			//string[] asioDevices = SoundPlayer.GetAsioDriverNames();
			//SoundPlayer player = SoundPlayer.GetAsioInstance(asioDevices[0], 250);
			SoundPlayer player = SoundPlayer.GetWaveOutInstance();
			
			//float[] audioData = AudioUtilsNAudio.ReadMonoFromFile(filePath, 44100, 0, 0);
			float[] audioData = Mirage.AudioFileReader.Decode(filePath, Analyzer.SAMPLING_RATE, Analyzer.SECONDS_TO_ANALYZE);
			if (audioData == null || audioData.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return;
			}
			
			//SoundProvider provicer = new SoundProvider(44100, audioData, 2);
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
					string filePath = dgv.SelectedRows[0].Cells[1].Value.ToString();
					Play(filePath);
				}
			}
			rowSelected = true;
		}
	}
}
