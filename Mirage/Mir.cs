/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
 * Changed and enhanced by Per Ivar Nerseth <perivar@nerseth.com>
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA  02110-1301, USA.
 */

using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Data;

using System.Threading;
using System.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Comirva.Audio.Feature;
using NDtw;
using CommonUtils;
using FindSimilar;

using FindSimilar.AudioProxies; // BassProxy
using CommonUtils.Audio.NAudio; // AudioUtilsNAudio

using Soundfingerprinting;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.Audio.Services;
using Soundfingerprinting.Fingerprinting;
using Soundfingerprinting.Hashing;

// Heavily modified by perivar@nerseth.com
namespace Mirage
{
	public class Mir
	{
		public static string VERSION = "1.0.22";
		public static FileInfo FAILED_FILES_LOG = new FileInfo("failed_files_log.txt");
		public static FileInfo WARNING_FILES_LOG = new FileInfo("warning_files_log.txt");
		
		// Supported audio files
		// These two string arrays needs to be in sync
		// Used Notepad+ and the following regexp to convert from Bass generated filterstring to these:
		// (\*\.[^;]+);  => "\1",
		//public static string[] extensionsWithStar = { "*.mp3", "*.wma", "*.mp4", "*.wav", "*.ogg", "*.flac" };
		public static string[] extensionsWithStar = { "*.wav", "*.ogg", "*.mp1", "*.m1a", "*.mp2", "*.m2a", "*.mpa", "*.mus", "*.mp3", "*.mpg", "*.mpeg", "*.mp3pro", "*.aif", "*.aiff", "*.bwf", "*.wma", "*.wmv", "*.aac", "*.adts", "*.mp4", "*.m4a", "*.m4b", "*.mod", "*.mdz", "*.mo3", "*.s3m", "*.s3z", "*.xm", "*.xmz", "*.it", "*.itz", "*.umx", "*.mtm", "*.flac", "*.fla", "*.oga", "*.ogg", "*.aac", "*.m4a", "*.m4b", "*.mp4", "*.mpc", "*.mp+", "*.mpp", "*.ac3", "*.wma", "*.ape", "*.mac" };

		//public static string[] extensions = { ".mp3", ".wma", ".mp4", ".wav", ".ogg", ".flac" };
		public static string[] extensions = { ".wav", ".ogg", ".mp1", ".m1a", ".mp2", ".m2a", ".mpa", ".mus", ".mp3", ".mpg", ".mpeg", ".mp3pro", ".aif", ".aiff", ".bwf", ".wma", ".wmv", ".aac", ".adts", ".mp4", ".m4a", ".m4b", ".mod", ".mdz", ".mo3", ".s3m", ".s3z", ".xm", ".xmz", ".it", ".itz", ".umx", ".mtm", ".flac", ".fla", ".oga", ".ogg", ".aac", ".m4a", ".m4b", ".mp4", ".mpc", ".mp+", ".mpp", ".ac3", ".wma", ".ape", ".mac" };
		
		#region Find Similar Methods
		private static void FindSimilar(int[] seedTrackIds, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence) {
			
			var similarTracks = SimilarTracks(seedTrackIds, seedTrackIds, db, analysisMethod, numToTake, percentage, distanceType);
			foreach (var entry in similarTracks)
			{
				Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
			}
		}
		
		private static void FindSimilar(string path, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence) {
			
			var similarTracks = SimilarTracks(path, db, analysisMethod, numToTake, percentage, distanceType);
			foreach (var entry in similarTracks)
			{
				Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
			}
		}
		
