using System;

namespace CommonUtils
{
	/// <summary>
	/// Discrete Cosine Transforms
	/// </summary>
	public interface DctInterface
	{
		double[,] Dct(double[,] f);

		double[,] InverseDct(double[,] F);
	}
}
