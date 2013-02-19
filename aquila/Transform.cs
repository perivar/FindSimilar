using System;
using System.Collections.Generic;
using System.Numerics; // for complex numbers
using System.Linq;

/**
 * @file Transform.cpp
 *
 * A few common signal transformations - implementation.
 *
 * The Transform class is a collection of methods used for
 * most important calculations like Fast Fourier Transform,
 * frame power, frame energy, Discrete Cosine Transform etc.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 0.2.0
 */
namespace Aquila
{
	/**
	 * A simple wrapper for few of the transformation options.
	 */
	public class TransformOptions
	{
		public int ZeroPaddedLength;
		public WindowType WindowType;
		public double PreemphasisFactor;
	}

	/**
	 * The transform class.
	 */
	public class Transform
	{
		/**
		 * ln(2) - needed for calculating number of stages in FFT.
		 */
		public const double LN_2 = 0.69314718055994530941723212145818;

		// PI
		public const double M_PI = Math.PI;
		
		/**
		 * Complex unit (0.0 + 1.0j).
		 */
		public readonly Complex j = new Complex();

		/**
		 * Frame length after padding with zeros.
		 */
		private int zeroPaddedLength;

		/**
		 * Preemphasis factor.
		 */
		private double preemphasisFactor;

		/**
		 * Window function type.
		 */
		private WindowType winType;
		
		/**
		 * Creates the transform object and explicitly sets the options.
		 *
		 * @param length spectrum length (a power of 2)
		 * @param window type of used window function (default is Hamming)
		 * @param factor preemphasis factor (default is 0.95)
		 */
		public Transform(int length, WindowType window) : this(length, window, 0.95)
		{
		}

		public Transform(int length) : this(length, WindowType.WIN_HAMMING, 0.95)
		{
		}

		public Transform(int length, WindowType window, double factor)
		{
			zeroPaddedLength = length;
			preemphasisFactor = factor;
			winType = window;
		}

		/**
		 * Creates the transform object and sets groupped options.
		 *
		 * @param options a struct of type TransformOptions
		 */
		public Transform(TransformOptions options)
		{
			zeroPaddedLength = options.ZeroPaddedLength;
			preemphasisFactor = options.PreemphasisFactor;
			winType = options.WindowType;
		}

		/**
		 * Makes sure no memory leaks are caused by the cache.
		 */
		public void Dispose()
		{
			ClearCosineCache();
			ClearFftWiCache();
		}

		/**
		 * Adds square of the second value to the first one.
		 *
		 * @param x initial value
		 * @param y squared value
		 * @return sum of initial value and squared value
		 */
		public static double AddSquare(double x, double y)
		{
			return x + y * y;
		}

		/**
		 * Calculates logarithm of a frame energy.
		 *
		 * Energy is a sum of squares of all samples in the frame.
		 *
		 * @param frame pointer to Frame object
		 * @return frame log energy (when greater than 0)
		 */
		public double FrameLogEnergy(Frame frame)
		{
			//double energy = std.accumulate(frame.begin(), frame.end(), 0.0, new SquareAndSum());
			double energy = 0;
			foreach(int i in frame) {
				energy += AddSquare(0.0, i);
			}
			
			return (energy > 0) ? Math.Log10(energy) : 0.0;
		}

		/**
		 * Calculates frame power.
		 *
		 * Frame power is the energy normalized by frame length.
		 *
		 * @param frame pointer to Frame object
		 * @return frame power
		 */
		public double FramePower(Frame frame)
		{
			//double energy = std.accumulate(frame.begin(), frame.end(), 0.0, new SquareAndSum());
			double energy = 0;
			foreach(int i in frame) {
				energy += AddSquare(0.0, i);
			}
			
			return energy / frame.GetLength();
		}

