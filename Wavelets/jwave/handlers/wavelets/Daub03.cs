using System;

namespace math.transform.jwave.handlers.wavelets
{

	///
	// * Ingrid Daubechies' orthonormal wavelet of six coefficients and the scales;
	// * normed, due to ||*||2 - euclidean norm.
	// * 
	// * @date 25.03.2010 14:03:20
	// * @author Christian Scheiblich
	public class Daub03 : Wavelet
	{

		//   * Constructor setting up the orthonormal Daubechie6 wavelet coeffs and the
		//   * scales; normed, due to ||*||2 - euclidean norm.
		//   * 
		//   * @date 25.03.2010 14:03:20
		//   * @author Christian Scheiblich
		public Daub03()
		{
			_waveLength = 6;

			double sqrt02 = 1.4142135623730951;
			double sqrt10 = Math.Sqrt(10.0);
			double constA = Math.Sqrt(5.0 + 2.0 * sqrt10);

			_scales = new double[_waveLength]; // can be done in static way also; faster?

			_scales[0] = (1.0 + 1.0 * sqrt10 + 1.0 * constA) / 16.0 / sqrt02; // h0
			_scales[1] = (5.0 + 1.0 * sqrt10 + 3.0 * constA) / 16.0 / sqrt02; // h1
			_scales[2] = (10.0 - 2.0 * sqrt10 + 2.0 * constA) / 16.0 / sqrt02; // h2
			_scales[3] = (10.0 - 2.0 * sqrt10 - 2.0 * constA) / 16.0 / sqrt02; // h3
			_scales[4] = (5.0 + 1.0 * sqrt10 - 3.0 * constA) / 16.0 / sqrt02; // h4
			_scales[5] = (1.0 + 1.0 * sqrt10 - 1.0 * constA) / 16.0 / sqrt02; // h5

			_coeffs = new double[_waveLength]; // can be done in static way also; faster?

			_coeffs[0] = _scales[5]; // h5
			_coeffs[1] = -_scales[4]; // -h4
			_coeffs[2] = _scales[3]; // h3
			_coeffs[3] = -_scales[2]; // -h2
			_coeffs[4] = _scales[1]; // h1
			_coeffs[5] = -_scales[0]; // -h0

		} // Daub03

	} // class

}