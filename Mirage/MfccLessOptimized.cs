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

namespace Mirage
{
	public class MfccLessOptimized
	{
		Matrix filterWeights;
		Matrix dct;
		
		public MfccLessOptimized(int winsize, int srate, int filters, int cc)
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
			double[] freqs = new double[filters + 2];
			
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
			
			double[] triangleh = new double[filters];
			for (int j = 0; j < triangleh.Length; j++) {
				triangleh[j] = 2.0/(freqs[j+2] - freqs[j]);
			}
			
			double[] fftFreq = new double[winsize/2 + 1];
			for (int j = 0; j < fftFreq.Length; j++) {
				fftFreq[j] = ((srate/2)/(fftFreq.Length -1.0)) * j;
			}
			
			// Compute the MFCC filter Weights
			filterWeights = new Matrix(filters, winsize/2 + 1);
			for (int j = 0; j < filters; j++) {
				for (int k = 0; k < fftFreq.Length; k++) {
					if ((fftFreq[k] > freqs[j]) && (fftFreq[k] <= freqs[j+1])) {
						
						filterWeights.d[j, k] = (float)(triangleh[j] *
						                                ((fftFreq[k]-freqs[j])/(freqs[j+1]-freqs[j])));
					}
					if ((fftFreq[k] > freqs[j+1]) &&
					    (fftFreq[k] < freqs[j+2])) {
						
						filterWeights.d[j, k] += (float)(triangleh[j] *
						                                 ((freqs[j+2]-fftFreq[k])/(freqs[j+2]-freqs[j+1])));
					}
				}
			}
			
			// Compute the DCT
			dct = new Matrix(cc, filters);
			double scalefac = 1.0 / Math.Sqrt(filters/2.0);
			
			for (int j = 0; j < cc; j++) {
				for (int k = 0; k < filters; k++) {
					dct.d[j, k] = (float)(scalefac * Math.Cos((j+1) * (2*k + 1.0) *
					                                          Math.PI/2.0/filters));
					if (j == 0)
						dct.d[j, k] = (float)(dct.d[j, k] * (Math.Sqrt(2.0)/2.0));
				}
			}
		}
		
		public Matrix Apply(ref Matrix m)
		{
			DbgTimer t = new DbgTimer();
			t.Start();

			Matrix mel = new Matrix(filterWeights.rows, m.columns);
			
			/*
			// Performance optimization of ...
			mel = filterWeights.Multiply(m);
			for (int i = 0; i < mel.rows; i++) {
				for (int j = 0; j < mel.columns; j++) {
					mel.d[i, j] = (mel.d[i, j] < 1.0f ? 0 : (float)(10.0 * Math.Log10(mel.d[i, j])));
					//mel.d[i, j] = (float)(10.0 * Math.Log10(mel.d[i, j]));
				}
			}
			 */
			
			int mc = m.columns;
			int mr = m.rows;
			int melcolumns = mel.columns;
			int fwc = filterWeights.columns;
			int fwr = filterWeights.rows;

			unsafe {
				fixed (float* md = m.d, fwd = filterWeights.d, meld = mel.d) {
					for (int i = 0; i < mc; i++) {
						for (int k = 0; k < fwr; k++) {
							int idx = k*melcolumns + i;
							int kfwc = k*fwc;

							for (int j = 0; j < mr; j++) {
								meld[idx] += fwd[kfwc + j] * md[j*mc + i];
							}

							meld[idx] = (meld[idx] < 1.0f ?
							             0 : (float)(10.0 * Math.Log10(meld[idx])));
						}
						
					}
				}
			}
			
			Matrix mfcc = dct.Multiply(mel);
			
			Dbg.WriteLine("mfcc (MfccLessOptimized) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			
			return mfcc;
		}
	}
}
