using System;

namespace Wavelets
{
	public static class Haar
	{
		/** The 1D Haar Transform **/
		public static void Haar1d(double[] vec, int n)
		{
			int i = 0;
			int w = n;
			double[] vecp = new double[n];

			while(w>1)
			{
				w/=2;
				for(i = 0; i < w; i++)
				{
					vecp[i] = (vec[2 * i] + vec[2 * i+1]) / Math.Sqrt(2.0);
					vecp[i+w] = (vec[2 * i] - vec[2 * i+1]) / Math.Sqrt(2.0);
				}

				for(i = 0; i < (w * 2); i++)
					vec[i] = vecp[i];
			}

			vecp = null;
		}

		/** A Modified version of 1D Haar Transform, used by the 2D Haar Transform function **/
		private static void Haar1(double[] vec, int n, int w)
		{
			int i = 0;
			double[] vecp = new double[n];

			w/=2;
			for(i = 0; i < w; i++)
			{
				vecp[i] = (vec[2 * i] + vec[2 * i+1]) / Math.Sqrt(2.0);
				vecp[i+w] = (vec[2 * i] - vec[2 * i +1]) / Math.Sqrt(2.0);
			}

			for(i = 0; i < (w * 2); i++)
				vec[i] = vecp[i];

			vecp = null;
		}

		/** The 2D Haar Transform **/
		public static void Haar2d(double[][] matrix, int rows, int cols)
		{
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

						Haar1(temp_row, cols, w);

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
						
						Haar1(temp_col, rows, h);
						
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
	}
}