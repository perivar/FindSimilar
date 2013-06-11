using System;
using System.Diagnostics.CodeAnalysis;

namespace Wavelets
{
	/// <summary>
	/// Standard Haar wavelet decomposition algorithm.
	/// According to Fast Multi-Resolution Image Query paper, Haar wavelet decomposition with standard basis function works better in image querying
	/// </summary>
	/// <remarks>
	/// Implemented according to the algorithm found here http://grail.cs.washington.edu/projects/wavelets/article/wavelet1.pdf
	/// </remarks>
	public class StandardHaarWaveletDecomposition : HaarWaveletDecomposition
	{
		#region IWaveletDecomposition Members

		/// <summary>
		/// Apply Haar Wavelet decomposition on the image
		/// </summary>
		/// <param name = "image">Image to be decomposed</param>
		public override void DecomposeImageInPlace(double[][] image)
		{
			DecomposeImageNew(image);
		}

		#endregion

		private void Decomposition(double[] array)
		{
			int h = array.Length;

			for (int i = 0; i < h; i++)
			{
				array[i] /= (double)Math.Sqrt(h);
			}

			while (h > 1)
			{
				DecompositionStep(array, h);
				h /= 2;
			}
		}

		/// <summary>
		/// The standard 2-dimensional Haar wavelet decomposition involves one-dimensional decomposition of each row followed by a one-dimensional decomposition of each column of the result.
		/// </summary>
		/// <remarks>Modified by perivar@nerseth.com to be more 'correct' ?!</remarks>
		/// <param name = "image">Image to be decomposed</param>
		private void DecomposeImageNew(double[][] matrix)
		{
			int rows = matrix.Length;
			int cols = matrix[0].Length;

			double[] temp_row = new double[cols];
			double[] temp_col = new double[rows];

			int i = 0;
			int j = 0;
			int w = cols;
			int h = rows;
			while(w > 1 || h > 1)
			{
				if(w > 1)
				{
					for(i = 0; i < h; i++)
					{
						for(j = 0; j < cols; j++)
							temp_row[j] = matrix[i][j];

						DecompositionStep(temp_row, w);

						for(j = 0; j < cols; j++)
							matrix[i][j] = temp_row[j];
					}
				}

				if(h > 1)
				{
					for(i = 0; i < w; i++)
					{
						for(j = 0; j < rows; j++)
							temp_col[j] = matrix[j][i];
						
						DecompositionStep(temp_col, h);
						
						for(j = 0; j < rows; j++)
							matrix[j][i] = temp_col[j];
					}
				}

				if(w > 1)
					w/=2;
				if(h > 1)
					h/=2;
			}

			temp_row = null;
			temp_col = null;
		}
		
		/// <summary>
		/// The standard 2-dimensional Haar wavelet decomposition involves one-dimensional decomposition of each row followed by a one-dimensional decomposition of each column of the result.
		/// </summary>
		/// <remarks>Copied from the Soundfingerprinting project
		/// Copyright © Soundfingerprinting, 2010-2011
		/// ciumac.sergiu@gmail.com
		/// </remarks>
		/// <param name = "image">Image to be decomposed</param>
		private void DecomposeImage(double[][] matrix)
		{
			int rows = matrix.Length; /*128*/
			int cols = matrix[0].Length; /*32*/

			// The order of decomposition is reversed because the matrix is 128x32 but we consider it reversed 32x128
			for (int col = 0; col < cols /*32*/; col++)
			{
				double[] column = new double[rows]; /*Length of each column is equal to number of rows*/
				for (int row = 0; row < rows; row++)
				{
					column[row] = matrix[row][col]; /*Copying Column vector*/
				}

				Decomposition(column); /*Decomposition of each row*/
				for (int row = 0; row < rows; row++)
				{
					matrix[row][col] = column[row];
				}
			}

			for (int row = 0; row < rows /*128*/; row++)
			{
				Decomposition(matrix[row]); /*Decomposition of each row*/
			}
		}
	}
}