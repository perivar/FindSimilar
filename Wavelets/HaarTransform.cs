using System;
using System.Globalization;

namespace Wavelets
{
	/// Author: John Burkardt
	/// Converted from C++ by perivar@nerseth.com
	/// http://people.sc.fsu.edu/~jburkardt/cpp_src/haar/haar.html
	public static class HaarTransform
	{
		public const int INCX = 5;
		public const int SIZE = 256;
		public const int MAX_ITER = 250;
		
		/// <summary>
		/// HAAR_1D computes the Haar transform of a vector.
		/// </summary>
		/// <remarks>
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    14 March 2011
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int N, the dimension of the vector.
		///    Input/output, double X[N], on input, the vector to be transformed.
		///    On output, the transformed vector.
		/// </remarks>
		/// <param name="n">the dimension of the vector.</param>
		/// <param name="x">double X[N], on input, the vector to be transformed. On output, the transformed vector</param>
		public static void haar_1d(int n, double[] x)
		{
			int i;
			int m;
			double s;
			double[] y;

			s = Math.Sqrt (2.0);

			y = new double[n];

			for (i = 0; i < n; i++)
			{
				y[i] = 0.0;
			}

			m = n;

			while (1 < m)
			{
				m = m / 2;
				for (i = 0; i < m; i++)
				{
					y[i] = (x[2 *i] + x[2 *i+1]) / s;
					y[i+m] = (x[2 *i] - x[2 *i+1]) / s;
				}
				for (i = 0; i < m * 2; i++)
				{
					x[i] = y[i];
				}
			}

			y = null;

			return;
		}

		/// <summary>
		/// HAAR_1D_INVERSE computes the inverse Haar transform of a vector.
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    The current version of this function requires that N be a power of 2.
		///    Otherwise, the function will not properly invert the operation of HAAR_1D.
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    14 March 2011
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int N, the dimension of the vector.  For proper calculation,
		///    N must be a power of 2.
		///    Input/output, double X[N], on input, the vector to be transformed.
		///    On output, the transformed vector.
		/// </remarks>
		/// <param name="n">the dimension of the vector.  For proper calculation, N must be a power of 2.</param>
		/// <param name="x">double X[N], on input, the vector to be transformed. On output, the transformed vector.</param>
		public static void haar_1d_inverse(int n, double[] x)
		{
			int i;
			int m;
			double s;
			double[] y;

			s = Math.Sqrt (2.0);

			y = new double[n];

			for (i = 0; i < n; i++)
			{
				y[i] = 0.0;
			}

			m = 1;
			while (m * 2 <= n)
			{
				for (i = 0; i < m; i++)
				{
					y[2 *i] = (x[i] + x[i+m]) / s;
					y[2 *i+1] = (x[i] - x[i+m]) / s;
				}
				for (i = 0; i < m * 2; i++)
				{
					x[i] = y[i];
				}
				m = m * 2;
			}

			y = null;

			return;
		}

		/// <summary>
		/// HAAR_2D computes the Haar transform of an array.
		/// </summary>
		/// <remarks>
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    17 March 2011
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int M, N, the dimensions of the array.
		///    M and N should be powers of 2.
		///    Input/output, double U[M*N], the array to be transformed.
		/// </remarks>
		/// <param name="m">int M, N, the dimensions of the array. M and N should be powers of 2.</param>
		/// <param name="n">int M, N, the dimensions of the array. M and N should be powers of 2.</param>
		/// <param name="u">double U[M*N], the array to be transformed.</param>
		public static void haar_2d(int m, int n, double[] u)
		{
			int i;
			int j;
			int k;
			double s;
			double[] v;

			s = Math.Sqrt (2.0);

			v = new double[m * n];

			for (j = 0; j < n; j++)
			{
				for (i = 0; i < m; i++)
				{
					v[i+j * m] = u[i+j * m];
				}
			}
			
			//  Transform all columns.
			k = m;

			while (1 < k)
			{
				k = k / 2;

				for (j = 0; j < n; j++)
				{
					for (i = 0; i < k; i++)
					{
						v[i +j * m] = (u[2 *i+j * m] + u[2 *i+1+j * m]) / s;
						v[k+i+j * m] = (u[2 *i+j * m] - u[2 *i+1+j * m]) / s;
					}
				}
				
				for (j = 0; j < n; j++)
				{
					for (i = 0; i < 2 * k; i++)
					{
						u[i+j * m] = v[i+j * m];
					}
				}
			}
			
			//  Transform all rows.
			k = n;

			while (1 < k)
			{
				k = k / 2;

				for (j = 0; j < k; j++)
				{
					for (i = 0; i < m; i++)
					{
						v[i+(j)* m] = (u[i+2 *j * m] + u[i+(2 *j+1)* m]) / s;
						v[i+(k+j)* m] = (u[i+2 *j * m] - u[i+(2 *j+1)* m]) / s;
					}
				}

				for (j = 0; j < 2 * k; j++)
				{
					for (i = 0; i < m; i++)
					{
						u[i+j * m] = v[i+j * m];
					}
				}
			}
			v = null;

			return;
		}

