using System;
using CommonUtils;

/// <summary>
/// Mimics the dct2 and idct2 methods from Octave (two-dimensional DCT-II and inverse DCT - DCT-III)
/// dct2 	= Computes the 2-D discrete cosine transform of matrix x
/// idct2 	= Computes the inverse 2-D discrete cosine transform of matrix x
/// </summary>
/// <remarks>taken from http://stackoverflow.com/questions/4240490/problems-with-dct-and-idct-algorithm-in-java</remarks>
public class Dct2 : DctInterface {
	
	private int N;
	private double[] c;

	public Dct2(int N) {
		this.N = N;
		this.initializeCoefficients();
	}

	// Some authors further multiply the X0 term by 1/√2
	// and multiply the resulting matrix by an overall scale factor of √(2/N)
	// This makes the DCT-II matrix orthogonal, but breaks the direct correspondence with a real-even DFT of half-shifted input.
	private void initializeCoefficients() {
		c = new double[N];

		for (int i = 1; i < N; i++) {
			c[i] = 1;
		}
		c[0] = 1 / Math.Sqrt(2.0);
	}

	// http://unix4lyfe.org/dct/
	public double[,] Dct(double[,] S) {
		
		double[,] F = new double[N, N];
		for (int v = 0; v < N; v++) {
			for (int u = 0; u < N; u++) {
				
				double sum = 0.0;
				for (int y = 0; y < N; y++) {
					for (int x = 0; x < N; x++) {
						sum +=
							Math.Cos( ( v * Math.PI * (2.0 * y + 1) / (2.0 * N) ) ) * 
							Math.Cos( ( u * Math.PI * (2.0 * x + 1) / (2.0 * N) ) ) * 
							S[x, y];
					}
				}
				
				// This only works for a 8x8 bloc of data, or else you would have to change this:
				// (c[u]*c[v])/4.0)
				// into something like this:
				// (2*c[u]*c[v])/Math.Sqrt(M*N)
				//Where M and N are the dimentions of the table
				//sum*=((c[u]*c[v])/4.0);
				sum *= (2 * c[u] * c[v]) / Math.Sqrt(N * N);
				
				F[u, v] = sum;
			}
		}
		return F;
	}

	// http://unix4lyfe.org/dct/
	public double[,] InverseDct(double[,] F) {
		
		double[,] S = new double[N, N];
		for (int y = 0; y < N; y++) {
			for (int x = 0; x < N; x++) {
				
				double sum = 0.0;
				for (int v = 0; v < N; v++) {
					for (int u = 0; u < N; u++) {
						// This only works for a 8x8 bloc of data, or else you would have to change this:
						// (c[u]*c[v])/4.0)
						// into something like this:
						// (2*c[u]*c[v])/Math.Sqrt(M*N)
						//Where M and N are the dimentions of the table
						//sum+=(c[u]*c[v])/4.0*Math.Cos(((2*i+1)/(2.0*N))*u*Math.PI)*Math.Cos(((2*j+1)/(2.0*N))*v*Math.PI)*F[u,v];
						sum+=( 2 * c[v] * c[u] ) / Math.Sqrt(N * N) *
							Math.Cos( ( (2.0 * y + 1 ) / (2.0 * N) ) * v * Math.PI) * 
							Math.Cos( ( (2.0 * x + 1 ) / (2.0 * N) ) * u * Math.PI) *
							F[v , u];
					}
				}
				S[y, x] = Math.Round(sum);
			}
		}
		return S;
	}
}