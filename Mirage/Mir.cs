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
using System.Linq;

using System.Data;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using System.Text.RegularExpressions;

using Comirva.Audio.Feature;

using CommonUtils;

namespace Mirage
{
	public class Mir
	{
		public static int[] SimilarTracks(int[] id, int[] exclude, Db db)
		{
			// Get Seed-Song AudioFeature models
			AudioFeature[] seedAudioFeatures = new MandelEllis[id.Length];
			for (int i = 0; i < seedAudioFeatures.Length; i++) {
				seedAudioFeatures[i] = db.GetTrack(id[i]);
			}
			
			// Get all tracks from the DB except the seedSongs
			IDataReader r = db.GetTracks(exclude);
			Hashtable ht = new Hashtable();
			AudioFeature[] audioFeatures = new MandelEllis[100];
			int[] mapping = new int[100];
			int read = 1;
			double d;
			double dcur;
			float count;
			
			Timer t = new Timer();
			t.Start();
			
			while (read > 0) {
				read = db.GetNextTracks(ref r, ref audioFeatures, ref mapping, 100);
				for (int i = 0; i < read; i++) {
					
					d = 0;
					count = 0;
					for (int j = 0; j < seedAudioFeatures.Length; j++) {
						dcur = seedAudioFeatures[j].GetDistance(audioFeatures[i]);
						
						// FIXME: Negative numbers indicate faulty scms models..
						if (dcur > 0) {
							d += dcur;
							count++;
						} else {
							Console.WriteLine("Faulty SCMS id={0}, dcur={1}, d={2}", mapping[i], dcur, d);
							d = 0;
							break;
						}
					}
					
					if (d > 0) {
						ht.Add(mapping[i], d/count);
					}
				}
			}
			
			float[] items = new float[ht.Count];
			int[] keys = new int[ht.Keys.Count];
			
			ht.Keys.CopyTo(keys, 0);
			ht.Values.CopyTo(items, 0);
			
			Array.Sort(items, keys);
			
			Dbg.WriteLine("playlist in: " + t.Stop() + "ms");
			
			return keys;
		}
		
		#region hide
		public static bool CheckFile(string wav) {
			using (Process toraw = new Process())
			{
				toraw.StartInfo.FileName = "./NativeLibraries\\sox\\sox.exe";
				//toraw.StartInfo.FileName = @"C:\Program Files (x86)\sox-14.4.1\sox.exe";
				toraw.StartInfo.Arguments = " --i \"" + wav + "\"";
				toraw.StartInfo.UseShellExecute = false;
				toraw.StartInfo.RedirectStandardOutput = true;
				toraw.StartInfo.RedirectStandardError = true;
				toraw.Start();
				toraw.WaitForExit();
				
				// Read in all the text from the process with the StreamReader.
				using (StreamReader reader = toraw.StandardError)
				{
					string result = reader.ReadToEnd();
					if (result != null && !result.Equals("")) {
						// 0x674f	= Ogg Vorbis (mode 1)
						// 0x6750	= Ogg Vorbis (mode 2)
						// 0x6751	= Ogg Vorbis (mode 3)
						// 0x676f	= Ogg Vorbis (mode 1+)
						// 0x6770	= Ogg Vorbis (mode 2+)
						// 0x6771	= Ogg Vorbis (mode 3+)
						Match match = Regex.Match(result, @"Unknown WAV file encoding \(type 67.*?\)",
						                          RegexOptions.IgnoreCase);
						if (match.Success) {
							// this is a ogg wrapped in WAV
							// http://music.columbia.edu/pipermail/linux-audio-user/2003-October/007279.html
							// Strip off the first 68 bytes. You will then have a standard Ogg Vorbis file.
							
							Console.WriteLine(match.Groups[0]);
							//Console.ReadKey();
						} else {
							Console.WriteLine(result);
						}
					}
				}

				// Read in all the text from the process with the StreamReader.
				using (StreamReader reader = toraw.StandardOutput)
				{
					string result = reader.ReadToEnd();
					if (result != null && !result.Equals("")) {
						// Channels       : 2
						// Sample Rate    : 44100
						// Precision      : 24-bit
						// Duration       : 00:00:02.78 = 122760 samples = 208.776 CDDA sectors
						// File Size      : 737k
						// Bit Rate       : 2.12M
						// Sample Encoding: 24-bit Signed Integer PCM
						
						Match match = Regex.Match(result, @"Sample Encoding: ([A-Za-z0-9\s\-]+)\n",
						                          RegexOptions.IgnoreCase);
						if (match.Success) {
							Console.WriteLine(match.Groups[1]);
							Console.ReadKey();
						} else {
							Console.WriteLine(result);
						}
					}
				}
				
				int exitCode = toraw.ExitCode;
				// 0 = succesfull
				// 1 = partially succesful
				if (exitCode == 0 || exitCode == 1) {
					return true;
				} else {
					return false;
				}
			}
		}
		