		/// <summary>
		/// Find Similar Tracks to an audio file using its file path
		/// </summary>
		/// <param name="searchForPath">audio file path</param>
		/// <param name="db">database</param>
		/// <param name="analysisMethod">analysis method (SCMS or MandelEllis)</param>
		/// <param name="numToTake">max number of entries to return</param>
		/// <param name="percentage">percentage below and above the duration in ms when querying (used if between 0.1 - 0.9)</param>
		/// <param name="distanceType">distance method to use (KullbackLeiblerDivergence is default)</param>
		/// <returns>a dictinary list of key value pairs (filepath and distance)</returns>
		public static Dictionary<KeyValuePair<int, string>, double> SimilarTracks(string searchForPath, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence)
		{
			DbgTimer t = new DbgTimer();
			t.Start();

			FileInfo fi = new FileInfo(searchForPath);
			AudioFeature seedAudioFeature = null;
			AudioFeature[] audioFeatures = null;
			switch (analysisMethod) {
				case Analyzer.AnalysisMethod.MandelEllis:
					seedAudioFeature = Analyzer.AnalyzeMandelEllis(fi);
					audioFeatures = new MandelEllis[100];
					break;
				case Analyzer.AnalysisMethod.SCMS:
					seedAudioFeature = Analyzer.AnalyzeScms(fi);
					audioFeatures = new Scms[100];
					break;
			}
			
			// Get all tracks from the DB except the seedSongs
			IDataReader r = db.GetTracks(null, seedAudioFeature.Duration, percentage);
			
			// store results in a dictionary
			var NameDictionary = new Dictionary<KeyValuePair<int, string>, double>();
			
			int[] mapping = new int[100];
			int read = 1;
			double dcur;
			
			while (read > 0) {
				read = db.GetNextTracks(ref r, ref audioFeatures, ref mapping, 100, analysisMethod);
				for (int i = 0; i < read; i++) {
					dcur = seedAudioFeature.GetDistance(audioFeatures[i], distanceType);
					
					// convert to positive values
					dcur = Math.Abs(dcur);
					
					NameDictionary.Add(new KeyValuePair<int,string>(mapping[i], audioFeatures[i].Name), dcur);
				}
			}
			
			// sort by non unique values
			var sortedDict = (from entry in NameDictionary orderby entry.Value ascending select entry)
				.Take(numToTake)
				.ToDictionary(pair => pair.Key, pair => pair.Value);
			
			Console.Out.WriteLine(String.Format("Found Similar to ({0}) in {1} ms", seedAudioFeature.Name, t.Stop().TotalMilliseconds));
			return sortedDict;
		}

		/// <summary>
		/// Find Similar Tracks to an audio file using its file path
		/// </summary>
		/// <param name="searchForPath">audio file path</param>
		/// <param name="db">database</param>
		/// <param name="analysisMethod">analysis method (SCMS or MandelEllis)</param>
		/// <param name="numToTake">max number of entries to return</param>
		/// <param name="percentage">percentage below and above the duration in ms when querying (used if between 0.1 - 0.9)</param>
		/// <param name="distanceType">distance method to use (KullbackLeiblerDivergence is default)</param>
		/// <returns>a  list of query results</returns>
		public static List<FindSimilar.QueryResult> SimilarTracksList(string searchForPath, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence) {

			FileInfo fi = new FileInfo(searchForPath);
			AudioFeature seedAudioFeature = null;
			AudioFeature[] audioFeatures = null;
			switch (analysisMethod) {
				case Analyzer.AnalysisMethod.MandelEllis:
					seedAudioFeature = Analyzer.AnalyzeMandelEllis(fi);
					audioFeatures = new MandelEllis[100];
					break;
				case Analyzer.AnalysisMethod.SCMS:
					seedAudioFeature = Analyzer.AnalyzeScms(fi);
					audioFeatures = new Scms[100];
					break;
			}
			
			// Get all tracks from the DB except the seedSongs
			IDataReader r = db.GetTracks(null, seedAudioFeature.Duration, percentage);
			
			// store results in a query results list
			List<FindSimilar.QueryResult> queryResultList = new List<FindSimilar.QueryResult>();
			
			int[] mapping = new int[100];
			int read = 1;
			double dcur;
			
			while (read > 0) {
				read = db.GetNextTracks(ref r, ref audioFeatures, ref mapping, 100, analysisMethod);
				for (int i = 0; i < read; i++) {
					dcur = seedAudioFeature.GetDistance(audioFeatures[i], distanceType);
					
					// convert to positive values
					dcur = Math.Abs(dcur);
					
					QueryResult queryResult = new QueryResult();
					queryResult.Id = mapping[i];
					queryResult.Path = audioFeatures[i].Name;
					queryResult.Duration = audioFeatures[i].Duration;
					queryResult.Similarity = dcur;
					queryResultList.Add(queryResult);
				}
			}
			
			var sortedList = (from row in queryResultList
			                  orderby row.Similarity ascending
			                  select new QueryResult {
			                  	Id = row.Id,
			                  	Path = row.Path,
			                  	Duration = row.Duration,
			                  	Similarity = row.Similarity
			                  }).Take(numToTake).ToList();
			
			return sortedList;
		}
		
