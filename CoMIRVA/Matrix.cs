using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

using System.Linq;
using System.Xml;
using System.Xml.Linq;

using CommonUtils;

namespace Comirva.Audio.Util.Maths
{
	/// <summary>
	/// CoMIRVA: Collection of Music Information Retrieval and Visualization Applications
	/// Ported from Java to C# by perivar@nerseth.com
	/// </summary>

	///   Jama = Java Matrix class.
	///<P>
	///   The Java Matrix Class provides the fundamental operations of numerical
	///   linear algebra.  Various constructors create Matrices from two dimensional
	///   arrays of double precision floating point numbers.  Various "gets" and
	///   "sets" provide access to submatrices and matrix elements.  Several methods
	///   implement basic matrix arithmetic, including matrix addition and
	///   multiplication, matrix norms, and element-by-element array operations.
	///   Methods for reading and printing matrices are also included.  All the
	///   operations in this version of the Matrix Class involve real matrices.
	///   Complex matrices may be handled in a future version.
	///<P>
	///   Five fundamental matrix decompositions, which consist of pairs or triples
	///   of matrices, permutation vectors, and the like, produce results in five
	///   decomposition classes.  These decompositions are accessed by the Matrix
	///   class to compute solutions of simultaneous linear equations, determinants,
	///   inverses and other matrix functions.  The five decompositions are:
	///<P><UL>
	///   <LI>Cholesky Decomposition of symmetric, positive definite matrices.
	///   <LI>LU Decomposition of rectangular matrices.
	///   <LI>QR Decomposition of rectangular matrices.
	///   <LI>Singular Value Decomposition of rectangular matrices.
	///   <LI>Eigenvalue Decomposition of both symmetric and nonsymmetric square matrices.
	///</UL>
	///<DL>
	///<DT><B>Example of use:</B></DT>
	///<P>
	///<DD>Solve a linear system A x = b and compute the residual norm, ||b - A x||.
	///<P><PRE>
	///      double[][] vals = {{1.,2.,3},{4.,5.,6.},{7.,8.,10.}};
	///      Matrix A = new Matrix(vals);
	///      Matrix b = Matrix.random(3,1);
	///      Matrix x = A.solve(b);
	///      Matrix r = A.times(x).minus(b);
	///      double rnorm = r.normInf();
	///</PRE></DD>
	///</DL>
	///@author The MathWorks, Inc. and the National Institute of Standards and Technology.
	///@version 5 August 1998
	public class Matrix
	{
		// ------------------------
		//   Class variables
		// ------------------------

		// Array for internal storage of elements.
		private double[][] A;

		//Number of rows.
		private int m;

		//Number of columns.
		private int n;

		// ------------------------
		//   Constructors
		// ------------------------

		/// <summary>Construct an m-by-n matrix of zeros.</summary>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		public Matrix (int m, int n) {
			this.m = m;
			this.n = n;
			A = new double[m][];
			for (int i=0;i<m;i++)
				A[i] = new double[n];
		}

		/// <summary>Construct an m-by-n constant matrix.</summary>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		/// <param name="s">Fill the matrix with this scalar value.</param>
		public Matrix (int m, int n, double s) {
			this.m = m;
			this.n = n;
			A = new double[m][];
			for (int i = 0; i < m; i++)
			{
				A[i] = new double[n];
				for (int j = 0; j < n; j++) {
					A[i][j] = s;
				}
			}
		}

		/// <summary>Construct a matrix from a 2-D array.</summary>
		/// <param name="A">Two-dimensional array of doubles.</param>
		/// <exception cref="">ArgumentException All rows must have the same length</exception>
		/// <seealso cref="">#constructWithCopy</seealso>
		public Matrix (double[][] A)
		{
			m = A.Length;
			n = A[0].Length;
			for (int i = 0; i < m; i++)
			{
				if (A[i].Length != n)
				{
					throw new ArgumentException("All rows must have the same length.");
				}
			}
			this.A = A;
		}

		/// <summary>Construct a matrix quickly without checking arguments.</summary>
		/// <param name="A">Two-dimensional array of doubles.</param>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		public Matrix (double[][] A, int m, int n)
		{
			this.A = A;
			this.m = m;
			this.n = n;
		}

		/// <summary>Construct a matrix from a one-dimensional packed array</summary>
		/// <param name="vals">One-dimensional array of doubles, packed by columns (ala Fortran).</param>
		/// <param name="m">Number of rows.</param>
		/// <exception cref="">ArgumentException Array length must be a multiple of m.</exception>
		public Matrix (double[] vals, int m)
		{
			this.m = m;
			n = (m != 0 ? vals.Length/m : 0);
			if (m*n != vals.Length)
			{
				throw new ArgumentException("Array length must be a multiple of m.");
			}

			A = new double[m][];
			for (int i = 0; i < m; i++)
			{
				A[i] = new double[n];
				for (int j = 0; j < n; j++)
				{
					A[i][j] = vals[i+j*m];
				}
			}
		}