		/// <summary>
		/// HAAR_2D_INVERSE inverts the Haar transform of an array.
		/// </summary>
		/// <remarks>
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    17 March 2011
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int M, N, the dimensions of the array.
		///    M and N should be powers of 2.
		///    Input/output, double U[M*N], the array to be transformed.
		/// </remarks>
		/// <param name="m">int M, N, the dimensions of the array. M and N should be powers of 2.</param>
		/// <param name="n">int M, N, the dimensions of the array. M and N should be powers of 2.</param>
		/// <param name="u">double U[M*N], the array to be transformed.</param>
		public static void haar_2d_inverse(int m, int n, double[] u)
		{
			int i;
			int j;
			int k;
			double s;
			double[] v;

			s = Math.Sqrt (2.0);

			v = new double[m * n];

			for (j = 0; j < n; j++)
			{
				for (i = 0; i < m; i++)
				{
					v[i+j * m] = u[i+j * m];
				}
			}
			
			//  Inverse transform of all rows.
			k = 1;

			while (k * 2 <= n)
			{
				for (j = 0; j < k; j++)
				{
					for (i = 0; i < m; i++)
					{
						v[i+(2 *j)* m] = (u[i+j * m] + u[i+(k+j)* m]) / s;
						v[i+(2 *j+1)* m] = (u[i+j * m] - u[i+(k+j)* m]) / s;
					}
				}

				for (j = 0; j < 2 * k; j++)
				{
					for (i = 0; i < m; i++)
					{
						u[i+j * m] = v[i+j * m];
					}
				}
				k = k * 2;
			}
			
			//  Inverse transform of all columns.
			k = 1;

			while (k * 2 <= m)
			{
				for (j = 0; j < n; j++)
				{
					for (i = 0; i < k; i++)
					{
						v[2 *i +j * m] = (u[i+j * m] + u[k+i+j * m]) / s;
						v[2 *i+1+j * m] = (u[i+j * m] - u[k+i+j * m]) / s;
					}
				}

				for (j = 0; j < n; j++)
				{
					for (i = 0; i < 2 * k; i++)
					{
						u[i+j * m] = v[i+j * m];
					}
				}
				k = k * 2;
			}
			v = null;

			return;
		}

		/// <summary>
		/// int_max returns the maximum of two int's.
		/// </summary>
		/// <remarks>
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    13 October 1998
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int I1, I2, are two integers to be compared.
		///    Output, int, the larger of I1 and I2.
		/// </remarks>
		/// <param name="i1">int I1, I2, are two integers to be compared.</param>
		/// <param name="i2">int I1, I2, are two integers to be compared.</param>
		/// <returns>int, the larger of I1 and I2.</returns>
		public static int int_max(int i1, int i2)
		{
			int value;

			if (i2 < i1)
			{
				value = i1;
			}
			else
			{
				value = i2;
			}
			return value;
		}

		/// <summary>
		/// int_min returns the minimum of two int's.
		/// </summary>
		/// <remarks>
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    13 October 1998
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int I1, I2, two integers to be compared.
		///    Output, int int_MIN, the smaller of I1 and I2.
		/// </remarks>
		/// <param name="i1">int I1, I2, two integers to be compared.</param>
		/// <param name="i2">int I1, I2, two integers to be compared.</param>
		/// <returns>int, the smaller of I1 and I2.</returns>
		public static int int_min(int i1, int i2)
		{
			int value;

			if (i1 < i2)
			{
				value = i1;
			}
			else
			{
				value = i2;
			}
			return value;
		}

