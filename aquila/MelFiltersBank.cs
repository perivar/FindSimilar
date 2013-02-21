using System;
using System.Collections.Generic;
using System.Numerics; // for complex numbers

// For drawing graph
using ZedGraph;
using System.Drawing;
using System.Drawing.Imaging;

/**
 * @file MelFiltersBank.cpp
 *
 * Mel filters bank - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.3
 * @since 0.3.3
 */
namespace Aquila
{
	/**
	 * A wrapper class for a vector of triangular filters.
	 */
	public class MelFiltersBank
	{
		/**
		 * Vector of pointers to Mel filters (allocated on heap).
		 */
		private List<MelFilter> filters = new List<MelFilter>();

		/**
		 * Sample frequency of the filtered signal.
		 */
		private double sampleFreq;

		/**
		 * Filter spectrum size (equal to zero-padded length of signal frame).
		 */
		private int N_;
		
		/**
		 * Creates all the filters in the bank.
		 *
		 * @param sampleFrequency sample frequency in Hz
		 * @param melFilterWidth filter width in Mel frequency scale
		 * @param N spectrum size of each filter
		 */
		public MelFiltersBank(double sampleFrequency, ushort melFilterWidth, int N)
		{
			sampleFreq = sampleFrequency;
			N_ = N;
			for (int i = 0; i < Dtw.MELFILTERS; ++i)
			{
				//filters.push_back(new MelFilter(sampleFrequency));
				MelFilter filter = new MelFilter(sampleFrequency);
				filter.CreateFilter(i, melFilterWidth, N);
				filters.Add(filter);
			}
		}

		/**
		 * Creates all the filters in the bank - tweaked for HFCC calculation.
		 *
		 * @param sampleFrequency sample frequency in Hz
		 * @param N spectrum size of each filter
		 * @param isHfcc a placeholder to overload the constructor
		 */
		public MelFiltersBank(double sampleFrequency, int N) : this(sampleFrequency, N, true)
		{
		}

		public MelFiltersBank(double sampleFrequency, int N, bool isHfcc)
		{
			sampleFreq = sampleFrequency;
			N_ = N;

			// and here the magic happens ;)
			double f_low_global = 0.0;
			double f_high_global = sampleFrequency / 2.0;
			const double a = 6.23e-6;
			const double b = 93.39e-3;
			const double c = 28.52;
			double a1 = 0.0;
			double b1 = 0.0;
			double c1 = 0.0;
			double b2 = 0.0;
			double c2 = 0.0;
			a1 = 0.5 / (700.0 + f_low_global);
			b1 = 700 / (700.0 + f_low_global);
			c1 = (-f_low_global / 2.0) * (1.0 + 700 / (700.0 + f_low_global));
			b2 = (b - b1)/(a - a1);
			c2 = (c - c1)/(a - a1);
			double fc_0 = 0.5 * (-b2 + Math.Sqrt(b2 *b2 - 4 *c2));
			a1 = -0.5 / (700.0 + f_high_global);
			b1 = -700 / (700.0 + f_high_global);
			c1 = (f_high_global / 2.0) * (1.0 + 700 / (700.0 + f_high_global));
			b2 = (b - b1)/(a - a1);
			c2 = (c - c1)/(a - a1);
			double fc_last = 0.5 * (-b2 + Math.Sqrt(b2 *b2 - 4 *c2));
			double fc_0_mel = MelFilter.LinearToMel(fc_0);
			double fc_last_mel = MelFilter.LinearToMel(fc_last);
			double delta_f_mel = (fc_last_mel - fc_0_mel) / (double)(Dtw.MELFILTERS - 1);
			double fc_mel = 0.0;
			double fc = 0.0;
			double ERB = 0.0;
			double f_low = 0.0;
			double f_high;
			
			//filters.reserve(Dtw.MELFILTERS);
			for (int i = 0; i < Dtw.MELFILTERS; i++)
			{
				if (0 == i)
				{
					fc_mel = fc_0_mel;
				}
				else if ((Dtw.MELFILTERS - 1) == i)
				{
					fc_mel = fc_last_mel;
				}
				else
				{
					fc_mel = fc_0_mel + i * delta_f_mel;
				}
				// convert to linear scale, calculate ERB and filter boundaries
				fc = MelFilter.MelToLinear(fc_mel);
				ERB = a * fc * fc + b * fc + c;
				f_low = -(700.0 + ERB) + Math.Sqrt((700.0 + ERB)*(700.0 + ERB) + fc*(fc + 1400));
				f_high = f_low + 2 * ERB;
				
				//filters.push_back(new MelFilter(sampleFrequency));
				filters[i].GenerateFilterSpectrum(f_low, fc, f_high, N);
			}
		}

