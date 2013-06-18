namespace math.transform.jwave.handlers.wavelets
{

	///
	// * Legendre's orthonormal wavelet of six coefficients and the scales; normed,
	// * due to ||*||2 - euclidean norm.
	// * 
	// * @date 03.06.2010 22:04:35
	// * @author Christian Scheiblich
	public class Lege06 : Wavelet
	{

		//   * Constructor setting up the orthonormal Legendre6 wavelet coeffs and the
		//   * scales; normed, due to ||*||2 - euclidean norm.
		//   * 
		//   * @date 03.06.2010 22:04:36
		//   * @author Christian Scheiblich
		public Lege06()
		{
			_waveLength = 6;

			_scales = new double[_waveLength]; // can be done in static way also; faster?

			_scales[0] = -63.0 / 128.0 / 1.4142135623730951; // h0
			_scales[1] = -35.0 / 128.0 / 1.4142135623730951; // h1
			_scales[2] = -30.0 / 128.0 / 1.4142135623730951; // h2
			_scales[3] = -30.0 / 128.0 / 1.4142135623730951; // h3
			_scales[4] = -35.0 / 128.0 / 1.4142135623730951; // h4
			_scales[5] = -63.0 / 128.0 / 1.4142135623730951; // h5

			_coeffs = new double[_waveLength]; // can be done in static way also; faster?

			_coeffs[0] = _scales[5]; // h5
			_coeffs[1] = -_scales[4]; // -h4
			_coeffs[2] = _scales[3]; // h3
			_coeffs[3] = -_scales[2]; // -h2
			_coeffs[4] = _scales[1]; // h1
			_coeffs[5] = -_scales[0]; // -h0
		} // Lege06

	} // class

}