		/// <summary>
		/// R8MAT_COPY_NEW copies one R8MAT to a "new" R8MAT.
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    An R8MAT is a doubly dimensioned array of R8's, which
		///    may be stored as a vector in column-major order.
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    03 July 2008
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int M, N, the number of rows and columns.
		///    Input, double A1[M*N], the matrix to be copied.
		///    Output, double R8MAT_COPY_NEW[M*N], the copy of A1.
		/// </remarks>
		/// <param name="m">int M, N, the number of rows and columns.</param>
		/// <param name="n">int M, N, the number of rows and columns.</param>
		/// <param name="a1">double A1[M*N], the matrix to be copied.</param>
		/// <returns>double R8MAT_COPY_NEW[M*N], the copy of A1.</returns>
		public static double[] r8mat_copy_new(int m, int n, double[] a1)
		{
			double[] a2;
			int i;
			int j;

			a2 = new double[m * n];

			for (j = 0; j < n; j++)
			{
				for (i = 0; i < m; i++)
				{
					a2[i+j * m] = a1[i+j * m];
				}
			}
			return a2;
		}
		
		/// <summary>
		/// R8MAT_PRINT prints an R8MAT.
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    An R8MAT is a doubly dimensioned array of R8 values, stored as a vector
		///    in column-major order.
		///    Entry A(I,J) is stored as A[I+J* m]
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    10 September 2009
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int M, the number of rows in A.
		///    Input, int N, the number of columns in A.
		///    Input, double A[M*N], the M by N matrix.
		///    Input, string TITLE, a title.
		/// </remarks>
		/// <param name="m">int M, the number of rows in A.</param>
		/// <param name="n">int N, the number of columns in A.</param>
		/// <param name="a">double A[M*N], the M by N matrix.</param>
		/// <param name="title">string TITLE, a title.</param>
		public static void r8mat_print(int m, int n, double[] a, string title)
		{
			r8mat_print_some (m, n, a, 1, 1, m, n, title);
			return;
		}

		/// <summary>
		/// R8MAT_PRINT_SOME prints some of an R8MAT.
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    An R8MAT is a doubly dimensioned array of R8 values, stored as a vector
		///    in column-major order.
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    20 August 2010
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int M, the number of rows of the matrix.
		///    M must be positive.
		///    Input, int N, the number of columns of the matrix.
		///    N must be positive.
		///    Input, double A[M*N], the matrix.
		///    Input, int ILO, JLO, IHI, JHI, designate the first row and
		///    column, and the last row and column to be printed.
		///    Input, string TITLE, a title.
		/// </remarks>
		/// <param name="m">int M, the number of rows of the matrix. M must be positive.</param>
		/// <param name="n">int N, the number of columns of the matrix. N must be positive.</param>
		/// <param name="a">double A[M*N], the matrix.</param>
		/// <param name="ilo">int ILO designate the first row to be printed.</param>
		/// <param name="jlo">int JLO designate the first column to be printed.</param>
		/// <param name="ihi">int IHI, JHI, designate the last row to be printed.</param>
		/// <param name="jhi">int IHI, JHI, designate the last column to be printed.</param>
		/// <param name="title">string TITLE, a title.</param>
		public static void r8mat_print_some(int m, int n, double[] a, int ilo, int jlo, int ihi, int jhi, string title)
		{
			int i;
			int i2hi;
			int i2lo;
			int j;
			int j2hi;
			int j2lo;

			Console.Write("\n");
			Console.Write(title);
			Console.Write("\n");

			if (m <= 0 || n <= 0)
			{
				Console.Write("\n");
				Console.Write("  (None)\n");
				return;
			}
			
			//  Print the columns of the matrix, in strips of 5.
			for (j2lo = jlo; j2lo <= jhi; j2lo = j2lo + INCX)
			{
				j2hi = j2lo + INCX - 1;
				j2hi = int_min (j2hi, n);
				j2hi = int_min (j2hi, jhi);

				Console.Write("\n");
				//  For each column J in the current range...
				//  Write the header.
				Console.Write("{0,-13}", "  Col:");
				for (j = j2lo; j <= j2hi; j++)
				{
					Console.Write("{0,-14}", j - 1);
				}
				Console.Write("\n");
				Console.Write("  Row\n");
				
				//  Determine the range of the rows in this strip.
				i2lo = int_max (ilo, 1);
				i2hi = int_min (ihi, m);

				for (i = i2lo; i <= i2hi; i++)
				{
					//  Print out (up to) 5 entries in row I, that lie in the current strip.
					Console.Write("{0,7}", i - 1);
					Console.Write("{0,-4}", " :");
					for (j = j2lo; j <= j2hi; j++)
					{
						Console.Write("{0,10:N6}", a[i-1+(j-1)* m]);
						Console.Write("{0,4}", "  ");
					}
					Console.Write("\n");
				}
			}

			return;
		}
		
