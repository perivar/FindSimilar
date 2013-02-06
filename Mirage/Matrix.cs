/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA  02110-1301, USA.
 */

using System;
using System.IO;

namespace Mirage
{
	public class MatrixException : Exception
	{
		public MatrixException(string error) : base(error)
		{
		}
	}

	/// <summary>
	/// A Matrix Class
	/// <summary>

	[Serializable]
	public class Matrix
	{
		public float[,] d;
		public int columns;
		public int rows;
		
		/// create a NxM Matrix
		public Matrix(int rows, int columns)
		{
			this.rows = rows;
			this.columns = columns;
			if (columns == -1)
				this.columns = 0;
			
			//Console.WriteLine(rows + " " + columns);
			d = new float[rows, columns];
		}
		
		public Matrix Multiply(Matrix m2)
		{
			if (columns != m2.rows) {
				throw new MatrixException("Can not multiply matrices if dimensions"
				                          + "do not match");
			}
			
			Matrix m1 = this;
			Matrix m3 = new Matrix(m1.rows, m2.columns);

			unsafe {
				int idx;
				int m1rows = m1.rows;
				int m2columns = m2.columns;
				int m3columns = m3.columns;
				int m1columns = m1.columns;
				int i;
				int j;
				int k;
				int im1columns;

				fixed (float* m1d = m1.d, m2d = m2.d, m3d = m3.d) {
					
					for (i = 0; i < m1rows; i++) {
						
						for (j = 0; j < m2columns; j++) {
							idx = i*m3columns + j;
							im1columns = i*m1columns;
							for (k = 0; k < m1columns; k++) {
								m3d[idx] += m1d[im1columns + k] * m2d[k*m2columns + j];
							}
						}
					}
				}
			}
			
			return m3;
		}
		

