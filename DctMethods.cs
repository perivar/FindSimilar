using System;
using System.Diagnostics;
using System.Globalization;

/// <summary>
/// Copied from https://github.com/voxsim/discrete-cosine-transform
/// </summary>
public class DctMethods
{
	/// <summary>
	/// Method for printing a Matrix
	/// for debug purpose
	/// </summary>
	/// <param name="z"></param>
	public static void PrintMatrix(double[][] z)
	{
		int n = z.Length;
		int m = z[0].Length;

		NumberFormatInfo format = new CultureInfo("en-US", false).NumberFormat;
		format.NumberDecimalDigits = 4;
		int width = 9;

		Console.Write("[");
		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < m; j++)
			{
				// round to better printable precision
				decimal d = (decimal) z[i][j];
				decimal rounded = Math.Round(d, ((NumberFormatInfo)format).NumberDecimalDigits);
				string s = rounded.ToString("G29", format);
				Console.Write(s.PadRight(width));
			}
			Console.WriteLine();
		}
		Console.WriteLine("]");
	}

	/// <summary>
	/// Method that calculate the Discrete Cosine Transform in one dimension.
	/// </summary>
	/// <param name="A">signal array</param>
	/// <returns>returns the one-dimensional discrete cosine transform of A. The resulting array B is the same size as A and contains the discrete cosine transform coefficients B(k1,k2).</returns>
	public static double[][] dct(double[][] A)
	{
		return dct(A, 0);
	}
	
	/// <summary>
	/// Method that calculate the Discrete Cosine Transform in one dimension.
	/// </summary>
	/// <param name="A">signal array</param>
	/// <param name="offset">value to add or substract to the signal value before multiplying with the Cosine Tranform</param>
	/// <returns>returns the one-dimensional discrete cosine transform of A. The resulting array B is the same size as A and contains the discrete cosine transform coefficients B(k1,k2).</returns>
	public static double[][] dct(double[][] A, double offset)
	{
		if (A.Length == 0)
			throw new Exception("A empty");

		if (A[0].Length == 0)
			throw new Exception("A row empty");

		int n = A.Length;
		int m = A[0].Length;

		double[][] B = new double[n][];
		for (int i=0;i<n;i++)
			B[i] = new double[m];
		
		double alfa;
		double sum;

		for (int k = 0; k < n; k++)
		{
			for (int l = 0; l < m; l++)
			{
				sum = 0;
				for (int i = 0; i < n; i++)
				{
					sum += (A[i][l] + offset) * Math.Cos((Math.PI * (2.0 * i + 1.0) * k) / (2.0 * n));
				}
				alfa = k == 0 ? 1.0 / Math.Sqrt(n) : Math.Sqrt(2.0 / n);
				B[k][l] = alfa * sum;
			}
		}

		return B;
	}

	/// <summary>
	/// Method that calculate the Inverse Discrete Cosine Transformin one dimension.
	/// </summary>
	/// <param name="A">signal array</param>
	/// <returns>returns the one-dimensional inverse discrete cosine transform (DCT) of A. The resulting array B is the same size as A and contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
	public static double[][] idct(double[][] A)
	{
		return idct(A, 0);
	}
	
	/// <summary>
	/// Method that calculate the Inverse Discrete Cosine Transformin one dimension.
	/// </summary>
	/// <param name="A">signal array</param>
	/// <param name="offset">value to add or substract to the signal value before multiplying with the Cosine Tranform</param>
	/// <returns>returns the one-dimensional inverse discrete cosine transform (DCT) of A. The resulting array B is the same size as A and contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
	public static double[][] idct(double[][] A, double offset)
	{
		if (A.Length == 0)
			throw new Exception("A empty");

		if (A[0].Length == 0)
			throw new Exception("A row empty");

		int n = A.Length;
		int m = A[0].Length;

		double[][] B = new double[n][];
		for (int i=0;i<n;i++)
			B[i] = new double[m];
		
		double alfa;

		for (int k = 0; k < n; k++)
		{
			for (int l = 0; l < m; l++)
			{
				B[k][l] = 0;
				for (int i = 0; i < n; i++)
				{
					alfa = i == 0 ? 1.0 / Math.Sqrt(n) : Math.Sqrt(2.0 / n);
					B[k][l] += alfa * A[i][l] * Math.Cos((Math.PI * (2.0 * k + 1) * i) / (2.0 * n));
				}
				B[k][l] += offset;
			}
		}

		return B;
	}

	/// <summary>
	/// Method that calculate the Discrete Cosine Transform2 in two dimensions, first
	/// calculate the Discrete Cosine Transform in row and after calculate the Discrete Cosine Transform in column
	/// </summary>
	/// <param name="A">signal array</param>
	/// <returns>returns the two-dimensional discrete cosine transform of A. The resulting array B is the same size as A and contains the discrete cosine transform coefficients B(k1,k2).</returns>
	public static double[][] dct2(double[][] A)
	{
		return dct2(A, 0);
	}
	
	/// <summary>
	/// Method that calculate the Discrete Cosine Transform2 in two dimensions, first
	/// calculate the Discrete Cosine Transform in row and after calculate the Discrete Cosine Transform in column
	/// </summary>
	/// <param name="A">signal array</param>
	/// <param name="offset">value to add or substract to the signal value before multiplying with the Cosine Tranform</param>
	/// <returns>returns the two-dimensional discrete cosine transform of A. The resulting array B is the same size as A and contains the discrete cosine transform coefficients B(k1,k2).</returns>
	public static double[][] dct2(double[][] A, double offset)
	{
		if (A.Length == 0)
			throw new Exception("A empty");

		if (A[0].Length == 0)
			throw new Exception("A row empty");

		int n = A.Length;
		int m = A[0].Length;

		double[][] B = new double[n][];
		for (int i=0;i<n;i++)
			B[i] = new double[m];
		
		double[][] C = new double[n][];
		for (int i=0;i<n;i++)
			C[i] = new double[m];

		double alfa;
		double sum;

		for (int k = 0; k < n; k++)
		{
			for (int l = 0; l < m; l++)
			{
				sum = 0;
				for (int i = 0; i < n; i++)
				{
					sum += (A[i][l] + offset) * Math.Cos((Math.PI * (2.0 * i + 1.0) * k) / (2.0 * n));
				}
				alfa = k == 0 ? 1.0 / Math.Sqrt(n) : Math.Sqrt(2.0 / n);
				B[k][l] = alfa * sum;
			}
		}

		for (int l = 0; l < m; l++)
		{
			for (int k = 0; k < n; k++)
			{
				sum = 0;
				for (int j = 0; j < m; j++)
				{
					sum += B[k][j] * Math.Cos((Math.PI * (2.0 * j + 1.0) * l) / (2.0 * m));
				}
				alfa = l == 0 ? 1.0 / Math.Sqrt(m) : Math.Sqrt(2.0 / m);
				C[k][l] = alfa * sum;
			}
		}

		return C;
	}

	/// <summary>
	/// Method that calculate idct2 in two dimensions, first
	/// calculate the Inverse Discrete Cosine Transformin row and after calculate the Inverse Discrete Cosine Transformin column
	/// </summary>
	/// <param name="A">signal array</param>
	/// <returns>returns the two-dimensional inverse discrete cosine transform (DCT) of A. The resulting array B is the same size as A and contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
	public static double[][] idct2(double[][] A)
	{
		return idct2(A, 0);
	}
	
	/// <summary>
	/// Method that calculate idct2 in two dimensions, first
	/// calculate the Inverse Discrete Cosine Transformin row and after calculate the Inverse Discrete Cosine Transformin column
	/// </summary>
	/// <param name="A">signal array</param>
	/// <param name="offset">value to add or substract to the signal value before multiplying with the Cosine Tranform</param>
	/// <returns>returns the two-dimensional inverse discrete cosine transform (DCT) of A. The resulting array B is the same size as A and contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
	public static double[][] idct2(double[][] A, double offset)
	{
		if (A.Length == 0)
			throw new Exception("A empty");

		if (A[0].Length == 0)
			throw new Exception("A row empty");

		int n = A.Length;
		int m = A[0].Length;

		double[][] B = new double[n][];
		for (int i=0;i<n;i++)
			B[i] = new double[m];
		
		double[][] C = new double[n][];
		for (int i=0;i<n;i++)
			C[i] = new double[m];

		double alfa;

		for (int k = 0; k < n; k++)
		{
			for (int l = 0; l < m; l++)
			{
				B[k][l] = 0;
				for (int i = 0; i < n; i++)
				{
					alfa = i == 0 ? 1.0 / Math.Sqrt(n) : Math.Sqrt(2.0 / n);
					B[k][l] += alfa * A[i][l] * Math.Cos((Math.PI * (2.0 * k + 1) * i) / (2.0 * n));
				}
			}
		}

		for (int l = 0; l < m; l++)
		{
			for (int k = 0; k < n; k++)
			{
				C[k][l] = 0;
				for (int j = 0; j < m; j++)
				{
					alfa = j == 0 ? 1.0 / Math.Sqrt(m) : Math.Sqrt(2.0 / m);
					C[k][l] += alfa * B[k][j] * Math.Cos((Math.PI * (2.0 * l + 1) * j) / (2.0 * m));
				}
				C[k][l] += offset;
			}
		}

		return C;
	}

	/// <summary>
	/// Method that calculate the Discrete Cosine Transform2 in two dimensions directly
	/// just as described here:
	/// http://www.mathworks.it/help/toolbox/images/ref/dct2.html
	/// </summary>
	/// <param name="A">signal array</param>
	/// <param name="offset">value to add or substract to the signal value before multiplying with the Cosine Tranform</param>
	/// <returns>returns the two-dimensional discrete cosine transform of A. The resulting array B is the same size as A and contains the discrete cosine transform coefficients B(k1,k2).</returns>
	/// <remarks>Even though this method is supposed to be faster than the dct2 version, it's not?!</remarks>
	public static double[][] dct2in2dimension(double[][] A, double offset)
	{
		if (A.Length == 0)
			throw new Exception("A empty");

		if (A[0].Length == 0)
			throw new Exception("A row empty");

		int n = A.Length;
		int m = A[0].Length;

		double[][] B = new double[n][];
		for (int i=0;i<n;i++)
			B[i] = new double[m];
		
		double[] alf1 = new double[n];
		double[] alf2 = new double[m];

		alf1[0] = 1.0 / Math.Sqrt(n);
		for (int k = 1; k < n; k++)
		{
			alf1[k] = Math.Sqrt(2.0 / n);
		}

		alf2[0] = 1.0 / Math.Sqrt(m);
		for (int l = 1; l < m; l++)
		{
			alf2[l] = Math.Sqrt(2.0 / m);
		}

		double sum;
		for (int k = 0; k < n; k++)
		{
			for (int l = 0; l < m; l++)
			{
				sum = 0;
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < m; j++)
					{
						sum += (A[i][j] + offset)
							* Math.Cos((Math.PI * (2.0 * i + 1) * k) / (2.0 * n))
							* Math.Cos((Math.PI * (2.0 * j + 1) * l) / (2.0 * m));
					}
				}
				B[k][l] = alf1[k] * alf2[l] * sum;
				//Console.WriteLine(k + " " + l + ": " + sum + "*" + alf1[k] + "*" + alf2[l] + " -> " + c[k][l]);
			}
		}

		return B;
	}
	
	/// <summary>
	/// Method that calculate idct2 in two dimensions directly
	/// just as described here:
	/// http://www.mathworks.it/help/toolbox/images/ref/idct2.html
	/// </summary>
	/// <param name="A">signal array</param>
	/// <param name="offset">value to add or substract to the signal value before multiplying with the Cosine Tranform</param>
	/// <returns>returns the two-dimensional inverse discrete cosine transform (DCT) of A. The resulting array B is the same size as A and contains the inverse discrete cosine transform coefficients B(k1,k2).</returns>
	/// <remarks>Even though this method is supposed to be faster than the dct2 version, it's not?!</remarks>
	public static double[][] idct2in2dimension(double[][] A, double offset)
	{
		if (A.Length == 0)
			throw new Exception("A empty");

		if (A[0].Length == 0)
			throw new Exception("A row empty");

		int n = A.Length;
		int m = A[0].Length;

		double[][] B = new double[n][];
		for (int i=0;i<n;i++)
			B[i] = new double[m];
		
		double[] alf1 = new double[n];
		double[] alf2 = new double[m];

		alf1[0] = 1.0 / Math.Sqrt(n);
		for (int k = 1; k < n; k++)
		{
			alf1[k] = Math.Sqrt(2.0 / n);
		}

		alf2[0] = 1.0 / Math.Sqrt(m);
		for (int l = 1; l < m; l++)
		{
			alf2[l] = Math.Sqrt(2.0 / m);
		}

		for (int k = 0; k < n; k++)
		{
			for (int l = 0; l < m; l++)
			{
				B[k][l] = 0;
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < m; j++)
					{
						B[k][l] += alf1[i] * alf2[j] * A[i][j]
							* Math.Cos((Math.PI * (2.0 * k + 1) * i) / (2.0 * n))
							* Math.Cos((Math.PI * (2.0 * l + 1) * j) / (2.0 * m));
					}
				}
				B[k][l] += offset;
				//Console.WriteLine(k + " " + l + ": " + c[k][l]);
			}
		}

		return B;
	}
	
	/// <summary>
	/// Compress the matrix with the threshold percentage
	/// </summary>
	/// <param name="A">signal</param>
	/// <param name="threshold">threshold</param>
	/// <returns>the signal where some values has been zeroed out</returns>
	public static double[][] Filter(double[][] A, double threshold)
	{
		if (A.Length == 0)
			throw new Exception("A empty");

		if (A[0].Length == 0)
			throw new Exception("A row empty");

		int n = A.Length;
		int m = A[0].Length;

		int i =((int) Math.Round(n*threshold, MidpointRounding.AwayFromZero));
		int j =((int) Math.Round(m*threshold, MidpointRounding.AwayFromZero));
		
		Console.WriteLine("Zeroing out from {0}:{1} to {2}:{3}", i+1, n, j+1, m);

		for(int x = i; x < n; x++)
		{
			for(int y = j; y < m; y++)
			{
				A[x][y] = 0.0;
			}
		}

		return A;
	}

	/// <summary>
	/// Cut Least Significant Coefficients
	/// </summary>
	/// <param name="A">signal</param>
	/// <returns>the signal where some values has been zeroed out</returns>
	public static double[][] CutLeastSignificantCoefficients(double[][] A) {
		
		if (A.Length == 0)
			throw new Exception("A empty");

		if (A[0].Length == 0)
			throw new Exception("A row empty");

		int n = A.Length;
		int m = A[0].Length;

		// remove least significant components (last half diagonally)
		for (int i = n - 1; i >= 0; i--) {
			for (int j = m - 1; j > (n-i-2); j--) {
				A[i][j] = 0.0;
			}
		}
		
		return A;
	}
	
	public static double[,] dct2(double[,] f) {
		
		int rows = f.GetLength(0);
		int columns = f.GetLength(1);

		double[][] fArray = new double[rows][];
		for (int i = 0; i < rows; i++) {
			fArray[i] = new double[columns];
			for (int j = 0; j < columns; j++) {
				fArray[i][j] = f[i, j];
			}
		}
		
		double[][] dctArray = dct2(fArray, 0);
		
		double[,] d = new double[rows, columns];
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < columns; j++) {
				d[i,j] = dctArray[i][j];
			}
		}

		return d;
	}
	
	public static double[,] idct2(double[,] F) {
		
		int rows = F.GetLength(0);
		int columns = F.GetLength(1);

		double[][] FArray = new double[rows][];
		for (int i = 0; i < rows; i++) {
			FArray[i] = new double[columns];
			for (int j = 0; j < columns; j++) {
				FArray[i][j] = F[i, j];
			}
		}
		
		double[][] dctArray = idct2(FArray, 0);
		
		double[,] d = new double[rows, columns];
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < columns; j++) {
				d[i,j] = dctArray[i][j];
			}
		}
		
		return d;
	}

	#region Testing Methods
	// test 1
	// Octave results:
	// format short g
	// z = [1 2 3; 4 5 6; 7 8 9; 10 11 12];
	//
	//octave:5> dct(z)
	//ans =
	//          11          13          15
	//     -6.6913     -6.6913     -6.6913
	//           0           0           0
	//    -0.47554    -0.47554    -0.47554
	//
	//octave:6> dct2(z)
	//ans =
	//      22.517     -2.8284           0
	//      -11.59           0           0
	//           0           0           0
	//    -0.82366           0           0
	//
	public static void test1(bool _2D = true)
	{
		double[][] vals = new double[][] {
			new double[] { 1.0, 2.0, 3.0 },
			new double[] { 4.0, 5.0, 6.0 },
			new double[] { 7.0, 8.0, 9.0 },
			new double[] { 10.0, 11.0, 12.0 }
		};
		
		double offset = 0;
		Console.WriteLine("vals: ");
		PrintMatrix(vals);

		Stopwatch stopWatch = Stopwatch.StartNew();
		long startS = stopWatch.ElapsedTicks;
		double[][] result;
		if (_2D) {
			result = dct2(vals, offset);
			Console.WriteLine("dct2 result: ");
		} else {
			result = dct(vals, offset);
			Console.WriteLine("dct result: ");
		}
		long endS = stopWatch.ElapsedTicks;
		PrintMatrix(result);
		Console.WriteLine("time in ticks: " + (endS - startS));

		//result = Filter(result, 1.0);
		//Console.WriteLine("dct2 filtered: ");
		//PrintMatrix(result);

		long startE = stopWatch.ElapsedTicks;
		double[][] ivals;
		if (_2D) {
			ivals = idct2(result, -offset);
			Console.WriteLine("idct2 result: ");
		} else {
			ivals = idct(result, -offset);
			Console.WriteLine("idct result: ");
		}
		long endE = stopWatch.ElapsedTicks;
		PrintMatrix(ivals);
		Console.WriteLine("time in ticks: " + (endE - startE));
	}

	// test 2
	// Octave results:
	// format short g
	/*
octave:43>
z = [139 144 149 153 155 155 155 155;
144 151 153 156 159 156 156 156;
150 155 160 163 158 156 156 156;
159 161 162 160 160 159 159 159;
159 160 161 162 162 155 155 155;
161 161 161 161 160 157 157 157;
162 162 161 163 162 157 157 157;
162 162 161 161 163 158 158 158];

octave:43> g = dct2(z-128)
g =
      235.62     -1.0333     -12.081     -5.2029       2.125     -1.6724      -2.708      1.3238
      -22.59     -17.484     -6.2405     -3.1574     -2.8557   -0.069456     0.43417     -1.1856
     -10.949     -9.2624     -1.5758      1.5301     0.20295    -0.94186    -0.56694   -0.062924
     -7.0816     -1.9072     0.22479      1.4539     0.89625   -0.079874   -0.042291     0.33154
      -0.625    -0.83811      1.4699      1.5563      -0.125    -0.66099     0.60885      1.2752
      1.7541    -0.20286      1.6205    -0.34244    -0.77554      1.4759       1.041    -0.99296
     -1.2825    -0.35995    -0.31694     -1.4601    -0.48996      1.7348      1.0758    -0.76135
     -2.5999      1.5519     -3.7628     -1.8448      1.8716      1.2139    -0.56788    -0.44564

octave:44> idct2(g)+128
ans =
         139         144         149         153         155         155         155         155
         144         151         153         156         159         156         156         156
         150         155         160         163         158         156         156         156
         159         161         162         160         160         159         159         159
         159         160         161         162         162         155         155         155
         161         161         161         161         160         157         157         157
         162         162         161         163         162         157         157         157
         162         162         161         161         163         158         158         158

octave:49> g = dct(z-128)
g =

      74.953      82.024      86.267      90.156      90.156      80.964      80.964      80.964
     -21.818     -14.969     -9.3908     -6.4728      -5.921     -1.7745     -1.7745     -1.7745
     -8.8097     -7.5031     -7.3446     -4.6522     -1.2737    -0.46194    -0.46194    -0.46194
     -2.4118     -3.7457     -3.9958     -3.0683     -1.4969     -1.7704     -1.7704     -1.7704
     0.70711    -0.70711    -0.70711     -2.4749     0.35355     0.35355     0.35355     0.35355
       1.365     0.22465     0.90791     0.57409     -1.7777      1.2224      1.2224      1.2224
    -0.94311     -1.4843     0.74614     0.77897     -2.1512    -0.19134    -0.19134    -0.19134
     -1.8165      -1.685     0.14561      2.9764     0.20231     -2.3922     -2.3922     -2.3922

octave:50> idct(g)+128
ans =

         139         144         149         153         155         155         155         155
         144         151         153         156         159         156         156         156
         150         155         160         163         158         156         156         156
         159         161         162         160         160         159         159         159
         159         160         161         162         162         155         155         155
         161         161         161         161         160         157         157         157
         162         162         161         163         162         157         157         157
         162         162         161         161         163         158         158         158
	 */
	public static void test2(bool _2D = true, bool random=false)
	{
		double[][] vals;
		if (random) {
			// Generate random integers between 0 and 255
			int N = 8;
			Random generator = new Random();

			vals = new double[N][];
			int val;
			for (int x=0;x<N;x++)
			{
				vals[x] = new double[N];
				for (int y=0;y<N;y++)
				{
					val = generator.Next(255);
					vals[x][y] = val;
				}
			}
		} else {
			vals = new double[][] {
				new double[] { 139.0, 144.0, 149.0, 153.0, 155.0, 155.0, 155.0, 155.0 },
				new double[] { 144.0, 151.0, 153.0, 156.0, 159.0, 156.0, 156.0, 156.0 },
				new double[] { 150.0, 155.0, 160.0, 163.0, 158.0, 156.0, 156.0, 156.0 },
				new double[] { 159.0, 161.0, 162.0, 160.0, 160.0, 159.0, 159.0, 159.0 },
				new double[] { 159.0, 160.0, 161.0, 162.0, 162.0, 155.0, 155.0, 155.0 },
				new double[] { 161.0, 161.0, 161.0, 161.0, 160.0, 157.0, 157.0, 157.0 },
				new double[] { 162.0, 162.0, 161.0, 163.0, 162.0, 157.0, 157.0, 157.0 },
				new double[] { 162.0, 162.0, 161.0, 161.0, 163.0, 158.0, 158.0, 158.0 } };
		}
		double offset = -128;
		Console.WriteLine("vals: ");
		PrintMatrix(vals);

		Stopwatch stopWatch = Stopwatch.StartNew();
		long startS = stopWatch.ElapsedTicks;
		double[][] result;
		if (_2D) {
			result = dct2(vals, offset);
			Console.WriteLine("dct2 result: ");
		} else {
			result = dct(vals, offset);
			Console.WriteLine("dct result: ");
		}
		long endS = stopWatch.ElapsedTicks;
		PrintMatrix(result);

		Console.WriteLine("time in ticks: " + (endS - startS));

		//result = Filter(result, 0.25);
		//result = CutLeastSignificantCoefficients(result);
		//Console.WriteLine("dct2 filtered: ");
		//PrintMatrix(result);

		long startE = stopWatch.ElapsedTicks;
		double[][] ivals;
		if (_2D) {
			ivals = idct2(result, -offset);
			Console.WriteLine("idct2 result: ");
		} else {
			ivals = idct(result, -offset);
			Console.WriteLine("idct result: ");
		}
		long endE = stopWatch.ElapsedTicks;
		PrintMatrix(ivals);
		Console.WriteLine("time in ticks: " + (endE - startE));
	}
	#endregion
}