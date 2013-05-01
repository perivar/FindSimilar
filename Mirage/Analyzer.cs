/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 *
 * Copyright (C) 2007-2008 Dominik Schnitzer <dominik@schnitzer.at>
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
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Comirva.Audio;
using Comirva.Audio.Extraction;
using Comirva.Audio.Feature;

using System.Globalization;

using CommonUtils;

// For drawing graph
using ZedGraph;
using System.Drawing;
using System.Drawing.Imaging;

// Heavily modified by perivar@nerseth.com
namespace Mirage
{
	public class Analyzer
	{
		public const bool DEBUG_INFO_VERBOSE = false;
		public const bool DEFAULT_DEBUG_INFO = false;
		
		public enum AnalysisMethod {
			SCMS = 1,
			MandelEllis = 2
		}
		
		public const int SAMPLING_RATE = 44100; //22050;
		private const int WINDOW_SIZE = 2048; //2048 1024;
		private const int MEL_COEFFICIENTS = 40; // 36 filters (SPHINX-III uses 40)
		public const int MFCC_COEFFICIENTS = 20; //20
		public const int SECONDS_TO_ANALYZE = 60;
		
		// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
		// Matlab multiplies with 2^15 (32768)
		public const int AUDIO_MULTIPLIER = 32768;
		
		//private static MfccLessOptimized mfcc = new MfccLessOptimized(WINDOW_SIZE, SAMPLING_RATE, MEL_COEFFICIENTS, MFCC_COEFFICIENTS);
		private static MfccMirage mfccMirage = new MfccMirage(WINDOW_SIZE, SAMPLING_RATE, MEL_COEFFICIENTS, MFCC_COEFFICIENTS);

		#if DEBUG
		//private static Mfcc mfccOptimized = new Mfcc(WINDOW_SIZE, SAMPLING_RATE, MEL_COEFFICIENTS, MFCC_COEFFICIENTS);
		//private static MFCC mfccComirva = new MFCC(SAMPLING_RATE, WINDOW_SIZE, MFCC_COEFFICIENTS, true, 20.0, SAMPLING_RATE/2, MEL_COEFFICIENTS);
		#endif
		
		// Create the STFS object with 50% overlap (half of the window size);
		//private static Stft stft = new Stft(WINDOW_SIZE, WINDOW_SIZE/2, new HannWindow());
		private static StftMirage stftMirage = new StftMirage(WINDOW_SIZE, WINDOW_SIZE/2, new HannWindow());
		
		public static AudioFeature AnalyzeMandelEllis(FileInfo filePath, bool doOutputDebugInfo=DEFAULT_DEBUG_INFO)
		{
			DbgTimer t = new DbgTimer();
			t.Start ();

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return null;
			}
			
			#if DEBUG
			DrawGraph(MathUtils.FloatToDouble(audiodata), "waveform.png");
			#endif
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;
			
			// Normalize
			//MathUtils.NormalizeInPlace(audiodata);
			
			// Matlab multiplies with 2^15 (32768)
			// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			MathUtils.Multiply(ref audiodata, AUDIO_MULTIPLIER); // 65536
			
			MandelEllisExtractor extractor = new MandelEllisExtractor(SAMPLING_RATE, WINDOW_SIZE, MFCC_COEFFICIENTS, MEL_COEFFICIENTS);
			AudioFeature audioFeature = extractor.Calculate(MathUtils.FloatToDouble(audiodata));
			
			if (audioFeature != null) {
				// Store duration
				audioFeature.Duration = (long) duration;
				
				// Store file name
				audioFeature.Name = filePath.Name;
			}
			
			Dbg.WriteLine ("MandelEllisExtractor - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);

			return audioFeature;
		}
		
		public static Scms AnalyzeScms(FileInfo filePath, bool doOutputDebugInfo=DEFAULT_DEBUG_INFO)
		{
			DbgTimer t = new DbgTimer();
			t.Start ();

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return null;
			}

			// Name of file being processed
			string name = StringUtils.RemoveNonAsciiCharacters(Path.GetFileNameWithoutExtension(filePath.Name));
			
			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				WriteAscii(audiodata, name + "_audiodata.ascii.txt");
			}
			#endif
			
