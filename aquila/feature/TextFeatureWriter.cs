using System;
using System.IO;

/**
 * @file TextFeatureWriter.cpp
 *
 * A simple text writer - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 2.3.0
 */
namespace Aquila
{
	/**
	 * A plain-text feature writer.
	 */
	public class TextFeatureWriter: FeatureWriter
	{
		/**
		 * Creates the writer object and assigns filename.
		 *
		 * @param filename full path to output file
		 */
		public TextFeatureWriter(string filename) : base(filename)
		{
		}

		/**
		 * Deletes the writer.
		 */
		public override void Dispose()
		{
			base.Dispose();
		}

		/**
		 * Writes the header and data to a plain text file.
		 *
		 * @param hdr const reference to feature header
		 * @param featureArray const reference to feature data array
		 */
		public override bool Write(FeatureHeader hdr, double[][] featureArray)
		{
			if (featureArray.Length == 0)
			{
				throw new Exception("Empty feature array!");
			}
			
			TextWriter ofs = File.CreateText(m_filename);
			ofs.WriteLine("# Generated with: Aquila v. {0}", Dtw.VERSION);
			ofs.WriteLine("# Original wave file: {0}", hdr.wavFilename);
			ofs.WriteLine("# Audio sampling frequency: ?????");
			ofs.WriteLine("# Frame length: {0}", hdr.frameLength);
			ofs.WriteLine("# Frames count: {0}", featureArray.Length);
			ofs.WriteLine("# Parameters type: {0}", hdr.type);
			ofs.WriteLine("# Parameters per frame: {0}", hdr.paramsPerFrame);
			ofs.WriteLine("# Save timestamp: {0}", hdr.timestamp);
			
			//ofs.precision(13);
			for (int i = 0, size = featureArray.Length; i < size; ++i)
			{
				ofs.WriteLine("#frame: {0}", i);
				for (int j = 0; j < hdr.paramsPerFrame; ++j)
				{
					ofs.WriteLine("{0}", featureArray[i][j]);
				}
			}
			ofs.Close();
			
			return true;
		}

	}
}

