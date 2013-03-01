/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 *
 * Copyright (C) 2007-2008 Dominik Schnitzer <dominik@schnitzer.at>
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
using System.Globalization;

// For drawing graph
using ZedGraph;
using System.Drawing;
using System.Drawing.Imaging;
using CommonUtils;

namespace Mirage
{

	public class MatrixDimensionMismatchException : Exception
	{
	}

	public class MatrixSingularException : Exception
	{
	}

	[Serializable]
	public class Matrix
	{
		public float[,] d;
		public int columns;
		public int rows;

		/// create a NxM Matrix
		public Matrix (int rows, int columns)
		{
			this.rows = rows;
			this.columns = columns;
			d = new float[rows, columns];
		}

		/// <summary>
		/// Construct a matrix from a 2-D array.
		/// </summary>
		/// <param name="A">Two-dimensional array of doubles.</param>
		public Matrix (double[][] jaggedArray) {
			this.rows = jaggedArray.Length;
			this.columns = jaggedArray[0].Length;
			d = new float[rows, columns];
			
			for (int i = 0; i < this.rows; i++) {
				if (jaggedArray[i].Length != this.columns) {
					throw new Exception("All rows must have the same length.");
				}
				for (int j = 0; j < this.columns; j++) {
					d[i,j] = (float) jaggedArray[i][j];
				}
			}
		}
		
		public Matrix Multiply (Matrix m2)
		{
			if (columns != m2.rows) {
				throw new MatrixDimensionMismatchException ();
			}

			Matrix m3 = new Matrix (this.rows, m2.columns);
			int m1rows = this.rows;
			int m2columns = m2.columns;
			int m3columns = m3.columns;
			int m1columns = this.columns;

			unsafe {
				fixed (float* m1d = this.d, m2d = m2.d, m3d = m3.d) {
					for (int i = 0; i < m1rows; i++) {
						for (int j = 0; j < m2columns; j++) {
							int idx = i*m3columns + j;
							int im1columns = i*m1columns;
							for (int k = 0; k < m1columns; k++) {
								m3d[idx] += m1d[im1columns + k] * m2d[k*m2columns + j];
							}
						}
					}
				}
			}

			return m3;
		}

		public Vector Mean ()
		{
			Vector mean = new Vector (rows);
			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < columns; j++) {
					mean.d[i, 0] += d[i, j] / columns;
				}
			}

