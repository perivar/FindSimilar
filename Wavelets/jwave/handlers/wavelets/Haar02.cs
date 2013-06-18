namespace math.transform.jwave.handlers.wavelets
{
	///
	// * Alfred Haar's orthonormal wavelet transform.
	// * 
	// * @date 08.02.2010 12:46:34
	// * @author Christian Scheiblich
	public class Haar02 : Wavelet
	{

		//   * Constructor setting up the orthonormal Haar2 wavelet coeffs and the scales;
		//   * normed, due to ||*||_2 -- euclidean norm. See the orthogonal version in
		//   * class Haar02Orthogonal for more details.
		//   * 
		//   * @date 08.02.2010 12:46:34
		//   * @author Christian Scheiblich
		public Haar02()
		{
			_waveLength = 2;

			_coeffs = new double[_waveLength];

			_coeffs[0] = 1.0 / 1.4142135623730951; // w0 - normed by sqrt( 2 )
			_coeffs[1] = -1.0 / 1.4142135623730951; // w1 - normed by sqrt( 2 )

			_scales = new double[_waveLength];

			_scales[0] = -_coeffs[1]; // -w1
			_scales[1] = _coeffs[0]; // w0

		} // Haar02

		//   * The forward wavelet transform using the Alfred Haar's wavelet.
		//   * 
		//   * @date 10.02.2010 08:26:06
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.wavelets.Wavelet#forward(double[])
		//   * The reverse wavelet transform using the Alfred Haar's wavelet. The arrHilb
		//   * array keeping coefficients of Hilbert domain should be of length 2 to the
		//   * power of p -- length = 2^p where p is a positive integer.
		//   * 
		//   * @date 10.02.2010 08:26:06
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.handlers.wavelets.Wavelet#reverse(double[])
	} // class

}