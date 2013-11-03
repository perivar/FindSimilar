using System;
using System.Runtime.Remoting.Lifetime;
using Comirva.Audio.Util.Maths;

using Wavelets;
using CommonUtils;

/// <summary>
/// Mfcc method copied from the Mirage project:
/// Mirage - High Performance Music Similarity and Automatic Playlist Generator
/// http://hop.at/mirage
///
/// Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
/// Changed and enhanced by Per Ivar Nerseth <perivar@nerseth.com>
///
/// This program is free software; you can redistribute it and/or
/// modify it under the terms of the GNU General Public License
/// as published by the Free Software Foundation; either version 2
/// of the License, or (at your option) any later version.
/// </summary>
namespace Comirva.Audio
{
	public class MfccMirage
	{
		public Matrix filterWeights;
		public Matrix dct;
		
		private int numberCoefficients; 			// number of MFCC COEFFICIENTS. E.g. 20
		private int[] melScaleFreqsIndex; 			// store the mel scale indexes
		private double[] melScaleTriangleHeights; 	// store the mel filter triangle heights
		private int numberWaveletTransforms = 2; 	// number of wavelet transform iterations, 3?
		
		/// <summary>
		/// Create a Mfcc object
		/// This method is not optimized in the sense that the Mel Filter Bands
		/// and the DCT is created here (and not read in)
		/// </summary>
		/// <param name="winsize">window size</param>
		/// <param name="srate">sample rate</param>
		/// <param name="numberFilters">number of filters (MEL COEFFICIENTS). E.g. 36 (SPHINX-III uses 40)</param>
		/// <param name="numberCoefficients">number of MFCC COEFFICIENTS. E.g. 20</param>
		public MfccMirage(int winsize, int srate, int numberFilters, int numberCoefficients)
		{
			this.numberCoefficients = numberCoefficients;
			
			double[] mel = new double[srate/2 - 19];
			double[] freq = new double[srate/2 - 19];
			int startFreq = 20;
			
			// Mel Scale from StartFreq to SamplingRate/2, step every 1Hz
			for (int f = startFreq; f <= srate/2; f++) {
				mel[f-startFreq] = LinearToMel(f);
				freq[f-startFreq] = f;
			}
			
			// Prepare filters
			double[] freqs = new double[numberFilters + 2];
			melScaleFreqsIndex = new int[numberFilters + 2];
			
			for (int f = 0; f < freqs.Length; f++) {
				double melIndex = 1.0 + ((mel[mel.Length - 1] - 1.0) /
				                         (freqs.Length - 1.0) * f);
				double min = Math.Abs(mel[0] - melIndex);
				freqs[f] = freq[0];
				
				for (int j = 1; j < mel.Length; j++) {
					double cur = Math.Abs(mel[j] - melIndex);
					if (cur < min) {
						min = cur;
						freqs[f] = freq[j];
					}
				}
				
				melScaleFreqsIndex[f] = MathUtils.FreqToIndex(freqs[f], srate, winsize);
			}
			
			// triangle heights
			melScaleTriangleHeights = new double[numberFilters];
			for (int j = 0; j < melScaleTriangleHeights.Length; j++) {
				melScaleTriangleHeights[j] = 2.0/(freqs[j+2] - freqs[j]);
			}
			
			double[] fftFreq = new double[winsize/2 + 1];
			for (int j = 0; j < fftFreq.Length; j++) {
				fftFreq[j] = ((srate/2)/(fftFreq.Length -1.0)) * j;
			}
			
			// Compute the MFCC filter Weights
			filterWeights = new Matrix(numberFilters, winsize/2);
			for (int j = 0; j < numberFilters; j++) {
				for (int k = 0; k < fftFreq.Length; k++) {
					if ((fftFreq[k] > freqs[j]) && (fftFreq[k] <= freqs[j+1])) {
						
						filterWeights.MatrixData[j][k] = (float)(melScaleTriangleHeights[j] *
						                                         ((fftFreq[k]-freqs[j])/(freqs[j+1]-freqs[j])));
					}
					if ((fftFreq[k] > freqs[j+1]) &&
					    (fftFreq[k] < freqs[j+2])) {
						
						filterWeights.MatrixData[j][k] += (float)(melScaleTriangleHeights[j] *
						                                          ((freqs[j+2]-fftFreq[k])/(freqs[j+2]-freqs[j+1])));
					}
				}
			}
			#if DEBUG
			if (Mirage.Analyzer.DEBUG_INFO_VERBOSE) {
				if (Mirage.Analyzer.DEBUG_OUTPUT_TEXT) filterWeights.WriteAscii("melfilters-mirage-orig.ascii");
				//filterWeights.DrawMatrixGraph("melfilters-mirage-orig.png");
			}
			#endif
			
			// Compute the DCT
			// This whole section is copied from GetDCTMatrix() from CoMirva package
			dct = new DctComirva(numberCoefficients, numberFilters).DCTMatrix;

			#if DEBUG
			if (Mirage.Analyzer.DEBUG_INFO_VERBOSE) {
				if (Mirage.Analyzer.DEBUG_OUTPUT_TEXT) dct.WriteAscii("dct-mirage-orig.ascii");
				//dct.DrawMatrixGraph("dct-mirage-orig.png");
			}
			#endif
		}
		
