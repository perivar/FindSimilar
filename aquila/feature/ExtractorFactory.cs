using System;
using System.Collections.Generic;

/**
 * @file ExtractorFactory.cpp
 *
 * A factory producing different extractor objects - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.1
 * @since 2.3.0
 */
namespace Aquila
{
	/**
	 * A factory class producing different extractor objects.
	 */
	public class ExtractorFactory
	{
		/**
		 * Returns an extractor object according to parameters.
		 *
		 * Supported types: MFCC, Energy, Power.
		 *
		 * Feature objects are created on heap and must by deleted by caller!
		 *
		 * @param featureType feature type
		 * @param frameLength frame length in milliseconds
		 * @param paramsPerFrame number of features computed for each frame
		 * @throw Aquila::Exception for an undefined feature type
		 * @return pointer to object of one of Extractor-derived classes
		 */
		public static Extractor GetExtractor(string featureType, int frameLength)
		{
			return GetExtractor(featureType, frameLength, 1);
		}

		public static Extractor GetExtractor(string featureType, int frameLength, int paramsPerFrame)
		{
			if ("MFCC" == featureType)
			{
				return new MfccExtractor(frameLength, paramsPerFrame);
			}
			else if ("HFCC" == featureType)
			{
				return new HfccExtractor(frameLength, paramsPerFrame);
			}
			else if ("Energy" == featureType)
			{
				return new EnergyExtractor(frameLength);
			}
			else if ("Power" == featureType)
			{
				return new PowerExtractor(frameLength);
			}
			else
			{
				throw new Exception("Unknown feature type: " + featureType);
			}
		}

		/**
		 * Creates an extractor object according to header information.
		 *
		 * This method is a wrapper for the main getExtractor method.
		 *
		 * @param hdr feature header
		 * @return pointer to object of one of Extractor-derived classes
		 */
		public static Extractor GetExtractor(FeatureHeader hdr)
		{
			return GetExtractor(hdr.type, hdr.frameLength, hdr.paramsPerFrame);
		}
	}
}