		/**
		 * Calculates Fast Fourier Transform using radix-2 algorithm.
		 *
		 * Input data is given as a const reference to the data vector.
		 * Output spectrum is written to the spectrum vector, which must be
		 * initialized prior to the call to fft(). The spectrum is
		 * normalized by N/2, wher N is input data length. The method
		 * returns maximum magnitude of the calculated spectrum, which
		 * can be used for example to scale a frequency plot.
		 *
		 * @param data const reference to input data vector
		 * @param spectrum initialized complex vector of the same length as data
		 * @return maximum magnitude of the spectrum
		 */
		public double Fft(double[] data, ref Complex[] spectrum)
		{
			// input signal size
			int N = data.Length;
			
			// bit-reversing the samples - a requirement of radix-2
			// instead of reversing in place, put the samples to result vector
			int a = 1;
			int b = 0;
			int c = 0;
			for (b = 1; b < N; ++b)
			{
				if (b < a)
				{
					spectrum[a - 1] = data[b - 1];
					spectrum[b - 1] = data[a - 1];
				}
				c = N / 2;
				while (c < a)
				{
					a -= c;
					c /= 2;
				}
				a += c;
			}
			
			// FFT calculation using "butterflies"
			// code ported from Matlab, based on book by Tomasz P. Zieliński
			
			// FFT stages count
			int numStages = (int)(Math.Log((double)N) / LN_2);
			
			// L = 2^k - DFT block length and offset
			// M = 2^(k-1) - butterflies per block, butterfly width
			// p - butterfly index
			// q - block index
			// r - index of sample in butterfly
			// Wi - starting value of Fourier base coefficient
			int L = 0;
			int M = 0;
			int p = 0;
			int q = 0;
			int r = 0;
			Complex Wi = new Complex(0, 0);
			Complex Temp = new Complex(0, 0);
			
			Complex[][] Wi_cache = GetCachedFftWi(numStages);
			
			// iterate over the stages
			for (int k = 1; k <= numStages; ++k)
			{
				L = 1 << k;
				M = 1 << (k - 1);
				Wi = Wi_cache[k][0];
				
				// iterate over butterflies
				for (p = 1; p <= M; ++p)
				{
					// iterate over blocks
					for (q = p; q <= N; q += L)
					{
						r = q + M;
						Temp = spectrum[r - 1] * Wi;
						spectrum[r - 1] = spectrum[q - 1] - Temp;
						spectrum[q - 1] = spectrum[q - 1] + Temp;
					}
					Wi = Wi_cache[k][p];
				}
			}
			
			double maxAbs = 0.0;
			double currAbs = 0.0;
			int N2 = N >> 1; // N/2
			
			// scaling by N/2 and searching for maximum magnitude
			// we can iterate only over the first half of the spectrum,
			// because of the symmetry, yet scaling is applied
			// to both halves
			for (int k = 0; k < N2; ++k)
			{
				spectrum[k] /= N2;
				spectrum[N2 + k] /= N2;
				currAbs = Complex.Abs(spectrum[k]);
				if ((currAbs > maxAbs))
					maxAbs = currAbs;
			}
			
			return maxAbs;
		}

		/**
		 * Calculates FFT of a signal frame using radix-2 algorithm.
		 *
		 * Input data is given as a pointer to Frame object.
		 * Output spectrum is written to the spectrum vector, which must be
		 * initialized prior to the call to fft(). The spectrum is
		 * normalized by N/2, wher N is input frame length (zero-padded).
		 * The method  returns maximum magnitude of the calculated spectrum,
		 * which can be used for example to scale a frequency plot.
		 *
		 * @param frame pointer to Frame object
		 * @param spectrum initialized complex vector of the same length as data
		 * @return maximum magnitude of the spectrum
		 * @since 2.0.1
		 */
		public double Fft(Frame frame, ref Complex[] spectrum)
		{
			// the vector is initialized to zero padded length,
			// what means that it contains default values of contained type
			// (0.0 in case of double); that allows us to loop
			// only to frame length without padding and
			// automatically have zeros at the end of data
			double[] data = new double[zeroPaddedLength];
			int length = frame.GetLength();
			
			short[] frameArray = frame.ToArray();
			
			// first sample does not need preemphasis
			data[0] = frameArray[0];

			double current = 0.0;
			double previous = data[0];
			
			// iterate over all samples of the frame
			// filter the data through preemphasis
			// and apply a chosen window function
			for (int n = 1; n < length; n++)
			{
				current = frameArray[n];
				data[n] = (current - preemphasisFactor * previous) * Window.Apply(winType, n, length);
				previous = current;
			}
			
			return Fft(data, ref spectrum);
		}

