using System;

// from http://code.google.com/p/dct-watermark/source/browse/trunk/src/net/watermark/DCT.java?r=2
public class DCT2 {
	
	public int N;
	public double[,] C;
	public double[,] Ct;

	public DCT2(int N) {
		this.N = N;
		this.C = new double[N,N];
		this.Ct = new double[N,N];
		
		int i;
		int j;
		double pi = Math.Atan(1.0) * 4.0;
		for (j = 0; j < N; j++) {
			this.C[0,j] = 1.0 / Math.Sqrt(N);
			this.Ct[j,0] = this.C[0,j];
		}
		for (i = 1; i < N; i++) {
			for (j = 0; j < N; j++) {
				this.C[i,j] = Math.Sqrt(2.0 / N) * Math.Cos(pi * (2 * j + 1) * i / (2.0 * N));
				this.Ct[j,i] = this.C[i,j];
			}
		}
	}

	public void DCT(double[,] input, double[,] output) {
		double[,] temp = new double[N,N];
		double temp1;
		int i, j, k;
		for (i = 0; i < N; i++) {
			for (j = 0; j < N; j++) {
				temp[i,j] = 0.0;
				for (k = 0; k < N; k++) {
					temp[i,j] += (input[i,k] - 128) * this.Ct[k,j];
				}
			}
		}

		for (i = 0; i < N; i++) {
			for (j = 0; j < N; j++) {
				temp1 = 0.0;
				for (k = 0; k < N; k++) {
					temp1 += this.C[i,k] * temp[k,j];
				}
				output[i,j] = (int) Math.Round(temp1);
			}
		}
	}

	public void IDCT(double[,] input, double[,] output) {
		double[,] temp = new double[N,N];
		double temp1;
		int i, j, k;

		for (i = 0; i < N; i++) {
			for (j = 0; j < N; j++) {
				temp[i,j] = 0.0;
				for (k = 0; k < N; k++) {
					temp[i,j] += input[i,k] * this.C[k,j];
				}
			}
		}

		for (i = 0; i < N; i++) {
			for (j = 0; j < N; j++) {
				temp1 = 0.0;
				for (k = 0; k < N; k++) {
					temp1 += this.Ct[i,k] * temp[k,j];
				}
				temp1 += 128.0;
				if (temp1 < 0) {
					output[i,j] = 0;
				} else if (temp1 > 255) {
					output[i,j] = 255;
				} else {
					output[i,j] = (int) Math.Round(temp1);
				}
			}
		}
	}
}