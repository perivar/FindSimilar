using System;

namespace Wavelets.Compress
{
	/// Author: Linzhi Qi
	/// Converted from C++ by perivar@nerseth.com
	/// https://github.com/linzhi/wavelet-compress
	public static class WaveletCompress
	{
		
		/// <summary>
		/// Transform using 2D haar wavelet
		/// </summary>
		/// <param name="data_input">data matrix</param>
		/// <param name="level">number of iterations</param>
		/// <param name="lastHeight">return last height processed</param>
		/// <param name="lastWidth">return last width processed</param>
		public static void HaarTransform2D(double[][] data_input, int level, out int lastHeight, out int lastWidth) {
			Compress2D(data_input, level, 0, out lastHeight, out lastWidth, false);
		}
		
		/// <summary>
		/// Compress a 2D matrix
		/// </summary>
		/// <param name="data_input">data matrix</param>
		/// <param name="level">number of wavelet levels</param>
		/// <param name="threshold">threshold where all absolute values less than this is set to zero</param>
		/// <param name="lastHeight">return last height processed</param>
		/// <param name="lastWidth">return last width processed</param>
		public static void Compress2D(double[][] data_input, int level, int threshold, out int lastHeight, out int lastWidth, bool doCompression=true)
		{
			int temp_level = level;

			int ex_height = data_input.Length;
			int ex_width = data_input[0].Length;
			
			int temp_ex_height = ex_height;
			int temp_ex_width = ex_width;

			while (temp_level > 0 && ex_height > 1 && ex_width > 1)
			{
				HaarWaveletTransform.HaarTransform2D(data_input, ex_height, ex_width);

				if (ex_width > 1)
					ex_width = ex_width / 2;
				if (ex_height > 1)
					ex_height = ex_height / 2;

				temp_level--;
			}

			if (doCompression) Quantize.DataQuantize2D(data_input, temp_ex_height, temp_ex_width, threshold);
			
			lastHeight = ex_height;
			lastWidth = ex_width;
		}
		
		/// <summary>
		/// Compress a 3D matrix
		/// </summary>
		/// <param name="data_input">data matrix</param>
		/// <param name="level">number of wavelet levels</param>
		/// <param name="threshold">threshold where all absolute values less than this is set to zero</param>
		public static void Compress3D(double[][][] data_input, int level, int threshold)
		{
			int temp_level = level;

			int ex_length = data_input[0][0].Length;
			int ex_width = data_input[0].Length;
			int ex_height = data_input.Length;
			
			int temp_ex_length = ex_length;
			int temp_ex_width = ex_width;
			int temp_ex_height = ex_height;
			
			while (temp_level > 0 && ex_length > 1 && ex_width > 1 && ex_height > 1)
			{
				HaarWaveletTransform.HaarTransform3D(data_input, ex_length, ex_width, ex_height);

				if (ex_length > 1)
					ex_length = ex_length / 2;
				if (ex_width > 1)
					ex_width = ex_width / 2;
				if (ex_height > 1)
					ex_height = ex_height / 2;

				temp_level--;
			}

			Quantize.DataQuantize3D(data_input, temp_ex_length, temp_ex_width, temp_ex_height, threshold);
		}
	}
}

