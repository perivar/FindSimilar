using System;
using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;
using System.Text;
using Wavelets;

// see:
// http://ubio.bioinfo.cnio.es/biotools/cpas/INSTALL/src/webapps/tools/modwt/Transform.java
// http://ubio.bioinfo.cnio.es/biotools/cpas/INSTALL/src/webapps/tools/modwt/Filter.java
// http://read.pudn.com/downloads182/sourcecode/graph/854377/SharpWavelet.cs__.htm
namespace Wavelets
{
	public enum WaveletFilterType
	{
		Haar,
		D4,
		D6,
		D8,
		D12,
		LA8,
		LA16
	}

	public enum WaveletTransformType
	{
		DWT,
		MODWT
	}

	public enum WaveletBoundaryCondition
	{
		Period,
		Reflect
	}

	public enum WaveletOperation
	{
		Decomposition,
		MultiResolutionAnalysis
	}

	public class WaveletTransform
	{
		private WaveletFilter f;
		
		public WaveletTransform(WaveletFilter filter)
		{
			this.f = filter;
		}
		
		private static int wrap(int i, int length)
		{
			if (i >= length) return i - length;
			if (i < 0) return i + length;
			return i;
		}
		
		/// <summary>
		/// Compute the discrete wavelet transform (DWT).  This method uses the
		/// pyramid algorithm and was adapted from pseudo-code written by
		/// D. B. Percival.  Periodic boundary conditions are assumed.
		/// </summary>
		/// <param name="Vin">vector of wavelet smooths (data if first iteration)</param>
		/// <param name="M">length of Vin</param>
		/// <param name="Wout">vector of wavelet coefficients</param>
		/// <param name="Vout">vector of wavelet smooths</param>
		private void dwt(double[] Vin, int M, double[] Wout, double[] Vout)
		{
			//Wout = new double[M];
			//Vout = new double[M];
			for (int t = 0; t < M / 2; t++)
			{
				Wout[t] = Vout[t] = 0.0;
				for (int l = 0, k = 2 * t + 1; l < f.L; l++)
				{
					Wout[t] += f.H[l] * Vin[k];
					Vout[t] += f.G[l] * Vin[k];
					k = k == 0 ? M - 1 : k - 1;
				}
			}
		}
		
		public static void dwt(double[] Vin, int M, WaveletFilter f, double[] Wout, double[] Vout) {
			WaveletTransform transform = new WaveletTransform(f);
			transform.dwt(Vin, M, Wout, Vout);
		}
		
		/// <summary>
		/// Compute the inverse DWT via the pyramid algorithm.  This code was
		/// adapted from pseudo-code written by D. B. Percival.  Periodic
		/// boundary conditions are assumed.
		/// </summary>
		/// <param name="Win">vector of wavelet coefficients</param>
		/// <param name="Vin">vector of wavelet smooths</param>
		/// <param name="M">length of Win, Vin</param>
		/// <param name="Xout">vector of reconstructed wavelet smooths (eventually the data)</param>
		private void idwt(double[] Win, double[] Vin, int M, double[] Xout)
		{
			//Xout = new double[M];
			for (int t = 0, m = 0, n = 1; t < M; t++)
			{
				int u = t;
				int i = 1;
				int j = 0;
				Xout[m] = Xout[n] = 0.0;
				for (int l = 0; l < f.L / 2; l++)
				{
					Xout[m] += f.H[i] * Win[u] + f.G[i] * Vin[u];
					Xout[n] += f.H[j] * Win[u] + f.G[j] * Vin[u];
					u = u + 1 >= M ? 0 : u + 1;
					i += 2;
					j += 2;
				}
				m += 2;
				n += 2;
			}
		}
		
		/// <summary>
		/// Compute the maximal overlap discrete wavelet transform (MODWT).
		/// This method uses the pyramid algorithm and was adapted from
		/// pseudo-code written by D. B. Percival.
		/// </summary>
		/// <param name="Vin">vector of wavelet smooths (data if j=1)</param>
		/// <param name="N">length of Vin to analyze</param>
		/// <param name="k">iteration (1, 2, ...)</param>
		/// <param name="Wout">vector of wavelet coefficients</param>
		/// <param name="Vout">vector of wavelet smooths</param>
		private void modwt(double[] Vin, int N, int k, double[] Wout, double[] Vout)
		{
			//Wout = new double[N];
			//Vout = new double[N];
			double[] ht = new double[f.L];
			double[] gt = new double[f.L];
			double inv_sqrt_2 = 1.0 / Math.Sqrt(2.0);
			int pow2_k = (int) Math.Pow(2.0, k - 1);

			for (int l = 0; l < f.L; l++)
			{
				ht[l] = f.H[l] * inv_sqrt_2;
				gt[l] = f.G[l] * inv_sqrt_2;
			}

			for (int t = 0; t < N; t++)
			{
				int j = t;
				Wout[t] = Vout[t] = 0.0;
				for (int l = 0; l < f.L; l++)
				{
					Wout[t] += ht[l] * Vin[j];
					Vout[t] += gt[l] * Vin[j];
					j = wrap(j - pow2_k, N);
				}
			}
		}

