using System;

namespace math.transform.jwave.handlers.wavelets
{

	///
	// * Ingrid Daubechies' orthonormal wavelet of four coefficients and the scales;
	// * normed, due to ||*||2 - euclidean norm.
	// * 
	// * @date 10.02.2010 15:42:45
	// * @author Christian Scheiblich
	public class Daub02 : Wavelet
	{

		//   * Constructor setting up the orthonormal Daubechie4 wavelet coeffs and the
		//   * scales; normed, due to ||*||2 - euclidean norm.
		//   * 
		//   * @date 10.02.2010 15:42:45
		//   * @author Christian Scheiblich
		public Daub02()
		{

			_waveLength = 4;

			_scales = new double[_waveLength]; // can be done in static way also; faster?

			double sqrt3 = Math.Sqrt(3.0); // 1.7320508075688772

			_scales[0] = ((1.0 + sqrt3) / 4.0) / 1.4142135623730951;
			_scales[1] = ((3.0 + sqrt3) / 4.0) / 1.4142135623730951;
			_scales[2] = ((3.0 - sqrt3) / 4.0) / 1.4142135623730951;
			_scales[3] = ((1.0 - sqrt3) / 4.0) / 1.4142135623730951;

			_coeffs = new double[_waveLength]; // can be done in static way also; faster?

			_coeffs[0] = _scales[3]; // h3
			_coeffs[1] = -_scales[2]; // -h2
			_coeffs[2] = _scales[1]; // h1
			_coeffs[3] = -_scales[0]; // -h0

		} // Daub02

	} // class

}