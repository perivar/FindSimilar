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
using System.Resources;
using System.IO;
using System.Reflection;

namespace Mirage
{
	public class MfccFailedException : Exception
	{
	}

	public class Mfcc
	{
		Matrix filterWeights;
		Matrix dct;
		int[,] fwFT;

		public Mfcc (int winsize, int srate, int filters, int cc)
		{
			// Load the DCT
			dct = Matrix.Load(new FileStream("Mirage/Resources/dct.filter", FileMode.Open));

			// Load the MFCC filters from the filter File.
			filterWeights = Matrix.Load(new FileStream("Mirage/Resources/filterweights.filter", FileMode.Open));

			fwFT = new int[filterWeights.rows, 2];
			for (int i = 0; i < filterWeights.rows; i++) {
				float last = 0;
				for (int j = 0; j < filterWeights.columns; j++) {
					if ((filterWeights.d[i, j] != 0) && (last == 0)) {
						fwFT[i, 0] = j;
					} else if ((filterWeights.d[i, j] == 0) && (last != 0)) {
						fwFT[i, 1] = j;
					}
					last = filterWeights.d[i, j];
				}

				if (last != 0) {
					fwFT[i, 1] = filterWeights.columns;
				}
			}
		}

		public Matrix Apply (ref Matrix m)
		{
			DbgTimer t = new DbgTimer();
			t.Start ();

			Matrix mel = new Matrix (filterWeights.rows, m.columns);

			int mc = m.columns;
			int melcolumns = mel.columns;
			int fwc = filterWeights.columns;
			int fwr = filterWeights.rows;

			unsafe {
				fixed (float* md = m.d, fwd = filterWeights.d, meld = mel.d) {
					for (int i = 0; i < mc; i++) {
						for (int k = 0; k < fwr; k++) {
							int idx = k*melcolumns + i;
							int kfwc = k*fwc;

							// The filter weights matrix is mostly 0.
							// So only multiply non-zero elements!
							for (int j = fwFT[k,0]; j < fwFT[k,1]; j++) {
								meld[idx] += fwd[kfwc + j] * md[j*mc + i];
							}

							meld[idx] = (meld[idx] < 1.0f ?
							             0 : (float)(10.0 * Math.Log10(meld[idx])));
						}
					}
				}
			}

			try {
				Matrix mfcc = dct.Multiply (mel);

				long stop = 0;
				t.Stop (ref stop);
				Dbg.WriteLine ("Mirage - mfcc Execution Time: {0} ms", stop);

				return mfcc;

			} catch (MatrixDimensionMismatchException) {
				throw new MfccFailedException ();
			}
		}
	}
}