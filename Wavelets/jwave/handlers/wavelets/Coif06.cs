using System;

namespace math.transform.jwave.handlers.wavelets
{

	///
	// * Ingrid Daubechies' orthonormal Coiflet wavelet of six coefficients and the
	// * scales; normed, due to ||*||2 - euclidean norm.
	// * 
	// * @date 10.02.2010 16:32:38
	// * @author Christian Scheiblich
	public class Coif06 : Wavelet
	{

		//   * Constructor setting up the orthonormal Coiflet6 wavelet coeffs and the
		//   * scales; normed, due to ||*||2 - euclidean norm.
		//   * 
		//   * @date 10.02.2010 16:32:38
		//   * @author Christian Scheiblich
		public Coif06()
		{

			_waveLength = 6; // minimal array size for transform

			double sqrt15 = Math.Sqrt(15.0);

			_scales = new double[_waveLength]; // can be done in static way also; faster?

			_scales[0] = 1.4142135623730951 * (sqrt15 - 3.0) / 32.0;
			_scales[1] = 1.4142135623730951 * (1.0 - sqrt15) / 32.0;
			_scales[2] = 1.4142135623730951 * (6.0 - 2 * sqrt15) / 32.0;
			_scales[3] = 1.4142135623730951 * (2.0 * sqrt15 + 6.0) / 32.0;
			_scales[4] = 1.4142135623730951 * (sqrt15 + 13.0) / 32.0;
			_scales[5] = 1.4142135623730951 * (9.0 - sqrt15) / 32.0;

			_coeffs = new double[_waveLength]; // can be done in static way also; faster?

			_coeffs[0] = _scales[5]; // h5
			_coeffs[1] = -_scales[4]; // -h4
			_coeffs[2] = _scales[3]; // h3
			_coeffs[3] = -_scales[2]; // -h2
			_coeffs[4] = _scales[1]; // h1
			_coeffs[5] = -_scales[0]; // -h0

		} // Coif06

	} // class

}