using System;
using CommonUtils;
using Comirva.Audio.Util.Maths;

/// <summary>
/// Discrete Cosine Transform using Comirva.
/// </summary>
public class DctComirva : DctInterface
{
	private Matrix dct;
	private int rows;
	private int columns;
	
	public DctComirva(int rows, int columns)
	{
		this.rows = rows;
		this.columns = columns;
		
		// Compute the DCT
		// This whole section is copied from GetDCTMatrix() from CoMirva package
		dct = new Matrix(rows, columns);
		
		// compute constants
		// For two-dimensional DCT-II and inverse DCT, columns is 2
		// http://unix4lyfe.org/dct/
		double PIdividedByColumns = Math.PI / columns;
		double uZero = 1.0 / (Math.Sqrt(columns));
		double uNotZero = Math.Sqrt(2.0 / columns);

		// Generate 1D DCT-II matrix
		for(int u = 0; u < rows; u++)  {
			for(int x = 0; x < columns; x++) {
				if(u == 0) {
					dct.Set(u, x, uZero * Math.Cos(u * PIdividedByColumns * (x + 0.5d)));
				} else {
					dct.Set(u, x, uNotZero * Math.Cos(u * PIdividedByColumns * (x + 0.5d)));
				}
			}
		}

		// Generate 1D DCT-III matrix (inverse DCT-II)

		#if DEBUG
		dct.DrawMatrixGraph("dct-comirva.png");
		#endif
	}
	
	public double[,] Dct(double[,] f)
	{
		// convert two dimensional data to a Comirva Matrix
		Matrix mat = new Matrix(f, rows, columns);
		#if DEBUG
		mat.WriteText("dct-before.txt");
		mat.DrawMatrixGraph("dct-before.png");
		#endif
		
		// Perform the DCT (Discrete Cosine Transform)
		Matrix dctResult = dct * mat;
		#if DEBUG
		dctResult.WriteText("dct-after.txt");
		dctResult.DrawMatrixGraph("dct-after.png");
		#endif

		// return as two dimensional array
		return dctResult.GetTwoDimensionalArray();
	}
	
	public double[,] InverseDct(double[,] F)
	{
		// convert two dimensional data to a Comirva Matrix
		Matrix mat = new Matrix(F, rows, columns);
		#if DEBUG
		mat.WriteText("idct-before.txt");
		#endif

		// Perform the IDCT (Inverse Discrete Cosine Transform)
		Matrix iDctResult = mat / dct;
		#if DEBUG
		iDctResult.WriteText("idct-after.txt");
		#endif

		// return as two dimensional array
		return iDctResult.GetTwoDimensionalArray();
	}
}