		// ------------------------
		// Public Methods
		// ------------------------

		/// <summary>Construct a matrix from a copy of a 2-D array.</summary>
		/// <param name="A">Two-dimensional array of doubles.</param>
		/// <exception cref="">ArgumentException All rows must have the same length</exception>
		public static Matrix ConstructWithCopy(double[][] A)
		{
			int m = A.Length;
			int n = A[0].Length;
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				if (A[i].Length != n)
				{
					throw new ArgumentException ("All rows must have the same length.");
				}
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j];
				}
			}
			return X;
		}

		/// Make a deep copy of a matrix
		public Matrix Copy ()
		{
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j];
				}
			}
			return X;
		}

		/// Clone the Matrix object.
		public object Clone()
		{
			return this.Copy();
		}

		/// Access the internal two-dimensional array.
		/// <returns>Pointer to the two-dimensional array of matrix elements.</returns>
		public double[][] GetArray ()
		{
			return A;
		}

		/// <summary>Copy the internal two-dimensional array.</summary>
		/// <returns>Two-dimensional array copy of matrix elements.</returns>
		public double[][] GetArrayCopy ()
		{
			double[][] C = new double[m][];
			for (int i = 0; i < m; i++)
			{
				C[i] = new double[n];
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j];
				}
			}
			return C;
		}

		/// <summary>Make a one-dimensional column packed copy of the internal array.</summary>
		/// <returns>Matrix elements packed in a one-dimensional array by columns.</returns>
		public double[] GetColumnPackedCopy ()
		{
			double[] vals = new double[m*n];
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					vals[i+j*m] = A[i][j];
				}
			}
			return vals;
		}

		/// <summary>Make a one-dimensional row packed copy of the internal array.</summary>
		/// <returns>Matrix elements packed in a one-dimensional array by rows.</returns>
		public double[] GetRowPackedCopy ()
		{
			double[] vals = new double[m*n];
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					vals[i*n+j] = A[i][j];
				}
			}
			return vals;
		}

		/// <summary>Get row dimension.</summary>
		/// <returns>m, the number of rows.</returns>
		public int GetRowDimension ()
		{
			return m;
		}

		/// <summary>Get column dimension.</summary>
		/// <returns>n, the number of columns.</returns>
		public int GetColumnDimension ()
		{
			return n;
		}

		/// <summary>Get a single element.</summary>
		/// <param name="i">Row index.</param>
		/// <param name="j">Column index.</param>
		/// <returns>A(i,j)</returns>
		/// <exception cref="">IndexOutOfRangeException</exception>
		public double Get(int i, int j)
		{
			return A[i][j];
		}

		/// <summary>Get a submatrix.</summary>
		/// <param name="i0">Initial row index</param>
		/// <param name="i1">Final row index</param>
		/// <param name="j0">Initial column index</param>
		/// <param name="j1">Final column index</param>
		/// <returns>A(i0:i1,j0:j1)</returns>
		/// <exception cref="">IndexOutOfRangeException Submatrix indices</exception>
		public Matrix GetMatrix (int i0, int i1, int j0, int j1) {
			Matrix X = new Matrix(i1-i0+1,j1-j0+1);
			double[][] B = X.GetArray();
			try {
				for (int i = i0; i <= i1; i++) {
					for (int j = j0; j <= j1; j++) {
						B[i-i0][j-j0] = A[i][j];
					}
				}
			} catch(Exception) {
				throw new Exception("Submatrix indices");
			}
			return X;
		}

		/// <summary>Get a submatrix.</summary>
		/// <param name="r">Array of row indices.</param>
		/// <param name="c">Array of column indices.</param>
		/// <returns>A(r(:),c(:))</returns>
		/// <exception cref="">IndexOutOfRangeException Submatrix indices</exception>
		public Matrix GetMatrix (int[] r, int[] c) {
			Matrix X = new Matrix(r.Length,c.Length);
			double[][] B = X.GetArray();
			try {
				for (int i = 0; i < r.Length; i++) {
					for (int j = 0; j < c.Length; j++) {
						B[i][j] = A[r[i]][c[j]];
					}
				}
			} catch(Exception) {
				throw new Exception("Submatrix indices");
			}
			return X;
		}

		/// <summary>Get a submatrix.</summary>
		/// <param name="i0">Initial row index</param>
		/// <param name="i1">Final row index</param>
		/// <param name="c">Array of column indices.</param>
		/// <returns>A(i0:i1,c(:))</returns>
		/// <exception cref="">IndexOutOfRangeException Submatrix indices</exception>
		public Matrix GetMatrix (int i0, int i1, int[] c) {
			Matrix X = new Matrix(i1-i0+1,c.Length);
			double[][] B = X.GetArray();
			try {
				for (int i = i0; i <= i1; i++) {
					for (int j = 0; j < c.Length; j++) {
						B[i-i0][j] = A[i][c[j]];
					}
				}
			} catch(Exception) {
				throw new Exception("Submatrix indices");
			}
			return X;
		}

		/// <summary>Get a submatrix.</summary>
		/// <param name="r">Array of row indices.</param>
		/// <param name="j0">Initial column index</param>
		/// <param name="j1">Final column index</param>
		/// <returns>A(r(:),j0:j1)</returns>
		/// <exception cref="">IndexOutOfRangeException Submatrix indices</exception>
		public Matrix GetMatrix (int[] r, int j0, int j1) {
			Matrix X = new Matrix(r.Length,j1-j0+1);
			double[][] B = X.GetArray();
			try {
				for (int i = 0; i < r.Length; i++) {
					for (int j = j0; j <= j1; j++) {
						B[i][j-j0] = A[r[i]][j];
					}
				}
			} catch(Exception) {
				throw new Exception("Submatrix indices");
			}
			return X;
		}

		/// <summary>Set a single element.</summary>
		/// <param name="i">Row index.</param>
		/// <param name="j">Column index.</param>
		/// <param name="s">A(i,j).</param>
		/// <exception cref="">IndexOutOfRangeException</exception>
		public void Set(int i, int j, double s)
		{
			A[i][j] = s;
		}

		/// <summary>Set a submatrix.</summary>
		/// <param name="i0">Initial row index</param>
		/// <param name="i1">Final row index</param>
		/// <param name="j0">Initial column index</param>
		/// <param name="j1">Final column index</param>
		/// <param name="X">A(i0:i1,j0:j1)</param>
		/// <exception cref="">Exception Submatrix indices</exception>
		public void SetMatrix (int i0, int i1, int j0, int j1, Matrix X) {
			try {
				for (int i = i0; i <= i1; i++) {
					for (int j = j0; j <= j1; j++) {
						A[i][j] = X.Get(i-i0,j-j0);
					}
				}
			} catch(Exception) {
				throw new Exception("Submatrix indices");
			}
		}

		/// <summary>Set a submatrix.</summary>
		/// <param name="r">Array of row indices.</param>
		/// <param name="c">Array of column indices.</param>
		/// <param name="X">A(r(:),c(:))</param>
		/// <exception cref="">Exception Submatrix indices</exception>
		public void SetMatrix (int[] r, int[] c, Matrix X) {
			try {
				for (int i = 0; i < r.Length; i++) {
					for (int j = 0; j < c.Length; j++) {
						A[r[i]][c[j]] = X.Get(i,j);
					}
				}
			} catch(Exception) {
				throw new Exception("Submatrix indices");
			}
		}

		/// <summary>Set a submatrix.</summary>
		/// <param name="r">Array of row indices.</param>
		/// <param name="j0">Initial column index</param>
		/// <param name="j1">Final column index</param>
		/// <param name="X">A(r(:),j0:j1)</param>
		/// <exception cref="">Exception Submatrix indices</exception>
		public void SetMatrix (int[] r, int j0, int j1, Matrix X) {
			try {
				for (int i = 0; i < r.Length; i++) {
					for (int j = j0; j <= j1; j++) {
						A[r[i]][j] = X.Get(i,j-j0);
					}
				}
			} catch(Exception) {
				throw new Exception("Submatrix indices");
			}
		}

		/// <summary>Set a submatrix.</summary>
		/// <param name="i0">Initial row index</param>
		/// <param name="i1">Final row index</param>
		/// <param name="c">Array of column indices.</param>
		/// <param name="X">A(i0:i1,c(:))</param>
		/// <exception cref="">Exception Submatrix indices</exception>
		public void SetMatrix (int i0, int i1, int[] c, Matrix X) {
			try {
				for (int i = i0; i <= i1; i++) {
					for (int j = 0; j < c.Length; j++) {
						A[i][c[j]] = X.Get(i-i0,j);
					}
				}
			} catch(Exception) {
				throw new Exception("Submatrix indices");
			}
		}

		/// <summary>Matrix transpose.</summary>
		/// <returns>A'</returns>
		public Matrix Transpose ()
		{
			Matrix X = new Matrix(n,m);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[j][i] = A[i][j];
				}
			}
			return X;
		}

		/// <summary>One norm</summary>
		/// <returns>maximum column sum.</returns>
		public double Norm1 ()
		{
			double f = 0;
			for (int j = 0; j < n; j++)
			{
				double s = 0;
				for (int i = 0; i < m; i++)
				{
					s += Math.Abs(A[i][j]);
				}
				f = Math.Max(f,s);
			}
			return f;
		}

		/// <summary>Two norm</summary>
		/// <returns>maximum singular value.</returns>
		public double Norm2 ()
		{
			return (new SingularValueDecomposition(this).Norm2());
		}

		/// <summary>Infinity norm</summary>
		/// <returns>maximum row sum.</returns>
		public double NormInf ()
		{
			double f = 0;
			for (int i = 0; i < m; i++)
			{
				double s = 0;
				for (int j = 0; j < n; j++)
				{
					s += Math.Abs(A[i][j]);
				}
				f = Math.Max(f,s);
			}
			return f;
		}

		/// <summary>Frobenius norm</summary>
		/// <returns>sqrt of sum of squares of all elements.</returns>
		public double NormF ()
		{
			double f = 0;
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					f = MathUtils.Hypot(f,A[i][j]);
				}
			}
			return f;
		}

		/// <summary>Unary minus</summary>
		/// <returns>-A</returns>
		public Matrix Uminus ()
		{
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = -A[i][j];
				}
			}
			return X;
		}

		/// <summary>C = A + B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A + B</returns>
		public Matrix Plus (Matrix B)
		{
			CheckMatrixDimensions(B);
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j] + B.A[i][j];
				}
			}
			return X;
		}

		/// <summary>A = A + B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A + B</returns>
		public Matrix PlusEquals (Matrix B)
		{
			CheckMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = A[i][j] + B.A[i][j];
				}
			}
			return this;
		}

		/// <summary>C = A - B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A - B</returns>
		public Matrix Minus (Matrix B)
		{
			CheckMatrixDimensions(B);
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j] - B.A[i][j];
				}
			}
			return X;
		}

		/// <summary>A = A - B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A - B</returns>
		public Matrix MinusEquals (Matrix B)
		{
			CheckMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = A[i][j] - B.A[i][j];
				}
			}
			return this;
		}

		/// <summary>Element-by-element multiplication, C = A.*B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A.*B</returns>
		public Matrix ArrayTimes (Matrix B)
		{
			CheckMatrixDimensions(B);
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j] * B.A[i][j];
				}
			}
			return X;
		}

		/// <summary>Element-by-element multiplication in place, A = A.*B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A.*B</returns>
		public Matrix ArrayTimesEquals (Matrix B)
		{
			CheckMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = A[i][j] * B.A[i][j];
				}
			}
			return this;
		}

		/// <summary>Element-by-element right division, C = A./B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A./B</returns>
		public Matrix ArrayRightDivide (Matrix B)
		{
			CheckMatrixDimensions(B);
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j] / B.A[i][j];
				}
			}
			return X;
		}

		/// <summary>Element-by-element right division in place, A = A./B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A./B</returns>
		public Matrix ArrayRightDivideEquals (Matrix B)
		{
			CheckMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = A[i][j] / B.A[i][j];
				}
			}
			return this;
		}

		/// <summary>Element-by-element left division, C = A.\B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A.\B</returns>
		public Matrix ArrayLeftDivide (Matrix B)
		{
			CheckMatrixDimensions(B);
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = B.A[i][j] / A[i][j];
				}
			}
			return X;
		}

		/// <summary>Element-by-element left division in place, A = A.\B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>A.\B</returns>
		public Matrix ArrayLeftDivideEquals (Matrix B)
		{
			CheckMatrixDimensions(B);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = B.A[i][j] / A[i][j];
				}
			}
			return this;
		}

		/// <summary>Multiply a matrix by a scalar, C = s*A</summary>
		/// <param name="s">scalar</param>
		/// <returns>s*A</returns>
		public Matrix Times (double s)
		{
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = s*A[i][j];
				}
			}
			return X;
		}

		/// <summary>Multiply a matrix by a scalar in place, A = s*A</summary>
		/// <param name="s">scalar</param>
		/// <returns>replace A by s*A</returns>
		public Matrix TimesEquals (double s)
		{
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					A[i][j] = s*A[i][j];
				}
			}
			return this;
		}

		/// <summary>Linear algebraic matrix multiplication, A * B</summary>
		/// <param name="B">another matrix</param>
		/// <returns>Matrix product, A * B</returns>
		/// <exception cref="">ArgumentException Matrix inner dimensions must agree.</exception>
		public Matrix Times (Matrix B)
		{
			if (B.m != n)
			{
				throw new ArgumentException("Matrix inner dimensions must agree.");
			}
			Matrix X = new Matrix(m,B.n);
			double[][] C = X.GetArray();
			double[] Bcolj = new double[n];
			for (int j = 0; j < B.n; j++)
			{
				for (int k = 0; k < n; k++)
				{
					Bcolj[k] = B.A[k][j];
				}
				for (int i = 0; i < m; i++)
				{
					double[] Arowi = A[i];
					double s = 0;
					for (int k = 0; k < n; k++)
					{
						s += Arowi[k]*Bcolj[k];
					}
					C[i][j] = s;
				}
			}
			return X;
		}

		/// <summary>Linear algebraic matrix multiplication, A * B
		/// B being a triangular matrix
		/// <b>Note:</b>
		/// Actually the matrix should be a <b>column orienten, upper triangular
		/// matrix</b> but use the <b>row oriented, lower triangular matrix</b>
		/// instead (transposed), because this is faster due to the easyer array
		/// access.</summary>
		/// <param name="B">another matrix</param>
		/// <returns>Matrix product, A * B</returns>
		/// <exception cref="">ArgumentException Matrix inner dimensions must agree.</exception>
		public Matrix TimesTriangular (Matrix B)
		{
			if (B.m != n)
				throw new ArgumentException("Matrix inner dimensions must agree.");

			Matrix X = new Matrix(m,B.n);
			double[][] c = X.GetArray();
			double[][] b;
			double s = 0;
			double[] Arowi;
			double[] Browj;

			b = B.GetArray();
			///multiply with each row of A
			for (int i = 0; i < m; i++)
			{
				Arowi = A[i];

				///for all columns of B
				for (int j = 0; j < B.n; j++)
				{
					s = 0;
					Browj = b[j];
					///since B being triangular, this loop uses k <= j
					for (int k = 0; k <= j; k++)
					{
						s += Arowi[k] * Browj[k];
					}
					c[i][j] = s;
				}
			}
			return X;
		}

		/// <summary>
		/// X.diffEquals() calculates differences between adjacent columns of this
		/// matrix. Consequently the size of the matrix is reduced by one. The result
		/// is stored in this matrix object again.
		/// </summary>
		public void DiffEquals()
		{
			double[] col = null;
			for(int i = 0; i < A.Length; i++)
			{
				col = new double[A[i].Length - 1];

				for(int j = 1; j < A[i].Length; j++)
					col[j-1] = Math.Abs(A[i][j] - A[i][j - 1]);

				A[i] = col;
			}
			n--;
		}

		/// <summary>
		/// X.logEquals() calculates the natural logarithem of each element of the
		/// matrix. The result is stored in this matrix object again.
		/// </summary>
		public void LogEquals()
		{
			for(int i = 0; i < A.Length; i++)
				for(int j = 0; j < A[i].Length; j++)
					A[i][j] = Math.Log(A[i][j]);
		}

		/// X.powEquals() calculates the power of each element of the matrix. The
		/// result is stored in this matrix object again.
		public void PowEquals(double exp)
		{
			for(int i = 0; i < A.Length; i++)
				for(int j = 0; j < A[i].Length; j++)
					A[i][j] = Math.Pow(A[i][j], exp);
		}

		/// X.powEquals() calculates the power of each element of the matrix.
		/// <returns>Matrix</returns>
		public Matrix Pow(double exp)
		{
			Matrix X = new Matrix(m,n);
			double[][] C = X.GetArray();

			for (int i = 0; i < m; i++)
				for (int j = 0; j < n; j++)
					C[i][j] = Math.Pow(A[i][j], exp);

			return X;
		}

		/// <summary>
		/// X.thrunkAtLowerBoundariy(). All values smaller than the given one are set
		/// to this lower boundary.
		/// </summary>
		/// <param name="value">Lower boundary value</param>
		public void ThrunkAtLowerBoundary(double @value)
		{
			for(int i = 0; i < A.Length; i++)
				for(int j = 0; j < A[i].Length; j++)
			{
				if(A[i][j] < @value)
					A[i][j] = @value;
			}
		}

		/// <summary>LU Decomposition</summary>
		/// <returns>LUDecomposition</returns>
		/// <seealso cref="">LUDecomposition</seealso>
		public LUDecomposition LU ()
		{
			return new LUDecomposition(this);
		}

		/// <summary>QR Decomposition</summary>
		/// <returns>QRDecomposition</returns>
		/// <seealso cref="">QRDecomposition</seealso>
		public QRDecomposition QR ()
		{
			return new QRDecomposition(this);
		}

		/// <summary>Cholesky Decomposition</summary>
		/// <returns>CholeskyDecomposition</returns>
		/// <seealso cref="">CholeskyDecomposition</seealso>
		public CholeskyDecomposition Chol ()
		{
			return new CholeskyDecomposition(this);
		}

		/// <summary>Singular Value Decomposition</summary>
		/// <returns>SingularValueDecomposition</returns>
		/// <seealso cref="">SingularValueDecomposition</seealso>
		public SingularValueDecomposition Svd ()
		{
			return new SingularValueDecomposition(this);
		}

		/// <summary>Eigenvalue Decomposition</summary>
		/// <returns>EigenvalueDecomposition</returns>
		/// <seealso cref="">EigenvalueDecomposition</seealso>
		public EigenvalueDecomposition Eig ()
		{
			return new EigenvalueDecomposition(this);
		}

		/// <summary>Solve A*X = B</summary>
		/// <param name="B">right hand side</param>
		/// <returns>solution if A is square, least squares solution otherwise</returns>
		public Matrix Solve (Matrix B)
		{
			return (m == n ? (new LUDecomposition(this)).Solve(B) : (new QRDecomposition(this)).Solve(B));
		}

		/// <summary>Solve X*A = B, which is also A'*X' = B'</summary>
		/// <param name="B">right hand side</param>
		/// <returns>solution if A is square, least squares solution otherwise.</returns>
		public Matrix SolveTranspose (Matrix B)
		{
			return Transpose().Solve(B.Transpose());
		}

		/// <summary>Matrix inverse or pseudoinverse</summary>
		/// <returns>inverse(A) if A is square, pseudoinverse otherwise.</returns>
		public Matrix Inverse ()
		{
			return Solve(Identity(m,m));
		}

		/// <summary>Matrix determinant</summary>
		/// <returns>determinant</returns>
		public double Det ()
		{
			return new LUDecomposition(this).Det();
		}

		/// <summary>Matrix rank</summary>
		/// <returns>effective numerical rank, obtained from SVD.</returns>
		public int Rank ()
		{
			return new SingularValueDecomposition(this).Rank();
		}

		/// <summary>Matrix condition (2 norm)</summary>
		/// <returns>ratio of largest to smallest singular value.</returns>
		public double Cond ()
		{
			return new SingularValueDecomposition(this).Cond();
		}

		/// <summary>Matrix trace.</summary>
		/// <returns>sum of the diagonal elements.</returns>
		public double Trace ()
		{
			double t = 0;
			for (int i = 0; i < Math.Min(m,n); i++)
			{
				t += A[i][i];
			}
			return t;
		}

		/// <summary>Generate matrix with random elements</summary>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		/// <returns>An m-by-n matrix with uniformly distributed random elements.</returns>
		public static Matrix Random (int m, int n) {
			Random rand = new Random();
			Matrix A = new Matrix(m,n);
			double[][] X = A.GetArray();
			for (int i = 0; i < m; i++) {
				for (int j = 0; j < n; j++) {
					X[i][j] = rand.NextDouble();
				}
			}
			return A;
		}

		/// <summary>Generate identity matrix</summary>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		/// <returns>An m-by-n matrix with ones on the diagonal and zeros elsewhere.</returns>
		public static Matrix Identity (int m, int n)
		{
			Matrix A = new Matrix(m,n);
			double[][] X = A.GetArray();
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					X[i][j] = (i == j ? 1.0 : 0.0);
				}
			}
			return A;
		}

		/// <summary>
		/// Print the matrix to stdout. Line the elements up in columns
		/// with a Fortran-like 'Fw.d' style format.
		/// </summary>
		/// <param name="w">Column width.</param>
		/// <param name="d">Number of digits after the decimal.</param>
		public void Print (int w, int d)
		{
			Print(System.Console.Out,w,d);
		}

		/// <summary>
		/// Print the matrix to the output stream. Line the elements up in
		/// columns with a Fortran-like 'Fw.d' style format.
		/// </summary>
		/// <param name="output">Output stream.</param>
		/// <param name="w">Column width.</param>
		/// <param name="d">Number of digits after the decimal.</param>
		public void Print (TextWriter output, int w, int d)
		{
			NumberFormatInfo format = new CultureInfo("en-US", false).NumberFormat;
			format.NumberDecimalDigits = d;
			Print(output,format,w+2);
		}

		/// <summary>
		/// Print the matrix to stdout. Line the elements up in columns.
		/// Use the format object, and right justify within columns of width
		/// characters.
		/// Note that is the matrix is to be read back in, you probably will want
		/// to use a NumberFormat that is set to US Locale.
		/// </summary>
		/// <param name="format">A  Formatting object for individual elements.</param>
		/// <param name="width">Field width for each column.</param>
		/// <seealso cref="">NumberFormatInfo</seealso>
		public void Print (NumberFormatInfo format, int width)
		{
			Print(System.Console.Out,format,width);
		}

		/// <summary>
		/// Print the matrix to the output stream. Line the elements up in columns.
		/// Use the format object, and right justify within columns of width
		/// characters.
		/// Note that is the matrix is to be read back in, you probably will want
		/// to use a NumberFormat that is set to US Locale.
		/// </summary>
		/// <param name="output">the output stream.</param>
		/// <param name="format">A formatting object to format the matrix elements</param>
		/// <param name="width">Column width.</param>
		/// <seealso cref="">NumberFormatInfo</seealso>
		public void Print (TextWriter output, NumberFormatInfo format, int width)
		{
			output.WriteLine(); /// start on new line.
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					string s = A[i][j].ToString("F", format); /// format the number
					output.Write(s.PadRight(width));
				}
				output.WriteLine();
			}
			output.WriteLine(); /// end with blank line.
		}
		
		/// <summary>
		/// Write XML to Text Writer
		/// </summary>
		/// <param name="textWriter"></param>
		/// <example>
		/// mfccs.Write(File.CreateText("mfccs.xml"));
		/// </example>
		public void Write(TextWriter textWriter)
		{
			XmlTextWriter xmlTextWriter = new XmlTextWriter(textWriter);
			xmlTextWriter.Formatting = Formatting.Indented;
			xmlTextWriter.Indentation = 4;
			WriteXML(xmlTextWriter, null);
			xmlTextWriter.Close();
		}

		/// <summary>
		/// Read XML from Text Reader
		/// </summary>
		/// <param name="textReader"></param>
		/// <example>
		/// mfccs.Read(new StreamReader("mfccs.xml"));
		/// </example>
		public void Read(TextReader textReader)
		{
			XmlTextReader xmlTextReader = new XmlTextReader(textReader);
			ReadXML(XDocument.Load(xmlTextReader), null);
			xmlTextReader.Close();
		}

		// ------------------------
		//   Private Methods
		// ------------------------

		/// <summary>
		/// Check if size(A) == size(B)
		/// </summary>
		/// <param name="B">Matrix</param>
		private void CheckMatrixDimensions (Matrix B)
		{
			if (B.m != m || B.n != n)
			{
				throw new ArgumentException("Matrix dimensions must agree.");
			}
		}

		/// <summary>
		/// Writes the xml representation of this object to the xml text writer.<br>
		/// <br>
		/// There is the convetion, that each call to a <code>WriteXML()</code> method
		/// results in one xml element in the output stream.
		/// </summary>
		/// <param name="writer">XmlTextWriter the xml output stream</param>
		/// <example>
		/// mfccs.WriteXML(new XmlTextWriter("mfccs.xml", null));
		/// </example>
		public void WriteXML(XmlWriter xmlWriter, string matrixName)
		{
			xmlWriter.WriteStartElement("matrix");
			xmlWriter.WriteAttributeString("rows", m.ToString());
			xmlWriter.WriteAttributeString("cols", n.ToString());
			xmlWriter.WriteAttributeString("name", matrixName);

			for(int i = 0; i < m; i++)
			{
				xmlWriter.WriteStartElement("matrixrow");
				for(int j = 0; j < n; j++)
				{
					xmlWriter.WriteStartElement("cn");
					//xmlWriter.WriteAttributeString("type","IEEE-754");
					xmlWriter.WriteString(A[i][j].ToString());
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}

			xmlWriter.WriteEndElement();
		}

		/// <summary>
		/// Reads the xml representation of an object form the xml text reader.<br>
		/// </summary>
		/// <param name="parser">XmlTextReader the xml input stream</param>
		/// <example>
		/// mfccs.ReadXML(new XmlTextReader("mfccs.xml"));
		/// </example>
		public void ReadXML(XDocument xdoc, string matrixName)
		{
			XElement dimensions = null;
			if (matrixName != null) {
				// look up by attribute name
				dimensions = (from x in xdoc.Descendants("matrix")
				              where x.Attribute("name").Value == matrixName
				              select x).FirstOrDefault();
			} else {
				dimensions = xdoc.Element("matrix");
			}
			
			string rows = dimensions.Attribute("rows").Value;
			string cols = dimensions.Attribute("cols").Value;
			int m = int.Parse(rows);
			int n = int.Parse(cols);

			var matrixrows = from row in dimensions.Descendants("matrixrow")
				select new {
				Children = row.Descendants("cn")
			};
			
			if (m != matrixrows.Count() || n != matrixrows.FirstOrDefault().Children.Count()) {
				// Dimension errors
				throw new ArgumentException("Matrix dimensions must agree.");
			} else {
				this.m = m;
				this.n = n;
			}
			
			this.A = new double[m][];

			int i = 0, j = 0;
			foreach (var matrixrow in matrixrows) {
				this.A[i] = new double[n];
				j = 0;
				foreach(var cn in matrixrow.Children) {
					string val = cn.Value;
					this.A[i][j] = double.Parse(val);
					j++;
				}
				i++;
			}
		}

		/// <summary>Returns the mean values along the specified dimension.</summary>
		/// <param name="dim">If 1, then the mean of each column is returned in a row
		/// vector. If 2, then the mean of each row is returned in a
		/// column vector.</param>
		/// <returns>A vector containing the mean values along the specified dimension.</returns>
		public Matrix Mean(int dim)
		{
			Matrix result;
			switch (dim)
			{
				case 1:
					result = new Matrix(1, n);
					for (int currN = 0; currN < n; currN++)
					{
						for (int currM = 0; currM < m; currM++)
							result.A[0][currN] += A[currM][currN];
						result.A[0][currN] /= m;
					}
					return result;
				case 2:
					result = new Matrix(m, 1);
					for (int currM = 0; currM < m; currM++)
					{
						for (int currN = 0; currN < n; currN++)
						{
							result.A[currM][0] += A[currM][currN];
						}
						result.A[currM][0] /= n;
					}
					return result;
				default:
					new ArgumentException("dim must be either 1 or 2, and not: " + dim);
					return null;
			}
		}

		/// <summary>Calculate the full covariance matrix.</summary>
		/// <returns>the covariance matrix</returns>
		public Matrix Cov()
		{
			Matrix transe = this.Transpose();
			Matrix result = new Matrix(transe.m, transe.m);
			for(int currM = 0; currM < transe.m; currM++)
			{
				for(int currN = currM; currN < transe.m; currN++)
				{
					double covMN = Cov(transe.A[currM], transe.A[currN]);
					result.A[currM][currN] = covMN;
					result.A[currN][currM] = covMN;
				}
			}
			return result;
		}

		/// <summary>Calculate the covariance between the two vectors.</summary>
		/// <param name="vec1">double values</param>
		/// <param name="vec2">double values</param>
		/// <returns>the covariance between the two vectors.</returns>
		private double Cov(double[] vec1, double[] vec2)
		{
			double result = 0;
			int dim = vec1.Length;
			if(vec2.Length != dim)
				new ArgumentException("vectors are not of same length");
			double meanVec1 = Mean(vec1), meanVec2 = Mean(vec2);
			for(int i=0; i<dim; i++)
			{
				result += (vec1[i]-meanVec1)*(vec2[i]-meanVec2);
			}
			return result / Math.Max(1, dim-1);
			
			/// int dim = vec1.Length;
			/// if(vec2.Length != dim)
			///  (new ArgumentException("vectors are not of same length")).printStackTrace();
			/// double[] times = new double[dim];
			/// for(int i=0; i<dim; i++)
			///   times[i] += vec1[i]*vec2[i];
			/// return mean(times) - mean(vec1)*mean(vec2);
		}

		/// <summary>The mean of the values in the double array</summary>
		/// <param name="vec">double values</param>
		/// <returns>the mean of the values in vec</returns>
		private double Mean(double[] vec)
		{
			double result = 0;
			for(int i=0; i<vec.Length; i++)
				result += vec[i];
			return result / vec.Length;
		}

		/// <summary>Returns the sum of the component of the matrix.</summary>
		/// <returns>the sum</returns>
		public double Sum()
		{
			double result = 0;
			foreach(double[] dArr in A)
				foreach(double d in dArr)
					result += d;
			return result;
		}

		/// <summary>returns a new Matrix object, where each value is set to the absolute value</summary>
		/// <returns>a new Matrix with all values being positive</returns>
		public Matrix Abs()
		{
			Matrix result = new Matrix(m, n); // don't use clone(), as the values are assigned in the loop.
			for(int i=0; i<result.A.Length; i++)
			{
				for(int j=0; j<result.A[i].Length; j++)
					result.A[i][j] = Math.Abs(A[i][j]);
			}
			return result;
		}

		/// <summary>Writes the Matrix to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');</summary>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\matrix.ascii"</param>
		public void WriteAscii(string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i< m; i++)
			{
				for(int j = 0; j < n; j++)
				{
					pw.Write("{0:#.0000000e+000} ", A[i][j]);
				}
				pw.Write("\r");
			}
			pw.Close();
		}
	}
}