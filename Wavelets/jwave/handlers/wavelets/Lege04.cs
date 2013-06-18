namespace math.transform.jwave.handlers.wavelets
{
	///
	// * Legendre's orthonormal wavelet of four coefficients and the scales; normed,
	// * due to ||*||2 - euclidean norm.
	// * 
	// * @date 03.06.2010 21:19:04
	// * @author Christian Scheiblich
	public class Lege04 : Wavelet
	{

		//   * Constructor setting up the orthonormal Legendre4 wavelet coeffs and the
		//   * scales; normed, due to ||*||2 - euclidean norm.
		//   * 
		//   * @date 03.06.2010 21:19:04
		//   * @author Christian Scheiblich
		public Lege04()
		{
			_waveLength = 4;

			_scales = new double[_waveLength]; // can be done in static way also; faster?

			_scales[0] = (-5.0 / 8.0) / 1.4142135623730951;
			_scales[1] = (-3.0 / 8.0) / 1.4142135623730951;
			_scales[2] = (-3.0 / 8.0) / 1.4142135623730951;
			_scales[3] = (-5.0 / 8.0) / 1.4142135623730951;

			_coeffs = new double[_waveLength]; // can be done in static way also; faster?

			_coeffs[0] = _scales[3]; // h3
			_coeffs[1] = -_scales[2]; // -h2
			_coeffs[2] = _scales[1]; // h1
			_coeffs[3] = -_scales[0]; // -h0

		} // Lege04

	} // class

}