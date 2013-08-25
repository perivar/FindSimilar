using System;
using System.Runtime.Remoting.Lifetime;
using Comirva.Audio.Util.Maths;
using Wavelets;

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
		
		private double[] freq; // store linear scale
		private double[] freqs; // store the mel scale filter frequencies
		private int[] freqsIndex;
		private double[] fftFreq; // store fft frequencies
		
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
			double[] mel = new double[srate/2 - 19];
			//double[] freq = new double[srate/2 - 19];
			freq = new double[srate/2 - 19];
			int startFreq = 20;
			
			// Mel Scale from StartFreq to SamplingRate/2, step every 1Hz
			for (int f = startFreq; f <= srate/2; f++) {
				//mel[f-startFreq] = Math.Log(1.0 + f/700.0) * 1127.01048;
				mel[f-startFreq] = LinearToMel(f);
				freq[f-startFreq] = f;
			}
			
			// Prepare filters
			//double[] freqs = new double[numberFilters + 2];
			freqs = new double[numberFilters + 2];
			freqsIndex = new int[numberFilters + 2];
			
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
				
				freqsIndex[f] = CommonUtils.MathUtils.FreqToIndex(freqs[f], srate, winsize);
			}
			
			double[] triangleh = new double[numberFilters];
			for (int j = 0; j < triangleh.Length; j++) {
				triangleh[j] = 2.0/(freqs[j+2] - freqs[j]);
			}
			
			//double[] fftFreq = new double[winsize/2 + 1];
			fftFreq = new double[winsize/2 + 1];
			for (int j = 0; j < fftFreq.Length; j++) {
				fftFreq[j] = ((srate/2)/(fftFreq.Length -1.0)) * j;
			}
			
			// write out
			/*
			Matrix m_freqs = new Matrix(freqs, freqs.Length);
			m_freqs.WriteCSV("m_freqs.csv", ";");
			Matrix m_fftFreq = new Matrix(fftFreq, fftFreq.Length);
			m_fftFreq.WriteCSV("m_fftFreq.csv", ";");
			Matrix m_freqsIndex = new Matrix(freqsIndex, freqsIndex.Length);
			m_freqsIndex.WriteCSV("m_freqsIndex.csv", ";");
			 */
			
			// Compute the MFCC filter Weights
			filterWeights = new Matrix(numberFilters, winsize/2);
			for (int j = 0; j < numberFilters; j++) {
				for (int k = 0; k < fftFreq.Length; k++) {
					if ((fftFreq[k] > freqs[j]) && (fftFreq[k] <= freqs[j+1])) {
						
						filterWeights.MatrixData[j][k] = (float)(triangleh[j] *
						                                         ((fftFreq[k]-freqs[j])/(freqs[j+1]-freqs[j])));
					}
					if ((fftFreq[k] > freqs[j+1]) &&
					    (fftFreq[k] < freqs[j+2])) {
						
						filterWeights.MatrixData[j][k] += (float)(triangleh[j] *
						                                          ((freqs[j+2]-fftFreq[k])/(freqs[j+2]-freqs[j+1])));
					}
				}
			}
			#if DEBUG
			if (Mirage.Analyzer.DEBUG_INFO_VERBOSE) {
				if (Mirage.Analyzer.DEBUG_OUTPUT_TEXT) filterWeights.WriteAscii("melfilters-mirage-orig.ascii");
				filterWeights.DrawMatrixGraph("melfilters-mirage-orig.png");
			}
			#endif
			
			// Compute the DCT
			// This whole section is copied from GetDCTMatrix() from CoMirva package
			dct = new DctComirva(numberCoefficients, numberFilters).DCTMatrix;

			#if DEBUG
			if (Mirage.Analyzer.DEBUG_INFO_VERBOSE) {
				if (Mirage.Analyzer.DEBUG_OUTPUT_TEXT) dct.WriteAscii("dct-mirage-orig.ascii");
				dct.DrawMatrixGraph("dct-mirage-orig.png");
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
		public Matrix Apply(ref Matrix m)
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
					mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (10.0 * Math.Log10(mel.MatrixData[i][j])));
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
		public Matrix ApplyComirvaWay(ref Matrix m)
		{
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			m = filterWeights * m;

			// 5. Take Logarithm
			// to db
			double log10 = 10 * (1 / Math.Log(10)); // log for base 10 and scale by factor 10
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
		public Matrix InverseMfcc(ref Matrix mfcc)
		{
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 6. Perform the IDCT (Inverse Discrete Cosine Transform)
			Matrix mel = dct.Transpose() * mfcc;
			
			// 5. Take Inverse Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = Math.Pow(10, mel.MatrixData[i][j] / 10);
				}
			}
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			// Matrix mel = new Matrix(filterWeights.Rows, m.Columns);
			// mel = filterWeights * m;
			Matrix m = filterWeights.Transpose() * mel;

			Mirage.Dbg.WriteLine("imfcc (MfccMirage-MirageWay) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
		public Matrix ApplyWavelet(ref Matrix m) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			Matrix mel = filterWeights * m;
			
			// 5. Take Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (10.0 * Math.Log10(mel.MatrixData[i][j])));
				}
			}
			
			// 6. WAVELET
			Matrix wavelet = WaveletUtils.HaarWaveletTransform(mel.MatrixData);
			
			Mirage.Dbg.WriteLine("Wavelet Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return wavelet;
		}
		
		public Matrix InverseWavelet(ref Matrix wavelet) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 6. Perform the Inverse Wavelet Transform
			Matrix mel = WaveletUtils.InverseHaarWaveletTransform(wavelet.MatrixData);
			
			// 5. Take Inverse Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = Math.Pow(10, mel.MatrixData[i][j] / 10);
				}
			}
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			// Matrix mel = new Matrix(filterWeights.Rows, m.Columns);
			// mel = filterWeights * m;
			Matrix m = filterWeights.Transpose() * mel;

			Mirage.Dbg.WriteLine("Inverse Wavelet Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
		
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
					mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (10.0 * Math.Log10(mel.MatrixData[i][j])));
				}
			}
			
			Mirage.Dbg.WriteLine("MelScaleAndLog Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return mel;
		}
		
		public Matrix InverseMelScaleAndLog(ref Matrix mel) {
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 5. Take Inverse Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = Math.Pow(10, mel.MatrixData[i][j] / 10);
				}
			}
			
			//mel.WriteCSV("mel.csv", ";");
			
			// 4. Inverse Mel Scale using interpolation
			// i.e. from e.g.
			// mel=Rows: 40, Columns: 165 (average freq, time slice)
			// to
			// m=Rows: 1024, Columns=165 (freq, time slice)
			
			Matrix m = new Matrix(filterWeights.Columns, mel.Columns);
			
			// for each row, interpolate values to next row according to mel scale
			for (int i = 0; i < mel.Rows-1; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					double startValue = mel.MatrixData[i][j];
					double endValue = mel.MatrixData[i+1][j];
					
					// what indexes in resulting matrix does this row cover?
					Console.Out.WriteLine("Mel Row index {0} corresponds to Linear Row index {1} - {2} [{3:0.00} - {4:0.00}]", i, freqsIndex[i+1], freqsIndex[i+2]-1, startValue, endValue);
					
					// interpolate values
					int partSteps =  freqsIndex[i+2] - freqsIndex[i+1];
					for (int step = 0; step < partSteps; step ++) {
						double p = (double) step / (double) partSteps;

						// interpolate
						double val = CommonUtils.MathUtils.Interpolate(startValue, endValue, p);
						
						// add to matrix data
						m.MatrixData[freqsIndex[i+1]-1+step][j] = val;
					}
				}
			}

			// last row
			int iLast = mel.Rows - 1;
			double startValueLast = mel.MatrixData[iLast-1][0];
			double endValueLast = mel.MatrixData[iLast][0];;

			// what indexes in resulting matrix does this row cover?
			Console.Out.WriteLine("Mel Row index {0} corresponds to Linear Row index {1} - {2} [{3:0.00} - {4:0.00}]", iLast, freqsIndex[iLast+1], freqsIndex[iLast+2]-1, startValueLast, endValueLast);
			
			
			Mirage.Dbg.WriteLine("InverseMelScaleAndLog Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
	}
}
