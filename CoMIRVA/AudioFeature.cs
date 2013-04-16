using System;
using System.Drawing;

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
		public enum DistanceType {
			KullbackLeiblerDivergence = 1,
			Dtw_Euclidean = 2,
			Dtw_Manhattan = 3,
			Dtw_SquaredEuclidean = 4,
			Dtw_Maximum = 5,
			UCR_Dtw = 6,
			CosineSimilarity = 7
		}
		
		private Image image;
		
		/// <summary>
		/// Image that represents the audio feature
		/// </summary>
		public Image Image {
			get {
				return image;
			}
			set {
				image = value;
			}
		}
		
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
		/// Measures the similarity/dissimilarity of two audio streams characterized
		/// by two audio features.
		/// </summary>
		/// <param name="f">AudioFeature another audio feature of the same type</param>
		/// <returns>double the distance between the two audio streams</returns>
		abstract public double GetDistance(AudioFeature f, DistanceType t);
		
		/// <summary>
		/// Manual serialization of a AudioFeature object to a byte array
		/// </summary>
		/// <returns>byte array</returns>
		abstract public byte[] ToBytes();
		
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