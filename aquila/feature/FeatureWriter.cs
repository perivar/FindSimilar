
/**
 * @file FeatureWriter.cpp
 *
 * Feature writer interface - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 2.3.0
 */
namespace Aquila
{
	/**
	 * Abstract base class of feature writer interface.
	 */
	public abstract class FeatureWriter
	{
		/**
		 * Creates the writer object and assigns filename.
		 *
		 * @param filename full path to output file
		 */
		public FeatureWriter(string filename)
		{
			m_filename = filename;
		}

		/**
		 * Deletes the writer.
		 */
		public virtual void Dispose()
		{
		}

		/**
		 * Writes the header and data - to be reimplemented in derived classes.
		 *
		 * @param hdr const reference to feature header
		 * @param featureArray const reference to feature data array
		 */
		public abstract bool Write(FeatureHeader hdr, double[][] featureArray);

		/**
		 * Output filename.
		 */
		protected string m_filename;
	}
}