		/// <summary>
		/// Compute the inverse MODWT via the pyramid algorithm.  Adapted from
		/// pseudo-code written by D. B. Percival.
		/// </summary>
		/// <param name="Win">vector of wavelet coefficients</param>
		/// <param name="Vin">vector of wavelet smooths</param>
		/// <param name="N">length of Win, Vin</param>
		/// <param name="k">detail number</param>
		/// <param name="Vout">vector of wavelet smooths</param>
		private void imodwt(double[] Win, double[] Vin, int N, int k, double[] Vout)
		{
			//Vout = new double[N];
			double[] ht = new double[f.L];
			double[] gt = new double[f.L];
			int pow2_k = (int) Math.Pow(2.0, k - 1);
			double inv_sqrt_2 = 1.0 / Math.Sqrt(2.0);

			for (int l = 0; l < f.L; l++)
			{
				ht[l] = f.H[l] * inv_sqrt_2;
				gt[l] = f.G[l] * inv_sqrt_2;
			}

			for (int t = 0; t < N; t++)
			{
				int j = t;
				Vout[t] = 0.0;
				for (int l = 0; l < f.L; l++)
				{
					Vout[t] += (ht[l] * Win[j]) + (gt[l] * Vin[j]); // GMA
					j = wrap(j + pow2_k, N);                        // GMA
				}
			}
		}

		/// <summary>
		/// The functions for computing wavelet transforms assume periodic
		/// boundary conditions, regardless of the data's true nature.  By
		/// adding a `backwards' version of the data to the end of the current
		/// data vector, we are essentially reflecting the data.  This allows
		/// the periodic methods to work properly.
		/// </summary>
		/// <param name="Xin">vector of input data</param>
		/// <param name="N">length of Xin</param>
		/// <param name="Xout">vector of output data</param>
		protected void reflect_vector(double[] Xin, int N, double[] Xout)
		{
			int t;
			
			for (t = 0; t < N; t++)
				Xout[t] = Xin[t];
			for (t = 0; t < N; t++)
				Xout[N + t] = Xin[N - t - 1];
		}

		/// <summary>
		/// Perform the discrete wavelet transform (DWT) or maximal overlap
		/// discrete wavelet transform (MODWT) to a time-series and obtain a
		/// specified number (K) of wavelet coefficients and subsequent
		/// wavelet smooth.  Reflection is used as the default boundary
		/// condition.
		/// </summary>
		/// <param name="X">time-series (vector of data)</param>
		/// <param name="N">length of X</param>
		/// <param name="K">number of details desired</param>
		/// <param name="transform">WaveletTransformType, DWT or MODWT</param>
		/// <param name="boundary">boundary condition (either "period" or "reflect")</param>
		/// <param name="Xout">matrix of wavelet coefficients and smooth (length = N for "period" and 2N for "reflect")</param>
		public void decompose(double[] X, int N, int K, WaveletTransformType transform,
		                      WaveletBoundaryCondition boundary, out double[,] Xout)
		{
			
			int i, k;
			int scale, length;
			double[] Wout, Vout, Vin;
			
			// Dyadic vector length required for DWT
			if (transform == WaveletTransformType.DWT && N%2 != 0)
				throw new InvalidDataException("...data must have dyadic length for DWT...");
			
			// The choice of boundary methods affect things...
			if (boundary == WaveletBoundaryCondition.Reflect)
			{
				length = 2*N;
				scale = length;
				Wout = new double[length];
				Vout = new double[length];
				Vin = new double[length];
				Xout = new double[length, K];
				reflect_vector(X, N, Vin);
				/* printf("Boundary Treatment is REFLECTION...\n"); */
			}
			else
			{
				length = N;
				scale = N;
				Wout = new double[length];
				Vout = new double[length];
				Vin = new double[length];
				Xout = new double[length, K];
				for (i=0; i < length; i++)
					Vin[i] = X[i];
				/* printf("Boundary Treatment is PERIODIC...\n"); */
			}
			/* printf("##### Length = %d #####\n", length); */
			
			for (k=0; k < K; k++)
			{
				for (i=0; i < length; i++)
				{
					Wout[i] = 0.0;
					Vout[i] = 0.0;
				}
				if (transform == WaveletTransformType.DWT)
				{
					dwt(Vin, scale, Wout, Vout);
				}
				else if (transform == WaveletTransformType.MODWT)
				{
					modwt(Vin, length, k, Wout, Vout);
				}
				
				for (i = 0; i < N; i++) // N
					Xout[i,k] = Wout[i];
				for (i=0; i < length; i++) // length
					Vin[i] = Vout[i];
				scale -= scale/2;
			}
			for (i=0; i < length; i++) Xout[i,K-1] = Wout[i]; // GMA
		}