		/// <summary>
		/// Find Similar Tracks to one or many audio files using their unique database id(s)
		/// </summary>
		/// <param name="id">an array of unique database ids for the audio files to search for similar matches</param>
		/// <param name="exclude">an array of unique database ids to ignore (normally the same as the id array)</param>
		/// <param name="db">database</param>
		/// <param name="analysisMethod">analysis method (SCMS or MandelEllis)</param>
		/// <param name="numToTake">max number of entries to return</param>
		/// <param name="percentage">percentage below and above the duration in ms when querying (used if between 0.1 - 0.9)</param>
		/// <param name="distanceType">distance method to use (KullbackLeiblerDivergence is default)</param>
		/// <returns>a dictinary list of key value pairs (filepath and distance)</returns>
		public static Dictionary<KeyValuePair<int, string>, double> SimilarTracks(int[] id, int[] exclude, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence)
		{
			DbgTimer t = new DbgTimer();
			t.Start();
			
			AudioFeature[] seedAudioFeatures = null;
			AudioFeature[] audioFeatures = null;
			switch (analysisMethod) {
				case Analyzer.AnalysisMethod.MandelEllis:
					seedAudioFeatures = new MandelEllis[id.Length];
					audioFeatures = new MandelEllis[100];
					break;
				case Analyzer.AnalysisMethod.SCMS:
					seedAudioFeatures = new Scms[id.Length];
					audioFeatures = new Scms[100];
					break;
			}
			
			for (int i = 0; i < seedAudioFeatures.Length; i++) {
				seedAudioFeatures[i] = db.GetTrack(id[i], analysisMethod);
			}
			
			// Get all tracks from the DB except the seedSongs
			IDataReader r = db.GetTracks(exclude, seedAudioFeatures[0].Duration, percentage);
			
			// store results in a dictionary
			var NameDictionary = new Dictionary<KeyValuePair<int, string>, double>();
			
			int[] mapping = new int[100];
			int read = 1;
			double d;
			double dcur;
			float count;
			
			while (read > 0) {
				read = db.GetNextTracks(ref r, ref audioFeatures, ref mapping, 100, analysisMethod);
				for (int i = 0; i < read; i++) {
					
					d = 0;
					count = 0;
					for (int j = 0; j < seedAudioFeatures.Length; j++) {
						dcur = seedAudioFeatures[j].GetDistance(audioFeatures[i], distanceType);
						
						// convert to positive values
						dcur = Math.Abs(dcur);

						d += dcur;
						count++;
					}
					if (d > 0) {
						NameDictionary.Add(new KeyValuePair<int,string>(mapping[i], audioFeatures[i].Name), d/count);
						//NameDictionary.Add(new KeyValuePair<int,string>(mapping[i], String.Format("{0} ({1} ms)", audioFeatures[i].Name, audioFeatures[i].Duration)), d/count);
					}
				}
			}
			
			// sort by non unique values
			var sortedDict = (from entry in NameDictionary orderby entry.Value ascending select entry)
				.Take(numToTake)
				.ToDictionary(pair => pair.Key, pair => pair.Value);

			Console.Out.WriteLine(String.Format("Found Similar to ({0}) in {1} ms", String.Join(",", seedAudioFeatures.Select(p=>p.Name)), t.Stop().TotalMilliseconds));
			return sortedDict;
		}
		
