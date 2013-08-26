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

using Wavelets;
using math.transform.jwave;
using math.transform.jwave.handlers;
using math.transform.jwave.handlers.wavelets;

// Heavily modified by perivar@nerseth.com
namespace Mirage
{
	public class Analyzer
	{
		public const bool DEBUG_INFO_VERBOSE = true;
		public const bool DEBUG_OUTPUT_TEXT = true;
		public const bool DEFAULT_DEBUG_INFO = true;
		
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
		
		// http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting
		// The parameters used in the Duplicates-detector-via-audio-fingerprinting transformation steps
		// is equal to those that have been found work well in other audio fingerprinting studies
		// (specifically in A Highly Robust Audio Fingerprinting System):
		// audio frames that are 371 ms long (2048 samples), taken every 11.6 ms (64 samples),
		// thus having an overlap of 31/32
		//
		// parameters: samplerate: 5512 hz, overlap: 31/32, window length: 2048
		// slice (window) size: 2048 / 5512 * 1000 =  371 ms
		// distance between slices: 64 / 5512 * 1000 =  11.6 ms

		// parameters: samplerate: 44100 hz, overlap: 1024 samples, window length: 2048
		// slice (window) size: 2048 / 44100 * 1000 =  46.44 ms
		// distance between slices: 1024 / 44100 * 1000 =  23.22 ms
		
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
			FindSimilar.AudioProxies.BassProxy bass = FindSimilar.AudioProxies.BassProxy.Instance;

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return null;
			}