		/// <summary>
		/// Perform a multiresolution analysis using the DWT or MODWT matrix
		/// obtained from `decompose.'  The inverse transform will be applied
		/// to selected wavelet detail coefficients.  The wavelet smooth
		/// coefficients from the original transform are added to the K+1
		/// column in order to preserve the additive decomposition.
		/// Reflection is used as the default boundary condition.
		/// </summary>
		/// <param name="Xin">matrix from `decompose'</param>
		/// <param name="N">number of rows in Xin</param>
		/// <param name="K">number of details in Xin</param>
		/// <param name="transform">WaveletTransformType, DWT or MODWT</param>
		/// <param name="boundary">boundary condition (either "period" or "reflect")</param>
		/// <param name="Xmra">matrix containg K wavelet details and 1 wavelet smooth</param>
		public void multiresolution(double[,] Xin, int N, int K, WaveletTransformType transform,
		                            WaveletBoundaryCondition boundary, out double[,] Xmra)
		{
			int i, k, t, length;
			double[] zero, Xout, Win;
			
			if (boundary == WaveletBoundaryCondition.Reflect)
				length = 2*N;
			else
				length = N;
			zero = new double[length];
			Xout = new double[length];
			Win = new double[length];
			Xmra = new double[N,K+1];
			
			//printf("##### Length = %d #####\n", length);
			
			for (k=0; k < K; k++)
			{
				for (t = 0; t < length; t++)
				{
					Win[t] = Xin[t,k];
					zero[t] = 0.0;
				}
				if (transform == WaveletTransformType.DWT)
					idwt(Win, zero, (int) (N/Math.Pow(2, k)), Xout);
				else if (transform == WaveletTransformType.MODWT)
					imodwt(Win, zero, length, k, Xout);
				
				for (i = k ; i >= 0; i--)
				{
					for (t = 0; t < length; t++)
					{
						Win[t] = Xout[t];
					}
					
					if (transform == WaveletTransformType.DWT)
						idwt(zero, Win, (int) (N/Math.Pow(2, i)), Xout);
					else
						imodwt(zero, Win, length, i, Xout);
				}
				for (t = 0; t < N; t++) Xmra[t,k] = Xout[t];
			}
			
			// One more iteration is required on the wavelet smooth coefficients
			// to complete the additive decomposition.
			for (t = 0; t < length; t++)
			{
				Win[t] = Xin[t,K-1];
				zero[t] = 0.0;
			}
			if (transform == WaveletTransformType.DWT)
				idwt(zero, Win, (int) (N/Math.Pow(2, K)), Xout);
			else
				imodwt(zero, Win, length, K, Xout);
			for (i = K; i >= 0; i--)
			{
				for (t = 0; t < length; t++)
					Win[t] = Xout[t];
				if (transform == WaveletTransformType.DWT)
					idwt(zero, Win, (int) (N/Math.Pow(2, i)), Xout);
				else
					imodwt(zero, Win, length, i, Xout);
				for (t = 0; t < length; t++)
				{
					Win[t] = Xout[t];
					zero[t] = 0.0;
				}
			}
			for (t = 0; t < N; t++) Xmra[t,K] = Xout[t];
		}
	}

	public class WaveletFilter
	{
		#region Wavelet Scaling Coefficients
		
		private static readonly double[] hhaar = { 0.7071067811865475, -0.7071067811865475 };
		private static readonly double[] ghaar = { 0.7071067811865475, 0.7071067811865475 };
		
