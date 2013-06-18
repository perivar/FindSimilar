namespace math.transform.jwave.handlers.wavelets
{

	///
	// * Ingrid Daubechies' orthonormal wavelet of eight coefficients and the scales;
	// * normed, due to ||*||2 - euclidean norm.
	// * 
	// * @date 26.03.2010 07:35:31
	// * @author Christian Scheiblich
	public class Daub04 : Wavelet
	{

		//   * Constructor setting up the orthonormal Daubechie6 wavelet coeffs and the
		//   * scales; normed, due to ||*||2 - euclidean norm.
		//   * 
		//   * @date 26.03.2010 07:35:31
		//   * @author Christian Scheiblich
		public Daub04()
		{

			_waveLength = 8;

			_scales = new double[_waveLength];

			double sqrt02 = 1.4142135623730951;

			// TODO Get analytical formulation, due to its precision; this is around 1.e-3 only
			_scales[0] = 0.32580343; // 0.32580343
			//JAVA TO VB & C# CONVERTER TODO TASK: Octal literals cannot be represented in C#:
			_scales[1] = 1.01094572; // 1.01094572
			_scales[2] = 0.8922014; // 0.8922014
			//JAVA TO VB & C# CONVERTER TODO TASK: Octal literals cannot be represented in C#:
			_scales[3] = -0.03967503; // -0.03967503
			_scales[4] = -0.2645071; // -0.2645071
			//JAVA TO VB & C# CONVERTER TODO TASK: Octal literals cannot be represented in C#:
			_scales[5] = 0.0436163; // 0.0436163
			//JAVA TO VB & C# CONVERTER TODO TASK: Octal literals cannot be represented in C#:
			_scales[6] = 0.0465036; // 0.0465036
			//JAVA TO VB & C# CONVERTER TODO TASK: Octal literals cannot be represented in C#:
			_scales[7] = -0.01498699; // -0.01498699

			// normalize to square root of 2 for being orthonormal
			for(int i = 0; i < _waveLength; i++)
				_scales[i] /= sqrt02;

			_coeffs = new double[_waveLength];

			_coeffs[0] = _scales[7]; // h7
			_coeffs[1] = -_scales[6]; // -h6
			_coeffs[2] = _scales[5]; // h5
			_coeffs[3] = -_scales[4]; // -h4
			_coeffs[4] = _scales[3]; // h3
			_coeffs[5] = -_scales[2]; // -h2
			_coeffs[6] = _scales[1]; // h1
			_coeffs[7] = -_scales[0]; // -h0

		} // Daub04

	} // class

}