			if (doOutputDebugInfo) {
				DrawGraph(MathUtils.FloatToDouble(audiodata), name + "_audiodata.png");
			}
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;
			
			// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// Matlab multiplies with 2^15 (32768)
			// e.g. if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			MathUtils.Multiply(ref audiodata, AUDIO_MULTIPLIER); // 65536
			
			// zero pad if the audio file is too short to perform a mfcc
			if (audiodata.Length < WINDOW_SIZE * 8)
			{
				int lenNew = WINDOW_SIZE * 8;
				Array.Resize<float>(ref audiodata, lenNew);
			}
			
			// 2. Windowing
			// 3. FFT
			Comirva.Audio.Util.Maths.Matrix stftdata = stftMirage.Apply(audiodata);

			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				stftdata.WriteAscii(name + "_stftdata.ascii.txt");
				stftdata.DrawMatrixGraph(name + "_stftdata.png");

				// same as specgram(audio*32768, 2048, 44100, hanning(2048), 1024);
				stftdata.DrawMatrixImageLogValues(name + "_specgram.png", true);
			}
			#endif

			if (doOutputDebugInfo) {
				stftdata.DrawMatrixImageLogY(name + "_specgramlog.png", SAMPLING_RATE, 20, SAMPLING_RATE/2, 120, WINDOW_SIZE);
			}
			
			// Test inverse Stft
			//double[] audiodata3 = stftMirage.InverseStft(stftdata);
			//WriteAscii(audiodata3, name + "_audiodata3.ascii.txt");
			//DrawGraph(audiodata3, name + "_audiodata3.png");
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			
			// 5. Take Logarithm
			// 6. DCT (Discrete Cosine Transform)
			// It seems the Mirage way of applying the DCT is slightly faster than the
			// Comirva way
			Comirva.Audio.Util.Maths.Matrix mfccdata = mfccMirage.Apply(ref stftdata);
			//Comirva.Audio.Util.Maths.Matrix mfccdata = mfccMirage.ApplyComirvaWay(ref stftdata);

			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				mfccdata.WriteAscii(name + "_mfccdata.ascii.txt");
				mfccdata.DrawMatrixGraph(name + "_mfccdata.png", true);
			}
			#endif

			#if DEBUG
			//if (Analyzer.DEBUG_INFO_VERBOSE) {
			// try to do an inverse mfcc and inverse stft
			// see http://stackoverflow.com/questions/1230906/reverse-spectrogram-a-la-aphex-twin-in-matlab
			Comirva.Audio.Util.Maths.Matrix stftdata2 = mfccMirage.InverseMfcc(ref mfccdata);
			stftdata2.DrawMatrixImageLogY(name + "_specgramlog2.png", SAMPLING_RATE, 20, SAMPLING_RATE/2, 120, WINDOW_SIZE);
			double[] audiodata2 = stftMirage.InverseStft(stftdata2);
			MathUtils.Divide(ref audiodata2, AUDIO_MULTIPLIER);
			//MathUtils.Normalize(ref audiodata2);
			DrawGraph(audiodata2, name + "_audiodata2.png");
			FindSimilar.AudioProxies.BassProxy bass = FindSimilar.AudioProxies.BassProxy.Instance;
			bass.SaveFile(MathUtils.DoubleToFloat(audiodata2), name + "_2.wav", Analyzer.SAMPLING_RATE);
			//}
			#endif
			
			// Store in a Statistical Cluster Model Similarity class.
			// A Gaussian representation of a song
			Scms audioFeature = Scms.GetScms(mfccdata, name);
			
			if (audioFeature != null) {
				
				// Store image if debugging
				if (doOutputDebugInfo) {
					audioFeature.Image = mfccdata.DrawMatrixImage(name + "_mfccdataimage.png");
				}

				// Store bitstring hash as well
				string hashString = GetBitString(mfccdata);
				audioFeature.BitString = hashString;
				
				// Store duration
				audioFeature.Duration = (long) duration;
				
				// Store file name
				audioFeature.Name = filePath.FullName;
			}
			
			Dbg.WriteLine ("Mirage - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);

			return audioFeature;
		}
		
		/// <summary>
		/// Graphs an array of doubles varying between -1 and 1
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="fileName">filename to save png to</param>
		/// <param name="onlyCanvas">true if no borders should be printed</param>
		public static void DrawGraph(double[] data, string fileName, bool onlyCanvas=false)
		{
			GraphPane myPane = new GraphPane( new RectangleF( 0, 0, 1200, 600 ), "", "", "" );
			
			if (onlyCanvas) {
				myPane.Chart.Border.IsVisible = false;
				myPane.Chart.Fill.IsVisible = false;
				myPane.Fill.Color = Color.Black;
				myPane.Margin.All = 0;
				myPane.Title.IsVisible = false;
				myPane.XAxis.IsVisible = false;
				myPane.YAxis.IsVisible = false;
			}
			myPane.XAxis.Scale.Max = data.Length - 1;
			myPane.XAxis.Scale.Min = 0;
			//myPane.YAxis.Scale.Max = 1;
			//myPane.YAxis.Scale.Min = -1;
			
			// add pretty stuff
			myPane.Fill = new Fill( Color.WhiteSmoke, Color.Lavender, 0F );
			myPane.Chart.Fill = new Fill( Color.FromArgb( 255, 255, 245 ),
			                             Color.FromArgb( 255, 255, 190 ), 90F );
			
			var timeData = Enumerable.Range(0, data.Length)
				.Select(i => (double) i)
				.ToArray();
			myPane.AddCurve(null, timeData, data, Color.Blue, SymbolType.None);
			
			Bitmap bm = new Bitmap( 1, 1 );
			using ( Graphics g = Graphics.FromImage( bm ) )
				myPane.AxisChange( g );
			
			myPane.GetImage().Save(fileName, ImageFormat.Png);
		}
		
		/// <summary>Writes the float array to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');</summary>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\data.ascii"</param>
		public static void WriteAscii(float[] data, string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write(" {0}\r", data[i].ToString("#.00000000e+000", CultureInfo.InvariantCulture));
			}
			pw.Close();
		}

		/// <summary>Writes the double array to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');</summary>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\data.ascii"</param>
		public static void WriteAscii(double[] data, string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write(" {0}\r", data[i].ToString("#.00000000e+000", CultureInfo.InvariantCulture));
				//pw.Write(" {0}\r", data[i].ToString("F3").PadLeft(8) + " ");
			}
			pw.Close();
		}
		
		/// <summary>
		/// Computes the perceptual hash of an audio file using the mfcc matrix
		/// </summary>
		/// <param name="mfcc">mfcc Matrix</param>
		/// <returns>Returns a 'binary string' (aka bitstring) (like. 001010111011100010) which is easy to do a hamming distance on.</returns>
		private static string GetBitString(Comirva.Audio.Util.Maths.Matrix mfcc) {

			int rows = mfcc.Rows;
			int columns = mfcc.Columns;
			
			// 5. Compute the average value.
			// Compute the mean DCT value (using only
			// the 8x8 DCT low-frequency values and excluding the first term
			// since the DC coefficient can be significantly different from
			// the other values and will throw off the average).
			double total = 0;
			for (int x = 0; x < rows; x++) {
				for (int y = 0; y < columns; y++) {
					total += mfcc.MatrixData[x][y];
				}
			}
			total -= mfcc.MatrixData[0][0];
			
			double avg = total / (double)((rows * columns) - 1);

			// 6. Further reduce the DCT.
			// This is the magic step. Set the 64 hash bits to 0 or 1
			// depending on whether each of the 64 DCT values is above or
			// below the average value. The result doesn't tell us the
			// actual low frequencies; it just tells us the very-rough
			// relative scale of the frequencies to the mean. The result
			// will not vary as long as the overall structure of the image
			// remains the same; this can survive gamma and color histogram
			// adjustments without a problem.
			string hash = "";
			for (int x = 0; x < rows; x++) {
				for (int y = 0; y < columns; y++) {
					if (x != 0 && y != 0) {
						hash += (mfcc.MatrixData[x][y] > avg ? "1" : "0");
					}
				}
			}
			return hash;
		}
	}
}
