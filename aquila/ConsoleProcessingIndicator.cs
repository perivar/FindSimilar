using System;

/**
 * @file ConsoleProcessingIndicator.cpp
 *
 * A console processing indicator - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.2
 * @since 2.5.2
 */
namespace Aquila
{
	/**
	 * A simple textual processing indicator using boost::progress_display.
	 */
	public class ConsoleProcessingIndicator : ProcessingIndicator
	{
		string message = "Processing...";
		int currElementIndex = 0;
		int totalElementCount = 100;
		
		// http://geekswithblogs.net/abhijeetp/archive/2010/02/21/showing-progress-in-a-.net-console-application.aspx
		public static void ShowPercentProgress(string message, int currElementIndex, int totalElementCount)
		{
			if (currElementIndex < 0 || currElementIndex >=totalElementCount)
			{
				throw new InvalidOperationException("currElement out of range");
			}
			int percent =  (100 * (currElementIndex + 1)) / totalElementCount;
			Console.Write("\r{0}{1}% complete",message, percent);
			if (currElementIndex == totalElementCount-1)
			{
				Console.WriteLine(Environment.NewLine);
			}
		}
		
		/**
		 * Creates the processing indicator.
		 */
		public ConsoleProcessingIndicator()
		{
			currElementIndex = 0;
		}

		/**
		 * Initializes the indicator, setting value boundaries.
		 *
		 * Has to create boost display on heap, because we don't know
		 * the range in the indicator's constructor.
		 *
		 * @param min minimum value
		 * @param max maximum value
		 */
		public override void Start(int min, int max)
		{
			currElementIndex = min;
			totalElementCount = max;
		}

		/**
		 * Updates the textual progress bar.
		 *
		 * @param value current progress value
		 */
		public override void Progress(int value)
		{
			currElementIndex = value;
			ShowPercentProgress(this.message, currElementIndex, totalElementCount);
		}

		/**
		 * Called at the end of processing, deletes the display.
		 */
		public override void Stop()
		{
			currElementIndex = 0;
		}
	}
}
