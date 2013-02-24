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
using System.Runtime.InteropServices;

namespace Mirage
{
	public class Fft
	{
		const uint FFTW_R2HC = 0;
		const uint FFTW_DESTROY_INPUT = 1;
		const uint FFTW_ESTIMATE = 64;

		[DllImport("libfftw3f-3")]
		static extern IntPtr fftwf_malloc(int size);

		[DllImport("libfftw3f-3")]
		static extern void fftwf_free(IntPtr p);
		
		[DllImport("libfftw3f-3")]
		static extern IntPtr fftwf_plan_r2r_1d(int n, IntPtr fftin,
		                                       IntPtr fftout, uint kind, uint flags);
		
		[DllImport("libfftw3f-3")]
		static extern void fftwf_destroy_plan(IntPtr plan);

		[DllImport("libfftw3f-3")]
		static extern void fftwf_execute(IntPtr plan);

		IntPtr fftwData;
		IntPtr fftwPlan;
		int winsize;
		int fftsize;
		float[] fft;
		IWindowFunction win;
		float[] data;

		public Fft(int winsize, IWindowFunction window)
		{
			this.winsize = winsize;
			this.fftsize = 2 * winsize;
			
			fftwData = fftwf_malloc(fftsize * sizeof(float));
			fftwPlan = fftwf_plan_r2r_1d(fftsize, fftwData, fftwData, FFTW_R2HC,
			                             FFTW_ESTIMATE | FFTW_DESTROY_INPUT);

			fft = new float[fftsize];
			window.Initialize(winsize);
			win = window;
			data = new float[fftsize];
		}
		
		public void Compute(ref Matrix m, int j, float[] audiodata, int pos)
		{
			win.Apply(ref data, audiodata, pos);

			Marshal.Copy(data, 0, fftwData, fftsize);
			fftwf_execute(fftwPlan);
			Marshal.Copy(fftwData, fft, 0, fftsize);
			
			m.d[0, j] = fft[0]*fft[0];
			for (int i = 1; i < winsize/2; i++) {
				m.d[i, j] = (fft[i*2]*fft[i*2] +
				             fft[fftsize-i*2]*fft[fftsize-i*2]);
			}
			m.d[winsize/2, j] = fft[winsize]*fft[winsize];
		}
		
		public void ComputeComirvaMatrix(ref Comirva.Audio.Util.Maths.Matrix m, int j, float[] audiodata, int pos)
		{
			win.Apply(ref data, audiodata, pos);

			Marshal.Copy(data, 0, fftwData, fftsize);
			fftwf_execute(fftwPlan);
			Marshal.Copy(fftwData, fft, 0, fftsize);
			
			m.MatrixData[0][j] = fft[0]*fft[0];
			for (int i = 1; i < winsize/2; i++) {
				m.MatrixData[i][j] = (fft[i*2]*fft[i*2] +
				                      fft[fftsize-i*2]*fft[fftsize-i*2]);
			}
			m.MatrixData[winsize/2][j] = fft[winsize]*fft[winsize];
		}
		
		~Fft()
		{
			fftwf_destroy_plan(fftwPlan);
			fftwf_free(fftwData);
		}
	}
}
