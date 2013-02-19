using System;
using System.Collections.Generic;

/**
 * @file EnergyExtractor.cpp
 *
 * Energy extraction - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 2.3.0
 */
namespace Aquila
{
	/**
	 * Energy extraction class.
	 */
	public class EnergyExtractor : Extractor
	{
		/**
		 * Constructor needs only the frame length, paramsPerFrame = 1.
		 *
		 * @param frameLength frame length in milliseconds
		 */
		public EnergyExtractor(int frameLength) : base(frameLength, 1)
		{
			type = "Energy";
		}

		/**
		 * Deletes the feature object.
		 */
		public new void Dispose()
		{
			base.Dispose();
		}

		/**
		 * Calculates energy for each frame.
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
			
			Transform transform = new Transform(options);
			for (int i = 0; i < framesCount; ++i)
			{
				List<double> @params = new List<double>();
				@params.Add(transform.FrameLogEnergy(wav.frames[i]));
				featureArray[i] = @params.ToArray();
				
				if (m_indicator != null)
					m_indicator.Progress(i);
			}
			
			if (m_indicator != null)
				m_indicator.Stop();
		}
	}
}
