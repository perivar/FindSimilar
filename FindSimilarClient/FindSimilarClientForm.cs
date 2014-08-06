using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using System.Diagnostics;

using CommonUtils;

using FindSimilar.AudioProxies;

using Mirage;
using Comirva.Audio.Feature;

using Soundfingerprinting;
using Soundfingerprinting.Fingerprinting;
using Soundfingerprinting.Hashing;
using Soundfingerprinting.Dao;
using Soundfingerprinting.Dao.Entities;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.DbStorage.Entities;

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
		
		private static int DEFAULT_NUM_TO_TAKE = 200;
		private static double DEFAULT_PERCENTAGE_ENABLED = 0.8;
		private static double DEFAULT_PERCENTAGE_DISABLED = 1.0;

		// Instance Variables
		private AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence;
		private Db db = null;
		private IAudio player = null;
		private string selectedFilePath = null;
		private double percentage = DEFAULT_PERCENTAGE_ENABLED;
		
		// Soundfingerprinting
		private DatabaseService databaseService = null;
		private Repository repository = null;
		
		BindingSource bs = new BindingSource();
		BindingList<QueryResult> queryResultList;

		public FindSimilarClientForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// Constructor code after the InitializeComponent() call.
			//
			this.version.Text = Mirage.Mir.VERSION;
			this.DistanceTypeCombo.DataSource = Enum.GetValues(typeof(AudioFeature.DistanceType));
			
			this.db = new Db();
			
			// Instansiate soundfingerprinting Repository
			FingerprintService fingerprintService = Analyzer.GetSoundfingerprintingService();
			this.databaseService = DatabaseService.Instance;

			//IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms.csv", ",");
			IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms-new.csv", ",");
			
			repository = new Repository(permutations, databaseService, fingerprintService);
			
			if (rbScms.Checked) {
				IgnoreFileLengthCheckBox.Visible = true;
				DistanceTypeCombo.Visible = true;
			} else {
				IgnoreFileLengthCheckBox.Visible = false;
				DistanceTypeCombo.Visible = false;
			}
			
			ReadAllTracks();
		}
		
		#region Play
		void AudioFilePlayBtnClick(object sender, EventArgs e)
		{
			string queryPath = AudioFileQueryTextBox.Text;
			if (player != null && !queryPath.Equals("")) {
				Play(queryPath);
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
		#endregion
		
		#region Drag and Drop
		void TabPage1DragEnter(object sender, DragEventArgs e)
		{
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
		
		void DataGridView1KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Space) {
				PlaySelected();
			}
		}
		
		void DataGridView1MouseDown(object sender, MouseEventArgs e)
		{
			// Get the row index of the item the mouse is below.
			DataGridView.HitTestInfo hti = dataGridView1.HitTest(e.X, e.Y);

			if (hti.ColumnIndex >= 0 && hti.RowIndex >= 0) {
				DataGridViewCell dragCell = dataGridView1[hti.ColumnIndex, hti.RowIndex];
				
				// set current cell
				dataGridView1.CurrentCell = null;
				dataGridView1.CurrentCell = dragCell;

				// check value
				if (e.Button == MouseButtons.Left) {
					
					if (dragCell.Value != null) {
						
						// The DoDragDrop method of a control is used to start a drag and drop operation.
						// We call it from MouseDown event of the DataGridView.
						// The first parameter is the data that we want to send in drag and drop operation.
						// The second parameter is a DragDropEffects enumeration that provides the drag and drop operation effect.
						// The cursor style changes accordingly while the drag and drop is being performed.
						// Possible values are DragDropEffects.All, DragDropEffects.Copy, DragDropEffects.Link, DragDropEffects.Move,
						// DragDropEffects.None and DragDropEffects.Scroll.
						
						string cellContent = dragCell.Value.ToString();
						//string dataFormat = DataFormats.Text;
						//dataGridView1.DoDragDrop(cellContent, DragDropEffects.Copy);
						
						string filePath = cellContent;
						if (File.Exists(filePath)) {
							string dataFormat = DataFormats.FileDrop;
							string[] filePathArray = { filePath };
							DataObject dataObject = new DataObject(dataFormat, filePathArray);
							dataGridView1.DoDragDrop(dataObject, DragDropEffects.Copy);
						}

					}
				}
			}
		}
		#endregion
		
		#region ToolStripMenu Clicks
		void FindSimilarToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (dataGridView1.SelectedRows[0].Cells[0].Value != null) {
				int queryId = (int) dataGridView1.SelectedRows[0].Cells[0].Value;
				FindById(queryId);
			}
		}
		
		void OpenFileLocationToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (!File.Exists(selectedFilePath))	{
				return;
			}
			
			string args = string.Format("/e, /select, \"{0}\"", selectedFilePath);

			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = "explorer";
			info.Arguments = args;
			Process.Start(info);
		}

		void DumpDebugInfoToolStripMenuItemClick(object sender, EventArgs e)
		{
			FileInfo fileInfo = new FileInfo(selectedFilePath);
			
			//TODO: Must also use right analyser, sound analyser missing completely
			
			AudioFeature feature = null;
			
			switch (analysisMethod) {
				case Analyzer.AnalysisMethod.MandelEllis:
					feature = Analyzer.AnalyzeMandelEllis(fileInfo, true);
					break;
				case Analyzer.AnalysisMethod.SCMS:
					feature = Analyzer.AnalyzeScms(fileInfo, true);
					break;
			}
			
			if (feature != null) {
				string text = String.Format("Name: {0}\nDuration: {1} ms", feature.Name, feature.Duration);
				MessageBox.Show(text, "Feature information");
			}
		}
		#endregion
		
		#region Button Clicks, Combo and Checkbox Changes and Form Closing
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
		
		void AutoPlayCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (!autoPlayCheckBox.Checked) {
				if (player != null) {
					player.Stop();
				}
			}
		}
		
		void IgnoreFileLengthCheckedChanged(object sender, EventArgs e)
		{
			if (IgnoreFileLengthCheckBox.Checked) {
				percentage = DEFAULT_PERCENTAGE_DISABLED;
			} else {
				percentage = DEFAULT_PERCENTAGE_ENABLED;
			}
		}
		
		void FindSimilarClientFormFormClosing(object sender, FormClosingEventArgs e)
		{
			if (player != null) player.Dispose();
		}
		#endregion

		#region ReadAllTracks
		private void ReadAllTracks() {
			if (rbScms.Checked) {
				ReadAllTracksScms();
			} else if (rbSoundfingerprinting.Checked) {
				ReadAllTracksSoundfingerprinting();
			}
		}
		
		private void ReadAllTracksScms() {
			
			queryResultList = new BindingList<QueryResult>( db.GetTracksList(DEFAULT_NUM_TO_TAKE) );
			
			bs.DataSource = queryResultList;
			dataGridView1.DataSource = queryResultList;
			
			this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

			this.database_count.Text = db.GetTrackCount().ToString();
		}

		private void ReadAllTracksSoundfingerprinting() {
			
			string limitClause = string.Format("LIMIT {0}", DEFAULT_NUM_TO_TAKE);
			IList<Track> tracks = databaseService.ReadTracks(limitClause);
			
			var fingerprintList = (from row in tracks
			                       orderby row.Id
			                       select new QueryResult {
			                       	Id = row.Id,
			                       	Path = row.FilePath,
			                       	Duration = row.TrackLengthMs
			                       }).ToList();
			
			queryResultList = new BindingList<QueryResult>( fingerprintList );
			
			bs.DataSource = queryResultList;
			dataGridView1.DataSource = queryResultList;
			
			this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			
			this.database_count.Text = databaseService.GetTrackCount().ToString();
		}
		#endregion
		
		#region Find Scsm methods
		private void FindByFilePathScms(string queryPath) {
			if (queryPath != "") {
				FileInfo fi = new FileInfo(queryPath);

				if (fi.Exists) {
					
					// Add the found similar tracks
					var similarTracks = Mir.SimilarTracksList(queryPath, db, analysisMethod, DEFAULT_NUM_TO_TAKE, percentage, distanceType);
					
					// Add the one we are querying with at the top
					similarTracks.Insert(0, new QueryResult(0, queryPath, 0, 0));
					
					queryResultList = new BindingList<QueryResult>( similarTracks );
					
					bs.DataSource = queryResultList;
					dataGridView1.DataSource = queryResultList;
					
					this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
					this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

				} else {
					MessageBox.Show("File does not exist!");
				}
			}
		}

		private void FindByIdScms(int queryId) {
			if (queryId != -1) {
				int[] seedTrackIds = new int[] { queryId };
				
				AudioFeature m1 = db.GetTrack(queryId, analysisMethod);
				
				if (m1 != null) {
					// Add the found similar tracks
					var similarTracks = Mir.SimilarTracksList(seedTrackIds, seedTrackIds, db, analysisMethod, DEFAULT_NUM_TO_TAKE, percentage, distanceType);
					
					// Add the one we are querying with at the top
					similarTracks.Insert(0, new QueryResult(queryId, m1.Name, m1.Duration, 0));
					
					queryResultList = new BindingList<QueryResult>( similarTracks );
					
					bs.DataSource = queryResultList;
					dataGridView1.DataSource = queryResultList;
					
					this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
					this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

				} else {
					MessageBox.Show("File-id does not exist!");
				}
			}
		}
		
		private void FindByStringScms(string queryString) {

			if (queryString != "") {
				
				// search for tracks
				string whereClause = string.Format("WHERE name like '%{0}%'", queryString);
				queryResultList = new BindingList<QueryResult>( db.GetTracksList(DEFAULT_NUM_TO_TAKE, whereClause) );
				
				bs.DataSource = queryResultList;
				dataGridView1.DataSource = queryResultList;
				
				this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			}
		}
		#endregion

		#region Find Soundfingerprinting methods
		private void FindByFilePathSoundfingerprinting(string queryPath) {
			if (queryPath != "") {
				FileInfo fi = new FileInfo(queryPath);
				if (fi.Exists) {
					
					List<QueryResult> queryList = Analyzer.SimilarTracksSoundfingerprintingList(fi, repository);
					queryResultList = new BindingList<QueryResult>( queryList );
					
					bs.DataSource = queryResultList;
					dataGridView1.DataSource = queryResultList;
					
					this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
					this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

				} else {
					MessageBox.Show("File does not exist!");
				}
			}
		}
		
		private void FindByIdSoundfingerprinting(int queryId) {
			
			if (queryId != -1) {
				
				Track track = databaseService.ReadTrackById(queryId);
				if (track != null) {
					
					List<QueryResult> queryList = Analyzer.SimilarTracksSoundfingerprintingList(new FileInfo(track.FilePath), repository);
					queryResultList = new BindingList<QueryResult>( queryList );
					
					bs.DataSource = queryResultList;
					dataGridView1.DataSource = queryResultList;
					
					this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
					this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

				} else {
					MessageBox.Show("File-id does not exist!");
				}
			}
		}
		
		private void FindByStringSoundfingerprinting(string queryString) {
			
			if (queryString != "") {
				
				// search for tracks
				string whereClause = string.Format("WHERE tags like '%{0}%' or title like '%{0}%' or filepath like '%{0}%'", queryString);
				IList<Track> tracks = databaseService.ReadTracks(whereClause);

				var fingerprintList = (from row in tracks
				                       orderby row.Id ascending
				                       select new QueryResult {
				                       	Id = row.Id,
				                       	Path = row.FilePath,
				                       	Duration = row.TrackLengthMs
				                       }).ToList();
				
				queryResultList = new BindingList<QueryResult>( fingerprintList );
				
				bs.DataSource = queryResultList;
				dataGridView1.DataSource = queryResultList;
				
				this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			}
		}
		#endregion
		
		#region Find methods
		private void FindByFilePath(string queryPath) {
			if (rbScms.Checked) {
				FindByFilePathScms(queryPath);
			} else if (rbSoundfingerprinting.Checked) {
				FindByFilePathSoundfingerprinting(queryPath);
			}
		}
		
		private void FindById(int queryId) {
			if (rbScms.Checked) {
				FindByIdScms(queryId);
			} else if (rbSoundfingerprinting.Checked) {
				FindByIdSoundfingerprinting(queryId);
			}
		}
		
		private void FindByString(string queryString) {
			if (rbScms.Checked) {
				FindByStringScms(queryString);
			} else if (rbSoundfingerprinting.Checked) {
				FindByStringSoundfingerprinting(queryString);
			}
		}
		#endregion
		
		#region Query Field Actions
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
		
		#region Radio Button Change Events
		void RbScmsCheckedChanged(object sender, EventArgs e)
		{
			if (rbScms.Checked) {
				IgnoreFileLengthCheckBox.Visible = true;
				DistanceTypeCombo.Visible = true;
			} else {
				IgnoreFileLengthCheckBox.Visible = false;
				DistanceTypeCombo.Visible = false;
			}
			ReadAllTracks();
		}
		
		void RbSoundfingerprintingCheckedChanged(object sender, EventArgs e)
		{
			ReadAllTracks();
		}
		#endregion
		
		#region Filtering of the query results
		void TxtFilterResultsKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char) Keys.Enter) {
				if (queryResultList != null) {
					BindingList<QueryResult> filtered = new BindingList<QueryResult>(
						queryResultList.Where(result => result.Path.ToLower().Contains(txtFilterResults.Text.ToLower())).ToList());
					dataGridView1.DataSource = filtered;
					dataGridView1.Update();
				}
			}
		}
		
		void BtnClearFilterClick(object sender, EventArgs e)
		{
			txtFilterResults.Text = "";
			if (queryResultList != null) {
				dataGridView1.DataSource = queryResultList;
				dataGridView1.Update();
			}
		}
		#endregion
		
	}
	
	// http://stackoverflow.com/questions/17309270/datagridview-binding-source-filter
	public class QueryResult {
		public QueryResult() { }
		
		public QueryResult(int Id, string Path, long Duration, double Similarity) {
			this.Id = Id;
			this.Path = Path;
			this.Duration = Duration;
			this.Similarity = Similarity;
		}
		
		public int Id { get; set; }
		public string Path { get; set; }
		public long Duration { get; set; }
		public double Similarity { get; set; }
	}
}