		private static readonly double[] hd4 = {
			-0.1294095225512603, -0.2241438680420134,
			0.8365163037378077, -0.4829629131445341
		};
		
		private static readonly double[] gd4 = {
			0.4829629131445341, 0.8365163037378077,
			0.2241438680420134, -0.1294095225512603
		};
		
		private static readonly double[] hd6 = {
			0.0352262918857096, 0.0854412738820267,
			-0.1350110200102546, -0.4598775021184915,
			0.8068915093110928, -0.3326705529500827
		};
		
		private static readonly double[] gd6 = {
			0.3326705529500827, 0.8068915093110928,
			0.4598775021184915, -0.1350110200102546,
			-0.0854412738820267, 0.0352262918857096
		};
		
		private static readonly double[] hd8 = {
			-0.0105974017850021, -0.0328830116666778,
			0.0308413818353661, 0.1870348117179132,
			-0.0279837694166834, -0.6308807679358788,
			0.7148465705484058, -0.2303778133074431
		};
		
		private static readonly double[] gd8 = {
			0.2303778133074431, 0.7148465705484058,
			0.6308807679358788, -0.0279837694166834,
			-0.1870348117179132, 0.0308413818353661,
			0.0328830116666778, -0.0105974017850021
		};
		
		private static readonly double[] hd12 = {
			-0.0010773010853085, -0.0047772575109455,
			0.0005538422011614, 0.0315820393174862,
			0.0275228655303053, -0.0975016055873224,
			-0.1297668675672624, 0.2262646939654399,
			0.3152503517091980, -0.7511339080210954,
			0.4946238903984530, -0.1115407433501094
		};
		
		private static readonly double[] gd12 = {
			0.1115407433501094, 0.4946238903984530,
			0.7511339080210954, 0.3152503517091980,
			-0.2262646939654399, -0.1297668675672624,
			0.0975016055873224, 0.0275228655303053,
			-0.0315820393174862, 0.0005538422011614,
			0.0047772575109455, -0.0010773010853085
		} ;
		
		private static readonly double[] hla8 = {
			0.03222310060407815, 0.01260396726226383,
			-0.09921954357695636, -0.29785779560560505,
			0.80373875180538600, -0.49761866763256290,
			-0.02963552764596039, 0.07576571478935668
		};
		
		private static readonly double[] gla8 = {
			-0.07576571478935668, -0.02963552764596039,
			0.49761866763256290, 0.80373875180538600,
			0.29785779560560505, -0.09921954357695636,
			-0.01260396726226383, 0.03222310060407815,
		};
		
		private static readonly double[] hla16 = {
			0.0018899503329007, 0.0003029205145516,
			-0.0149522583367926, -0.0038087520140601,
			0.0491371796734768, 0.0272190299168137,
			-0.0519458381078751, -0.3644418948359564,
			0.7771857516997478, -0.4813596512592012,
			-0.0612733590679088, 0.1432942383510542,
			0.0076074873252848, -0.0316950878103452,
			-0.0005421323316355, 0.0033824159513594
		};
		
		private static readonly double[] gla16 = {
			-0.0033824159513594, -0.0005421323316355,
			0.0316950878103452, 0.0076074873252848,
			-0.1432942383510542, -0.0612733590679088,
			0.4813596512592012, 0.7771857516997478,
			0.3644418948359564, -0.0519458381078751,
			-0.0272190299168137, 0.0491371796734768,
			0.0038087520140601, -0.0149522583367926,
			-0.0003029205145516, 0.0018899503329007
		};
		#endregion
		
		private double[] h;
		private double[] g;
		private int l;
		private WaveletFilterType filterType;
		
