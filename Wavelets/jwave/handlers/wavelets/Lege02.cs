namespace math.transform.jwave.handlers.wavelets
{

	///
	// * Orthonormal Legendre wavelet transform of 2 coefficients based on the
	// * Legendre polynomial. But, actually for the smallest Legendre wavelet, the
	// * wavelet is the mirrored Haar Wavelet.
	// * 
	// * @date 08.06.2010 09:32:08
	// * @author Christian Scheiblich
	public class Lege02 : Wavelet
	{

		//   * Constructor setting up the orthonormal Legendre 2 wavelet coeffs and the
		//   * scales; normed, due to ||*||_2 -- euclidean norm. Actually these
		//   * coefficients are the mirrored ones of Alfred Haar's wavelet -- see class
		//   * Haar02 and Haar02Orthogonal.
		//   * 
		//   * @date 08.06.2010 09:32:08
		//   * @author Christian Scheiblich
		public Lege02()
		{

			_waveLength = 2;

			_coeffs = new double[_waveLength];

			_coeffs[0] = -1.0 / 1.4142135623730951; // w0 - normed by sqrt( 2 )
			_coeffs[1] = 1.0 / 1.4142135623730951; // w1 - normed by sqrt( 2 )

			_scales = new double[_waveLength];

			_scales[0] = -_coeffs[1]; // -w1 -> -1. / sqrt(2.)
			_scales[1] = _coeffs[0]; // w0 -> -1. / sqrt(2.)

		} // Lege02

	} // class

}