using System;
using Wavelet = math.transform.jwave.handlers.wavelets.Wavelet;
using WaveletInterface = math.transform.jwave.handlers.wavelets.WaveletInterface;

namespace math.transform.jwave.handlers
{
	///
	// * Base class for the forward and reverse Fast Wavelet Transform in 1-D, 2-D,
	// * and 3-D using a specified Wavelet by inheriting class.
	// * 
	// * @date 10.02.2010 08:10:42
	// * @author Christian Scheiblich
	public class FastWaveletTransform : WaveletTransform
	{
		//   * Constructor receiving a Wavelet object.
		//   * 
		//   * @date 10.02.2010 08:10:42
		//   * @author Christian Scheiblich
		//   * @param wavelet
		//   *          object of type Wavelet; Haar02, Daub02, Coif06, ...
		public FastWaveletTransform(WaveletInterface wavelet) : base(wavelet)
		{
		} // FastWaveletTransform

		//   * Constructor receiving a Wavelet object.
		//   * 
		//   * @date 10.02.2010 08:10:42
		//   * @author Christian Scheiblich
		//   * @param wavelet
		//   *          object of type Wavelet; Haar02, Daub02, Coif06, ...
		public FastWaveletTransform(WaveletInterface wavelet, int iteration) : base(wavelet, iteration)
		{
		} // FastWaveletTransform

		//   * Performs the 1-D forward transform for arrays of dim N from time domain to
		//   * Hilbert domain for the given array using the Fast Wavelet Transform (FWT)
		//   * algorithm.
		//   * 
		//   * @date 10.02.2010 08:23:24
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.BasicTransform#forward(double[])
		public override double[] forwardWavelet(double[] arrTime)
		{
			double[] arrHilb = new double[arrTime.Length];
			for(int i = 0; i < arrTime.Length; i++)
				arrHilb[i] = arrTime[i];

			int level = 0;
			int h = arrTime.Length;
			int minWaveLength = _wavelet.getWaveLength();
			if(h >= minWaveLength)
			{
				while(h >= minWaveLength)
				{
					double[] iBuf = new double[h];

					for(int i = 0; i < h; i++)
						iBuf[i] = arrHilb[i];

					double[] oBuf = _wavelet.forward(iBuf);

					for(int i = 0; i < h; i++)
						arrHilb[i] = oBuf[i];

					h = h >> 1;

					level++;

				} // levels

			} // if

			return arrHilb;
		} // forward

		//   * Performs the 1-D reverse transform for arrays of dim N from Hilbert domain
		//   * to time domain for the given array using the Fast Wavelet Transform (FWT)
		//   * algorithm and the selected wavelet.
		//   * 
		//   * @date 10.02.2010 08:23:24
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[])
		public override double[] reverseWavelet(double[] arrHilb)
		{
			double[] arrTime = new double[arrHilb.Length];

			for(int i = 0; i < arrHilb.Length; i++)
				arrTime[i] = arrHilb[i];

			int level = 0;
			int minWaveLength = _wavelet.getWaveLength();
			int h = minWaveLength;
			if(arrHilb.Length >= minWaveLength)
			{
				while(h <= arrTime.Length && h >= minWaveLength)
				{
					double[] iBuf = new double[h];

					for(int i = 0; i < h; i++)
						iBuf[i] = arrTime[i];

					double[] oBuf = _wavelet.reverse(iBuf);

					for(int i = 0; i < h; i++)
						arrTime[i] = oBuf[i];

					h = h << 1;

					level++;

				} // levels

			} // if

			return arrTime;
		} // reverse

		//   * Performs the 1-D forward transform for arrays of dim N from time domain to
		//   * Hilbert domain for the given array using the Fast Wavelet Transform (FWT)
		//   * algorithm. The number of transformation levels applied is limited by
		//   * threshold.
		//   * 
		//   * @date 15.07.2010 13:26:26
		//   * @author Thomas Haider
		//   * @date 15.08.2010 00:31:36
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.BasicTransform#forward(double[], int)
		public override double[] forwardWavelet(double[] arrTime, int toLevel)
		{
			double[] arrHilb = new double[arrTime.Length];
			for(int i = 0; i < arrTime.Length; i++)
				arrHilb[i] = arrTime[i];

			int level = 0;
			int h = arrTime.Length;
			int minWaveLength = _wavelet.getWaveLength();
			if(h >= minWaveLength)
			{
				while(h >= minWaveLength && level < toLevel)
				{
					double[] iBuf = new double[h];

					for(int i = 0; i < h; i++)
						iBuf[i] = arrHilb[i];

					double[] oBuf = _wavelet.forward(iBuf);

					for(int i = 0; i < h; i++)
						arrHilb[i] = oBuf[i];

					h = h >> 1;

					level++;

				} // levels

			} // if

			return arrHilb;
		} // forward

		//   * Performs the 1-D reverse transform for arrays of dim N from Hilbert domain
		//   * to time domain for the given array using the Fast Wavelet Transform (FWT)
		//   * algorithm and the selected wavelet. The number of transformation levels
		//   * applied is limited by threshold.
		//   * 
		//   * @date 15.07.2010 13:28:06
		//   * @author Thomas Haider
		//   * @date 15.08.2010 00:31:09
		//   * @author Christian Scheiblich
		//   * @date 20.06.2011 13:03:27
		//   * @author Pol Kennel
		//   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[], int)
		public override double[] reverseWavelet(double[] arrHilb, int fromLevel)
		{
			double[] arrTime = new double[arrHilb.Length];

			for(int i = 0; i < arrHilb.Length; i++)
				arrTime[i] = arrHilb[i];

			int level = 0;

			int minWaveLength = _wavelet.getWaveLength();

			// int h = minWaveLength; // bug ... 20110620
			int h = (int)(arrHilb.Length / (Math.Pow(2, fromLevel - 1))); // added by Pol

			if(arrHilb.Length >= minWaveLength)
			{
				while(h <= arrTime.Length && h >= minWaveLength && level < fromLevel)
				{
					double[] iBuf = new double[h];

					for(int i = 0; i < h; i++)
						iBuf[i] = arrTime[i];

					double[] oBuf = _wavelet.reverse(iBuf);

					for(int i = 0; i < h; i++)
						arrTime[i] = oBuf[i];

					h = h << 1;

					level++;

				} // levels

			} // if

			return arrTime;
		} // reverse

	} // class

}