		public static void TestReadWriteDB(string wav, Db db) {
			
			Scms scms = Analyzer.Analyze(wav);
			Console.WriteLine(scms);
			foreach (byte b in scms.ToBytes())
			{
				Console.Write(b);
			}
			Console.ReadKey();
			
			db.AddTrack(1, scms, new FileInfo(wav).Name);

			AudioFeature scms2 = db.GetTrack(1);
			Console.WriteLine(scms2);
			foreach (byte b in scms2.ToBytes())
			{
				Console.Write(b);
			}
			Console.ReadKey();
		}
		#endregion

		public static void Compare(string path1, string path2) {
			AudioFeature m1 = Analyzer.AnalyzeAudioFeature(path1);
			AudioFeature m2 = Analyzer.AnalyzeAudioFeature(path2);
			
			System.Console.Out.WriteLine("Similarity between m1 and m2 is: "
			                             + m1.GetDistance(m2));
			
			System.Console.ReadLine();
		}
		
		public static void Compare(int trackId1, int trackId2, Db db) {
			
			AudioFeature m1 = db.GetTrack(trackId1);
			AudioFeature m2 = db.GetTrack(trackId2);
			
			System.Console.Out.WriteLine("Similarity between m1 and m2 is: "
			                             + m1.GetDistance(m2));
			
			System.Console.ReadLine();
		}
		
		public static void ScanDirectory(string path, Db db) {
			// scan directory for audio files
			try
			{
				string[] extensions = { "*.mp3", "*.wma", "*.mp4", "*.wav", "*.ogg" };
				var files = IOUtils.GetFiles(path, extensions, SearchOption.AllDirectories);

				int fileCounter = 0;
				foreach (var f in files)
				{
					FileInfo fileInfo = new FileInfo(f);
					Console.WriteLine("Processing {0} ...", fileInfo.Name);
					
					AudioFeature feature = Analyzer.AnalyzeAudioFeature(fileInfo.FullName);
					if (feature != null) {
						db.AddTrack(fileCounter, feature, fileInfo.Name);
						fileCounter++;
						Console.Out.WriteLine("Succesfully analyzed audio and added to database {0}! [{1}/{2}].", fileInfo.Name, fileCounter, files.Count());
					} else {
						Console.Out.WriteLine("Failed! Could not generate audio fingerprint for {0}!", fileInfo.Name);
					}
					
					/*
					Scms scms = Analyzer.Analyze(fileInfo.FullName);
					if (scms != null) {
						db.AddTrack(fileCounter, scms, fileInfo.Name);
						fileCounter++;
					} else {
						Console.Out.WriteLine("Error! Could not generate audio fingerprint!");
					}
					 */
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
		
		public static void FindSimilar(int seedTrackId, Db db) {
			
			int[] i = SimilarTracks(new int[] { seedTrackId }, new int[] { seedTrackId }, db);
			
			foreach (int d in i) {
				Console.Out.WriteLine(d);
			}
			
		}
		
		public static void Main(string[] args) {
			Db db = new Db();
			
			string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects";
			//string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon";
			//string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\David Guetta - Who's That Chick FL Studio Remake";
			//string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\Deadmau5 - Right the second Mehran abbasi reworked";
			ScanDirectory(path, db);
			
			//TestReadWriteDB(@"C:\Users\perivar.nerseth\Music\Sleep Away.mp3", db);

			//string path1 = @"C:\Users\perivar.nerseth\Music\Sleep Away.mp3";
			//string path2 = @"C:\Users\perivar.nerseth\Music\Climb Every Mountain - Bryllup.wav";
			//string path1 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Uplifting Tutorial by Phil Doon\Uplifting Tutorial by Phil Doon.mp3";
			//string path2 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\2Pac - Changes Remake (by BacardiProductions)\Changes (Acapella).mp3";
			//string path1 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon\DNC_Hat.wav";
			//string path2 = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\!Tutorials\Electro Dance tutorial by Phil Doon\DNC_Kick.wav";
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
			
			System.Console.ReadLine();
			
			Compare(1000, 1001, db);
			
			 */
			//Scms m11 = db.GetTrack(1);
			//Console.Out.WriteLine(m11);
			
			//FindSimilar(127, db);
			
			System.Console.ReadLine();
			return;

			
			// HASH creation
			// https://github.com/viat/YapHash/blob/master/sources/YapHash/src/YapHash.cpp
		}
	}
}