		/// <summary>
		/// Find Similar Tracks to one or many audio files using their unique database id(s)
		/// </summary>
		/// <param name="id">an array of unique database ids for the audio files to search for similar matches</param>
		/// <param name="exclude">an array of unique database ids to ignore (normally the same as the id array)</param>
		/// <param name="db">database</param>
		/// <param name="analysisMethod">analysis method (SCMS or MandelEllis)</param>
		/// <param name="numToTake">max number of entries to return</param>
		/// <param name="percentage">percentage below and above the duration in ms when querying (used if between 0.1 - 0.9)</param>
		/// <param name="distanceType">distance method to use (KullbackLeiblerDivergence is default)</param>
		/// <returns>a  list of query results</returns>
		public static List<FindSimilar.QueryResult> SimilarTracksList(int[] id, int[] exclude, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence) {

			AudioFeature[] seedAudioFeatures = null;
			AudioFeature[] audioFeatures = null;
			switch (analysisMethod) {
				case Analyzer.AnalysisMethod.MandelEllis:
					seedAudioFeatures = new MandelEllis[id.Length];
					audioFeatures = new MandelEllis[100];
					break;
				case Analyzer.AnalysisMethod.SCMS:
					seedAudioFeatures = new Scms[id.Length];
					audioFeatures = new Scms[100];
					break;
			}
			
			for (int i = 0; i < seedAudioFeatures.Length; i++) {
				seedAudioFeatures[i] = db.GetTrack(id[i], analysisMethod);
			}
			
			// Get all tracks from the DB except the seedSongs
			IDataReader r = db.GetTracks(exclude, seedAudioFeatures[0].Duration, percentage);
			
			// store results in a query results list
			List<FindSimilar.QueryResult> queryResultList = new List<FindSimilar.QueryResult>();
			
			int[] mapping = new int[100];
			int read = 1;
			double d;
			double dcur;
			float count;
			
			while (read > 0) {
				read = db.GetNextTracks(ref r, ref audioFeatures, ref mapping, 100, analysisMethod);
				for (int i = 0; i < read; i++) {
					
					d = 0;
					count = 0;
					for (int j = 0; j < seedAudioFeatures.Length; j++) {
						dcur = seedAudioFeatures[j].GetDistance(audioFeatures[i], distanceType);
						
						// convert to positive values
						dcur = Math.Abs(dcur);

						d += dcur;
						count++;
					}
					if (d > 0) {
						QueryResult queryResult = new QueryResult();
						queryResult.Id = mapping[i];
						queryResult.Path = audioFeatures[i].Name;
						queryResult.Duration = audioFeatures[i].Duration;
						queryResult.Similarity = d/count;
						queryResultList.Add(queryResult);
					}
				}
			}
			
			var sortedList = (from row in queryResultList
			                  orderby row.Similarity ascending
			                  select new QueryResult {
			                  	Id = row.Id,
			                  	Path = row.Path,
			                  	Duration = row.Duration,
			                  	Similarity = row.Similarity
			                  }).Take(numToTake).ToList();
			
			return sortedList;
		}
		#endregion
		
		#region Compare Methods
		/// <summary>
		/// Compare to audio files and print the distance between them
		/// </summary>
		/// <param name="path1">audio 1 file path</param>
		/// <param name="path2">audio 2 file path</param>
		/// <param name="analysisMethod">analysis method (SCMS or MandelEllis)</param>
		public static void Compare(string path1, string path2, Analyzer.AnalysisMethod analysisMethod) {
			
			AudioFeature m1 = null;
			AudioFeature m2 = null;
			
			FileInfo filePath1 = new FileInfo(path1);
			FileInfo filePath2 = new FileInfo(path2);
			
			switch (analysisMethod) {
				case Analyzer.AnalysisMethod.MandelEllis:
					m1 = Analyzer.AnalyzeMandelEllis(filePath1);
					m2 = Analyzer.AnalyzeMandelEllis(filePath2);
					break;
				case Analyzer.AnalysisMethod.SCMS:
					m1 = Analyzer.AnalyzeScms(filePath1);
					m2 = Analyzer.AnalyzeScms(filePath2);
					break;
			}
			
			System.Console.Out.WriteLine("Similarity between m1 and m2 is: "
			                             + m1.GetDistance(m2));
		}
		
