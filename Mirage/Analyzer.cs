/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 *
 * Copyright (C) 2007-2008 Dominik Schnitzer <dominik@schnitzer.at>
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
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Comirva.Audio;
using Comirva.Audio.Extraction;
using Comirva.Audio.Feature;

using CommonUtils;

namespace Mirage
{
	public class Analyzer
	{
		public enum AnalysisMethod {
			SCMS = 1,
			MandelEllis = 2
		}
		
		private const int SAMPLING_RATE = 22050; //22050;
		private const int WINDOW_SIZE = 1024; //1024;
		private const int MEL_COEFFICIENTS = 36; // 36 filters (SPHINX-III uses 40)
		public const int MFCC_COEFFICIENTS = 20; //20
		private const int SECONDS_TO_ANALYZE = 120;

		private static MfccLessOptimized mfcc = new MfccLessOptimized(WINDOW_SIZE, SAMPLING_RATE, MEL_COEFFICIENTS, MFCC_COEFFICIENTS);
		//private static Mfcc mfcc = new Mfcc(WINDOW_SIZE, SAMPLING_RATE, MEL_COEFFICIENTS, MFCC_COEFFICIENTS);
		
		private static Stft stft = new Stft(WINDOW_SIZE, WINDOW_SIZE, new HannWindow());
		
		public static void Init () {}

		public static AudioFeature AnalyzeMandelEllis(FileInfo filePath)
		{
			DbgTimer t = new DbgTimer();
			t.Start ();

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return null;
			}
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;

			/*
			SoundIO.WriteWaveFile(new CommonUtils.BinaryFile("audiodata.wav", CommonUtils.BinaryFile.ByteOrder.LittleEndian, true),
			                      new float[1][] { audiodata },
			                      1,
			                      audiodata.Length,
			                      SAMPLING_RATE,
			                      32);
			 */
			
			// Normalize
			//MathUtils.NormalizeInPlace(audiodata);
			Multiply(ref audiodata, 65536);
			
			/*
			SoundIO.WriteWaveFile(new CommonUtils.BinaryFile("audiodata-normalized.wav", CommonUtils.BinaryFile.ByteOrder.LittleEndian, true),
			                      new float[1][] { audiodata },
			                      1,
			                      audiodata.Length,
			                      SAMPLING_RATE,
			                      32);
			 */
			
			MandelEllisExtractor extractor = new MandelEllisExtractor(SAMPLING_RATE, WINDOW_SIZE, MFCC_COEFFICIENTS, MEL_COEFFICIENTS);
			AudioFeature audioFeature = extractor.Calculate(MathUtils.FloatToDouble(audiodata));
			
			if (audioFeature != null) {
				// Store duration
				audioFeature.Duration = (long) duration;
				
				// Store file name
				audioFeature.Name = filePath.Name;
			}
			
			long stop = 0;
			t.Stop (ref stop);
			Dbg.WriteLine ("MandelEllisExtractor - Total Execution Time: {0}ms", stop);

			return audioFeature;
		}
		
		public static Scms AnalyzeScms(FileInfo filePath)
		{
			DbgTimer t = new DbgTimer();
			t.Start ();

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return null;
			}
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;
			
			/*
			SoundIO.WriteWaveFile(new CommonUtils.BinaryFile("audiodata.wav", CommonUtils.BinaryFile.ByteOrder.LittleEndian, true),
			                      new float[1][] { audiodata },
			                      1,
			                      audiodata.Length,
			                      SAMPLING_RATE,
			                      32);
			 */
			
			// Normalize
			//MathUtils.NormalizeInPlace(audiodata);
			Multiply(ref audiodata, 65536);
			
			/*
			SoundIO.WriteWaveFile(new CommonUtils.BinaryFile("audiodata-normalized.wav", CommonUtils.BinaryFile.ByteOrder.LittleEndian, true),
			                      new float[1][] { audiodata },
			                      1,
			                      audiodata.Length,
			                      SAMPLING_RATE,
			                      32);
			 */
			
			// 2. Windowing
			// 3. FFT
			
			// zero pad if the audio file is too short to perform a mfcc
			if (audiodata.Length < WINDOW_SIZE * 8)
			{
				int lenNew = WINDOW_SIZE * 8;
				Array.Resize<float>(ref audiodata, lenNew);
			}
			
			//check for correct array length
			/*
			if ((audiodata.Length % WINDOW_SIZE) != 0)
			{
				double l = (double) audiodata.Length / WINDOW_SIZE;
				l = MathUtils.RoundUp(l);
				int lenNew = (int) l * WINDOW_SIZE;
				Array.Resize<float>(ref audiodata, lenNew);
				//throw new Exception("Input data must be multiple of hop size (windowSize/2).");
			}
			 */
			
			Matrix stftdata = stft.Apply(audiodata);
			
			//stftdata.DrawMatrix("matrix-stftdata.png");
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			
			// 5. Take Logarithm
			// 6. DCT (Discrete cosine transform)
			Matrix mfccdata = mfcc.Apply(ref stftdata);
			
			//mfccdata.DrawMatrix("matrix-mfccdata.png");
			
			// Store in a Statistical Cluster Model Similarity class.
			// A Gaussian representation of a song
			Scms audioFeature = Scms.GetScms(mfccdata);

			if (audioFeature != null) {
				// Store duration
				audioFeature.Duration = (long) duration;
				
				// Store file name
				audioFeature.Name = filePath.Name;
			}
			
			long stop = 0;
			t.Stop (ref stop);
			Dbg.WriteLine ("Mirage - Total Execution Time: {0}ms", stop);

			return audioFeature;
		}
		
		/// <summary>
		/// Pre-Emphasis Alpha (Set to 0 if no pre-emphasis should be performed)
		/// </summary>
		private static float PREEMPHASISALPHA = (float) 0.95;
		
		/// <summary>
		/// The goal of pre-emphasis is to compensate the high-frequency part
		/// that was suppressed during the sound production mechanism of humans.
		/// Moreover, it can also amplify the importance of high-frequency formants.
		/// It's not neccesary for only music, but important for speech
		/// </summary>
		/// <param name="samples">audio data to preemphase</param>
		/// <returns>processed audio</returns>
		private static float[] preEmphase(float[] samples){
			float[] EmphasedSamples = new float[samples.Length];
			for (int i = 1; i < samples.Length; i++){
				EmphasedSamples[i] = (float) samples[i] - PREEMPHASISALPHA * samples[i - 1];
			}
			return EmphasedSamples;
		}
		
		/// <summary>
		/// Multiply signal with factor
		/// </summary>
		/// <param name="data">Signal to be processed</param>
		public static void Multiply(ref float[] data, float factor)
		{
			// multiply by factor and return
			data = data.Select(i => i * factor).ToArray();
		}
	}
}