			// Name of file being processed
			string name = StringUtils.RemoveNonAsciiCharacters(Path.GetFileNameWithoutExtension(filePath.Name));
			
			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				if (DEBUG_OUTPUT_TEXT) WriteAscii(audiodata, name + "_audiodata.ascii");
				if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata, name + "_audiodata.txt");
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
				if (DEBUG_OUTPUT_TEXT) stftdata.WriteAscii(name + "_stftdata.ascii");
				//stftdata.DrawMatrixGraph(name + "_stftdata.png");

				//stftdata.WriteCSV(name + "_stftdata.csv", ";");
				
				// same as specgram(audio*32768, 2048, 44100, hanning(2048), 1024);
				stftdata.DrawMatrixImageLogValues(name + "_specgram.png", true);
			}
			#endif

			if (doOutputDebugInfo) {
				stftdata.DrawMatrixImageLogY(name + "_specgramlog.png", SAMPLING_RATE, 20, SAMPLING_RATE/2, 120, WINDOW_SIZE);
			}
			
			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				// Test inverse stft
				double[] audiodata_inverse_stft = stftMirage.InverseStft(stftdata);
				MathUtils.Divide(ref audiodata_inverse_stft, AUDIO_MULTIPLIER);

				if (DEBUG_OUTPUT_TEXT) WriteAscii(audiodata_inverse_stft, name + "_audiodata_inverse_stft.ascii");
				if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata_inverse_stft, name + "_audiodata_inverse_stft.txt");
				
				DrawGraph(audiodata_inverse_stft, name + "_audiodata_inverse_stft.png");
				
				float[] audiodata_inverse_float = MathUtils.DoubleToFloat(audiodata_inverse_stft);
				bass.SaveFile(audiodata_inverse_float, name + "_inverse_stft.wav", Analyzer.SAMPLING_RATE);

				/*
				MathUtils.Multiply(ref audiodata_inverse_float, AUDIO_MULTIPLIER); // 65536
				Comirva.Audio.Util.Maths.Matrix stftdataInverse = stftMirage.Apply(audiodata_inverse_float);

				// same as specgram(audio*32768, 2048, 44100, hanning(2048), 1024);
				stftdataInverse.DrawMatrixImageLogValues(name + "_inverse_specgram.png", true);
				stftdataInverse.DrawMatrixImageLogY(name + "_inverse_specgramlog.png", SAMPLING_RATE, 20, SAMPLING_RATE/2, 120, WINDOW_SIZE);
				 */
			}
			#endif

			Comirva.Audio.Util.Maths.Matrix mellog = mfccMirage.ApplyMelScaleAndLog(ref stftdata);
			mellog.DrawMatrixImage(name + "_mel_log.png", 600, 400, true, true);
			Comirva.Audio.Util.Maths.Matrix inverse_mellog = mfccMirage.InverseMelScaleAndLog(ref mellog);
			inverse_mellog.DrawMatrixImageLogValues(name + "_mel_log_inverse.png", true);
			
			double[] audiodata_inverse_mellog = stftMirage.InverseStft(inverse_mellog);
			//MathUtils.Divide(ref audiodata_inverse_mellog, AUDIO_MULTIPLIER);

			if (DEBUG_OUTPUT_TEXT) WriteAscii(audiodata_inverse_mellog, name + "_audiodata_inverse_mellog.ascii");
			if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata_inverse_mellog, name + "_audiodata_inverse_mellog.txt");
			
			DrawGraph(audiodata_inverse_mellog, name + "_audiodata_inverse_mellog.png");
			
			float[] audiodata_inverse_mellog_float = MathUtils.DoubleToFloat(audiodata_inverse_mellog);
			bass.SaveFile(audiodata_inverse_mellog_float, name + "_inverse_mellog.wav", Analyzer.SAMPLING_RATE);
			
			/*
			Comirva.Audio.Util.Maths.Matrix waveletdata = mfccMirage.ApplyWavelet(ref stftdata);

			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				if (DEBUG_OUTPUT_TEXT) waveletdata.WriteAscii(name + "_waveletdata.ascii");
				waveletdata.DrawMatrixImageLogValues(name + "_wavelet_specgram.png", true);
			}
			#endif
			 */
			
			// Haar Wavelet Transform
			Comirva.Audio.Util.Maths.Matrix haarMatrix = WaveletUtils.HaarWaveletTransform(stftdata.MatrixData);
			haarMatrix.DrawMatrixImageLogValues(name + "_wavelet.png", true);
			Comirva.Audio.Util.Maths.Matrix haarInverseMatrix = WaveletUtils.InverseHaarWaveletTransform(haarMatrix.MatrixData);
			haarInverseMatrix.DrawMatrixImageLogValues(name + "_inverse_wavelet.png", true);

			/*
			Wavelets.Dwt dwt = new Wavelets.Dwt(8);
			Comirva.Audio.Util.Maths.Matrix haarMatrix = dwt.Transform(stftdata);
			haarMatrix.DrawMatrixImageLogValues(name + "_wavelet.png", true);
			Comirva.Audio.Util.Maths.Matrix haarInverseMatrix = dwt.TransformBack(haarMatrix);
			haarInverseMatrix.DrawMatrixImageLogValues(name + "_inverse_wavelet.png", true);
			
			WaveletInterface wavelet = null;
			wavelet = new Haar02();
			TransformInterface bWave = null;
			bWave = new FastWaveletTransform(wavelet);
			Transform trans = new Transform(bWave); // perform all steps
			double[][] dwtArray = trans.forward(stftdata.MatrixData);
			Comirva.Audio.Util.Maths.Matrix haarMatrix = new Comirva.Audio.Util.Maths.Matrix(dwtArray);
			haarMatrix.DrawMatrixImageLogValues(name + "_wavelet.png", true);
			double[][] idwtArray = trans.reverse(haarMatrix.MatrixData);
			Comirva.Audio.Util.Maths.Matrix haarInverseMatrix = new Comirva.Audio.Util.Maths.Matrix(idwtArray);
			haarInverseMatrix.DrawMatrixImageLogValues(name + "_inverse_wavelet.png", true);
			 */
			
			// Wavelet thresholding
			double threshold = 0.15;
			double[][] yHard = Thresholding.perform_hard_thresholding(haarMatrix.MatrixData, threshold);
			double[][] ySoft = Thresholding.perform_soft_thresholding(haarMatrix.MatrixData, threshold);
			double[][] ySemisoft = Thresholding.perform_semisoft_thresholding(haarMatrix.MatrixData, threshold, threshold*2);
			double[][] ySemisoft2 = Thresholding.perform_semisoft_thresholding(haarMatrix.MatrixData, threshold, threshold*4);
			double[][] yStrict = Thresholding.perform_strict_thresholding(haarMatrix.MatrixData, 20);
			
			// Inverse 2D Haar Wavelet Transform
			Comirva.Audio.Util.Maths.Matrix zHard = WaveletUtils.InverseHaarWaveletTransform(yHard);
			Comirva.Audio.Util.Maths.Matrix zSoft = WaveletUtils.InverseHaarWaveletTransform(ySoft);
			Comirva.Audio.Util.Maths.Matrix zSemisoft = WaveletUtils.InverseHaarWaveletTransform(ySemisoft);
			Comirva.Audio.Util.Maths.Matrix zSemisoft2 = WaveletUtils.InverseHaarWaveletTransform(ySemisoft2);
			Comirva.Audio.Util.Maths.Matrix zStrict = WaveletUtils.InverseHaarWaveletTransform(yStrict);
			
			// Output the images
			zHard.DrawMatrixImageLogValues(name + "_wavelet-thresholding-hard.png", true);
			zSoft.DrawMatrixImageLogValues(name + "_wavelet-thresholding-soft.png", true);
			zSemisoft.DrawMatrixImageLogValues(name + "_wavelet-thresholding-semisoft.png", true);
			zSemisoft2.DrawMatrixImageLogValues(name + "_wavelet-thresholding-semisoft2.png", true);
			zStrict.DrawMatrixImageLogValues(name + "_wavelet-thresholding-strict.png", true);
			
			/*
			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				// try to do an inverse wavelet transform
				Comirva.Audio.Util.Maths.Matrix stftdata_inverse_wavelet = mfccMirage.InverseWavelet(ref waveletdata);
				stftdata_inverse_wavelet.DrawMatrixImageLogY(name + "_specgramlog_inverse_wavelet.png", SAMPLING_RATE, 20, SAMPLING_RATE/2, 120, WINDOW_SIZE);
				double[] audiodata_inverse_wavelet = stftMirage.InverseStft(stftdata_inverse_wavelet);

				if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata_inverse_wavelet, name + "_audiodata_inverse_wavelet.txt");
				DrawGraph(audiodata_inverse_wavelet, name + "_audiodata_inverse_wavelet.png");
				FindSimilar.AudioProxies.BassProxy bass = FindSimilar.AudioProxies.BassProxy.Instance;
				bass.SaveFile(MathUtils.DoubleToFloat(audiodata_inverse_wavelet), name + "_inverse_wavelet.wav", Analyzer.SAMPLING_RATE);
			}
			#endif
			 */
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			
			// 5. Take Logarithm
			// 6. DCT (Discrete Cosine Transform)
			// It seems the Mirage way of applying the DCT is slightly faster than the
			// Comirva way due to less loops
			Comirva.Audio.Util.Maths.Matrix mfccdata = mfccMirage.Apply(ref stftdata);
			//Comirva.Audio.Util.Maths.Matrix mfccdata = mfccMirage.ApplyComirvaWay(ref stftdata);

			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				if (DEBUG_OUTPUT_TEXT) mfccdata.WriteAscii(name + "_mfccdata.ascii");
				mfccdata.DrawMatrixGraph(name + "_mfccdata.png", true);
			}
			#endif

			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				// try to do an inverse mfcc
				// see http://stackoverflow.com/questions/1230906/reverse-spectrogram-a-la-aphex-twin-in-matlab
				Comirva.Audio.Util.Maths.Matrix stftdata_inverse_mfcc = mfccMirage.InverseMfcc(ref mfccdata);
				stftdata_inverse_mfcc.DrawMatrixImageLogY(name + "_specgramlog_inverse_mfcc.png", SAMPLING_RATE, 20, SAMPLING_RATE/2, 120, WINDOW_SIZE);
				double[] audiodata_inverse_mfcc = stftMirage.InverseStft(stftdata_inverse_mfcc);

				if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata_inverse_mfcc, name + "_audiodata_inverse_mfcc.txt");
				DrawGraph(audiodata_inverse_mfcc, name + "_audiodata_inverse_mfcc.png");
				bass.SaveFile(MathUtils.DoubleToFloat(audiodata_inverse_mfcc), name + "_inverse_mfcc.wav", Analyzer.SAMPLING_RATE);
			}
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
			}
			pw.Close();
		}
		
		/// <summary>
		/// Write matrix to file using F3 formatting
		/// </summary>
		/// <param name="filename">filename</param>
		public static void WriteF3Formatted(float[] data, string filename) {
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write("{0}", data[i].ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) + " ");
				pw.Write("\r");
			}
			pw.Close();
		}
		
		/// <summary>
		/// Write matrix to file using F3 formatting
		/// </summary>
		/// <param name="filename">filename</param>
		public static void WriteF3Formatted(double[] data, string filename) {
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write("{0}", data[i].ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) + " ");
				pw.Write("\r");
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