		/// <summary>
		/// Determines which wavelet Filter, and corresponding scaling Filter,
		/// based on the WaveletFilterType `fType.'
		/// fills in h, g, and L for a wavelet Filter structure
		/// </summary>
		/// <param name="fType">WaveletFilterType (e.g., "haar", "d4, "d6", "d8", "la8", "la16")</param>
		public WaveletFilter(WaveletFilterType fType)
		{
			filterType = fType;
			
			double[] hMatrix = null;
			double[] gMatrix = null;
			
			if (fType == WaveletFilterType.Haar)
			{
				L = 2;
				hMatrix = hhaar;
				gMatrix = ghaar;
			}
			else if (fType == WaveletFilterType.D4)
			{
				L = 4;
				hMatrix = hd4;
				gMatrix = gd4;
			}
			else if (fType == WaveletFilterType.D6)
			{
				L = 6;
				hMatrix = hd6;
				gMatrix = gd6;
			}
			else if (fType == WaveletFilterType.D8)
			{
				L = 8;
				hMatrix = hd8;
				gMatrix = gd8;
			}
			else if (fType == WaveletFilterType.D12)
			{
				L = 12;
				hMatrix = hd12;
				gMatrix = gd12;
			}
			else if (fType == WaveletFilterType.LA8)
			{
				L = 8;
				hMatrix = hla8;
				gMatrix = gla8;
			}
			else if (fType == WaveletFilterType.LA16)
			{
				L = 16;
				hMatrix = hla16;
				gMatrix = gla16;
			}
			
			if (hMatrix == null || gMatrix == null)
				throw new NullReferenceException("no wavelet filter selected");
			
			H = new double[L];
			G = new double[L];
			Array.Copy(hMatrix, H, L);
			Array.Copy(gMatrix, G, L);
		}
		
		/// <summary>
		/// Determines which wavelet Filter, and corresponding scaling Filter,
		/// based on the character string `choice.'
		/// fills in h, g, and L for a wavelet Filter structure
		/// </summary>
		/// <param name="choice">character string (e.g., "haar", "d4, "d6", "d8", "la8", "la16")</param>
		public WaveletFilter(string choice)
		{
			double[] hMatrix = null;
			double[] gMatrix = null;
			
			if ("haar".Equals(choice))
			{
				L = 2;
				hMatrix = hhaar;
				gMatrix = ghaar;
			}
			else if ("d4".Equals(choice))
			{
				L = 4;
				hMatrix = hd4;
				gMatrix = gd4;
			}
			else if ("d6".Equals(choice))
			{
				L = 6;
				hMatrix = hd6;
				gMatrix = gd6;
			}
			else if ("d8".Equals(choice))
			{
				L = 8;
				hMatrix = hd8;
				gMatrix = gd8;
			}
			else if ("la8".Equals(choice))
			{
				L = 8;
				hMatrix = hla8;
				gMatrix = gla8;
			}
			else if ("la16".Equals(choice))
			{
				L = 16;
				hMatrix = hla16;
				gMatrix = gla16;
			}
			else
				throw new NotImplementedException("...unimplemented wavelet choice...");

			if (hMatrix == null || gMatrix == null)
				throw new NullReferenceException("no wavelet filter selected");
			
			H = new double[L];
			G = new double[L];
			Array.Copy(hMatrix, H, L);
			Array.Copy(gMatrix, G, L);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Wavelet Filter (L = ").Append(L).Append("):\n");
			sb.Append("  h := \n");
			PrintVector(sb, h);
			sb.Append("  g := \n");
			PrintVector(sb, g);
			return sb.ToString();
		}

		private static void PrintVector(StringBuilder sb, double[] v)
		{
			for (int i = 0; i < v.Length; i++)
			{
				sb.Append(v[i]).Append(' ');
			}
			sb.Append('\n');
		}
		
		public int L
		{
			get { return l; }
			set { l = value; }
		}
		
		public double[] G
		{
			get { return g; }
			set { g = value; }
		}
		
		public double[] H
		{
			get { return h; }
			set { h = value; }
		}
		
		/// <summary>
		/// This function converts the basic Haar wavelet filter
		/// (h0 = -0.7017, h1 = 0.7017) into a filter of length `scale.'
		/// The filter is re-normalized so that it's sum of squares equals 1.
		/// Output: Fills in h, g, and L for a wavelet filter structure
		/// </summary>
		/// <param name="f">haar wavelet filter structure</param>
		/// <param name="scale">integer</param>
		public void convert_haar(WaveletFilter f, int scale)
		{
			if (f.filterType != WaveletFilterType.Haar)
				throw new InvalidDataException("must be a haar wavelet structure");
			
			double sqrt_scale = Math.Sqrt(scale);
			
			this.L = (int) f.L*scale;
			this.H = new double[f.L*scale + 1];
			this.G = new double[f.L*scale + 1];
			
			for (int l = 1; l <= scale; l++)
			{
				this.H[l] = f.H[1]/sqrt_scale;
				this.G[l] = f.G[1]/sqrt_scale;
				this.H[l + scale] = f.H[2]/sqrt_scale;
				this.G[l + scale] = f.G[2]/sqrt_scale;
			}
		}
	}
}