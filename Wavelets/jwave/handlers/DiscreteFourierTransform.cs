using System;
using Complex = math.transform.jwave.types.Complex;

namespace math.transform.jwave.handlers
{

	///
	// * The Discrete Fourier Transform (DFT) is - as the name says - the discrete
	// * version of the Fourier Transform applied to a discrete complex valued series.
	// * While the DFT can be applied to any complex valued series; of any length, in
	// * practice for large series it can take considerable time to compute, while the
	// * time taken being proportional to the square of the number on points in the
	// * series.
	// * 
	// * @date 25.03.2010 19:56:29
	// * @author Christian Scheiblich
	// 
	public class DiscreteFourierTransform : BasicTransform
	{

		//   * Constructor; does nothing
		//   * 
		//   * @date 25.03.2010 19:56:29
		//   * @author Christian Scheiblich
		public DiscreteFourierTransform()
		{
		} // DiscreteFourierTransform

		//   * The 1-D forward version of the Discrete Fourier Transform (DFT); The input
		//   * array arrTime is organized by real and imaginary parts of a complex number
		//   * using even and odd places for the index. For example: arrTime[ 0 ] = real1,
		//   * arrTime[ 1 ] = imag1, arrTime[ 2 ] = real2, arrTime[ 3 ] = imag2, ... The
		//   * output arrFreq is organized by the same scheme.
		//   * 
		//   * @date 25.03.2010 19:56:29
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.BasicTransform#forward(double[])
		public override double[] forward(double[] arrTime)
		{
			int m = arrTime.Length;
			double[] arrFreq = new double[m]; // result

			int n = m >> 1; // half of m

			for(int i = 0; i < n; i++)
			{
				int iR = i * 2;
				int iC = i * 2 + 1;

				arrFreq[iR] = 0.0;
				arrFreq[iC] = 0.0;

				double arg = -2.0 * Math.PI * (double)i / (double)n;

				for(int k = 0; k < n; k++)
				{
					int kR = k * 2;
					int kC = k * 2 + 1;

					double cos = Math.Cos(k * arg);
					double sin = Math.Sin(k * arg);

					arrFreq[iR] += arrTime[kR] * cos - arrTime[kC] * sin;
					arrFreq[iC] += arrTime[kR] * sin + arrTime[kC] * cos;

				} // k

				arrFreq[iR] /= (double)n;
				arrFreq[iC] /= (double)n;

			} // i

			return arrFreq;
		} // forward

		//   * The 1-D reverse version of the Discrete Fourier Transform (DFT); The input
		//   * array arrFreq is organized by real and imaginary parts of a complex number
		//   * using even and odd places for the index. For example: arrTime[ 0 ] = real1,
		//   * arrTime[ 1 ] = imag1, arrTime[ 2 ] = real2, arrTime[ 3 ] = imag2, ... The
		//   * output arrTime is organized by the same scheme.
		//   * 
		//   * @date 25.03.2010 19:56:29
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[])
		public override double[] reverse(double[] arrFreq)
		{
			int m = arrFreq.Length;
			double[] arrTime = new double[m]; // result

			int n = m >> 1; // half of m

			for(int i = 0; i < n; i++)
			{
				int iR = i * 2;
				int iC = i * 2 + 1;

				arrTime[iR] = 0.0;
				arrTime[iC] = 0.0;

				double arg = 2.0 * Math.PI * (double)i / (double)n;

				for(int k = 0; k < n; k++)
				{
					int kR = k * 2;
					int kC = k * 2 + 1;

					double cos = Math.Cos(k * arg);
					double sin = Math.Sin(k * arg);

					arrTime[iR] += arrFreq[kR] * cos - arrFreq[kC] * sin;
					arrTime[iC] += arrFreq[kR] * sin + arrFreq[kC] * cos;
				} // k
			} // i

			return arrTime;
		} // reverse

