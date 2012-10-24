using System;

// taken from http://stackoverflow.com/questions/4240490/problems-with-dct-and-idct-algorithm-in-java
public class DCT {
	
	private int N;
	private double[] c;

	public DCT(int N) {
		this.N = N;
		
		this.initializeCoefficients();
	}

	private void initializeCoefficients() {
		c = new double[N];

		for (int i=1;i<N;i++) {
			c[i]=1;
		}
		c[0]=1/Math.Sqrt(2.0);
	}

	public double[,] ApplyDCT(double[,] f) {
		double[,] F = new double[N,N];
		for (int u=0;u<N;u++) {
			for (int v=0;v<N;v++) {
				double sum = 0.0;
				for (int i=0;i<N;i++) {
					for (int j=0;j<N;j++) {
						sum+=Math.Cos(((2*i+1)/(2.0*N))*u*Math.PI)*Math.Cos(((2*j+1)/(2.0*N))*v*Math.PI)*f[i,j];
					}
				}
				
				// This only works for a 8x8 bloc of data, or else you would have to change this:
				// (c[u]*c[v])/4.0)
				// into something like this:
				// (2*c[u]*c[v])/Math.Sqrt(M*N)
				//Where M and N are the dimentions of the table
				//sum*=((c[u]*c[v])/4.0);
				sum*=(2*c[u]*c[v])/Math.Sqrt(N*N);
				
				F[u,v]=sum;
			}
		}
		return F;
	}

	public double[,] ApplyIDCT(double[,] F) {
		double[,] f = new double[N,N];
		for (int i=0;i<N;i++) {
			for (int j=0;j<N;j++) {
				double sum = 0.0;
				for (int u=0;u<N;u++) {
					for (int v=0;v<N;v++) {
						// This only works for a 8x8 bloc of data, or else you would have to change this:
						// (c[u]*c[v])/4.0)
						// into something like this:
						// (2*c[u]*c[v])/Math.Sqrt(M*N)
						//Where M and N are the dimentions of the table
						//sum+=(c[u]*c[v])/4.0*Math.Cos(((2*i+1)/(2.0*N))*u*Math.PI)*Math.Cos(((2*j+1)/(2.0*N))*v*Math.PI)*F[u,v];
						sum+=(2*c[u]*c[v])/Math.Sqrt(N*N)*Math.Cos(((2*i+1)/(2.0*N))*u*Math.PI)*Math.Cos(((2*j+1)/(2.0*N))*v*Math.PI)*F[u,v];
					}
				}
				f[i,j]=Math.Round(sum);
			}
		}
		return f;
	}
}