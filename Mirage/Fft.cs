/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
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
using System.Runtime.InteropServices;

using Lomont;
using CommonUtils;
using CommonUtils.FFT;

namespace Mirage
{
	/// <summary>
	/// This class applies windowing (e.g HammingWindow, HannWindow) and then performs a Fast Fourier Transform
	/// Float precision
	/// Modified by perivar@nerseth.com
	/// </summary>
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
		Lomont.LomontFFT lomonFFT;
		
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
			
			lomonFFT = new Lomont.LomontFFT();
		}
		
		public void ComputeMirageMatrixUsingFftw(ref Matrix m, int j, float[] audiodata, int pos)
		{
			// apply the window method (e.g HammingWindow, HannWindow etc)
			win.Apply(ref data, audiodata, pos);

			Marshal.Copy(data, 0, fftwData, fftsize);
			fftwf_execute(fftwPlan);
			Marshal.Copy(fftwData, fft, 0, fftsize);

			m.d[0, j] = (float) Math.Sqrt(fft[0]*fft[0]);
			for (int i = 1; i < winsize/2; i++) {
				// amplitude (or magnitude) is the square root of the power spectrum
				// the magnitude spectrum is abs(fft), i.e. Math.Sqrt(re*re + img*img)
				// use 20*log10(Y) to get dB from amplitude
				// the power spectrum is the magnitude spectrum squared
				// use 10*log10(Y) to get dB from power spectrum
				m.d[i, j] = (float) Math.Sqrt((fft[i*2]*fft[i*2] +
				                               fft[fftsize-i*2]*fft[fftsize-i*2]));
			}
			m.d[winsize/2, j] = (float) Math.Sqrt(fft[winsize]*fft[winsize]);
		}
		
		public void ComputeComirvaMatrixUsingFftw(ref Comirva.Audio.Util.Maths.Matrix m, int j, float[] audiodata, int pos)
		{
			// apply the window method (e.g HammingWindow, HannWindow etc)
			win.Apply(ref data, audiodata, pos);

			Marshal.Copy(data, 0, fftwData, fftsize);
			fftwf_execute(fftwPlan);
			Marshal.Copy(fftwData, fft, 0, fftsize);
			
			// fft input will now contain the FFT values in a Half Complex format
			// r0, r1, r2, ..., rn/2, i(n+1)/2-1, ..., i2, i1
			// Here, rk is the real part of the kth output, and ikis the imaginary part. (Division by 2 is rounded down.)
			// For a halfcomplex array hc[n], the kth component thus has its real part in hc[k] and its imaginary part in hc[n-k],
			// with the exception of k == 0 or n/2 (the latter only if n is even)—in these two cases, the imaginary part is zero due to symmetries of the real-input DFT, and is not stored.
			m.MatrixData[0][j] = Math.Sqrt(fft[0] * fft[0]);
			for (int i = 1; i < winsize/2; i++) {
				// amplitude (or magnitude) is the square root of the power spectrum
				// the magnitude spectrum is abs(fft), i.e. Math.Sqrt(re*re + img*img)
				// use 20*log10(Y) to get dB from amplitude
				// the power spectrum is the magnitude spectrum squared
				// use 10*log10(Y) to get dB from power spectrum
				m.MatrixData[i][j] = Math.Sqrt((fft[i * 2]* fft[i * 2] +
				                                fft[fftsize - i * 2] * fft[fftsize - i * 2]));
			}
			//m.MatrixData[winsize/2][j] = Math.Sqrt(fft[winsize] * fft[winsize]);
		}
		
		public void ComputeComirvaMatrixUsingLomontRealFFT(ref Comirva.Audio.Util.Maths.Matrix m, int column, float[] audiodata, int pos) {

			// apply the window method (e.g HammingWindow, HannWindow etc)
			win.Apply(ref data, audiodata, pos);
			
			double[] fft = new double[data.Length/2];
			Array.Copy(data, fft, data.Length/2);
			lomonFFT.RealFFT(fft, true);
			
			// fft input will now contain the FFT values
			// r0, r(n/2), r1, i1, r2, i2 ...
			m.MatrixData[0][column] = Math.Sqrt(fft[0] * fft[0] * winsize);
			m.MatrixData[winsize/2-1][column] = Math.Sqrt(fft[1] * fft[1] * winsize);
			for (int row = 1; row < winsize/2; row++) {
				// amplitude (or magnitude) is the square root of the power spectrum
				// the magnitude spectrum is abs(fft), i.e. Math.Sqrt(re*re + img*img)
				// use 20*log10(Y) to get dB from amplitude
				// the power spectrum is the magnitude spectrum squared
				// use 10*log10(Y) to get dB from power spectrum
				m.MatrixData[row][column] = Math.Sqrt((fft[2 * row] * fft[2 * row] +
				                                       fft[2 * row + 1] * fft[2 * row + 1]) * winsize);
			}
		}

		public void ComputeComirvaMatrixUsingLomontTableFFT(ref Comirva.Audio.Util.Maths.Matrix m, int column, float[] audiodata, int pos) {

			// apply the window method (e.g HammingWindow, HannWindow etc)
			win.Apply(ref data, audiodata, pos);
			
			double[] complexSignal = FFTUtilsLomont.FloatToComplexDouble(data);
			lomonFFT.TableFFT(complexSignal, true);
			
			int row = 0;
			for (int i = 0; i < complexSignal.Length/4; i += 2) {
				double re = complexSignal[2*i];
				double img = complexSignal[2*i + 1];
				m.MatrixData[row][column] = Math.Sqrt( (re*re + img*img) * complexSignal.Length/2);
				row++;
			}
		}
		
		public void ComputeInverseComirvaMatrixUsingLomontRealFFT(Comirva.Audio.Util.Maths.Matrix m, int column, ref Comirva.Audio.Util.Maths.Matrix signal) {

			// NOTE! THIS METHOD DOES NOT WORK?!
			throw new NotImplementedException("Lomont Inverse RealFFT is not implemented. Cannot get it to work?! Try the Inverse TableFFT instead.");
			
			double[] spectrogramWindow = m.GetColumn(column);
			int winsize = MathUtils.NextPowerOfTwo(spectrogramWindow.Length);

			// ifft input must contain the FFT values
			// r0, r(n/2), r1, i1, r2, i2 ...

			// Perform the ifft and take just the real part
			//double[] signalWindow = real(ifft(spectrogramWindow));
			double[] ifft = new double[winsize*2];
			ifft[0] = spectrogramWindow[0];
			ifft[1] = spectrogramWindow[winsize/2];
			for (int i = 1; i < spectrogramWindow.Length; i++) {
				ifft[2 * i] = spectrogramWindow[i];
			}

			lomonFFT.RealFFT(ifft, false);

			for (int i = 0; i < winsize; i++) {
				signal.MatrixData[i][column] = ifft[i] * winsize;
			}
		}
		
		public void ComputeInverseComirvaMatrixUsingLomontTableFFT(Comirva.Audio.Util.Maths.Matrix m, int column, ref Comirva.Audio.Util.Maths.Matrix signal) {

			double[] spectrogramWindow = m.GetColumn(column);

			double[] complexSignal = FFTUtilsLomont.DoubleToComplexDouble(spectrogramWindow);
			lomonFFT.TableFFT(complexSignal, false);
			
			// According to Dave Gamble the first and last entry should be ignored
			for (int row = 1; row < complexSignal.Length/2 - 1; row++) {
				double re = complexSignal[2*row];
				//double img = complexSignal[2*row + 1];
				signal.MatrixData[row][column] = re / Math.Sqrt(winsize/2);
			}
		}
		
		~Fft()
		{
			fftwf_destroy_plan(fftwPlan);
			fftwf_free(fftwData);
			lomonFFT = null;
		}
	}
}