		/// <summary>
		/// Compare to audio files using their audio ids and print the distance between them
		/// </summary>
		/// <param name="trackId1">audio 1 id</param>
		/// <param name="trackId2">audio 2 id</param>
		/// <param name="db">database</param>
		/// <param name="analysisMethod">analysis method (SCMS or MandelEllis)</param>
		public static void Compare(int trackId1, int trackId2, Db db, Analyzer.AnalysisMethod analysisMethod) {
			
			AudioFeature m1 = db.GetTrack(trackId1, analysisMethod);
			AudioFeature m2 = db.GetTrack(trackId2, analysisMethod);
			
			System.Console.Out.WriteLine("Similarity between m1 and m2 is: "
			                             + m1.GetDistance(m2));
		}
		#endregion
		
		/// <summary>
		/// Scan a directory recursively and add all the audio files found to the database
		/// </summary>
		/// <param name="path">Path to directory</param>
		/// <param name="db">MandelEllis or Scms Database Instance</param>
		/// <param name="repository">Soundfingerprinting Repository</param>
		/// <param name="skipDurationAboveSeconds">Skip files with duration longer than this number of seconds (0 or less disables this)</param>
		/// <param name="silent">true if silent mode (reduced console output)</param>
		public static void ScanDirectory(string path, Db db, Repository repository, double skipDurationAboveSeconds, bool silent=false) {
			
			Stopwatch stopWatch = Stopwatch.StartNew();
			
			FAILED_FILES_LOG.Delete();
			WARNING_FILES_LOG.Delete();
			
			// scan directory for audio files
			try
			{
				// By some reason the IOUtils.GetFiles returns a higher count than what seams correct?!
				IEnumerable<string> filesAll =
					Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
					.Where(f => extensions.Contains(Path.GetExtension(f).ToLower()));
				Console.Out.WriteLine("Found {0} files in scan directory.", filesAll.Count());
				
				// Get all already processed files stored in the database and store in memory
				// It seems to work well with huge volumes of files (200k)
				IList<string> filesAlreadyProcessed = repository.DatabaseService.ReadTrackFilenames();
				Console.Out.WriteLine("Database contains {0} already processed files.", filesAlreadyProcessed.Count);

				// find the files that has not already been added to the database
				List<string> filesRemaining = filesAll.Except(filesAlreadyProcessed).ToList();
				Console.Out.WriteLine("Found {0} files remaining in scan directory to be processed.", filesRemaining.Count);

				int filesCounter = 1;
				int filesAllCounter = filesAlreadyProcessed.Count + 1;
				
				#if !DEBUG
				Console.Out.WriteLine("Running in multi-threaded mode!");
				Parallel.ForEach(filesRemaining, file =>
				                 {
				                 	#else
				                 	Console.Out.WriteLine("Running in single-threaded mode!");
				                 	foreach (string file in filesRemaining)
				                 	{
				                 		#endif
				                 		
				                 		FileInfo fileInfo = new FileInfo(file);

				                 		// Try to use Un4Seen Bass to check duration
				                 		BassProxy bass = BassProxy.Instance;
				                 		double duration = bass.GetDurationInSeconds(fileInfo.FullName);

				                 		// check if we should skip files longer than x seconds
				                 		if ( (skipDurationAboveSeconds > 0 && duration > 0 && duration < skipDurationAboveSeconds)
				                 		    || skipDurationAboveSeconds <= 0
				                 		    || duration < 0) {

				                 			if(!Analyzer.AnalyzeAndAddComplete(fileInfo, db, repository)) {
				                 				//if(!Analyzer.AnalyzeAndAddSoundfingerprinting(fileInfo, repository)) {
				                 				//if(!Analyzer.AnalyzeAndAddScms(fileInfo, db)) {
				                 				Console.Out.WriteLine("Failed! Could not generate audio fingerprint for {0}!", fileInfo.Name);
				                 				IOUtils.LogMessageToFile(FAILED_FILES_LOG, fileInfo.FullName);
				                 			} else {
				                 				Console.Out.WriteLine("[{1}/{2} - {3}/{4}] Succesfully added {0} to database. (Thread: {5})", fileInfo.Name, filesCounter, filesRemaining.Count, filesAllCounter, filesAll.Count(), Thread.CurrentThread.ManagedThreadId);
				                 				
				                 				//filesCounter++;
				                 				//filesAllCounter++;
				                 				
				                 				// Threadsafe increment
				                 				Interlocked.Increment(ref filesCounter);
				                 				Interlocked.Increment(ref filesAllCounter);
				                 			}
				                 		} else {
				                 			if (!silent) Console.Out.WriteLine("Skipping {0} since duration exceeds limit ({1:0.00} > {2:0.00} sec.)", fileInfo.Name, duration, skipDurationAboveSeconds);
				                 		}
				                 		
				                 		fileInfo = null;
				                 	}
				                 	
				                 	#if !DEBUG
				                 	);
				                 	#endif
				                 	int filesActuallyProcessed = filesCounter -1;
				                 	Console.WriteLine("Added {0} out of a total remaining set of {1} files. (Of {2} files found).", filesActuallyProcessed, filesRemaining.Count(), filesAll.Count());
				                 }
				                 catch (UnauthorizedAccessException UAEx)
				                 {
				                 	Console.WriteLine(UAEx.Message);
				                 }
				                 catch (PathTooLongException PathEx)
				                 {
				                 	Console.WriteLine(PathEx.Message);
				                 }
				                 catch (System.NullReferenceException NullEx) {
				                 	Console.WriteLine(NullEx.Message);
				                 }

				                 Console.WriteLine("Time used: {0}", stopWatch.Elapsed);
				}
			
			private static void StartGUI() {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FindSimilarClientForm());
			}
			