		#region Mel scale to Linear and Linear to Mel scale
		/// <summary>
		/// Converts frequency from linear to Mel scale.
		/// Mel-frequency is proportional to the logarithm of the linear frequency,
		/// reflecting similar effects in the human's subjective aural perception)
		/// </summary>
		/// <param name="lFrequency">lFrequency frequency in linear scale</param>
		/// <returns>frequency in Mel scale</returns>
		public static double LinearToMel(double lFrequency)
		{
			return 1127.01048 * Math.Log(1.0 + lFrequency / 700.0);
		}

		/// <summary>
		/// Converts frequency from Mel to linear scale.
		/// Mel-frequency is proportional to the logarithm of the linear frequency,
		/// reflecting similar effects in the human's subjective aural perception)
		/// </summary>
		/// <param name="mFrequency">frequency in Mel scale</param>
		/// <returns>frequency in linear scale</returns>
		public static double MelToLinear(double mFrequency)
		{
			return 700.0 * (Math.Exp(mFrequency / 1127.01048) - 1);
		}
		#endregion
		
		/// <summary>
		/// Apply internal DCT and Mel Filterbands
		/// This method is faster than ApplyComirvaWay since it uses fewer loops.
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled and dct'ed</returns>
		public Matrix ApplyMelScaleDCT(ref Matrix m)
		{
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			Matrix mel = filterWeights * m;
			
			// 5. Take Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (20.0 * Math.Log10(mel.MatrixData[i][j])));
				}
			}
			
			// 6. DCT (Discrete Cosine Transform)
			Matrix mfcc = dct * mel;
			
			Mirage.Dbg.WriteLine("mfcc (MfccMirage-MirageWay) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return mfcc;
		}

