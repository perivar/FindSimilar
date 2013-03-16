using System;
using Comirva.Audio.Util.Maths;

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
		
		/// <summary>
		/// Create a Mfcc object
		/// This method is not optimized in the sense that the Mel Filter Bands
		/// and the DCT is created here (and not read in)
		/// </summary>
		/// <param name="winsize">window size</param>
		/// <param name="srate">sample rate</param>
		/// <param name="numberFilters">number of filters (MEL COEFFICIENTS). E.g. 36 (SPHINX-III uses 40)</param>
		/// <param name="numberCoefficients">number of MFCC COEFFICIENTS</param>
		public MfccMirage(int winsize, int srate, int numberFilters, int numberCoefficients)
		{
			double[] mel = new double[srate/2 - 19];
			double[] freq = new double[srate/2 - 19];
			int startFreq = 20;
			
			// Mel Scale from StartFreq to SamplingRate/2, step every 1Hz
			for (int f = startFreq; f <= srate/2; f++) {
				mel[f-startFreq] = Math.Log(1.0 + f/700.0) * 1127.01048;
				freq[f-startFreq] = f;
			}
			
			// Prepare filters
			double[] freqs = new double[numberFilters + 2];
			
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
			}
			
			double[] triangleh = new double[numberFilters];
			for (int j = 0; j < triangleh.Length; j++) {
				triangleh[j] = 2.0/(freqs[j+2] - freqs[j]);
			}
			
			double[] fftFreq = new double[winsize/2 + 1];
			for (int j = 0; j < fftFreq.Length; j++) {
				fftFreq[j] = ((srate/2)/(fftFreq.Length -1.0)) * j;
			}
			
			// Compute the MFCC filter Weights
			filterWeights = new Matrix(numberFilters, winsize/2 + 1);
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
			//filterWeights.DrawMatrixGraph("melfilters-mirage-orig.png");
			#endif
			
			// Compute the DCT
			// This whole section is copied from GetDCTMatrix() from CoMirva package
			dct = new Matrix(numberCoefficients, numberFilters);
			
			// compute constants
			double k1 = Math.PI/numberFilters;
			double w1 = 1.0/(Math.Sqrt(numberFilters));
			double w2 = Math.Sqrt(2.0/numberFilters);

			//generate dct matrix
			for(int i = 0; i < numberCoefficients; i++)
			{
				for(int j = 0; j < numberFilters; j++)
				{
					if(i == 0)
						dct.Set(i, j, w1 * Math.Cos(k1*i*(j + 0.5d)));
					else
						dct.Set(i, j, w2 * Math.Cos(k1*i*(j + 0.5d)));
				}
			}

			#if DEBUG
			//dct.DrawMatrixGraph("dct-mirage-orig.png");
			#endif
		}
		
		/// <summary>
		/// Apply internal DCT and Mel Filterbands
		/// </summary>
		/// <param name="m">matrix</param>
		/// <returns>matrix mel scaled and dct'ed</returns>
		public Matrix Apply(ref Matrix m)
		{
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			Matrix mel = new Matrix(filterWeights.Rows, m.Columns);
			mel = filterWeights * m;
			
			// 5. Take Logarithm
			for (int i = 0; i < mel.Rows; i++) {
				for (int j = 0; j < mel.Columns; j++) {
					mel.MatrixData[i][j] = (mel.MatrixData[i][j] < 1.0 ? 0 : (10.0 * Math.Log10(mel.MatrixData[i][j])));
				}
			}
			
			// 6. DCT (Discrete cosine transform)
			Matrix mfcc = dct * mel;

			Mirage.Dbg.WriteLine("mfcc (MfccMirage-MirageWay) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return mfcc;
		}

		/// <summary>
		/// Apply internal DCT and Mel Filterbands utilising the Comirva Matrix methods
		/// </summary>
		/// <param name="m">matrix</param>
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

			// 6. DCT (Discrete cosine transform)
			m = dct * m;
			
			Mirage.Dbg.WriteLine("mfcc (MfccMirage-ComirvaWay) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return m;
		}
	}
}
