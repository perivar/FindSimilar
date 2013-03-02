/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
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

using System.IO;
using System.Globalization;
using System.Linq;

using System.Data;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using System.Text.RegularExpressions;

using Comirva.Audio.Feature;

using NDtw;

using CommonUtils;

namespace Mirage
{
	public class Mir
	{
		static string _version = "1.0.0";
		
		#region Similarity Search
		public static void FindSimilar(int[] seedTrackIds, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence) {
			
			var similarTracks = SimilarTracks(seedTrackIds, seedTrackIds, db, analysisMethod, numToTake, percentage, distanceType);
			foreach (var entry in similarTracks)
			{
				Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
			}
		}
		
		public static void FindSimilar(string path, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence) {
			
			var similarTracks = SimilarTracks(path, db, analysisMethod, numToTake, percentage, distanceType);
			foreach (var entry in similarTracks)
			{
				Console.WriteLine("{0}, {1}", entry.Key, entry.Value);
			}
		}
		
		public static Dictionary<KeyValuePair<int, string>, double> SimilarTracks(string searchForPath, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence)
		{
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
			IDataReader r = db.GetTracks(null, 0, percentage);
			
			// store results in a dictionary
			var NameDictionary = new Dictionary<KeyValuePair<int, string>, double>();
			
			int[] mapping = new int[100];
			int read = 1;
			double dcur;
			
			DbgTimer t = new DbgTimer();
			t.Start();
			
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
			
			Console.Out.WriteLine(String.Format("Found Similar to ({0}) in {1} ms", seedAudioFeature.Name, t.Stop()));
			return sortedDict;
		}

