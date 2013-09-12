namespace Soundfingerprinting.Fingerprinting.Configuration
{
	using System;
	using Soundfingerprinting.Audio.Strides;
	using Mirage;

	public class DefaultFingerprintingConfiguration : IFingerprintingConfiguration
	{
		public DefaultFingerprintingConfiguration()
		{
			// The parameters used in these transformation steps will be equal to those that have been found to work well in other audio fingerprinting studies
			// (specifically in A Highly Robust Audio Fingerprinting System):
			// audio frames that are 371 ms long (2048 samples),
			// taken every 11.6 ms (64 samples),
			// thus having an overlap of 31/32
			FingerprintLength = 128;
			WdftSize = 2048;
			Overlap = 1024; // 64;
			SamplesPerFingerprint = FingerprintLength * Overlap;
			MinFrequency = 20; // 318;
			MaxFrequency = 22050; // 2000;
			SampleRate = 44100; // 5512;
			LogBase = Math.E; // 2 or 10;
			
			// In Content Fingerprinting Using Wavelets, a static 928 ms stride was used in database creation, and a random 0-46 ms stride was used in querying (random stride was used in order to minimize the coarse effect of unlucky time alignment).
			//Stride = new IncrementalStaticStride(5115, FingerprintLength * Overlap); // 5115 / 5512 = 0,928 sec
			Stride = new IncrementalStaticStride(40924, FingerprintLength * Overlap); // 40924 / 44100 = 0,928 sec
			
			TopWavelets = 200;
			LogBins = 32; // 40; // 32;
			WindowFunction = new HannWindow(WdftSize);
			NormalizeSignal = true; // true;
			UseDynamicLogBase = false;
		}

		/// <summary>
		/// Gets number of samples to read in order to create single signature. The granularity is 1.48 seconds
		/// </summary>
		/// <remarks>
		///   Default = 8192
		/// </remarks>
		public int SamplesPerFingerprint { get; private set; }

		/// <summary>
		/// Gets overlap between the sub fingerprints, 11.6 ms
		/// </summary>
		/// <remarks>
		///   Default = 64
		/// </remarks>
		public int Overlap { get; private set; }

		/// <summary>
		///   Gets size of the WDFT block, 371 ms
		/// </summary>
		/// <remarks>
		///   Default = 2048
		/// </remarks>
		public int WdftSize { get; private set; }

		/// <summary>
		/// Gets frequency range which is taken into account when creating the signature
		/// </summary>
		/// <remarks>
		///   Default = 318
		/// </remarks>
		public int MinFrequency { get; private set; }

		/// <summary>
		/// Gets frequency range which is taken into account when creating the signature
		/// </summary>
		/// <remarks>
		///   Default = 2000
		/// </remarks>
		public int MaxFrequency { get; private set; }

		/// <summary>
		/// Gets number of Top wavelets to consider
		/// </summary>
		/// <remarks>
		///   Default = 200
		/// </remarks>
		public int TopWavelets { get; private set; }

		/// <summary>
		/// Gets sample rate at which the audio file will be pre-processed
		/// </summary>
		/// <remarks>
		///   Default = 5512
		/// </remarks>
		public int SampleRate { get; private set; }

		/// <summary>
		/// Gets log base used for computing the logarithmically spaced frequency bins
		/// </summary>
		/// <remarks>
		///   Default = 10
		/// </remarks>
		public double LogBase { get; private set; }

		/// <summary>
		/// Gets number of logarithmically spaced bins between the frequency components computed by Fast Fourier Transform.
		/// </summary>
		public int LogBins { get; private set; }

		/// <summary>
		/// Gets signature's length
		/// </summary>
		public int FingerprintLength { get; private set; }

		/// <summary>
		/// Gets default stride size between 2 consecutive signature
		/// </summary>
		/// <remarks>
		///  Default = 5115
		/// </remarks>
		public IStride Stride { get; private set; }

		public IWindowFunction WindowFunction { get; private set; }

		public bool NormalizeSignal { get; private set; }

		public bool UseDynamicLogBase { get; private set; }
	}
}