		/// <summary>
		/// R8MAT_UNIFORM_01_NEW returns a unit pseudorandom R8MAT.
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    An R8MAT is a doubly dimensioned array of R8's,  stored as a vector
		///    in column-major order.
		///    This routine implements the recursion
		///      seed = 16807 * seed mod ( 2**31 - 1 )
		///      unif = seed / ( 2**31 - 1 )
		///    The integer arithmetic never requires more than 32 bits,
		///    including a sign bit.
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    03 October 2005
		///  Author:
		///    John Burkardt
		///  Reference:
		///    Paul Bratley, Bennett Fox, Linus Schrage,
		///    A Guide to Simulation,
		///    Springer Verlag, pages 201-202, 1983.
		///    Bennett Fox,
		///    Algorithm 647:
		///    Implementation and Relative Efficiency of Quasirandom
		///    Sequence Generators,
		///    ACM Transactions on Mathematical Software,
		///    Volume 12, Number 4, pages 362-376, 1986.
		///    Philip Lewis, Allen Goodman, James Miller,
		///    A Pseudo-Random Number Generator for the System/360,
		///    IBM Systems Journal,
		///    Volume 8, pages 136-143, 1969.
		///  Parameters:
		///    Input, int M, N, the number of rows and columns.
		///    Input/output, int *SEED, the "seed" value.  Normally, this
		///    value should not be 0, otherwise the output value of SEED
		///    will still be 0, and R8_UNIFORM will be 0.  On output, SEED has
		///    been updated.
		///    Output, double R8MAT_UNIFORM_01_NEW[M*N], a matrix of pseudorandom values.
		/// </remarks>
		/// <param name="m">int M, N, the number of rows and columns.</param>
		/// <param name="n">int M, N, the number of rows and columns.</param>
		/// <param name="seed"></param>
		/// <returns>double R8MAT_UNIFORM_01_NEW[M*N], a matrix of pseudorandom values.</returns>
		public static double[] r8mat_uniform_01_new(int m, int n, ref int seed)
		{
			int i;
			int j;
			int k;
			double[] r;

			r = new double[m * n];

			for (j = 0; j < n; j++)
			{
				for (i = 0; i < m; i++)
				{
					k = seed / 127773;

					seed = 16807 * ( seed - k * 127773) - k * 2836;

					if ( seed < 0)
					{
						seed = seed + 2147483647;
					}
					
					//  Although SEED can be represented exactly as a 32 bit integer,
					//  it generally cannot be represented exactly as a 32 bit real number
					r[i+j * m] = (double)(seed) * 4.656612875E-10;
				}
			}

			return r;
		}

		/// <summary>
		/// R8VEC_COPY_NEW copies an R8VEC to a "new" R8VEC.
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    An R8VEC is a vector of R8's.
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    03 July 2008
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int N, the number of entries in the vectors.
		///    Input, double A1[N], the vector to be copied.
		///    Output, double R8VEC_COPY_NEW[N], the copy of A1.
		/// </remarks>
		/// <param name="n">int N, the number of entries in the vectors.</param>
		/// <param name="a1">double A1[N], the vector to be copied.</param>
		/// <returns>double R8VEC_COPY_NEW[N], the copy of A1.</returns>
		public static double[] r8vec_copy_new(int n, double[] a1)
		{
			double[] a2;
			int i;

			a2 = new double[n];

			for (i = 0; i < n; i++)
			{
				a2[i] = a1[i];
			}
			
			return a2;
		}
		
