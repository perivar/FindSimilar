using System;

namespace Comirva.Audio.Feature
{
	///
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
		/// Measures the similarity/dissimilarity of two audio streams characterized
		/// by two audio features.
		/// </summary>
		/// <param name="f">AudioFeature another audio feature of the same type</param>
		/// <returns>double the distance between the two audio streams</returns>
		abstract public double GetDistance(AudioFeature f);
	}
}