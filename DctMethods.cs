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
	/// Method that calculate dct2 in two dimensions directly
	/// just as described here:
	/// http://www.mathworks.it/help/toolbox/images/ref/dct2.html
	/// </summary>
	/// <param name="A">signal array</param>
	/// <param name="offset">value to add or substract to the signal value before multiplying with the Cosine Tranform</param>
	/// <returns>returns the two-dimensional discrete cosine transform of A. The resulting array B is the same size as A and contains the discrete cosine transform coefficients B(k1,k2).</returns>
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
						sum += (A[i][j] + offset) * Math.Cos((Math.PI * (2 * i + 1) * k) / (2 * n)) * Math.Cos((Math.PI * (2 * j + 1) * l) / (2 * m));
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
	/// <returns>returns the two-dimensional inverse discrete cosine transform (DCT) of A. The resulting array B is the same size as A and contains the discrete cosine transform coefficients B(k1,k2).</returns>
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
						B[k][l] += alf1[i] * alf2[j] * A[i][j] * Math.Cos((Math.PI * (2 * k + 1) * i) / (2 * n)) * Math.Cos((Math.PI * (2 * l + 1) * j) / (2 * m));
					}
				}
				B[k][l] += offset;
				//Console.WriteLine(k + " " + l + ": " + c[k][l]);
			}
		}

		return B;
	}

	/// <summary>
	/// Method that calculate dct2 in two dimensions, first
	/// calculate dct in row and after calculate dct in column
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
		
		double[][] c2 = new double[n][];
		for (int i=0;i<n;i++)
			c2[i] = new double[m];

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
				c2[k][l] = alfa * sum;
			}
		}

		return c2;
	}

	/// <summary>
	/// Method that calculate idct2 in two dimensions, first
	/// calculate idct in row and after calculate idct in column
	/// </summary>
	/// <param name="A">signal array</param>
	/// <param name="offset">value to add or substract to the signal value before multiplying with the Cosine Tranform</param>
	/// <returns>returns the two-dimensional inverse discrete cosine transform (DCT) of A. The resulting array B is the same size as A and contains the discrete cosine transform coefficients B(k1,k2).</returns>
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
		
		double[][] c2 = new double[n][];
		for (int i=0;i<n;i++)
			c2[i] = new double[m];

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
				c2[k][l] = 0;
				for (int j = 0; j < m; j++)
				{
					alfa = j == 0 ? 1.0 / Math.Sqrt(m) : Math.Sqrt(2.0 / m);
					c2[k][l] += alfa * B[k][j] * Math.Cos((Math.PI * (2.0 * l + 1) * j) / (2.0 * m));
				}
				c2[k][l] += offset;
			}
		}

		return c2;
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
	
	// test from example 1
	// format short g
	// z = [1 2 3; 4 5 6];
	//
	// dct2(z) = [+8.5732   -2.0000    0.0000
	//            -3.6742         0         0];
	public static void test1()
	{
		double[][] vals = new double[][] {
			new double[] { 1.0, 2.0, 3.0 },
			new double[] { 4.0, 5.0, 6.0 }
		};
		
		double offset = 0;
		Console.WriteLine("vals: ");
		PrintMatrix(vals);

		Stopwatch stopWatch = Stopwatch.StartNew();
		long startS = stopWatch.ElapsedTicks;
		double[][] result = dct2(vals, offset);
		long endS = stopWatch.ElapsedTicks;

		Console.WriteLine("dct2 result: ");
		PrintMatrix(result);

		Console.WriteLine("time: " + (endS - startS));

		result = Filter(result, 1.0);

		Console.WriteLine("dct2 filtered: ");
		PrintMatrix(result);

		double[][] ivals = idct2(result, -offset);
		Console.WriteLine("idct2 result: ");
		PrintMatrix(ivals);
	}

	// test from example 2
	// format short g
	/*
z = [139 144 149 153 155 155 155 155;
144 151 153 156 159 156 156 156;
150 155 160 163 158 156 156 156;
159 161 162 160 160 159 159 159;
159 160 161 162 162 155 155 155;
161 161 161 161 160 157 157 157;
162 162 161 163 162 157 157 157;
162 162 161 161 163 158 158 158];
	 */
	// dct2(z-128) =
	/*
	   235.6250   -1.0333  -12.0809   -5.2029    2.1250   -1.6724   -2.7080    1.3238
	   -22.5904  -17.4842   -6.2405   -3.1574   -2.8557   -0.0695    0.4342   -1.1856
	   -10.9493   -9.2624   -1.5758    1.5301    0.2029   -0.9419   -0.5669   -0.0629
	    -7.0816   -1.9072    0.2248    1.4539    0.8963   -0.0799   -0.0423    0.3315
	    -0.6250   -0.8381    1.4699    1.5563   -0.1250   -0.6610    0.6088    1.2752
	     1.7541   -0.2029    1.6205   -0.3424   -0.7755    1.4759    1.0410   -0.9930
	    -1.2825   -0.3600   -0.3169   -1.4601   -0.4900    1.7348    1.0758   -0.7613
	    -2.5999    1.5519   -3.7628   -1.8448    1.8716    1.2139   -0.5679   -0.4456
	 */
	public static void test2()
	{
		double[][] vals = new double[][] {
			new double[] { 139.0, 144.0, 149.0, 153.0, 155.0, 155.0, 155.0, 155.0 },
			new double[] { 144.0, 151.0, 153.0, 156.0, 159.0, 156.0, 156.0, 156.0 },
			new double[] { 150.0, 155.0, 160.0, 163.0, 158.0, 156.0, 156.0, 156.0 },
			new double[] { 159.0, 161.0, 162.0, 160.0, 160.0, 159.0, 159.0, 159.0 },
			new double[] { 159.0, 160.0, 161.0, 162.0, 162.0, 155.0, 155.0, 155.0 },
			new double[] { 161.0, 161.0, 161.0, 161.0, 160.0, 157.0, 157.0, 157.0 },
			new double[] { 162.0, 162.0, 161.0, 163.0, 162.0, 157.0, 157.0, 157.0 },
			new double[] { 162.0, 162.0, 161.0, 161.0, 163.0, 158.0, 158.0, 158.0 } };

		double offset = -128;
		Console.WriteLine("vals: ");
		PrintMatrix(vals);

		Stopwatch stopWatch = Stopwatch.StartNew();
		long startS = stopWatch.ElapsedTicks;
		double[][] result = dct2(vals, offset);
		long endS = stopWatch.ElapsedTicks;

		Console.WriteLine("dct2 result: ");
		PrintMatrix(result);

		Console.WriteLine("time: " + (endS - startS));

		//result = Filter(result, 0.25);
		result = CutLeastSignificantCoefficients(result);
		Console.WriteLine("dct2 filtered: ");
		PrintMatrix(result);

		long startE = stopWatch.ElapsedTicks;
		double[][] ivals = idct2(result, -offset);
		long endE = stopWatch.ElapsedTicks;

		Console.WriteLine("idct2 result: ");
		PrintMatrix(ivals);
		Console.WriteLine("time: " + (endE - startE));
	}

	// test from example 3
	// 
	// z =  [3     7    -5
	//       8    -9     7];
	//
	// dct2(z) =
	// 
	//     4.4907    4.5000    4.9075
	//    -0.4082    3.5000  -14.1451
	public static void test3()
	{
		double[][] vals = {
			new double[] { 3.0, 7.0, -5.0 },
			new double[] { 8.0, -9.0, 7.0 } };
		double offset = 0;
		Console.WriteLine("vals: ");
		PrintMatrix(vals);

		Stopwatch stopWatch = Stopwatch.StartNew();
		long startS = stopWatch.ElapsedTicks;
		double[][] result = dct2(vals, offset);
		long endS = stopWatch.ElapsedTicks;

		Console.WriteLine("dct2 result: ");
		PrintMatrix(result);

		Console.WriteLine("time: " + (endS - startS));

		//result = filter(result, 0.25);
		//Console.WriteLine("dct2 filtered: ");
		//printMatrix(result);

		double[][] ivals = idct2(result, -offset);
		Console.WriteLine("idct2 result: ");
		PrintMatrix(ivals);
	}
}