		//   * The 1-D forward version of the Discrete Fourier Transform (DFT); The input
		//   * array arrTime is organized by a class called Complex keeping real and
		//   * imaginary part of a complex number. The output arrFreq is organized by the
		//   * same scheme.
		//   * 
		//   * @date 23.11.2010 18:57:34
		//   * @author Christian Scheiblich
		//   * @param arrTime
		//   *          array of type Complex keeping coefficients of complex numbers
		//   * @return array of type Complex keeping the discrete fourier transform
		//   *         coefficients
		public override Complex[] forward(Complex[] arrTime)
		{
			int n = arrTime.Length;

			Complex[] arrFreq = new Complex[n]; // result

			for(int i = 0; i < n; i++)
			{
				arrFreq[i] = new Complex(); // 0. , 0.

				double arg = -2.0 * Math.PI * (double)i / (double)n;

				for(int k = 0; k < n; k++)
				{
					double cos = Math.Cos(k * arg);
					double sin = Math.Sin(k * arg);

					double real = arrTime[k].Re;
					double imag = arrTime[k].Im;

					arrFreq[i].Re += (real * cos - imag * sin);
					arrFreq[i].Im += (real * sin + imag * cos);

				} // k

				arrFreq[i].Re *= (1.0 / (double)n);
				arrFreq[i].Im *= (1.0 / (double)n);

			} // i

			return arrFreq;
		} // forward

		//   * The 1-D reverse version of the Discrete Fourier Transform (DFT); The input
		//   * array arrFreq is organized by a class called Complex keeping real and
		//   * imaginary part of a complex number. The output arrTime is organized by the
		//   * same scheme.
		//   * 
		//   * @date 23.11.2010 19:02:12
		//   * @author Christian Scheiblich
		//   * @param arrFreq
		//   *          array of type Complex keeping the discrete fourier transform
		//   *          coefficients
		//   * @return array of type Complex keeping coefficients of tiem domain
		public override Complex[] reverse(Complex[] arrFreq)
		{
			int n = arrFreq.Length;
			Complex[] arrTime = new Complex[n]; // result

			for(int i = 0; i < n; i++)
			{
				arrTime[i] = new Complex(); // 0. , 0.

				double arg = 2.0 * Math.PI * (double)i / (double)n;

				for(int k = 0; k < n; k++)
				{
					double cos = Math.Cos(k * arg);
					double sin = Math.Sin(k * arg);

					double real = arrFreq[k].Re;
					double imag = arrFreq[k].Im;

					arrTime[i].Re += (real * cos - imag * sin);
					arrTime[i].Im += (real * sin + imag * cos);

				} // k

			} // i

			return arrTime;
		} // reverse

		//   * The 2-D forward version of the Discrete Fourier Transform (DFT); The input
		//   * array matTime is organized by real and imaginary parts of a complex number
		//   * using even and odd places for the indices. For example: matTime[0][0] =
		//   * real11, matTime[0][1] = imag11, matTime[0][2] = real12, matTime[0][3] =
		//   * imag12, matTime[1][0] = real21, matTime[1][1] = imag21, matTime[1][2] =
		//   * real22, matTime[1][3] = imag2... The output matFreq is organized by the
		//   * same scheme.
		//   * 
		//   * @date 25.03.2010 19:56:29
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.BasicTransform#forward(double[][])
		public override double[][] forward(double[][] matTime)
		{
			// TODO someone should implement this method
			return null;
		} // forward

		//   * The 2-D reverse version of the Discrete Fourier Transform (DFT); The input
		//   * array matFreq is organized by real and imaginary parts of a complex number
		//   * using even and odd places for the indices. For example: matFreq[0][0] =
		//   * real11, matFreq[0][1] = imag11, matFreq[0][2] = real12, matFreq[0][3] =
		//   * imag12, matFreq[1][0] = real21, matFreq[1][1] = imag21, matFreq[1][2] =
		//   * real22, matFreq[1][3] = imag2... The output matTime is organized by the
		//   * same scheme.
		//   * 
		//   * @date 25.03.2010 19:56:29
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[][])
		public override double[][] reverse(double[][] matFreq)
		{
			// TODO someone should implement this method
			return null;
		} // reverse

		//   * The 3-D forward version of the Discrete Fourier Transform (DFT);
		//   * 
		//   * @date 10.07.2010 18:10:43
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.BasicTransform#forward(double[][][])
		public override double[][][] forward(double[][][] spcTime)
		{
			// TODO someone should implement this method
			return null;
		} // forward

		//   * The 3-D reverse version of the Discrete Fourier Transform (DFT);
		//   * 
		//   * @date 10.07.2010 18:10:45
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[][][])
		public override double[][][] reverse(double[][][] spcHilb)
		{
			// TODO someone should implement this method
			return null;
		} // reverse

	} // class
}