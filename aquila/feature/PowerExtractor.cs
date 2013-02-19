using System;
using System.Collections.Generic;

/**
 * @file PowerExtractor.cpp
 *
 * Power extraction - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 2.3.0
 */

namespace Aquila
{
	/**
	 * Power extraction class.
	 */
	public class PowerExtractor : Extractor
	{
		/**
		 * Constructor needs only the frame length, paramsPerFrame = 1.
		 *
		 * @param frameLength frame length in milliseconds
		 */
		public PowerExtractor(int frameLength) : base(frameLength, 1)
		{
			type = "Power";
		}

		/**
		 * Deletes the feature object.
		 */
		public new void Dispose()
		{
			base.Dispose();
		}

		/**
		 * Calculates power for each frame.
		 *
		 * @param wav recording object
		 * @param options transform options
		 */
		public override void Process(WaveFile wav, TransformOptions options)
		{
			wavFilename = wav.GetFilename();
			
			int framesCount = wav.GetFramesCount();
			//featureArray.resize(framesCount);
			Array.Resize(ref featureArray, framesCount);
			
			if (m_indicator != null)
				m_indicator.Start(0, framesCount-1);
			
			Transform transform = new Transform(options);
			for (int i = 0; i < framesCount; ++i)
			{
				List<double> @params = new List<double>();
				@params.Add(transform.FramePower(wav.frames[i]));
				featureArray[i] = @params.ToArray();
				
				if (m_indicator != null)
					m_indicator.Progress(i);
			}
			
			if (m_indicator != null)
				m_indicator.Stop();
		}
	}
}
