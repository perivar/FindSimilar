namespace math.transform.jwave.handlers
{
	///
	// * 
	// * Interface for all transform methods.
	// *
	// * @date 30 juin 2011 10:14:22
	// * @author Pol Kennel
	public interface TransformInterface
	{

		// 1-D
		double[] forward(double[] arrTime);

		double[] reverse(double[] arrHilb);

		// 2-D
		double[][] forward(double[][] matTime);

		double[][] reverse(double[][] matHilb);

		// 3-D
		double[][][] forward(double[][][] spcTime);

		double[][][] reverse(double[][][] spcHilb);
	}
}