		/**
		 * Deletes filter objects.
		 */
		public void Dispose()
		{
			for (int i = 0; i < Dtw.MELFILTERS; ++i)
				filters[i] = null;
		}

		/**
		 * Processes frame spectrum through all filters.
		 *
		 * If a filter is enabled, the dot product of filter spectrum and frame
		 * spectrum is computed. If the filter is disabled, 0 is inserted.
		 *
		 * The output vector must be initialized and its size must be MELFILTERS.
		 *
		 * @param frameSpectrum frame spectrum
		 * @param N spectrum size
		 * @param filtersOutput results vector
		 */
		public void ApplyAll(ref Complex[] frameSpectrum, int N, ref double[] filtersOutput)
		{
			// precalculate spectrum magnitude
			List<double> frameAbsSpectrum = new List<double>();
			frameAbsSpectrum.Capacity = N / 2 - 1;
			for (int i = 0; i < N/2 - 1; ++i)
			{
				frameAbsSpectrum.Add(Complex.Abs(frameSpectrum[i]));
			}
			
			for (int i = 0; i < Dtw.MELFILTERS; ++i)
			{
				if (filters[i].IsEnabled())
					filtersOutput[i] = filters[i].Apply(frameAbsSpectrum, N);
				else
					filtersOutput[i] = 0.0;
			}
		}

		/**
		 * Enables only selected filters.
		 *
		 * @param enabledFilters an array representing the selection
		 */
		public void SetEnabledFilters(bool[] enabledFilters)
		{
			if (filters.Count == 0)
				return;
			
			for (int i = 0; i < Dtw.MELFILTERS; ++i)
				filters[i].SetEnabled(enabledFilters[i]);
		}

		/**
		 * Returns sample frequency of all filters.
		 *
		 * @return sample frequency in Hz
		 */
		public double GetSampleFrequency()
		{
			return sampleFreq;
		}

		/**
		 * Returns spectrum size of any filter spectra.
		 *
		 * @return spectrum size
		 */
		public int GetSpectrumLength()
		{
			return N_;
		}
		
		public void DrawMelFiltersBank(string fileName) {
			GraphPane myPane = new GraphPane( new RectangleF( 0, 0, 1200, 600 ),
			                                 "Mel Filter Bank", "X Title", "Y Title" );

			Random random = new Random();
			
			PointPairList ppl = new PointPairList();
			double[] filterSpectrum;
			foreach(var filter in filters) {
				ppl.Clear();
				if (filter.IsEnabled()) {
					filterSpectrum = filter.GetFilterSpectrum();
					for (int i = 0; i < 200; i++) {
						ppl.Add(i, filterSpectrum[i]);
					}
					Color color = Color.FromArgb(random.Next(0, 255), random.Next(0,255),random.Next(0,255));
					LineItem myCurve = myPane.AddCurve("", ppl.Clone(), color, SymbolType.None );
				}
			}

			Bitmap bm = new Bitmap( 1, 1 );
			using ( Graphics g = Graphics.FromImage( bm ) )
				myPane.AxisChange( g );
			
			myPane.GetImage().Save(fileName, ImageFormat.Png);
		}
		
	}
}