			return mean;
		}

		public void Print ()
		{
			Print (rows, columns);
		}

		public void Print (int rows, int columns)
		{
			System.Console.WriteLine ("Rows: " + this.rows + " Columns: " + this.columns);
			System.Console.WriteLine ("[");

			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < columns; j++) {
					System.Console.Write (d[i, j] + " ");
				}
				System.Console.WriteLine (";");
			}
			System.Console.WriteLine ("]");

		}

		public void PrintTurn ()
		{
			PrintTurn (rows, columns);
		}

		public void PrintTurn (int rows, int columns)
		{
			System.Console.WriteLine ("Rows: " + this.rows + " Columns: " + this.columns);
			System.Console.WriteLine ("[");

			for (int i = 0; i < columns; i++) {
				for (int j = 0; j < rows; j++) {
					System.Console.Write (d[j, i] + " ");
				}
				System.Console.WriteLine (";");
			}
			System.Console.WriteLine ("]");
		}

		public Matrix Covariance (Vector mean)
		{
			Matrix cache = new Matrix (rows, columns);
			float factor = 1.0f/(float)(columns - 1);
			for (int j = 0; j < rows; j++) {
				for (int i = 0; i < columns; i++) {
					cache.d[j, i] = (d[j, i] - mean.d[j, 0]);
				}
			}

			Matrix cov = new Matrix (mean.rows, mean.rows);
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

		public void Write (string file)
		{
			using (var binWriter = new BinaryWriter (File.Open (file, FileMode.Create))) {
				binWriter.Write (rows);
				binWriter.Write (columns);

				for (int i = 0; i < rows; i++) {
					for (int j = 0; j < columns; j++) {
						binWriter.Write (d[i, j]);
					}
				}
			}
		}

		public static Matrix Load (Stream filestream)
		{
			using (var binReader = new BinaryReader (filestream)) {
				int rows = binReader.ReadInt32 ();
				int columns = binReader.ReadInt32 ();
				Matrix m = new Matrix (rows, columns);

				for (int i = 0; i < rows; i++) {
					for (int j = 0; j < columns; j++) {
						m.d[i, j] = binReader.ReadSingle();
					}
				}

				return m;
			}
		}

		// Gauss-Jordan routine to invert a matrix
		// decimal precision
		public Matrix Inverse ()
		{
			decimal[,] e = new decimal[rows+1, columns+1];
			for (int i = 1; i <= rows; i++) {
				e[i,i] = 1;
			}
			decimal[,] m = new decimal[rows+1, columns+1];
			for (int i = 1; i <= rows; i++) {
				for (int j = 1; j <= columns; j++) {
					if (!float.IsNaN(d[i-1, j-1]))
						m[i, j] = (decimal) d[i-1, j-1];
				}
			}

			GaussJordan (ref m, rows, ref e, rows);
			Matrix inv = new Matrix(rows, columns);

			for (int i = 1; i <= rows; i++) {
				for (int j = 1; j <= columns; j++) {
					inv.d[i-1, j-1] = (float) m[i, j];
				}
			}

			return inv;
		}

		private void GaussJordan (ref decimal [,] a, int n, ref decimal [,] b, int m)
		{
			int [] indxc = new int[n+1];
			int [] indxr = new int[n+1];
			int [] ipiv = new int[n+1];
			int i, icol = 0, irow = 0, j, k, l, ll;
			decimal big, dum, pivinv, temp;

			for (j = 1; j <= n; j++) {
				ipiv[j] = 0;
			}

			for (i = 1; i <= n; i++) {
				big = 0;
				for (j = 1; j <= n; j++) {
					if (ipiv[j] != 1) {
						for (k = 1; k <= n; k++) {
							if (ipiv[k] == 0) {
								if (Math.Abs(a[j,k]) >= big) {
									big=Math.Abs(a[j, k]);
									irow=j;
									icol=k;
								}
							} else if (ipiv[k] > 1) {
								Dbg.WriteLine("Mirage - Gauss/Jordan Singular Matrix (1)");
								throw new MatrixSingularException();
							}
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
					Dbg.WriteLine ("Mirage - Gauss/Jordan Singular Matrix (2)");
					throw new MatrixSingularException();
				}

				pivinv = 1 / a[icol,icol];
				a[icol,icol] = 1;

				for (l = 1; l <= n; l++) {
					a[icol,l] *= pivinv;
				}

				for (l = 1; l <= m; l++) {
					b[icol,l] *= pivinv;
				}

				for (ll = 1; ll <= n; ll++) {
					if (ll != icol) {
						dum = a[ll,icol];
						a[ll,icol] = 0;

						for (l = 1; l <= n; l++) {
							a[ll,l] -= a[icol, l]*dum;
						}

						for (l = 1; l <= m; l++) {
							b[ll,l] -= b[icol, l]*dum;
						}
					}
				}
			}

			for (l = n; l >= 1; l--) {
				if (indxr[l] != indxc[l]) {
					for (k = 1; k <= n; k++) {
						temp = a[k,indxr[l]];
						a[k,indxr[l]] = a[k, indxc[l]];
						a[k,indxc[l]] = temp;
					}
				}
			}
		}
		
		/// <summary>Writes the Matrix to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');</summary>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\matrix.ascii"</param>
		public void WriteAscii(string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i< rows; i++)
			{
				for(int j = 0; j < columns; j++)
				{
					pw.Write("{0:#.0000000e+000} ", d[i,j]);
				}
				pw.Write("\r");
			}
			pw.Close();
		}
		
		/// <summary>
		/// Write matrix to file
		/// </summary>
		/// <param name="filename"></param>
		public void WriteText(string filename)
		{
			TextWriter output = File.CreateText(filename);
			NumberFormatInfo format = new CultureInfo("en-US", false).NumberFormat;
			format.NumberDecimalDigits = 5;
			
			output.WriteLine(); // start on new line.
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					string s = d[i,j].ToString("F", format); // format the number
					output.Write(s.PadRight(20));
				}
				output.WriteLine();
			}
			output.WriteLine(); // end with blank line.
			
			output.Flush();
			output.Close();
		}
		
		public void DrawMatrixImage(string fileName, bool useColumnAsXCoordinate=true) {
			
			GraphPane myPane;
			RectangleF rect = new RectangleF( 0, 0, 1200, 600 );
			
			PointPairList ppl = new PointPairList();
			if (columns == 1) {
				myPane = new GraphPane( rect, "Matrix", "Rows", "Value" );
				for(int i = 0; i < rows; i++) {
					ppl.Add(i, d[i,0]);
				}
				LineItem myCurve = myPane.AddCurve("", ppl.Clone(), Color.Black, SymbolType.None);
			} else if (rows == 1) {
				myPane = new GraphPane( rect, "Matrix", "Columns", "Value" );
				for(int i = 0; i < columns; i++) {
					ppl.Add(i, d[0,i]);
				}
				LineItem myCurve = myPane.AddCurve("", ppl.Clone(), Color.Black, SymbolType.None);
			} else if (columns > rows) {
				myPane = new GraphPane( rect, "Matrix", "Columns", "Value" );
				for(int i = 0; i < rows; i++)
				{
					ppl.Clear();
					for(int j = 0; j < columns; j++)
					{
						if (useColumnAsXCoordinate) {
							ppl.Add(j, d[i,j]);
						} else {
							ppl.Add(i, d[i,j]);
						}
					}
					Color color = ColorUtils.MatlabGraphColor(i);
					LineItem myCurve = myPane.AddCurve("", ppl.Clone(), color, SymbolType.None);
				}
			} else { // (columns < rows)
				myPane = new GraphPane( rect, "Matrix", "Rows", "Value" );
				for(int i = 0; i < rows; i++)
				{
					ppl.Clear();
					for(int j = 0; j < columns; j++)
					{
						if (useColumnAsXCoordinate) {
							ppl.Add(j, d[i,j]);
						} else {
							ppl.Add(i, d[i,j]);
						}
					}
					Color color = ColorUtils.MatlabGraphColor(i);
					LineItem myCurve = myPane.AddCurve("", ppl.Clone(), color, SymbolType.None);
				}
			}

			Bitmap bm = new Bitmap( 1, 1 );
			using ( Graphics g = Graphics.FromImage( bm ) )
				myPane.AxisChange( g );
			
			myPane.GetImage().Save(fileName, ImageFormat.Png);
		}
		
		public Comirva.Audio.Util.Maths.Matrix GetComirvaMatrix() {
			double[][] matrixData = new double[rows][];
			
			for (int i = 0; i < rows; i++) {
				matrixData[i] = new double[columns];
				for (int j = 0; j < columns; j++) {
					matrixData[i][j] = d[i, j];
				}
			}
			
			return new Comirva.Audio.Util.Maths.Matrix(matrixData);
		}
	}
}