		public Vector Mean()
		{
			Vector mean = new Vector(rows);
			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < columns; j++) {
					mean.d[i, 0] += d[i, j] / columns;
				}
			}
			return mean;
		}

		public void Print()
		{
			Print(rows, columns);
		}

		public void Print(int rows, int columns)
		{
			System.Console.WriteLine("Rows: " + this.rows +
			                         " Columns: " + this.columns);
			System.Console.WriteLine("[");
			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < columns; j++) {
					System.Console.Write(d[i, j] + " ");
				}
				System.Console.WriteLine(";");
			}
			System.Console.WriteLine("]");
		}

		public void PrintTurn()
		{
			PrintTurn(rows, columns);
		}

		public void PrintTurn(int rows, int columns)
		{
			System.Console.WriteLine("Rows: " + this.rows +
			                         " Columns: " + this.columns);
			System.Console.WriteLine("[");
			float[] max = new float[rows];
			float[] min = new float[rows];
			for (int i = 0; i < columns; i++) {
				for (int j = 0; j < rows; j++) {
					System.Console.Write(d[j, i] + " ");
					if (d[j, i] > max[j]) {
						max[j] = d[j, i];
					}
					if (d[j, i] < min[j]) {
						min[j] = d[j, i];
					}
					
				}
				System.Console.WriteLine(";");
			}
			System.Console.WriteLine("]");
			
			for (int i = 0; i < max.Length; i++) {
				System.Console.WriteLine("max=" + max[i] + " min=" + min[i]);
			}
		}

		public Matrix Covariance(Vector mean)
		{
			Matrix cache = new Matrix(rows, columns);
			float factor = 1.0f/(float)(columns - 1);
			for (int j = 0; j < rows; j++) {
				for (int i = 0; i < columns; i++) {
					cache.d[j, i] = (d[j, i] - mean.d[j, 0]);
				}
			}
			
			Matrix cov = new Matrix(mean.rows, mean.rows);
			for (int i = 0; i < cov.rows; i++) {
				for (int j = 0; j <= i; j++) {
					float sum = 0.0f;
					for (int k = 0; k < columns; k++) {
						sum += cache.d[i, k] * cache.d[j, k];
					}
					sum *= factor;
					cov.d[i, j] = sum;
					if (i == j) {
						continue;
					}
					cov.d[j, i] = sum;
				}
			}

			return cov;
		}

		public void Write(string file)
		{
			BinaryWriter binWriter =
				new BinaryWriter(File.Open(file, FileMode.Create));
			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < columns; j++) {
					binWriter.Write(d[i, j]);
				}
			}
			binWriter.Close();
		}
		
		/* Gauss-Jordan routine, Numerical Recipes. */
		public Matrix Inverse()
		{
			decimal[,] e = new decimal[rows+1, columns+1];
			for (int i = 1; i <= rows; i++) {
				e[i,i] = 1;
			}
			decimal[,] m = new decimal[rows+1, columns+1];
			for (int i = 1; i <= rows; i++) {
				for (int j = 1; j <= columns; j++) {
					m[i, j] = (decimal) d[i-1, j-1];
				}
			}
			
			GaussJordan(ref m, rows, ref e, rows);
			
			// in case of error
			if (m == null)
				return null;
			
			Matrix inv = new Matrix(rows, columns);
			
			for (int i = 1; i <= rows; i++) {
				for (int j = 1; j <= columns; j++) {
					inv.d[i-1, j-1] = (float) m[i, j];
				}
			}
			return inv;
		}

		private void GaussJordan(ref decimal[,] a, int n, ref decimal[,] b, int m)
		{
			int[] indxc = new int[n+1];
			int[] indxr = new int[n+1];
			int[] ipiv = new int[n+1];
			int i, icol = 0, irow = 0, j, k, l, ll;
			decimal big, dum, pivinv, temp;
			
			for (j = 1; j <= n; j++)
				ipiv[j]=0;
			
			for (i = 1 ;i <= n; i++) {
				
				big=0;
				for (j = 1; j <= n; j++) {
					if (ipiv[j] != 1)
						for (k = 1; k <= n; k++) {
						if (ipiv[k] == 0) {
							if (Math.Abs(a[j,k]) >= big) {
								big=Math.Abs(a[j, k]);
								irow=j;
								icol=k;
							}
						} else if (ipiv[k] > 1) {
							//Console.WriteLine("gaussj: Singular Matrix-1");
							a = null;
							return;
						}
					}
				}
				
				ipiv[icol]++;
				if (irow != icol) {
					for (l = 1; l <= n; l++) {
						temp = a[irow,l];
						a[irow,l] = a[icol, l];
						a[icol,l] = temp;
					}
					for (l = 1; l <= m; l++) {
						temp = b[irow,l];
						b[irow,l] = b[icol, l];
						b[icol,l] = temp;
					}
				}
				
				indxr[i] = irow;
				indxc[i] = icol;
				if (a[icol,icol] == 0) {
					Console.WriteLine("gaussj: Singular Matrix-2");
					a = null;
					return;
				}
				
				pivinv = 1 / a[icol,icol];
				a[icol,icol] = 1;
				
				for (l = 1; l <= n; l++)
					a[icol,l] *= pivinv;
				for (l = 1; l <= m; l++)
					b[icol,l] *= pivinv;
				
				for (ll = 1; ll <= n; ll++) {
					if (ll != icol) {
						dum = a[ll,icol];
						a[ll,icol] = 0;
						for (l = 1; l <= n; l++)
							a[ll,l] -= a[icol, l]*dum;
						for (l = 1; l <= m; l++)
							b[ll,l] -= b[icol, l]*dum;
					}
				}
			}
			
			for (l = n; l >= 1; l--) {
				if (indxr[l] != indxc[l])
					for (k = 1; k <= n; k++) {
					temp = a[k,indxr[l]];
					a[k,indxr[l]] = a[k, indxc[l]];
					a[k,indxc[l]] = temp;
				}
			}
		}
	}
}
