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

using CommonUtils;

namespace Mirage
{
	public class Mir
	{
		private static int samplingrate = 11025;
		private static int windowsize = 512;
		
		public static Mfcc mfcc = new Mfcc(windowsize, samplingrate, 36, 20);
		public static Stft stft = new Stft(windowsize, windowsize, new HannWindow());
		
		public static Scms Analyze(string file)
		{
			Timer t = new Timer();
			t.Start();
			
			float[] audiodata = AudioFileReader.Decode(file, samplingrate);
			if (audiodata == null && audiodata.Length > 0)  {
				return null;
			}
			
			Matrix stft1 = stft.Apply(audiodata);
			Matrix mfcc1 = mfcc.Apply(stft1);
			Scms scms = Scms.GetScms(mfcc1);
			
			Dbg.WriteLine("Total Execution Time: " + t.Stop() + "ms");
			
			return scms;
		}

		public static int[] SimilarTracks(int[] id, int[] exclude, Db db)
		{
			// Get Seed-Song SCMS
			Scms[] seedScms = new Scms[id.Length];
			for (int i = 0; i < seedScms.Length; i++) {
				seedScms[i] = db.GetTrack(id[i]);
			}
			
			// Get all tracks from the DB except the seedSongs
			IDataReader r = db.GetTracks(exclude);
			Hashtable ht = new Hashtable();
			Scms[] scmss = new Scms[100];
			int[] mapping = new int[100];
			int read = 1;
			float d;
			float dcur;
			float count;
			
			Timer t = new Timer();
			t.Start();
			
			while (read > 0) {
				read = db.GetNextTracks(ref r, ref scmss, ref mapping, 100);
				for (int i = 0; i < read; i++) {
					
					d = 0;
					count = 0;
					for (int j = 0; j < seedScms.Length; j++) {
						dcur = seedScms[j].Distance(scmss[i]);
						
						// FIXME: Negative numbers indicate faulty scms models..
						if (dcur > 0) {
							d += dcur;
							count++;
						} else {
							Console.WriteLine("Faulty SCMS id=" + mapping[i] + "d=" + d);
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
		
		public static bool CheckFile(string wav) {
			using (Process toraw = new Process())
			{
				//toraw.StartInfo.FileName = "./NativeLibraries\\sox\\sox.exe";
				toraw.StartInfo.FileName = @"C:\Program Files (x86)\sox-14.4.1\sox.exe";
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
		
		
		public static void Main(string[] args) {
			
			/*
			Scms m1 = Mir.Analyze(@"C:\Users\perivar.nerseth\Music\Kalimba.mp3");
			Scms m2 = Mir.Analyze(@"C:\Users\perivar.nerseth\Music\Maid with the Flaxen Hair.mp3");
			
			System.Console.Out.WriteLine("Similarity between m1 and m2 is: "
			                             + m1.Distance(m2));
			
			System.Console.ReadLine();
			return;
			 */

			// scan directory for audio files
			try
			{
				string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects";
				//string path = @"C:\Users\perivar.nerseth\SkyDrive\Audio\FL Studio Projects\David Guetta - Who's That Chick FL Studio Remake";
				string[] extensions = { "*.mp3", "*.wma", "*.mp4", "*.wav", "*.ogg" };
				var files = IOUtils.GetFiles(path, extensions, SearchOption.AllDirectories);
				Db db = new Db();

				int fileCounter = 1;
				foreach (var f in files)
				{
					FileInfo fileInfo = new FileInfo(f);
					Console.WriteLine("Processing {0}", fileInfo.Name);
					
					Scms scms = Mir.Analyze(fileInfo.FullName);
					if (scms != null) {
						db.AddTrack(fileCounter, scms, fileInfo.Name);
						fileCounter++;
						//Console.ReadKey();
					}
				}
				Console.WriteLine("{0} files found.", files.Count().ToString());
			}
			catch (UnauthorizedAccessException UAEx)
			{
				Console.WriteLine(UAEx.Message);
			}
			catch (PathTooLongException PathEx)
			{
				Console.WriteLine(PathEx.Message);
			}

			/*
			Scms scms = Mir.Analyze(@"C:\Users\perivar.nerseth\Music\Kalimba.mp3");
			Console.WriteLine(scms);
			foreach (byte b in scms.ToBytes())
			{
				Console.Write(b);
			}
			Console.ReadKey();
			
			Db db = new Db();
			db.AddTrack(1, scms);

			Scms scms2 = db.GetTrack(1);
			Console.WriteLine(scms2);
			foreach (byte b in scms2.ToBytes())
			{
				Console.Write(b);
			}
			 */
			//Console.ReadKey();
			
			// HASH creation
			// https://github.com/viat/YapHash/blob/master/sources/YapHash/src/YapHash.cpp
		}
	}
}
