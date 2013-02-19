
/**
 * @file FeatureReader.cpp
 *
 * Feature reader interface - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 2.3.0
 */
namespace Aquila
{
	/**
	 * Abstract base class of feature reader interface.
	 */
	public abstract class FeatureReader
	{
		/**
		 * Input filename.
		 */
		protected string m_filename;
		
		/**
		 * Creates the reader object and assigns filename.
		 *
		 * @param filename full path to input file
		 */
		public FeatureReader(string filename)
		{
			m_filename = filename;
		}

		/**
		 * Deletes the reader.
		 */
		public virtual void Dispose()
		{
		}

		/**
		 * Reads only the header - to be reimplemented in derived classes.
		 *
		 * @param hdr non-const reference to feature header
		 */
		public abstract bool ReadHeader(ref FeatureHeader hdr);

		/**
		 * Reads the header and data - to be reimplemented in derived classes.
		 *
		 * @param hdr non-const reference to feature header
		 * @param featureArray non-const reference to feature data array
		 */
		public abstract bool Read(ref FeatureHeader hdr, ref double[][] featureArray);
	}
}