		/// <summary>
		/// Apply internal DCT and Mel Filterbands utilising the Comirva Matrix methods
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled and dct'ed</returns>
		public Matrix ApplyMelScaleDCTComirva(ref Matrix m)
		{
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			m = filterWeights * m;

			// 5. Take Logarithm
			// to db
			double log10 = 20 * (1 / Math.Log(10)); // log for base 10 and scale by factor 10
			m.ThrunkAtLowerBoundary(1);
			m.LogEquals();
			m *= log10;

			// 6. DCT (Discrete Cosine Transform)
			m = dct * m;
			
			Mirage.Dbg.WriteLine("mfcc (MfccMirage-ComirvaWay) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
		/// <summary>
		/// Perform an inverse mfcc. E.g. perform an idct and inverse Mel Filterbands and return stftdata
		/// </summary>
		/// <param name="mfcc">mfcc matrix</param>
		/// <returns>matrix idct'ed and mel removed (e.g. stftdata)</returns>
		public Matrix InverseMelScaleDCT(ref Matrix mfcc)
		{
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 6. Perform the IDCT (Inverse Discrete Cosine Transform)
			Matrix mel = dct.Transpose() * mfcc;
			
			// 5. Take Inverse Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = Math.Pow(10, (mel.MatrixData[i][j] / 20)) / melScaleTriangleHeights[0];
				}
			}
			
			// 4. Inverse Mel Scale using interpolation
			// i.e. from e.g.
			// mel=Rows: 40, Columns: 165 (average freq, time slice)
			// to
			// m=Rows: 1024, Columns: 165 (freq, time slice)
			//Matrix m = filterWeights.Transpose() * mel;
			Matrix m = new Matrix(filterWeights.Columns, mel.Columns);
			InverseMelScaling(mel, m);
			
			Mirage.Dbg.WriteLine("imfcc (MfccMirage-MirageWay) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
		/// <summary>
		/// DCT
		/// </summary>
		/// <param name="m">matrix (logSpectrogram)</param>
		/// <returns>matrix dct'ed</returns>
		public Matrix ApplyDCT(ref Matrix m) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 6. DCT (Discrete Cosine Transform)
			m = dct * m;
			
			Mirage.Dbg.WriteLine("ApplyDCT Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
		/// <summary>
		/// Perform an inverse DCT
		/// </summary>
		/// <param name="mfcc">dct matrix</param>
		/// <returns>matrix idct'ed (e.g. logSpectrogram)</returns>
		public Matrix InverseDCT(ref Matrix input) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 6. Perform the IDCT (Inverse Discrete Cosine Transform)
			Matrix m = dct.Transpose() * input;
			
			Mirage.Dbg.WriteLine("InverseDCT Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
		/// <summary>
		/// Mel Scale Haar Wavelet Transform
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled and wavelet'ed</returns>
		public Matrix ApplyMelScaleWaveletPadding(ref Matrix m) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			Matrix mel = filterWeights * m;
			
			// 5. Take Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (20.0 * Math.Log10(mel.MatrixData[i][j])));
				}
			}
			
			// 6. Wavelet Transform
			// make sure the matrix is square before transforming (by zero padding)
			Matrix resizedMatrix;
			if (!mel.IsSymmetric() || !MathUtils.IsPowerOfTwo(mel.Rows)) {
				int size = (mel.Rows > mel.Columns ? mel.Rows : mel.Columns);
				int sizePow2 = MathUtils.NextPowerOfTwo(size);
				resizedMatrix = mel.Resize(sizePow2, sizePow2);
			} else {
				resizedMatrix = mel;
			}
			Matrix wavelet = WaveletUtils.HaarWaveletTransform(resizedMatrix.MatrixData, true);
			
			Mirage.Dbg.WriteLine("Wavelet Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return wavelet;
		}
		
		/// <summary>
		/// Perform an inverse haar wavelet mel scaled transform. E.g. perform an ihaar2d and inverse Mel Filterbands and return stftdata
		/// </summary>
		/// <param name="wavelet">wavelet matrix</param>
		/// <returns>matrix inverse wavelet'ed and mel removed (e.g. stftdata)</returns>
		public Matrix InverseMelScaleWaveletPadding(ref Matrix wavelet) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 6. Perform the Inverse Wavelet Transform
			Matrix mel = WaveletUtils.InverseHaarWaveletTransform(wavelet.MatrixData);
			
			// Resize (remove padding)
			mel = mel.Resize(melScaleFreqsIndex.Length - 2, wavelet.Columns);
			
			// 5. Take Inverse Logarithm
			// Divide with first triangle height in order to scale properly
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = Math.Pow(10, (mel.MatrixData[i][j] / 20)) / melScaleTriangleHeights[0];
				}
			}
			
			// 4. Inverse Mel Scale using interpolation
			// i.e. from e.g.
			// mel=Rows: 40, Columns: 165 (average freq, time slice)
			// to
			// m=Rows: 1024, Columns: 165 (freq, time slice)
			//Matrix m = filterWeights.Transpose() * mel;
			Matrix m = new Matrix(filterWeights.Columns, mel.Columns);
			InverseMelScaling(mel, m);