		/**
		 * Calculates the Discrete Cosine Transform.
		 *
		 * Uses cosine value caching in order to speed up computations.
		 *
		 * @param data input data vector
		 * @param output initialized vector of output values
		 */
		public void Dct(double[] data, ref double[] output)
		{
			// output size determines how many coefficients will be calculated
			int outputLength = output.Length;
			int inputLength = data.Length;
			
			// DCT scaling factor
			double c0 = Math.Sqrt(1.0 / inputLength);
			double cn = Math.Sqrt(2.0 / inputLength);
			
			// cached cosine values
			double[][] cosines = GetCachedCosines(inputLength, outputLength);
			
			for (int n = 0; n < outputLength; ++n)
			{
				output[n] = 0.0;
				for (int k = 0; k < inputLength; ++k)
				{
					// 1e-10 added for the logarithm value to be grater than 0
					//output[n] += log(fabs(data[k]) + 1e-10) * cosines[n][k];
					output[n] += Math.Log(Math.Abs(data[k]) + 1e-10) * cosines[n][k];
				}
				
				output[n] *= (0 == n) ? c0 : cn;
			}
		}
		
		/**
		 * Cache object, implemented as a map.
		 */
		private Dictionary<KeyValuePair<int, int>, double[][]> cosineCache = new Dictionary<KeyValuePair<int, int>, double[][]>();
		
		/**
		 * Returns a table of DCT cosine values stored in memory cache.
		 *
		 * The two params unambigiously identify which cache to use.
		 *
		 * @param inputLength length of the input vector
		 * @param outputLength length of the output vector
		 * @return pointer to array of pointers to arrays of doubles
		 */
		private double[][] GetCachedCosines(int inputLength, int outputLength)
		{
			KeyValuePair<int, int> key = new KeyValuePair<int, int>(inputLength, outputLength);
			
			// if we have that key cached, return immediately!
			if (cosineCache.ContainsKey(key))
			{
				return cosineCache[key];
			}
			
			// nothing in cache for that pair, calculate cosines
			double[][] cosines = new double[outputLength][];
			for (int n = 0; n < outputLength; ++n)
			{
				cosines[n] = new double[inputLength];
				
				for (uint k = 0; k < inputLength; ++k)
				{
					// from the definition of DCT
					cosines[n][k] = Math.Cos((M_PI * (2 *k+1) * n) / (2 * inputLength));
				}
			}
			
			// store in cache and return
			cosineCache[key] = cosines;
			
			return cosines;
		}

		/**
		 * Deletes all the memory used by cache.
		 */
		private void ClearCosineCache()
		{
			foreach (var pair in cosineCache) {
				KeyValuePair<int, int> key = pair.Key;
				double[][] cosines = pair.Value;
				
				int outputLength = key.Value;
				for (int i = 0; i < outputLength; ++i)
				{
					cosines[i] = null;
				}
				cosines = null;
			}
		}

		/**
		 * Twiddle factor cache - implemented as a map.
		 */
		private Dictionary<int, Complex[][]> fftWiCache = new Dictionary<int, Complex[][]>();

		/**
		 * Returns a table of Wi (twiddle factors) stored in cache.
		 *
		 * @param numStages the FFT stages count
		 * @return pointer to an array of pointers to arrays of complex numbers
		 */
		private Complex[][] GetCachedFftWi(int numStages)
		{
			int key = numStages;
			
			// cache hit, return immediately
			if (fftWiCache.ContainsKey(key)) {
				return fftWiCache[key];
			}
			
			// nothing in cache, calculate twiddle factors
			Complex[][] Wi = new Complex[numStages+1][];
			for (int k = 1; k <= numStages; ++k)
			{
				// L = 2^k - DFT block length and offset
				// M = 2^(k-1) - butterflies per block, butterfly width
				// W - Fourier base multiplying factor
				int L = 1 << k;
				int M = 1 << (k-1);
				//Complex W = Math.Exp((-j) * 2.0 * M_PI / (double)L);
				Complex W = Math.Exp((-j.Real) * 2.0 * M_PI / (double)L);
				Wi[k] = new Complex[M+1];
				//Wi[k][0] = Complex(1.0);
				Wi[k][0] = Complex.One;
				for (uint p = 1; p <= M; ++p)
				{
					Wi[k][p] = Wi[k][p-1] * W;
				}
			}
			
			// store in cache and return
			fftWiCache[key] = Wi;
			
			return Wi;
		}

		/**
		 * Clears the twiddle factor cache.
		 */
		private void ClearFftWiCache()
		{
			foreach (var pair in fftWiCache) {
				int NumStages = pair.Key;
				Complex[][] c = pair.Value;
				for (int i = 0; i < NumStages; ++i)
				{
					c[i] = null;
				}
				c = null;
			}
		}
	}
}

