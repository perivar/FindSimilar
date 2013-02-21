using System.Collections.Generic;
using System;
using System.Numerics; // for complex numbers

/**
 * @file MelFilter.cpp
 *
 * Triangular filters in Mel frequency scale - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.3
 * @since 0.3.3
 */
namespace Aquila
{
	/**
	 * Encapsulation of a single Mel-frequency filter.
	 */
	public class MelFilter
	{
		/**
		 * Sample frequency of a signal which spectrum is to be filtered.
		 */
		private double sampFreq;

		/**
		 * Filter spectrum (real).
		 */
		private double[] filterSpectrum;

		/**
		 * Is the filter enabled.
		 */
		private bool enabled;

		/**
		 * Creates the filter and sets sample frequency.
		 *
		 * @param sampleFrequency sample frequency in Hz
		 */
		public MelFilter(double sampleFrequency)
		{
			sampFreq = sampleFrequency;
			enabled = true;
		}

		public double[] GetFilterSpectrum() {
			return filterSpectrum;
		}
		
		/**
		 * Designs the Mel filter and creates triangular spectrum.
		 *
		 * @param filterNum which filter in a sequence it is?
		 * @param melFilterWidth filter width in Mel scale (eg. 200)
		 * @param N filter spectrum size (must be the same as filtered spectrum)
		 */
		public void CreateFilter(int filterNum, int melFilterWidth, int N)
		{
			// calculating frequencies in Mel scale
			double melMinFreq = filterNum * melFilterWidth / 2.0;
			double melCenterFreq = melMinFreq + melFilterWidth / 2.0;
			double melMaxFreq = melMinFreq + melFilterWidth;
			
			// converting them to linear
			double minFreq = MelToLinear(melMinFreq);
			double centerFreq = MelToLinear(melCenterFreq);
			double maxFreq = MelToLinear(melMaxFreq);
			
			// generating spectrum in linear scale
			GenerateFilterSpectrum(minFreq, centerFreq, maxFreq, N);
		}

		/**
		 * Returns a single value computed by multiplying signal spectrum with
		 * Mel filter spectrum, and summing all the products.
		 *
		 * @param dataSpectrum complex signal spectrum
		 * @param N spectrum length
		 * @return dot product of the spectra
		 */
		public double Apply(List<Complex> dataSpectrum, int N)
		{
			double value = 0.0;
			for (int i = 0; i < N / 2 - 1; ++i)
				value += Complex.Abs(dataSpectrum[i]) * filterSpectrum[i];
			return value;
		}

		/**
		 * Returns a single value computed by multiplying signal spectrum with
		 * Mel filter spectrum, and summing all the products.
		 *
		 * This is an overload for real-valued data spectrum.
		 *
		 * @param dataAbsSpectrum magnitude of signal spectrum
		 * @param N spectrum length
		 * @return dot product of the spectra
		 */
		public double Apply(List<double> dataAbsSpectrum, int N)
		{
			double value = 0.0;
			for (int i = 0; i < N / 2 - 1; ++i)
				value += dataAbsSpectrum[i] * filterSpectrum[i];
			return value;
		}

		/**
		 * Turns on/off the filter.
		 *
		 * param enable true - filter enabled; false - filter disabled
		 */
		public void SetEnabled(bool enable)
		{
			enabled = enable;
		}

		/**
		 * Checks if the filter is enabled.
		 *
		 * @return true, when the filter is enabled
		 */
		public bool IsEnabled()
		{
			return enabled;
		}

		/**
		 * Converts frequency from linear to Mel scale.
		 *
		 * @param lFrequency frequency in linear scale
		 * @return frequency in Mel scale
		 */
		public static double LinearToMel(double lFrequency)
		{
			return 1127.01048 * Math.Log(1 + lFrequency / 700.0);
		}

		/**
		 * Converts frequency from Mel to linear scale.
		 *
		 * @param mFrequency frequency in Mel scale
		 * @return frequency in linear scale
		 */
		public static double MelToLinear(double mFrequency)
		{
			return 700.0 * (Math.Exp(mFrequency / 1127.01048) - 1);
		}

		/**
		 * Fills the vector with spectrum values, but in linear scale.
		 *
		 * @param minFreq low filter frequency in linear scale
		 * @param centerFreq center filter frequency in linear scale
		 * @param maxFreq high filter frequency in linear scale
		 * @param N spectrum size
		 */
		public void GenerateFilterSpectrum(double minFreq, double centerFreq, double maxFreq, int N)
		{
			//filterSpectrum.Clear();
			//filterSpectrum.resize(N);
			Array.Resize(ref filterSpectrum, N);
			
			// scale the frequencies according to spectrum size
			minFreq *= N / sampFreq;
			centerFreq *= N / sampFreq;
			maxFreq *= N / sampFreq;
			
			// maximum and current value of the filter spectrum
			double max = 1.0;
			double value;
			
			for (int k = 0; k < N; k++)
			{
				// outside the triangle spectrum has 0 values
				if (k < minFreq || k > maxFreq)
				{
					value = 0.0;
				}
				else
				{
					// in the triangle, on the ascending slope
					if (k < centerFreq)
					{
						value = k * max / (centerFreq - minFreq) - minFreq * max / (centerFreq - minFreq);
					}
					// in the triangle, on the descending slope
					else
					{
						value = k * max / (centerFreq - maxFreq) - maxFreq * max / (centerFreq - maxFreq);
					}
				}
				
				filterSpectrum[k] = value;
			}
		}
	}
}