			Mirage.Dbg.WriteLine("Inverse Wavelet Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
		/// <summary>
		/// Haar Wavelet Transform and Compress
		/// </summary>
		/// <param name="m">matrix (logSpectrogram)</param>
		/// <returns>matrix wavelet'ed</returns>
		public Matrix ApplyWaveletCompression(ref Matrix m, out int lastHeight, out int lastWidth) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// Wavelet Transform
			Matrix wavelet = m.Copy();
			Wavelets.Compress.WaveletCompress.HaarTransform2D(wavelet.MatrixData, numberWaveletTransforms, out lastHeight, out lastWidth);
			
			// Compress
			Matrix waveletCompressed = wavelet.Resize(numberCoefficients, wavelet.Columns);
			
			Mirage.Dbg.WriteLine("Wavelet Compression Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return waveletCompressed;
		}
		
		/// <summary>
		/// Perform an inverse decompressed haar wavelet transform. E.g. perform an ihaar2d and return logSpectrogram
		/// </summary>
		/// <param name="wavelet">wavelet matrix</param>
		/// <returns>matrix inverse wavelet'ed (e.g. logSpectrogram)</returns>
		public Matrix InverseWaveletCompression(ref Matrix wavelet, int firstHeight, int firstWidth, int rows, int columns) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// Resize, e.g. Uncompress
			wavelet = wavelet.Resize(rows, columns);

			// 6. Perform the Inverse Wavelet Transform
			Matrix m = wavelet.Copy();
			Wavelets.Compress.WaveletDecompress.Decompress2D(m.MatrixData, numberWaveletTransforms, firstHeight, firstWidth);
			
			Mirage.Dbg.WriteLine("Inverse Wavelet Compression Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
		/// <summary>
		/// Mel Scale Haar Wavelet Transform and Compress
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled and wavelet'ed</returns>
		public Matrix ApplyMelScaleWaveletCompression(ref Matrix m, out int lastHeight, out int lastWidth) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			Matrix mel = filterWeights * m;
			
			// 5. Take Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (20.0 * Math.Log10(mel.MatrixData[i][j])));
				}
			}
			
			// 6. Perform the Wavelet Transform and Compress
			Matrix waveletCompressed = ApplyWaveletCompression(ref mel, out lastHeight, out lastWidth);
			
			Mirage.Dbg.WriteLine("Wavelet Compression Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return waveletCompressed;
		}

		/// <summary>
		/// Perform an inverse haar wavelet mel scaled transform. E.g. perform an ihaar2d and inverse Mel Filterbands and return stftdata
		/// </summary>
		/// <param name="wavelet">wavelet matrix</param>
		/// <returns>matrix inverse wavelet'ed and mel removed (e.g. stftdata)</returns>
		public Matrix InverseMelScaleWaveletCompression(ref Matrix wavelet, int firstHeight, int firstWidth) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 6. Ucompress and then perform the Inverse Wavelet Transform
			Matrix mel = InverseWaveletCompression(ref wavelet, firstHeight, firstWidth, melScaleFreqsIndex.Length - 2, wavelet.Columns);
			
