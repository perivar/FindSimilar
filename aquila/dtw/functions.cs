using System.Collections.Generic;
using System;

namespace Aquila
{
	public static class Functions
	{
		/**
		 * Returns Euclidean distance between two vectors.
		 *
		 * @param v1 first vector
		 * @param v2 second vector
		 * @return Euclidean distance
		 */
		public static double euclideanDistance(double[] v1, double[] v2)
		{
			double d = 0.0;
			for (int i = 0; i < v1.Length; i++)
				d += (v1[i] - v2[i])*(v1[i] - v2[i]);
			return Math.Sqrt(d);
		}

		/**
		 * Returns Manhattan (taxicab) distance between two vectors.
		 *
		 * @param v1 first vector
		 * @param v2 second vector
		 * @return Manhattan distance
		 */
		public static double manhattanDistance(double[] v1, double[] v2)
		{
			double d = 0.0;
			for (int i = 0; i < v1.Length; i++)
				d += Math.Abs(v1[i] - v2[i]);
			return d;
		}

		/**
		 * Returns Chebyshev distance between two vectors.
		 *
		 * @param v1 first vector
		 * @param v2 second vector
		 * @return Chebyshev distance
		 */
		public static double chebyshevDistance(double[] v1, double[] v2)
		{
			double d = 0.0;
			double max = 0.0;
			for (int i = 0; i < v1.Length; i++)
			{
				d = Math.Abs(v1[i] - v2[i]);
				if (d > max)
					max = d;
			}
			return max;
		}

		/**
		 * Returns Minkowski distance (with p = 0.33) between two vectors.
		 *
		 * @param v1 first vector
		 * @param v2 second vector
		 * @return Minkowski distance
		 */
		public static double minkowskiDistance(double[] v1, double[] v2)
		{
			double d = 0.0;
			double p = 0.33;
			for (int i = 0; i < v1.Length; i++)
				d += Math.Pow(Math.Abs(v1[i] - v2[i]), p);

			return Math.Pow(d, 1.0/p);
		}
	}
}
