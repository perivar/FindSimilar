using System;
using System.Collections.Generic;

/**
 * @file Extractor.cpp
 *
 * Feature extraction interface - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 0.4.6
 */
namespace Aquila
{
	/**
	 * Simple structure to provde header data for readers/writers.
	 */
	public class FeatureHeader
	{
		public FeatureHeader()
		{
			type = "";
			frameLength = 0;
			paramsPerFrame = 0;
			wavFilename = "";
			timestamp = DateTime.Now;
		}

		public string type;
		public int frameLength;
		public int paramsPerFrame;
		public string wavFilename;
		public DateTime timestamp = DateTime.Now;
	}

	/**
	 * Abstract base class - an interface to feature extraction.
	 */
	public abstract class Extractor
	{
		/**
		 * Frame length.
		 */
		protected int m_frameLength;

		/**
		 * Number of params (features) calculated for each frame.
		 */
		protected int m_paramsPerFrame;

		/**
		 * Computed feature array.
		 */
		protected double[][] featureArray;

		/**
		 * Feature type.
		 */
		protected string type;

		/**
		 * Filename of the recording being processed.
		 */
		protected string wavFilename;

		/**
		 * Time when feature data was saved to file.
		 */
		protected DateTime timestamp = DateTime.Now;

		/**
		 * Optional processing indicator.
		 */
		protected ProcessingIndicator m_indicator;
		
		/**
		 * Sets frame length and number of parameters per frame.
		 *
		 * @param frameLength frame length in milliseconds
		 * @param paramsPerFrame number of params per frame
		 */
		public Extractor(int frameLength, int paramsPerFrame)
		{
			m_frameLength = frameLength;
			m_paramsPerFrame = paramsPerFrame;
			type = "";
			m_indicator = null;
		}

		/**
		 * To be reimplemented in derived classes.
		 *
		 * @param wav instance of wave file object
		 * @param options various transformation options
		 */
		public abstract void Process(WaveFile wav, TransformOptions options);

		/**
		 * Saves calculated feature to a writer object (usually to file).
		 *
		 * @param writer non-const reference to a writer object
		 */
		public bool Save(FeatureWriter writer)
		{
			FeatureHeader hdr = new FeatureHeader();
			hdr.type = type;
			hdr.frameLength = m_frameLength;
			hdr.paramsPerFrame = m_paramsPerFrame;
			hdr.wavFilename = wavFilename;
			hdr.timestamp = DateTime.Now;
			
			return writer.Write(hdr, featureArray);
		}

		/**
		 * Reads feature from a reader object.
		 *
		 * @param reader non-const reference to a reader object
		 */
		public bool Read(ref FeatureReader reader)
		{
			FeatureHeader hdr = new FeatureHeader();
			//featureArray.Clear();
			
			if (reader.Read(ref hdr, ref featureArray))
			{
				type = hdr.type;
				m_frameLength = hdr.frameLength;
				m_paramsPerFrame = hdr.paramsPerFrame;
				wavFilename = hdr.wavFilename;
				timestamp = hdr.timestamp;
				
				return true;
			}
			else
			{
				return false;
			}
		}

		/**
		 * Returns frame length of the data.
		 *
		 * @return frame length in milliseconds
		 */
		public int GetFrameLength()
		{
			return m_frameLength;
		}

		/**
		 * Returns number of calculated parameters per frame.
		 *
		 * @return how many feature parameters are computed for each frame
		 */
		public int GetParamsPerFrame()
		{
			return m_paramsPerFrame;
		}

		/**
		 * Returns the type, or name, of the extractor.
		 *
		 * @return feature type
		 */
		public new string GetType()
		{
			return type;
		}

		/**
		 * Returns how many frames are processed.
		 *
		 * @return first dimension of the feature array
		 */
		public int GetFramesCount()
		{
			return featureArray.Length;
		}

		/**
		 * Returns the name of source wave file.
		 *
		 * @return full path to the file
		 */
		public string GetWaveFilename()
		{
			return wavFilename;
		}

		/**
		 * Returns timestamp of feature save.
		 *
		 * @param timestamp at the moment of saving feature file
		 */
		public DateTime GetTimestamp()
		{
			return timestamp;
		}

		/**
		 * Sets an optional processing indicator.
		 *
		 * @param indicator pointer to an indicator object
		 */
		public void SetProcessingIndicator(ProcessingIndicator indicator)
		{
			m_indicator = indicator;
		}

		/**
		 * Returns feature value at given coordinates.
		 *
		 * @param x frame number
		 * @param y param number
		 * @return y-th param in x-th frame
		 */
		public double GetParam(int x, int y)
		{
			return featureArray[x][y];
		}

		/**
		 * Checks whether the two extractor objects are compatible with each other.
		 *
		 * @param other another Extractor object
		 * @return true, if two extractors are compatible
		 */
		public bool IsCompatible(Extractor other)
		{
			return GetType() == other.GetType() && GetParamsPerFrame() == other.GetParamsPerFrame() && GetFrameLength() == other.GetFrameLength();
		}

		/**
		 * Enables access to single feature vector of a given frame.
		 *
		 * @param frame frame number
		 * @return const reference to feature vector
		 */
		public double[] GetVector(int frame)
		{
			return featureArray[frame];
		}

		/**
		 * Deletes the feature object.
		 */
		public void Dispose()
		{
		}
		
	}
}

