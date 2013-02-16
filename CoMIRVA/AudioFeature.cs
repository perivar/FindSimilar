using System;

namespace Comirva.Audio.Feature
{
	/// <summary>
	/// CoMIRVA: Collection of Music Information Retrieval and Visualization Applications
	/// Ported from Java to C# by perivar@nerseth.com
	/// </summary>

	/// An audio feature is a special attribute designed to describe characteristics
	/// of audiostreams. A audio feature is the result of a audio feature extraction
	/// process.<br>
	/// <br>
	/// Beside describing the characteristic of audio streams, audio features also
	/// support comparing two audio streams. So based on the supported
	/// characterization a metric has to be implemented, which allows to compute the
	/// distance (similarity/dissimilarity) of two audio streams.
	/// @author Markus Schedl, Klaus Seyerlehner
	/// @see comirva.audio.extraction.AudioFeatureExtractor
	public abstract class AudioFeature
	{
		/// <summary>
		/// Name of the Audio Feature
		/// </summary>
		public abstract string Name { get; set; }
		
		/// <summary>
		/// Duration (milliseconds) of the Audio Feature
		/// </summary>
		public long Duration { get; set; }		
		
		/// <summary>
		/// Measures the similarity/dissimilarity of two audio streams characterized
		/// by two audio features.
		/// </summary>
		/// <param name="f">AudioFeature another audio feature of the same type</param>
		/// <returns>double the distance between the two audio streams</returns>
		abstract public double GetDistance(AudioFeature f);
		
		/// <summary>
		/// Manual serialization of a AudioFeature object to a byte array
		/// </summary>
		/// <returns>byte array</returns>
		abstract public byte[] ToBytes();
		
		// Static Helper methods
		public static bool isLE = BitConverter.IsLittleEndian;
		public static int GetInt32(byte [] buf, int i, byte [] buf4)
		{
			if (isLE) {
				return BitConverter.ToInt32 (buf, i);
			} else {
				return BitConverter.ToInt32 (Reverse (buf, i, 4, buf4), 0);
			}
		}

		public static float GetFloat(byte [] buf, int i, byte [] buf4)
		{
			if (isLE) {
				return BitConverter.ToSingle (buf, i);
			} else {
				return BitConverter.ToSingle (Reverse (buf, i, 4, buf4), 0);
			}
		}

		public static byte [] Reverse(byte [] buf, int start, int length, byte [] out_buf)
		{
			var ret = out_buf;
			int end = start + length -1;
			for (int i = 0; i < length; i++) {
				ret[i] = buf[end - i];
			}
			return ret;
		}
		
		public override string ToString() {
			/*
			string s = "";
			foreach (byte b in ToBytes())
			{
				s += b;
			}
			return s;
			*/
			return String.Format("{0} ({1} ms)", Name, Duration);
		}
	}
}