		/// <summary>
		/// R8VEC_LINSPACE_NEW creates a vector of linearly spaced values.
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    An R8VEC is a vector of R8's.
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    14 March 2011
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int N, the number of entries in the vector.
		///    Input, double A_FIRST, A_LAST, the first and last entries.
		///    Output, double R8VEC_ONES_NEW[N], a vector of linearly spaced data.
		/// </remarks>
		/// <param name="n">int N, the number of entries in the vector.</param>
		/// <param name="a_first">double A_FIRST, the first entries.</param>
		/// <param name="a_last">double A_LAST, the last entries.</param>
		/// <returns>double R8VEC_ONES_NEW[N], a vector of linearly spaced data.</returns>
		public static double[] r8vec_linspace_new(int n, double a_first, double a_last)
		{
			double[] a;
			int i;

			a = new double[n];

			if (n == 1)
			{
				a[0] = (a_first + a_last) / 2.0;
			}
			else
			{
				for (i = 0; i < n; i++)
				{
					a[i] = ((double)(n - 1 - i) * a_first + (double)(i) * a_last) / (double)(n - 1);
				}
			}
			return a;
		}
		
		/// <summary>
		/// R8VEC_ONES_NEW creates a vector of 1's.
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    An R8VEC is a vector of R8's.
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    14 March 2011
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int N, the number of entries in the vector.
		///    Output, double R8VEC_ONES_NEW[N], a vector of 1's.
		/// </remarks>
		/// <param name="n">int N, the number of entries in the vector.</param>
		/// <returns>double R8VEC_ONES_NEW[N], a vector of 1's.</returns>
		public static double[] r8vec_ones_new(int n)
		{
			double[] a;
			int i;

			a = new double[n];

			for (i = 0; i < n; i++)
			{
				a[i] = 1.0;
			}
			return a;
		}

		/// <summary>
		/// R8VEC_TRANSPOSE_PRINT prints an R8VEC "transposed".
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    An R8VEC is a vector of R8's.
		///  Example:
		///    A = (/ 1.0, 2.1, 3.2, 4.3, 5.4, 6.5, 7.6, 8.7, 9.8, 10.9, 11.0 /)
		///    TITLE = 'My vector:  '
		///    My vector:
		///        1.0    2.1    3.2    4.3    5.4
		///        6.5    7.6    8.7    9.8   10.9
		///       11.0
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    12 November 2010
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    Input, int N, the number of components of the vector.
		///    Input, double A[N], the vector to be printed.
		///    Input, string TITLE, a title.
		/// </remarks>
		/// <param name="n">int N, the number of components of the vector.</param>
		/// <param name="a">double A[N], the vector to be printed.</param>
		/// <param name="title">string TITLE, a title.</param>
		public static void r8vec_transpose_print(int n, double[] a, string title)
		{
			int i;
			int ihi;
			int ilo;

			Console.Write("\n");
			Console.Write(title);
			Console.Write("\n");
			Console.Write("\n");

			if (n <= 0)
			{
				Console.Write("  (Empty)\n");
				return;
			}

			for (ilo = 0; ilo < n; ilo = ilo + 5)
			{
				ihi = int_min (ilo + 5, n);
				for (i = ilo; i < ihi; i++)
				{
					Console.Write("  ");
					Console.Write("{0,12}", a[i]);
				}
				Console.Write("{0,12}", "\n");
			}

			return;
		}
		