			[STAThread]
			public static void Main(string[] args) {

				Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.SCMS;
				//Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.MandelEllis;
				
				string scanPath = "";
				double skipDurationAboveSeconds = -1; // less than zero disables this
				string queryPath = "";
				int queryId = -1;
				int numToTake = 20;
				double percentage = 0.4; // percentage below and above when querying
				bool resetdb = false;
				bool silent = false;
				AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence;
				
				// Command line parsing
				Arguments CommandLine = new Arguments(args);
				if(CommandLine["match"] != null) {
					queryPath = CommandLine["match"];
				}
				if(CommandLine["matchid"] != null) {
					string matchId = CommandLine["matchid"];
					queryId = int.Parse(matchId);
				}
				if(CommandLine["scandir"] != null) {
					scanPath = CommandLine["scandir"];
				}
				if(CommandLine["skipduration"] != null) {
					double.TryParse(CommandLine["skipduration"], NumberStyles.Number,CultureInfo.InvariantCulture, out skipDurationAboveSeconds);
				}
				if(CommandLine["num"] != null) {
					string num = CommandLine["num"];
					numToTake = int.Parse(num);
				}
				if(CommandLine["percentage"] != null) {
					double.TryParse(CommandLine["percentage"], NumberStyles.Number,CultureInfo.InvariantCulture, out percentage);
				}
				if(CommandLine["type"] != null) {
					string type = CommandLine["type"];
					if (type.Equals("kl", StringComparison.InvariantCultureIgnoreCase)) {
						distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence;
					} else if (type.StartsWith("dtw", StringComparison.InvariantCultureIgnoreCase)) {
						if (type.Equals("dtwe", StringComparison.InvariantCultureIgnoreCase)) {
							distanceType = AudioFeature.DistanceType.Dtw_Euclidean;
						} else if (type.Equals("dtwe2", StringComparison.InvariantCultureIgnoreCase)) {
							distanceType = AudioFeature.DistanceType.Dtw_SquaredEuclidean;
						} else if (type.Equals("dtwman", StringComparison.InvariantCultureIgnoreCase)) {
							distanceType = AudioFeature.DistanceType.Dtw_Manhattan;
						} else if (type.Equals("dtwmax", StringComparison.InvariantCultureIgnoreCase)) {
							distanceType = AudioFeature.DistanceType.Dtw_Maximum;
						} else if (type.Equals("ucrdtw", StringComparison.InvariantCultureIgnoreCase)) {
							distanceType = AudioFeature.DistanceType.UCR_Dtw;
						} else {
							distanceType = AudioFeature.DistanceType.Dtw_Euclidean;
						}
					}
				}
				if(CommandLine["dtw"] != null || CommandLine["dtwe"] != null) {
					distanceType = AudioFeature.DistanceType.Dtw_Euclidean;
				}
				if(CommandLine["dtwe2"] != null) {
					distanceType = AudioFeature.DistanceType.Dtw_SquaredEuclidean;
				}
				if(CommandLine["dtwman"] != null) {
					distanceType = AudioFeature.DistanceType.Dtw_Manhattan;
				}
				if(CommandLine["dtwmax"] != null) {
					distanceType = AudioFeature.DistanceType.Dtw_Maximum;
				}
				if(CommandLine["kl"] != null) {
					distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence;
				}
				if(CommandLine["ucrdtw"] != null) {
					distanceType = AudioFeature.DistanceType.UCR_Dtw;
				}
				if(CommandLine["resetdb"] != null) {
					resetdb = true;
				}
				if(CommandLine["silent"] != null) {
					silent = true;
				}
				if(CommandLine["permutations"] != null) {
					Console.WriteLine("Generating hash permutations for used by the Soundfingerprinting methods.");
					Console.WriteLine("Saving to file: {0}", "Soundfingerprinting\\perms-new.csv");
					Console.WriteLine();
					PermutationGeneratorService permutationGeneratorService = new PermutationGeneratorService();
					Analyzer.GenerateAndSavePermutations(permutationGeneratorService, "Soundfingerprinting\\perms-new.csv");
					return;
				}
				if(CommandLine["?"] != null) {
					PrintUsage();
					return;
				}
				if(CommandLine["help"] != null) {
					PrintUsage();
					return;
				}
				if(CommandLine["gui"] != null) {
					StartGUI();
					return;
				}
				if (queryPath == "" && queryId == -1 && scanPath == "") {
					PrintUsage();
					return;
				}
				
				// Get database
				Db mandelEllisScmsDatabase = new Db(); // For MandelEllis and SCMS

				// Instansiate soundfingerprinting Repository
				FingerprintService fingerprintService = Analyzer.GetSoundfingerprintingService();
				DatabaseService databaseService = DatabaseService.Instance; // For AudioFingerprinting

				//IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms.csv", ",");
				IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms-new.csv", ",");
				
				Repository repository = new Repository(permutations, databaseService, fingerprintService);
				
				if (scanPath != "") {
					if (IOUtils.IsDirectory(scanPath)) {
						if (resetdb) {
							// For MandelEllis and Scms
							mandelEllisScmsDatabase.RemoveTable();
							mandelEllisScmsDatabase.AddTable();
							
							// For AudioFingerprinting
							databaseService.RemoveFingerprintTable();
							databaseService.AddFingerprintTable();
							databaseService.RemoveHashBinTable();
							databaseService.AddHashBinTable();
							databaseService.RemoveTrackTable();
							databaseService.AddTrackTable();
						}
						Console.WriteLine("FindSimilar. Version {0}.", VERSION);
						ScanDirectory(scanPath, mandelEllisScmsDatabase, repository, skipDurationAboveSeconds, silent);
					} else {
						Console.Out.WriteLine("No directory found {0}!", scanPath);
					}
				}

				if (queryPath != "") {
					FileInfo fi = new FileInfo(queryPath);
					if (fi.Exists) {
						FindSimilar(queryPath, mandelEllisScmsDatabase, analysisMethod, numToTake, percentage, distanceType);
					} else {
						Console.Out.WriteLine("No file found {0}!", queryPath);
					}
				}
				
				if (queryId != -1) {
					FindSimilar(new int[] { queryId }, mandelEllisScmsDatabase, analysisMethod, numToTake, percentage, distanceType);
				}
				
				System.Console.ReadLine();
			}
			