		public static Dictionary<KeyValuePair<int, string>, double> SimilarTracks(int[] id, int[] exclude, Db db, Analyzer.AnalysisMethod analysisMethod, int numToTake=25, double percentage=0.2, AudioFeature.DistanceType distanceType = AudioFeature.DistanceType.KullbackLeiblerDivergence)
		{
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
			
			DbgTimer t = new DbgTimer();
			t.Start();
			
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
						NameDictionary.Add(new KeyValuePair<int,string>(mapping[i], String.Format("{0} ({1} ms)", audioFeatures[i].Name, audioFeatures[i].Duration)), d/count);
					}
				}
			}
			
			// sort by non unique values
			var sortedDict = (from entry in NameDictionary orderby entry.Value ascending select entry)
				.Take(numToTake)
				.ToDictionary(pair => pair.Key, pair => pair.Value);
			
			Console.Out.WriteLine(String.Format("Found Similar to ({0}) in {1} ms", String.Join(",", seedAudioFeatures.Select(p=>p.Name)), t.Stop()));
			return sortedDict;
		}
		#endregion
		
		#region Compare Methods
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
		
		public static void Compare(int trackId1, int trackId2, Db db, Analyzer.AnalysisMethod analysisMethod) {
			
			AudioFeature m1 = db.GetTrack(trackId1, analysisMethod);
			AudioFeature m2 = db.GetTrack(trackId2, analysisMethod);
			
			System.Console.Out.WriteLine("Similarity between m1 and m2 is: "
			                             + m1.GetDistance(m2));
		}
		#endregion
		
		public static void ScanDirectory(string path, Db db, Analyzer.AnalysisMethod analysisMethod) {
			
			FileInfo failedFilesLog = new FileInfo("failed_files_log.txt");
			failedFilesLog.Delete();
			
			// scan directory for audio files
			try
			{
				string[] extensions = { "*.mp3", "*.wma", "*.mp4", "*.wav", "*.ogg" };
				var files = IOUtils.GetFiles(path, extensions, SearchOption.AllDirectories);
				
				int fileCounter = 0;
				foreach (var f in files)
				{
					FileInfo fileInfo = new FileInfo(f);
					AudioFeature feature = null;
					switch (analysisMethod) {
						case Analyzer.AnalysisMethod.MandelEllis:
							feature = Analyzer.AnalyzeMandelEllis(fileInfo);
							break;
						case Analyzer.AnalysisMethod.SCMS:
							feature = Analyzer.AnalyzeScms(fileInfo);
							break;
					}
					
					if (feature != null) {
						db.AddTrack(fileCounter, feature);
						fileCounter++;
						Console.Out.WriteLine("[{1}/{2}] Succesfully added fingerprint to database {0} ({3} ms)!", fileInfo.Name, fileCounter, files.Count(), feature.Duration);
					} else {
						Console.Out.WriteLine("Failed! Could not generate audio fingerprint for {0}!", fileInfo.Name);
						IOUtils.LogMessageToFile(failedFilesLog, fileInfo.FullName);
					}
				}
				Console.WriteLine("Added {0} out of a total {1} files found.", fileCounter, files.Count());
			}
			catch (UnauthorizedAccessException UAEx)
			{
				Console.WriteLine(UAEx.Message);
			}
			catch (PathTooLongException PathEx)
			{
				Console.WriteLine(PathEx.Message);
			}
		}

		#region Testing
		private static Aquila.Extractor ReadIntoExtractor(string filename)
		{
			Aquila.WaveFile wav = new Aquila.WaveFile(20, 0.66);
			wav.Load(filename);
			Aquila.Extractor extractor = new Aquila.MfccExtractor(20, 10);
			//Aquila.Extractor extractor = new Aquila.EnergyExtractor(20);
			//Aquila.Extractor extractor = new Aquila.PowerExtractor(20);
			Aquila.TransformOptions options = new Aquila.TransformOptions();
			// Set to 0 if no pre-emphasis should be performed
			options.PreemphasisFactor = 0; // 0.9375;
			options.WindowType = Aquila.WindowType.WIN_HANN;
			options.ZeroPaddedLength = wav.GetSamplesPerFrameZP();
			Aquila.ConsoleProcessingIndicator cpi = new Aquila.ConsoleProcessingIndicator();
			extractor.SetProcessingIndicator(cpi);
			Console.WriteLine("Extracting {0} features from file {1} ...", extractor.GetType(), filename);
			extractor.Process(wav, options);
			return extractor;
		}

		private static void TestComirvaMatrix() {

			// http://www.itl.nist.gov/div898/handbook/pmc/section5/pmc541.htm

			// octave-3.2.4.exe
			// > format short g
			// > X = [4, 2, 0.6; 4.2, 2.1, .59; 3.9, 2, .58; 4.3, 2.1, 0.6; 4.1, 2.2, 0.63]
			// > mean (X)
			// 	ans =
			// 		4.10000   2.08000   0.60000

			// 	> mean (X')
			// 	ans =
			// 		2.2      2.2967        2.16      2.3333        2.31

			// > cov (X)
			//	ans =
			// 	 	0.025     0.0075   	0.00075
			// 		0.0075    0.007    	0.00125
			// 		0.00075   0.00125  	0.00035

			// > inverse ( cov (X) )
			// 	ans =
			// 		 70.297 	-133.66     326.73
			// 	   -133.66     	 648.51   -2029.7
			// 		326.73 	   -2029.7     9405.9
			
			double[][] x = new double[][] {
				new double[] {4.00000, 2.00000, 0.60000},
				new double[] {4.20000, 2.10000, 0.59000},
				new double[] {3.90000, 2.00000, 0.58000},
				new double[] {4.30000, 2.10000, 0.60000},
				new double[] {4.10000, 2.20000, 0.63000}
			};
			Comirva.Audio.Util.Maths.Matrix X = new Comirva.Audio.Util.Maths.Matrix(5, 3);
			X.MatrixData = x;
			
			X.Print();
			X.Mean(1).Print();
			X.Transpose().Mean(1).Print(); // or X.Mean(2).Transpose().Print();
			X.Cov().Print();
			X.Cov().Inverse().Print();
			
			X.Cov(X.Mean(2)).Print();
			
			Console.In.ReadLine();
			return;
		}
		
		private static void TestMirageMatrix() {

			// http://www.itl.nist.gov/div898/handbook/pmc/section5/pmc541.htm

			// Tested in:
			// octave-3.2.4.exe or
			// octave3.6.2_gcc4.6.2
			
			// > format short g
			// > X = [4, 2, 0.6; 4.2, 2.1, .59; 3.9, 2, .58; 4.3, 2.1, 0.6; 4.1, 2.2, 0.63]
			
			// > mean (X)
			// ans =
			// 	4.1        2.08         0.6
			
			// 	> mean (X')
			// 	ans =
			// 		2.2      2.2967        2.16      2.3333        2.31
			
			// > cov (X)
			//	ans =
			// 	 	0.025     0.0075   	0.00075
			// 		0.0075    0.007    	0.00125
			// 		0.00075   0.00125  	0.00035

			// 	> cov (X')
			// 	ans =
			// 		2.92       3.098       2.846        3.18       2.966
			// 		3.098       3.287      3.0199      3.3737      3.1479
			// 		2.846      3.0199      2.7748       3.099      2.8933
			// 		3.18      3.3737       3.099      3.4633       3.229
			// 		2.966      3.1479      2.8933       3.229      3.0193
			
			// > inverse ( cov (X) )
			// 	ans =
			// 		 70.297 	-133.66     326.73
			// 	   -133.66     	 648.51   -2029.7
			// 		326.73 	   -2029.7     9405.9
			
			// > inverse (cov (X'))
			// warning: inverse: matrix singular to machine precision, rcond = 2.41562e-018
			// ans =
			//   -1.1505e+015  6.7533e+014   1.9306e+015  -4.7521e+014  -9.1573e+014
			//   -7.9177e+015 -9.2709e+015   1.0708e+016   7.809e+015   -1.1689e+015
			//    3.8489e+015  1.4136e+015  -3.5083e+015  -2.405e+015    6.7916e+014
			//    4.7087e+015  5.3658e+015  -7.4667e+015  -4.0211e+015   1.2355e+015
			//    6.6107e+014  1.9093e+015  -1.7135e+015  -1.0698e+015   1.4605e+014
			
			double[][] x = new double[][] {
				new double[] {4.00000, 2.00000, 0.60000},
				new double[] {4.20000, 2.10000, 0.59000},
				new double[] {3.90000, 2.00000, 0.58000},
				new double[] {4.30000, 2.10000, 0.60000},
				new double[] {4.10000, 2.20000, 0.63000}
			};
			Mirage.Matrix X = new Matrix(x);
			
			X.Print();
			Vector mean = X.Mean();
			mean.Print();
			Matrix cov = X.Covariance(mean);
			cov.Print();
			Matrix icov = cov.Inverse();
			icov.Print();
			
			Console.In.ReadLine();
			return;
		}
		#endregion
		
		public static void Main(string[] args) {
			
			//TestMirageMatrix();
			TestComirvaMatrix();
			return;
			
			Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.SCMS;
			//Analyzer.AnalysisMethod analysisMethod = Analyzer.AnalysisMethod.MandelEllis;
			
			/*
			MatchBox.MelFilterBank melFilterBank = new MatchBox.MelFilterBank(20, 22050/2, 40, 2048/2, 22050, true);
			melFilterBank.Print();
			Comirva.Audio.Util.Maths.Matrix melFilterBanks = melFilterBank.Matrix;
			melFilterBanks.Write(File.CreateText("melFilterBanks-new.xml"));
			melFilterBanks.DrawMatrixImage("matrix-melFilterBanks-new.png");
			 */
			
			/*
			string path1 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon\DNC_Hat.wav";
			string path2 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon\DNC_Kick.wav";
			
			int SAMPLERATE = 22050;
			//int SAMPLESPERFRAME = 44100 * 20 / 1000;
			int SAMPLEPERFRAME = 30;
			float[] audiodata = AudioFileReader.Decode(path1, SAMPLERATE, 120);
			SpeechRecognitionHMM.PreProcess pre = new SpeechRecognitionHMM.PreProcess(audiodata, SAMPLEPERFRAME, SAMPLERATE);
			float[][] framedSignal = pre.framedSignal;
			SpeechRecognitionHMM.MFCC mfcc = new SpeechRecognitionHMM.MFCC(SAMPLEPERFRAME, SAMPLERATE, 40);
			
			double[][] mfccFeature = new double[pre.noOfFrames][];
			for (int i = 0; i < pre.noOfFrames; i++) {
				// for each frame i, make mfcc from current framed signal
				mfccFeature[i] = mfcc.doMFCC(framedSignal[i]);// 2D data
				//SpeechRecognitionHMM.ArrayWriter.PrintDoubleArrayToConsole(features);
			}
			return;
			 */
			
			/*
			Compare(path1, path2, analysisMethod);
			System.Console.ReadLine();
			return;
			 */
			
			//string path1 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon\DNC_Hat.wav";
			//string path2 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon\DNC_Kick.wav";
			/*
			string path1 = @"aquila\examples\test.wav";
			string path2 = @"aquila\examples\test2.wav";
			
			Aquila.Extractor from = ReadIntoExtractor(path1);
			from.Save(new Aquila.TextFeatureWriter("from.txt"));
			Aquila.Extractor to = ReadIntoExtractor(path2);
			to.Save(new Aquila.TextFeatureWriter("to.txt"));

			Console.WriteLine("Calculating DTW distance...");
			Aquila.Dtw dtw = new Aquila.Dtw(from);
			double distance = dtw.GetDistance(to);
			Console.WriteLine("Finished, distance = {0}", distance);
			
			System.Console.ReadLine();
			return;
			
			//Imghash.Program.DCTTester();

			DbgTimer t = new DbgTimer();
			t.Start();
			
			bool UseBoundaryConstraintStart = true;
			bool UseBoundaryConstraintEnd = true;
			Dtw dtw = new Dtw(
				new[] { 4.0, 4.0, 4.5, 4.5, 5.0, 5.0, 5.0, 4.5, 4.5, 4.0, 4.0, 3.5 },
				new[] { 1.0, 1.5, 2.0, 2.5, 3.5, 4.0, 3.0, 2.5, 2.0, 2.0, 2.0, 1.5 },
				DistanceMeasure.Euclidean,
				UseBoundaryConstraintStart,
				UseBoundaryConstraintEnd,
				null,
				null,
				null);
			
			double cost = dtw.GetCost();
			Console.Out.WriteLine(String.Format("DTW: {0} in {1} ms", cost, t.Stop()));

			System.Console.ReadLine();
			return;
			 */
			
			string scanPath = "";
			string queryPath = "";
			int queryId = -1;
			int numToTake = 25;
			double percentage = 0.2;
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
					} else {
						distanceType = AudioFeature.DistanceType.Dtw_Euclidean;
					}
				}
			}
			if(CommandLine["?"] != null) {
				PrintUsage();
				return;
			}
			if(CommandLine["help"] != null) {
				PrintUsage();
				return;
			}
			
			if (queryPath == "" && queryId == -1 && scanPath == "") {
				PrintUsage();
				return;
			}
			
			Db db = new Db();

			if (scanPath != "") {
				if (IOUtils.IsDirectory(scanPath)) {
					db.RemoveTable();
					db.AddTable();
					ScanDirectory(scanPath, db, analysisMethod);
				} else {
					Console.Out.WriteLine("No directory found {0}!", scanPath);
				}
			}

			if (queryPath != "") {
				FileInfo fi = new FileInfo(queryPath);
				if (fi.Exists) {
					FindSimilar(queryPath, db, analysisMethod, numToTake, percentage, distanceType);
				} else {
					Console.Out.WriteLine("No file found {0}!", queryPath);
				}
			}
			
			if (queryId != -1) {
				FindSimilar(new int[] { queryId }, db, analysisMethod, numToTake, percentage, distanceType);
			}
			
			//string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects";
			//string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon";
			//string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\David Guetta - Who's That Chick FL Studio Remake";
			//string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\Deadmau5 - Right the second Mehran abbasi reworked";
			//ScanDirectory(path, db, analysisMethod);
			
			//TestReadWriteDB(@"C:\Users\perivar.nerseth\Music\Sleep Away.mp3", db);

			//string path1 = @"C:\Users\perivar.nerseth\Music\Sleep Away.mp3";
			//string path2 = @"C:\Users\perivar.nerseth\Music\Climb Every Mountain - Bryllup.wav";
			//string path1 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Uplifting Tutorial by Phil Doon\Uplifting Tutorial by Phil Doon.mp3";
			//string path2 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\2Pac - Changes Remake (by BacardiProductions)\Changes (Acapella).mp3";
			//string path1 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon\DNC_Hat.wav";
			//string path2 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon\DNC_Kick.wav";
			
			//AudioFeature feature = Analyzer.AnalyzeScms(@"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\SHM - Greyhound\MISC2_2.wav");
			
			//Compare(path1, path2);
			//Compare(0, 1, db);

			/*
			Scms m1 = Analyzer.Analyze(path1);
			Console.Out.WriteLine(m1);
			db.AddTrack(1000, m1, new FileInfo(path1).Name);
			Scms m2 = Analyzer.Analyze(path2);
			db.AddTrack(1001, m2, new FileInfo(path2).Name);
			
			System.Console.Out.WriteLine("Similarity between m1 and m2 is: "
			                             + Scms.Distance(m1, m2, new ScmsConfiguration(Analyzer.MFCC_COEFFICIENTS)));
						
			 */
			//Compare(89, 109, db, Analyzer.AnalysisMethod.SCMS);
			
			//Scms m11 = db.GetTrack(1);
			//Console.Out.WriteLine(m11);
			
			//FindSimilar(new int[] { 97, 0, 234 }, db, analysisMethod);
			//FindSimilar(path2, db, analysisMethod);
			
			// HASH creation
			// https://github.com/viat/YapHash/blob/master/sources/YapHash/src/YapHash.cpp

			System.Console.ReadLine();
		}
		
		public static void PrintUsage() {
			Console.WriteLine("FindSimilar. Version {0}.", _version);
			Console.WriteLine("Copyright (C) 2012-2013 Per Ivar Nerseth.");
			Console.WriteLine();
			Console.WriteLine("Usage: FindSimilar.exe <Arguments>");
			Console.WriteLine();
			Console.WriteLine("Arguments:");
			Console.WriteLine("\t-scandir=<rescan directory path and create audio fingerprints>");
			Console.WriteLine("\t-match=<path to the wave file to find matches for>");
			Console.WriteLine("\t-matchid=<database id to the wave file to find matches for>");
			Console.WriteLine();
			Console.WriteLine("Optional Arguments:");
			Console.WriteLine("\t-num=<number of matches to return when querying>");
			Console.WriteLine("\t-percentage=0.x <percentage above and below duration when querying>");
			Console.WriteLine("\t-type=<distance method to use when querying. Choose between:>");
			Console.WriteLine("\t\tkl\t=Kullback Leibler Divergence/ Distance (default)");
			Console.WriteLine("\t\tdtw\t=Dynamic Time Warping - Euclidean");
			Console.WriteLine("\t\tdtwe\t=Dynamic Time Warping - Euclidean");
			Console.WriteLine("\t\tdtwe2\t=Dynamic Time Warping - Squared Euclidean");
			Console.WriteLine("\t\tdtwman\t=Dynamic Time Warping - Manhattan");
			Console.WriteLine("\t\tdtwmax\t=Dynamic Time Warping - Maximum");
			Console.WriteLine("\t-? or -help=show this usage help>");
		}
	}
}
