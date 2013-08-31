using System;

namespace Wavelets.Compress
{
	/// Author: Linzhi Qi
	/// Converted from C++ by perivar@nerseth.com
	/// https://github.com/linzhi/wavelet-compress
	public static class WaveletDecompress
	{
		/// <summary>
		/// Decompress a 2D matrix
		/// </summary>
		/// <param name="data_input">data matrix</param>
		/// <param name="level">number of wavelet levels</param>
		/// <param name="firstHeight">first height to be processed</param>
		/// <param name="firstWidth">first width to be processed</param>
		public static void Decompress2D(double[][] data_input, int level, int firstHeight, int firstWidth)
		{
			int temp_level = 0;
			
			while (temp_level < level && firstHeight > 1 && firstWidth > 1)
			{
				if (firstWidth > 1)
					firstWidth = firstWidth * 2;
				if (firstHeight > 1)
					firstHeight = firstHeight * 2;

				HaarWaveletTransform.InverseHaarTransform2D(data_input, firstHeight, firstWidth);

				temp_level++;
			}
		}
		
		/// <summary>
		/// Compress a 3D matrix
		/// </summary>
		/// <param name="data_input">data matrix</param>
		/// <param name="level">number of wavelet levels</param>
		/// <param name="firstLength">first length to be processed</param>
		/// <param name="firstWidth">first width to be processed</param>
		/// <param name="firstHeight">first height to be processed</param>
		public static void Decompress3D(double[][][] data_input, int level, int firstLength, int firstWidth, int firstHeight)
		{
			int temp_level = 0;
			
			while (temp_level < level && firstLength > 1 && firstWidth > 1 && firstHeight > 1)
			{
				if (firstLength > 1)
					firstLength = firstLength * 2;
				if (firstWidth > 1)
					firstWidth = firstWidth * 2;
				if (firstHeight > 1)
					firstHeight = firstHeight * 2;

				HaarWaveletTransform.InverseHaarTransform3D(data_input, firstLength, firstWidth, firstHeight);

				temp_level++;
			}
		}
	}
}