			private static void PrintUsage() {
				Console.WriteLine("FindSimilar. Version {0}.", VERSION);
				Console.WriteLine("Copyright (C) 2012-2014 Per Ivar Nerseth.");
				Console.WriteLine();
				Console.WriteLine("Usage: FindSimilar.exe <Arguments>");
				Console.WriteLine();
				Console.WriteLine("Arguments:");
				Console.WriteLine("\t-scandir=<scan directory path and create audio fingerprints - ignore existing files>");
				Console.WriteLine("\t-match=<path to the wave file to find matches for>");
				Console.WriteLine("\t-matchid=<database id to the wave file to find matches for>");
				Console.WriteLine("\t-permutations\tGenerate Permutation file used by Soundfingerprinting methods");
				Console.WriteLine();
				Console.WriteLine("Optional Arguments:");
				Console.WriteLine("\t-gui\t<open up the Find Similar Client GUI>");
				Console.WriteLine("\t-resetdb\t<clean database, used together with scandir>");
				Console.WriteLine("\t-skipduration=x.x <skip files longer than x seconds, used together with scandir>");
				Console.WriteLine("\t-silent\t<do not output so much info, used together with scandir>");
				Console.WriteLine("\t-num=<number of matches to return when querying>");
				Console.WriteLine("\t-percentage=0.x <percentage above and below duration when querying>");
				Console.WriteLine("\t-type=<distance method to use when querying. Choose between:>");
				Console.WriteLine("\t\tkl\t=Kullback Leibler Divergence/ Distance (default)");
				Console.WriteLine("\t\tdtw\t=Dynamic Time Warping - Euclidean");
				Console.WriteLine("\t\tdtwe\t=Dynamic Time Warping - Euclidean");
				Console.WriteLine("\t\tdtwe2\t=Dynamic Time Warping - Squared Euclidean");
				Console.WriteLine("\t\tdtwman\t=Dynamic Time Warping - Manhattan");
				Console.WriteLine("\t\tdtwmax\t=Dynamic Time Warping - Maximum");
				Console.WriteLine("\t\tucrdtw\t=Dynamic Time Warping - UCR Suite (fast)");
				Console.WriteLine("\t\tOr use the distance method directly:");
				Console.WriteLine("\t-kl\t<Use Kullback Leibler Divergence/ Distance (default)>");
				Console.WriteLine("\t-dtw\t<Use Dynamic Time Warping - Euclidean>");
				Console.WriteLine("\t-dtwe\t<Use Dynamic Time Warping - Euclidean>");
				Console.WriteLine("\t-dtwe2\t<Use Dynamic Time Warping - Squared Euclidean>");
				Console.WriteLine("\t-dtwman\t<Use Dynamic Time Warping - Manhattan>");
				Console.WriteLine("\t-dtwmax\t<Use Dynamic Time Warping - Maximum>");
				Console.WriteLine("\t-ucrdtw\t<Use Dynamic Time Warping - UCR Suite (fast)>");
				Console.WriteLine();
				Console.WriteLine("\t-? or -help=show this usage help>");
			}
		}
	}
