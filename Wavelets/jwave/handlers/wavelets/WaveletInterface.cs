namespace math.transform.jwave.handlers.wavelets
{
	///
	// * 
	// * Interface for the base class of an wavelet.
	// *
	// * @date 30 juin 2011 10:31:38
	// * @author Pol Kennel
	// * @date 22.01.2012 18:20:33
	// * @author Christian Scheiblich
	// * 
	public interface WaveletInterface
	{
		double[] forward(double[] values);

		double[] reverse(double[] values);

		int getWaveLength();

		double[] getCoeffs();

		double[] getScales();
	}
}