
/**
 * @file DtwPoint.cpp
 *
 * A single DTW point - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 0.5.7
 */
namespace Aquila
{
	/**
	 * A struct representing a single point in DTW array.
	 */
	public class DtwPoint
	{
		/**
		 * Coordinates of the point in the DTW array.
		 */
		public int x;
		public int y;

		/**
		 * Local distance at this point.
		 */
		public double dLocal;

		/**
		 * Accumulated distance at this point.
		 */
		public double dAccumulated;

		/**
		 * Points to previous point in the DTW lowest-cost path.
		 */
		public DtwPoint previous;
		
		/**
		 * Creates the point with default values.
		 */
		public DtwPoint()
		{
			x = 0;
			y = 0;
			dLocal = 0.0;
			dAccumulated = 0.0;
			previous = null;
		}

		/**
		 * Creates the point and associates it with given coordinates.
		 *
		 * @param x_ x coordinate in DTW array
		 * @param y_ y coordinate in DTW array
		 * @param distanceLocal value of local distance at point (x, y)
		 */
		public DtwPoint(int x_, int y_, double distanceLocal)
		{
			x = x_;
			y = y_;
			dLocal = distanceLocal;

			dAccumulated = 0.0;
			previous = null;

			if (0 == x || 0 == y)
				dAccumulated = dLocal;
		}
	}
}