			// 5. Take Inverse Logarithm
			// Divide with first triangle height in order to scale properly
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = Math.Pow(10, (mel.MatrixData[i][j] / 20)) / melScaleTriangleHeights[0];
				}
			}
			
			// 4. Inverse Mel Scale using interpolation
			// i.e. from e.g.
			// mel=Rows: 40, Columns: 165 (average freq, time slice)
			// to
			// m=Rows: 1024, Columns: 165 (freq, time slice)
			//Matrix m = filterWeights.Transpose() * mel;
			Matrix m = new Matrix(filterWeights.Columns, mel.Columns);
			InverseMelScaling(mel, m);

			Mirage.Dbg.WriteLine("Inverse Wavelet Compression Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
		/// <summary>
		/// Mel Scale and Log
		/// </summary>
		/// <param name="m">matrix (stftdata)</param>
		/// <returns>matrix mel scaled</returns>
		public Matrix ApplyMelScaleAndLog(ref Matrix m) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			Matrix mel = filterWeights * m;
			
			// 5. Take Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (20.0 * Math.Log10(mel.MatrixData[i][j])));
					//mel.MatrixData[i][j] = 20.0 * Math.Log10(mel.MatrixData[i][j]);
				}
			}
			
			Mirage.Dbg.WriteLine("MelScaleAndLog Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return mel;
		}
		
		/// <summary>
		/// Perform an inverse mel scale and log.
		/// </summary>
		/// <param name="wavelet">mel scaled matrix</param>
		/// <returns>matrix mel removed and un-logged (e.g. stftdata)</returns>
		public Matrix InverseMelScaleAndLog(ref Matrix mel) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 5. Take Inverse Logarithm
			// Divide with first triangle height in order to scale properly
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = Math.Pow(10, (mel.MatrixData[i][j] / 20)) / melScaleTriangleHeights[0];
				}
			}
			
			// 4. Inverse Mel Scale using interpolation
			// i.e. from e.g.
			// mel=Rows: 40, Columns: 165 (average freq, time slice)
			// to
			// m=Rows: 1024, Columns: 165 (freq, time slice)
			//Matrix m = filterWeights.Transpose() * mel;
			Matrix m = new Matrix(filterWeights.Columns, mel.Columns);
			InverseMelScaling(mel, m);
			
			Mirage.Dbg.WriteLine("InverseMelScaleAndLog Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
		/// <summary>
		/// Perform an inverse mel scale using interpolation
		/// i.e. from e.g.
		/// mel=Rows: 40, Columns: 165 (average freq, time slice)
		/// to
		/// m=Rows: 1024, Columns: 165 (freq, time slice)
		/// </summary>
		/// <param name="mel"></param>
		/// <param name="m"></param>
		private void InverseMelScaling(Matrix mel, Matrix m) {

			// for each row, interpolate values to next row according to mel scale
			for (int j = 0; j < mel.Columns; j++) {
				for (int i = 0; i < mel.Rows-1; i++) {
					double startValue = mel.MatrixData[i][j];
					double endValue = mel.MatrixData[i+1][j];
					
					// what indexes in resulting matrix does this row cover?
					//Console.Out.WriteLine("Mel Row index {0} corresponds to Linear Row index {1} - {2} [{3:0.00} - {4:0.00}]", i, freqsIndex[i+1], freqsIndex[i+2]-1, startValue, endValue);

					// add interpolated values
					AddInterpolatedValues(m, melScaleFreqsIndex[i+1], melScaleFreqsIndex[i+2], startValue, endValue, j);
				}

				// last row
				int iLast = mel.Rows - 1;
				double startValueLast = mel.MatrixData[iLast][j];
				double endValueLast = 0.0; // mel.MatrixData[iLast][j];

				// what indexes in resulting matrix does this row cover?
				//Console.Out.WriteLine("Mel Row index {0} corresponds to Linear Row index {1} - {2} [{3:0.00} - {4:0.00}]", iLast, freqsIndex[iLast+1], freqsIndex[iLast+2]-1, startValueLast, endValueLast);

				// add interpolated values
				AddInterpolatedValues(m, melScaleFreqsIndex[iLast+1], melScaleFreqsIndex[iLast+2], startValueLast, endValueLast, j);
			}
			
		}
		
		private void AddInterpolatedValues(Matrix m, int startIndex, int endIndex, double startValue, double endValue, int columnIndex) {
			
			// interpolate and add values
			int partSteps =  endIndex - startIndex;
			for (int step = 0; step < partSteps; step ++) {
				double p = (double) step / (double) partSteps;

				// interpolate
				double val = MathUtils.Interpolate(startValue, endValue, p);
				
				// add to matrix data
				m.MatrixData[startIndex+step][columnIndex] = val;
			}
		}
	}
}
