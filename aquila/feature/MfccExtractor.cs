using System;
using System.Numerics;
using System.Collections.Generic;

/**
 * @file MfccExtractor.cpp
 *
 * MFCC feature extraction - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.1
 * @since 0.4.6
 */
namespace Aquila
{
	/**
	 * MFCC feature extractor.
	 */
	public class MfccExtractor : Extractor
	{
		/**
		 * Mel filters bank, static and common to all MFCC extractors.
		 */
		protected static MelFiltersBank filters;

		/**
		 * Selection of enabled Mel filters.
		 */
		protected bool[] enabledFilters;

		/**
		 * Sets frame length and number of parameters per frame.
		 *
		 * @param frameLength frame length in milliseconds
		 * @param paramsPerFrame number of params per frame
		 */
		public MfccExtractor(int frameLength, int paramsPerFrame) : base(frameLength, paramsPerFrame)
		{
			enabledFilters = new bool[0];
			type = "MFCC";
		}

		/**
		 * Deletes the extractor object.
		 */
		public new void Dispose()
		{
			enabledFilters = null;
			base.Dispose();
		}

		/**
		 * Calculates MFCC features for each frame.
		 *
		 * @param wav recording object
		 * @param options transform options
		 */
		public override void Process(WaveFile wav, TransformOptions options)
		{
			wavFilename = wav.GetFilename();
			
			int framesCount = wav.GetFramesCount();
			Array.Resize(ref featureArray, framesCount);
			
			if (m_indicator != null)
				m_indicator.Start(0, framesCount-1);
			
			int N = wav.GetSamplesPerFrameZP();
			UpdateFilters(wav.GetSampleFrequency(), N);
			
			//filters.DrawMelFiltersBank("melfilters.png");
			
			Complex[] frameSpectrum = new Complex[N];
			double[] filtersOutput = new double[Dtw.MELFILTERS];
			double[] frameMfcc = new double[m_paramsPerFrame];
			
			Transform transform = new Transform(options);
			
			// for each frame: FFT -> Mel filtration -> DCT
			for (int i = 0; i < framesCount; ++i)
			{
				transform.Fft(wav.frames[i], ref frameSpectrum);
				filters.ApplyAll(ref frameSpectrum, N, ref filtersOutput);
				transform.Dct(filtersOutput, ref frameMfcc);
				
				//featureArray[i] = frameMfcc;
				featureArray[i] = new double[frameMfcc.Length];
				frameMfcc.CopyTo(featureArray[i], 0);
				
				if (m_indicator != null)
					m_indicator.Progress(i);
			}
			
			if (m_indicator != null)
				m_indicator.Stop();
		}

		/**
		 * Enables only selected Mel filters.
		 */
		public void SetEnabledMelFilters(bool[] enabled)
		{
			enabledFilters = new bool[Dtw.MELFILTERS];
			for (int i = 0; i < Dtw.MELFILTERS; ++i)
			{
				enabledFilters[i] = enabled[i];
			}
		}

		/**
		 * Updates the filter bank.
		 *
		 * (Re)creates new filter bank when sample frequency or spectrum size
		 * changed. If requested, enables only some filters.
		 *
		 * @param frequency sample frequency
		 * @param N spectrum size
		 */
		protected virtual void UpdateFilters(uint frequency, int N)
		{
			if (filters == null)
			{
				filters = new MelFiltersBank(frequency, 200, N);
			}
			else
			{
				if (filters.GetSampleFrequency() != frequency || filters.GetSpectrumLength() != N)
				{
					if (filters != null)
						filters.Dispose();
					filters = new MelFiltersBank(frequency, 200, N);
				}
			}
			
			if (enabledFilters.Length != 0)
				filters.SetEnabledFilters(enabledFilters);
		}
	}
}




