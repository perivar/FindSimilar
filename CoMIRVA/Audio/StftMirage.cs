using System;
using Comirva.Audio.Util.Maths;

/// <summary>
/// Short Term Fourier Transformation method copied from the Mirage project:
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
namespace Comirva.Audio {

	/// <summary>
	/// Class to perform a Short Time Fourier Transformation
	/// </summary>
	public class StftMirage
	{
		int winsize;
		int hopsize;
		Mirage.Fft fft; // Use the mirage fft class
		
		/// <summary>
		/// Instantiate a new Stft Class
		/// </summary>
		/// <param name="winsize">FFT window size</param>
		/// <param name="hopsize">Value to hop on to the next window</param>
		/// <param name="window">Window function to apply to every window processed</param>
		public StftMirage(int winsize, int hopsize, Mirage.IWindowFunction window)
		{
			this.winsize = winsize;
			this.hopsize = hopsize;
			fft = new Mirage.Fft(winsize, window);
		}
		
		/// <summary>
		/// Apply the STFT on the audiodata
		/// </summary>
		/// <param name="audiodata">Audiodata to apply the STFT on</param>
		/// <returns>A matrix with the result of the STFT</returns>
		public Matrix Apply(float[] audiodata)
		{
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			int hops = (audiodata.Length - winsize)/hopsize;
			
			// Create a Matrix with "winsize" Rows and "hops" Columns
			// Matrix[Row, Column]
			Matrix stft = new Matrix(winsize/2 +1, hops);
			
			for (int i = 0; i < hops; i++) {
				//fft.ComputeComirvaMatrix(ref stft, i, audiodata, i*hopsize);
				fft.ComputeComirvaMatrixUsingLomont(ref stft, i, audiodata, i*hopsize);
			}
			
			Mirage.Dbg.WriteLine("Stft Execution Time: " + t.Stop().Milliseconds + " ms");
			return stft;
		}
	}
}
