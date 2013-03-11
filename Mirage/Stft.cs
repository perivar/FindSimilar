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

namespace Mirage
{

	/// <summary>
	/// Class to perform a Short Time Fourier Transformation
	/// </summary>
	public class Stft
	{
		int winsize;
		int hopsize;
		Fft fft;
		
		/// <summary>
		/// Instantiate a new Stft Class
		/// </summary>
		/// <param name="winsize">FFT window size</param>
		/// <param name="hopsize">Value to hop on to the next window (50% overlap is when hopsize is half of the window size)</param>
		/// <param name="window">Window function to apply to every window processed</param>
		public Stft(int winsize, int hopsize, IWindowFunction window)
		{
			this.winsize = winsize;
			this.hopsize = hopsize;
			fft = new Fft(winsize, window);
		}
		
		/// <summary>
		/// Apply the STFT on the audiodata
		/// </summary>
		/// <param name="audiodata">Audiodata to apply the STFT on</param>
		/// <returns>A matrix with the result of the STFT</returns>
		public Matrix Apply(float[] audiodata)
		{
			DbgTimer t = new DbgTimer();
			t.Start();
			
			// calculate how many hops (bands) we have using the current overlap (hopsize)
			int hops = (audiodata.Length - winsize)/ hopsize;
			
			// Create a Matrix with "winsize" Rows and "hops" Columns
			// Matrix[Row, Column]
			Matrix stft = new Matrix(winsize/2 +1, hops);
			
			for (int i = 0; i < hops; i++) {
				fft.ComputeMirageMatrix(ref stft, i, audiodata, i*hopsize);
			}
			
			Dbg.WriteLine("Stft (ComputeMirageMatrix) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			
			return stft;
		}
	}
}
