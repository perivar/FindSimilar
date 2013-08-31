using System;

namespace Wavelets.Compress
{
	/// Author: Linzhi Qi
	/// Converted from C++ by perivar@nerseth.com
	/// https://github.com/linzhi/wavelet-compress
	public static class HaarWaveletTransform
	{
		public static double SQRT2 = Math.Sqrt(2.0);
		
		/*
		 * Transform once using 1D haar wavelet
		 * Standard Haar Transform as the behaviour of the JPEG 2000 standard. (I.e. not Tensor)
		 */
		public static void HaarTransform1D(double[] data_input, int size)
		{
			double[] temp_data = new double[size];
			int temp_size = size;
			int i;

			temp_size = temp_size / 2;

			for (i = 0; i < temp_size; i++)
			{
				temp_data[i] = (data_input[2 * i] + data_input[(2 * i) + 1]) / SQRT2;
				temp_data[temp_size + i] = (data_input[2 * i] - data_input[(2 * i) + 1]) / SQRT2;
			}

			for (i = 0; i < size; i++)
			{
				data_input[i] = temp_data[i];
			}

			temp_data = null;
		}

		/*
		 * Inverse transform once using 1D haar wavelet
		 */
		public static void InverseHaarTransform1D(double[] data_input, int size)
		{
			double[] temp_data = new double[size];
			int temp_size = size;
			int i;

			temp_size = temp_size / 2;

			for (i = 0; i < temp_size; i++)
			{
				temp_data[2 * i] = (data_input[i] + data_input[temp_size + i]) / SQRT2;
				temp_data[2 * i + 1] = (data_input[i] - data_input[temp_size + i]) / SQRT2;
			}

			for (i = 0; i < size; i++)
			{
				data_input[i] = temp_data[i];
			}

			temp_data = null;
		}

		/*
		 * Transform once using 2D haar wavelet
		 * Standard Haar Transform as the behaviour of the JPEG 2000 standard. (I.e. not Tensor)
		 */
		public static void HaarTransform2D(double[][] data_input, int height, int width)
		{
			int i;
			int j;
			double[] temp_height = new double[height];
			double[] temp_width = new double[width];

			//haar transform for width
			if (width > 1)
			{
				for (i = 0; i < height; i++)
				{
					for (j = 0; j < width; j++)
						temp_width[j] = data_input[i][j];

					HaarTransform1D(temp_width, width);

					for (j = 0; j < width; j++)
						data_input[i][j] = temp_width[j];
				}
			}

			//haar transform for height
			if (height > 1)
			{
				for (i = 0; i < width; i++)
				{
					for (j = 0; j < height; j++)
						temp_height[j] = data_input[j][i];

					HaarTransform1D(temp_height, height);

					for (j = 0; j < height; j++)
						data_input[j][i] = temp_height[j];
				}
			}

			temp_height = null;
			temp_width = null;
		}

		/*
		 * Inverse transform once using 2D haar wavelet
		 */
		public static void InverseHaarTransform2D(double[][] data_input, int height, int width)
		{
			int i;
			int j;
			double[] temp_height = new double[height];
			double[] temp_width = new double[width];

			//inverse haar transform for height
			if (height > 1)
			{
				for (i = 0; i < width; i++)
				{
					for (j = 0; j < height; j++)
						temp_height[j] = data_input[j][i];

					InverseHaarTransform1D(temp_height, height);

					for (j = 0; j < height; j++)
						data_input[j][i] = temp_height[j];
				}
			}

			// inverse haar transform for width
			if (width > 1)
			{
				for (i = 0; i < height; i++)
				{
					for (j = 0; j < width; j++)
						temp_width[j] = data_input[i][j];

					InverseHaarTransform1D(temp_width, width);

					for (j = 0; j < width; j++)
						data_input[i][j] = temp_width[j];
				}
			}

			temp_height = null;
			temp_width = null;
		}

		/*
		 * Transform once using 3D haar wavelet
		 * Standard Haar Transform as the behaviour of the JPEG 2000 standard. (I.e. not Tensor)
		 */
		public static void HaarTransform3D(double[][][] data_input, int length, int width, int height)
		{
			//in the data_input[i][j][k] k is standard for height, j is standard for long
			// and i is standard for width
			int i;
			int j;
			int k;
			double[] temp_length = new double [length];
			double[] temp_width = new double [width];
			double[] temp_height = new double [height];

			//haar transform for long
			if ((width > 1) && (height > 1))
			{
				for (i = 0; i < height; i++)
				{
					for (j = 0; j < width; j++)
					{
						for (k = 0; k < length; k++)
						{
							temp_length[k] = data_input[k][j][i];
						}

						HaarTransform1D(temp_length, length);

						for (k = 0; k < length; k++)
						{
							data_input[k][j][i] = temp_length[k];
						}
					}
				}
			}

			//haar transform for width
			if ((length > 1) && (height > 1))
			{
				for (i = 0; i < height; i++)
				{
					for (j = 0; j < length; j++)
					{
						for (k = 0; k < width; k++)
						{
							temp_width[k] = data_input[j][k][i];
						}

						HaarTransform1D(temp_width, width);

						for (k = 0; k < width; k++)
						{
							data_input[j][k][i] = temp_width[k];
						}
					}
				}
			}

			//haar transform for height
			if ((length > 1) && (width > 1))
			{
				for (i = 0; i < length; i++)
				{
					for (j = 0; j < width; j++)
					{
						for (k = 0; k < height; k++)
						{
							temp_height[k] = data_input[i][j][k];
						}

						HaarTransform1D(temp_height, height);

						for (k = 0; k < height; k++)
						{
							data_input[i][j][k] = temp_height[k];
						}
					}
				}
			}

			temp_length = null;
			temp_width = null;
			temp_height = null;
		}

		/*
		 * Inverse transform once using 3D haar wavelet
		 */
		public static void InverseHaarTransform3D(double[][][] data_input, int length, int width, int height)
		{
			// in the data_input[i][j][k] k is standard for height, j is standard for long
			// and i is standard for width
			int i;
			int j;
			int k;
			double[] temp_length = new double [length];
			double[] temp_width = new double [width];
			double[] temp_height = new double [height];

			//haar transform for height
			if ((length > 1) && (width > 1))
			{
				for (i = 0; i < length; i++)
				{
					for (j = 0; j < width; j++)
					{
						for (k = 0; k < height; k++)
						{
							temp_height[k] = data_input[i][j][k];
						}

						InverseHaarTransform1D(temp_height, height);

						for (k = 0; k < height; k++)
						{
							data_input[i][j][k] = temp_height[k];
						}
					}
				}
			}

			//haar transfomr for width
			if ((length > 1) && (height > 1))
			{
				for (i = 0; i < height; i++)
				{
					for (j = 0; j < length; j++)
					{
						for (k = 0; k < width; k++)
						{
							temp_width[k] = data_input[j][k][i];
						}

						InverseHaarTransform1D(temp_width, width);

						for (k = 0; k < width; k++)
						{
							data_input[j][k][i] = temp_width[k];
						}
					}
				}
			}

			//haar transform for long
			if ((width > 1) && (height > 1))
			{
				for (i = 0; i < height; i++)
				{
					for (j = 0; j < width; j++)
					{
						for (k = 0; k < length; k++)
						{
							temp_length[k] = data_input[k][j][i];
						}

						InverseHaarTransform1D(temp_length, length);

						for (k = 0; k < length; k++)
						{
							data_input[k][j][i] = temp_length[k];
						}
					}
				}
			}

			temp_length = null;
			temp_width = null;
			temp_height = null;
		}
	}
}