using System;
using System.Diagnostics.CodeAnalysis;

namespace Wavelets
{
	/// <summary>
	/// Non-Standard Haar wavelet decomposition
	/// Algorithm impelmented from http://grail.cs.washington.edu/projects/wavelets/article/wavelet1.pdf
	/// According to Fast Multiresolution Image Query, standard Haar wavelet decomposition works better on image querying.
	/// </summary>
	public class NonStandardHaarWaveletDecomposition : HaarWaveletDecomposition
	{
		#region IWaveletDecomposition Members

		public override void DecomposeImageInPlace(double[][] image)
		{
			DecomposeImage(image);
		}

		#endregion

		/// <summary>
		/// Decompose image according to the JPEG 2000 standard (in contrast to the tensor product layout)
		/// </summary>
		/// <param name="matrix"></param>
		private void DecomposeImage(double[][] matrix)
		{
			int rows = matrix.GetLength(0); /*128*/
			int cols = matrix[0].Length; /*32*/
			double[] column = new double[rows];

			int w = cols, h = rows;
			while (w > 1 || h > 1)
			{
				// The order of decomposition is reversed because the image is 128x32 but we consider it reversed 32x128
				// final image does not change even with the reversed processing
				if (h > 1)
				{
					for (int i = 0; i < w; i++)
					{
						for (int j = 0; j < rows; j++)
						{
							column[j] = matrix[j][i];
						}

						DecompositionStep(column, h);

						for (int j = 0; j < rows; j++)
						{
							matrix[j][i] = column[j];
						}
					}
				}

				if (w > 1)
				{
					for (int i = 0; i < h; i++)
					{
						DecompositionStep(matrix[i], w);
					}
				}

				if (w > 1)
				{
					w = w >> 1;
				}

				if (h > 1)
				{
					h = h >> 1;
				}
			}
		}
	}
}