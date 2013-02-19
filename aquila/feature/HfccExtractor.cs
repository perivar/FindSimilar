using System;
using System.Numerics;
using System.Collections.Generic;

/**
 * @file HfccExtractor.cpp
 *
 * HFCC feature extraction - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.1
 * @since 2.5.1
 */
namespace Aquila
{
	/**
	 * HFCC feature extractor, basing on MFCC.
	 */
	public class HfccExtractor : MfccExtractor
	{

		/**
		 * Sets frame length and number of parameters per frame.
		 *
		 * @param frameLength frame length in milliseconds
		 * @param paramsPerFrame number of params per frame
		 */
		public HfccExtractor(int frameLength, int paramsPerFrame) : base(frameLength, paramsPerFrame)
		{
			type = "HFCC";
		}

		/**
		 * Deletes the extractor object.
		 */
		public new void Dispose()
		{
			base.Dispose();
		}

		/**
		 * Calculates HFCC features for each frame.
		 *
		 * @param wav recording object
		 * @param options transform options
		 */
		public new void Process(WaveFile wav, TransformOptions options)
		{
			wavFilename = wav.GetFilename();
			
			int framesCount = wav.GetFramesCount();
			
			//featureArray.resize(framesCount);
			Array.Resize(ref featureArray, framesCount);
			
			if (m_indicator != null)
				m_indicator.Start(0, framesCount-1);
			
			int N = wav.GetSamplesPerFrameZP();
			UpdateFilters(wav.GetSampleFrequency(), N);
			
			List<Complex> frameSpectrum = new List<Complex>(N);
			List<double> filtersOutput = new List<double>(Dtw.MELFILTERS);
			List<double> frameHfcc = new List<double>(m_paramsPerFrame);
			Transform transform = new Transform(options);
			
			// for each frame: FFT -> Mel filtration -> DCT
			for (int i = 0; i < framesCount; ++i)
			{
				transform.Fft(wav.frames[i], ref frameSpectrum);
				hfccFilters.ApplyAll(ref frameSpectrum, N, ref filtersOutput);
				transform.Dct(filtersOutput, ref frameHfcc);
				featureArray[i] = frameHfcc.ToArray();
				
				if (m_indicator != null)
					m_indicator.Progress(i);
			}
			
			if (m_indicator != null)
				m_indicator.Stop();
		}

		/**
		 * Mel filters bank, static and common to all HFCC extractors.
		 */
		protected static MelFiltersBank hfccFilters;

		/**
		 * Updates the filter bank.
		 *
		 * (Re)creates new filter bank when sample frequency or spectrum size
		 * changed. If requested, enables only some filters.
		 *
		 * @param frequency sample frequency
		 * @param N spectrum size
		 */
		protected new void UpdateFilters(uint frequency, int N)
		{
			if (hfccFilters == null)
			{
				hfccFilters = new MelFiltersBank(frequency, N, true);
			}
			else
			{
				if (hfccFilters.GetSampleFrequency() != frequency || hfccFilters.GetSpectrumLength() != N)
				{
					if (hfccFilters != null)
						hfccFilters.Dispose();
					hfccFilters = new MelFiltersBank(frequency, N, true);
				}
			}
			
			if (enabledFilters.Length != 0)
				hfccFilters.SetEnabledFilters(enabledFilters);
		}
	}
}




