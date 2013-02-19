/**
 * @file ProcessingIndicator.h
 *
 * An interface of any progress indicator - header.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 2.3.0
 */

namespace Aquila
{
	/**
	 * An abstract base class ("interface") for processing indicators.
	 */
	public abstract class ProcessingIndicator
	{
		/**
		 * Initializes the indicator, setting value boundaries.
		 *
		 * @param min minimum value
		 * @param max maximum value
		 */
		public abstract void Start(int min, int max);

		/**
		 * This should be called in the iteration with current progress value.
		 *
		 * @param value current progress value
		 */
		public abstract void Progress(int value);

		/**
		 * Called at the end of processing, deinitializes the indicator.
		 */
		public abstract void Stop();
	}
}
