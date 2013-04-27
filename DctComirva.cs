using System;
using CommonUtils;
using Comirva.Audio.Util.Maths;

/// <summary>
/// Discrete Cosine Transform using Comirva.
/// </summary>
public class DctComirva
{
	private Matrix dctMatrix;
	private int rows;
	private int columns;
	
	public Matrix DCTMatrix {
		get { return dctMatrix; }
	}
	
	public DctComirva(int rows, int columns)
	{
		this.rows = rows;
		this.columns = columns;
		
		// Compute the DCT
		// This whole section is copied from GetDCTMatrix() from CoMirva package
		dctMatrix = new Matrix(rows, columns);
		
		// Compute constants for DCT
		// http://unix4lyfe.org/dct/
		double k1 = Math.PI/columns;
		double w1 = 1.0/(Math.Sqrt(columns));
		double w2 = Math.Sqrt(2.0/columns);

		// Generate 1D DCT-II matrix
		for(int i = 0; i < rows; i++) {
			for(int j = 0; j < columns; j++) {
				if(i == 0)
					dctMatrix.Set(i, j, w1 * Math.Cos(k1*i*(j + 0.5d)));
				else
					dctMatrix.Set(i, j, w2 * Math.Cos(k1*i*(j + 0.5d)));
			}
		}
	}
	
	public double[][] dct(double[][] f)
	{
		// convert two dimensional data to a Comirva Matrix
		Matrix mat = new Matrix(f, rows, columns);
		#if DEBUG
		mat.WriteText("dct-before.txt");
		#endif
		
		// Perform the DCT (Discrete Cosine Transform)
		Matrix dctResult = dctMatrix * mat;
		
		#if DEBUG
		dctResult.WriteText("dct-after.txt");
		#endif

		// return as two dimensional array
		return dctResult.MatrixData;
	}
	
	public double[][] idct(double[][] F)
	{
		// convert two dimensional data to a Comirva Matrix
		Matrix mat = new Matrix(F, rows, columns);
		#if DEBUG
		mat.WriteText("idct-before.txt");
		#endif

		// Perform the IDCT (Inverse Discrete Cosine Transform)
		Matrix iDctResult = dctMatrix.Transpose() * mat;
		
		#if DEBUG
		iDctResult.WriteText("idct-after.txt");
		#endif

		// return as two dimensional array
		return iDctResult.MatrixData;
	}
	
	#region Testing Methods
	
	// Test the methods
	/*
z = [139 144 149 153 155 155 155 155;
144 151 153 156 159 156 156 156;
150 155 160 163 158 156 156 156;
159 161 162 160 160 159 159 159;
159 160 161 162 162 155 155 155;
161 161 161 161 160 157 157 157;
162 162 161 163 162 157 157 157;
162 162 161 161 163 158 158 158];

octave:55> x = dct(z)
x =
      436.99      444.06      448.31      452.19      452.19         443         443         443
     -21.818     -14.969     -9.3908     -6.4728      -5.921     -1.7745     -1.7745     -1.7745
     -8.8097     -7.5031     -7.3446     -4.6522     -1.2737    -0.46194    -0.46194    -0.46194
     -2.4118     -3.7457     -3.9958     -3.0683     -1.4969     -1.7704     -1.7704     -1.7704
     0.70711    -0.70711    -0.70711     -2.4749     0.35355     0.35355     0.35355     0.35355
       1.365     0.22465     0.90791     0.57409     -1.7777      1.2224      1.2224      1.2224
    -0.94311     -1.4843     0.74614     0.77897     -2.1512    -0.19134    -0.19134    -0.19134
     -1.8165      -1.685     0.14561      2.9764     0.20231     -2.3922     -2.3922     -2.3922

octave:56> idct(x)
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
	public static void test() {
		double[][] vals = new double[][] {
			new double[] { 139.0, 144.0, 149.0, 153.0, 155.0, 155.0, 155.0, 155.0 },
			new double[] { 144.0, 151.0, 153.0, 156.0, 159.0, 156.0, 156.0, 156.0 },
			new double[] { 150.0, 155.0, 160.0, 163.0, 158.0, 156.0, 156.0, 156.0 },
			new double[] { 159.0, 161.0, 162.0, 160.0, 160.0, 159.0, 159.0, 159.0 },
			new double[] { 159.0, 160.0, 161.0, 162.0, 162.0, 155.0, 155.0, 155.0 },
			new double[] { 161.0, 161.0, 161.0, 161.0, 160.0, 157.0, 157.0, 157.0 },
			new double[] { 162.0, 162.0, 161.0, 163.0, 162.0, 157.0, 157.0, 157.0 },
			new double[] { 162.0, 162.0, 161.0, 161.0, 163.0, 158.0, 158.0, 158.0 } };
		
		DctMethods.PrintMatrix(vals);
		
		DctComirva dctCom = new DctComirva(vals.Length, vals[0].Length);
		
		// dct
		double[][] dctVals = dctCom.dct(vals);
		DctMethods.PrintMatrix(dctVals);
		
		// idct
		double[][] idctVals = dctCom.idct(dctVals);
		DctMethods.PrintMatrix(idctVals);
	}
	#endregion
	
}
