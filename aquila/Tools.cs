/**
 * @file Tools.cpp
 *
 * Some utility methods - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2009-2010
 * @version 2.5.0
 * @since 2.2.1
 */

namespace Aquila
{
	/**
	 * Static utility class grouping some functions.
	 */
	public class Tools
	{
		/**
		 * Converts number of a spectral peak to frequency in Hz.
		 *
		 * @param peakNum peak number
		 * @param sampleFrequency sample frequency
		 * @param spectrumSize spectrum size
		 * @return frequency in Hz
		 */
		public static double SpectrumPeakToHz(double peakNum, double sampleFrequency, uint spectrumSize)
		{
			return sampleFrequency * peakNum / spectrumSize;
		}

		/**
		 * Converts frame number to time in milliseconds.
		 *
		 * @param frameNum frame number
		 * @param frameLength frame length in milliseconds
		 * @param frameOverlap frame overlap
		 * @return time from the beginning of the recording in milliseconds
		 */
		public static double FrameNumToMs(double frameNum, int frameLength, double frameOverlap)
		{
			return (1.0 - frameOverlap) * frameLength * frameNum;
		}

		/**
		 * Converts one of the WindowType enumeration values to its name.
		 *
		 * @param type window type as an enum
		 * @return window function name
		 */
		public static string WindowTypeToString(WindowType type)
		{
			switch (type)
			{
				case WindowType.WIN_RECT:
					return "Rectangular";
				case WindowType.WIN_HAMMING:
					return "Hamming";
				case WindowType.WIN_HANN:
					return "Hann";
				case WindowType.WIN_BLACKMAN:
					return "Blackman";
				case WindowType.WIN_BARLETT:
					return "Barlett";
				case WindowType.WIN_FLATTOP:
					return "Flat-top";
			}
			return "Unknown";
		}

		/**
		 * Converts window name to an enumeration value.
		 *
		 * If the name cannot be recognized, WIN_HAMMING is returned.
		 *
		 * @param name window function name
		 * @return one of WindowType values
		 */
		public static WindowType StringToWindowType(string name)
		{
			if ("Rectangular" == name)
			{
				return WindowType.WIN_RECT;
			}
			else if ("Hamming" == name)
			{
				return WindowType.WIN_HAMMING;
			}
			else if ("Hann" == name)
			{
				return WindowType.WIN_HANN;
			}
			else if ("Blackman" == name)
			{
				return WindowType.WIN_BLACKMAN;
			}
			else if ("Barlett" == name)
			{
				return WindowType.WIN_BARLETT;
			}
			else if ("Flat-top" == name)
			{
				return WindowType.WIN_FLATTOP;
			}
			
			return WindowType.WIN_HAMMING;
		}
	}
}