		/// <summary>
		/// R8VEC_UNIFORM_01_NEW returns a new unit pseudorandom R8VEC.
		/// </summary>
		/// <remarks>
		///  Discussion:
		///    This routine implements the recursion
		///      seed = ( 16807 * seed ) mod ( 2^31 - 1 )
		///      u = seed / ( 2^31 - 1 )
		///    The integer arithmetic never requires more than 32 bits,
		///    including a sign bit.
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    19 August 2004
		///  Author:
		///    John Burkardt
		///  Reference:
		///    Paul Bratley, Bennett Fox, Linus Schrage,
		///    A Guide to Simulation,
		///    Second Edition,
		///    Springer, 1987,
		///    ISBN: 0387964673,
		///    LC: QA76.9.C65.B73.
		///    Bennett Fox,
		///    Algorithm 647:
		///    Implementation and Relative Efficiency of Quasirandom
		///    Sequence Generators,
		///    ACM Transactions on Mathematical Software,
		///    Volume 12, Number 4, December 1986, pages 362-376.
		///    Pierre L'Ecuyer,
		///    Random Number Generation,
		///    in Handbook of Simulation,
		///    edited by Jerry Banks,
		///    Wiley, 1998,
		///    ISBN: 0471134031,
		///    LC: T57.62.H37.
		///    Peter Lewis, Allen Goodman, James Miller,
		///    A Pseudo-Random Number Generator for the System/360,
		///    IBM Systems Journal,
		///    Volume 8, Number 2, 1969, pages 136-143.
		///  Parameters:
		///    Input, int N, the number of entries in the vector.
		///    Input/output, int *SEED, a seed for the random number generator.
		///    Output, double R8VEC_UNIFORM_01_NEW[N], the vector of pseudorandom values.
		/// </remarks>
		/// <param name="n"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		public static double[] r8vec_uniform_01_new(int n, ref int seed)
		{
			int i;
			int int_huge = 2147483647;
			int k;
			double[] r;

			if ( seed == 0)
			{
				Console.Error.Write("R8VEC_UNIFORM_01_NEW - Fatal error!\n");
				Console.Error.Write("  Input value of SEED = 0.\n");
				return null;
			}

			r = new double[n];

			for (i = 0; i < n; i++)
			{
				k = seed / 127773;

				seed = 16807 * ( seed - k * 127773) - k * 2836;

				if ( seed < 0)
				{
					seed = seed + int_huge;
				}

				r[i] = (double)( seed) * 4.656612875E-10;
			}

			return r;
		}
		
		/// <summary>
		/// TIMESTAMP prints the current YMDHMS date as a time stamp.
		/// </summary>
		/// <remarks>
		///  Example:
		///    31 May 2001 09:45:54 AM
		///    02-Jun-2013 19:12:52
		///  Licensing:
		///    This code is distributed under the GNU LGPL license.
		///  Modified:
		///    08 July 2009
		///  Author:
		///    John Burkardt
		///  Parameters:
		///    None
		/// </remarks>
		public static void timestamp()
		{
			DateTime now = DateTime.Now;
			Console.WriteLine(now.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture));
		}
		
		// returns the percentage of coefficients under <amount>
		// Copyright (c) 2003 Emil Mikulic.
		// http://dmr.ath.cx/
		public static double percent_under(double[,] data, double amount)
		{
			int num_thrown = 0;
			int x;
			int y;

			for (y = 0; y < SIZE; y++)
				for (x = 0; x < SIZE; x++)
					if (Math.Abs(data[y, x]) <= amount)
						num_thrown++;

			return (double)(100 * num_thrown) / (double)(SIZE * SIZE);
		}

		// throw away weakest <percentage>% of coefficients
		// Copyright (c) 2003 Emil Mikulic.
		// http://dmr.ath.cx/
		public static void throw_away(double[,] data, double percentage)
		{
			double low;
			double high;
			double thresh = 0;
			double loss;
			int i;
			int j;

			// find max
			low = high = 0.0;
			for (j =0; j < SIZE; j++)
				for (i =0; i < SIZE; i++)
					if (Math.Abs(data[j, i]) > high)
						high = Math.Abs(data[j, i]);

			// binary search
			for (i = 0; i < MAX_ITER; i++)
			{
				thresh = (low+high)/2.0;
				loss = percent_under(data, thresh);

				Console.Write("binary search: " + "iteration={0,4:D}, thresh={1,4:f}, loss={2,3:f2}%\r", i+1, thresh, loss);
				
				if (loss < percentage) {
					low = thresh;
				} else {
					high = thresh;
				}

				if (Math.Abs(loss - percentage) < 0.01)
					i = MAX_ITER;
				if (Math.Abs(low - high) < 0.0000001)
					i = MAX_ITER;
			}

			// zero out anything too low
			for (j = 0; j < SIZE; j++)
				for (i = 0; i < SIZE; i++)
					if (Math.Abs(data[j, i]) < thresh)
						data[j, i] = 0.0;

		}
	}
}