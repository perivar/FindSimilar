namespace Soundfingerprinting.Fingerprinting.Configuration
{
	using System;
	using Soundfingerprinting.Audio.Strides;
	using Mirage;

	public class FullFrequencyFingerprintingConfiguration : IFingerprintingConfiguration
	{
		public FullFrequencyFingerprintingConfiguration(bool useRandomStride = false)
		{
			// http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting
			// The parameters used in these transformation steps will be equal to those that have been found to work well in other audio fingerprinting studies
			// (specifically in A Highly Robust Audio Fingerprinting System):
			// audio frames that are 371 ms long
			// taken every 11.6 ms,
			// thus having an overlap of 31/32
			FingerprintLength = 128;
			
			// 8192 / 32000 = 256 ms
			WindowSize = 8192; 	// 371 ms 	is	2048/5512	or 	16384/44100	or 11889/32000
			
			// 512 / 32000 = 16 ms
			Overlap = 512; 			// 11,6 ms	is 	64/5512		or	512/44100	or 372/32000
			SamplesPerFingerprint = FingerprintLength * Overlap;
			
			MinFrequency = 20; 		// 318; 	Full Frequency: 20
			MaxFrequency = 16000; 	// 2000; 	Full Frequency: 22050
			
			// Using 32000 (instead of 44100) gives us a max of 16 khz resolution, which is OK for normal adult human hearing
			SampleRate = 32000; 	// 5512 or 44100
			LogBase = Math.E; 		// Math.E, 2 or 10;
			
			// In Content Fingerprinting Using Wavelets, a static 928 ms stride was used in database creation,
			// and a random 0-46 ms stride was used in querying (random stride was used in order to minimize the coarse effect of unlucky time alignment).
			if (useRandomStride) {
				// 0,046 sec is 2028 / 44100	or 	1472/32000
				// use a 128 ms random stride instead = 4096, since every 46 ms gives way too many fingerprints to query efficiently
				Stride = new IncrementalRandomStride(0, 4096, SamplesPerFingerprint);
			} else {
				// 0,928 sec is	5115 / 5512 or 40924 / 44100	or	29695/32000
				Stride = new IncrementalStaticStride(29695, SamplesPerFingerprint);
			}
			
			TopWavelets = 200;
			LogBins = 40; 	// (Originally this was 32, but 40 seems to work better with SCMS?!)
			WindowFunction = new HannWindow(WindowSize);
			NormalizeSignal = true; 	// true;
			UseDynamicLogBase = false;	// false;
		}
		
		/// <summary>
		/// Gets number of samples to read in order to create single signature.
		/// The granularity is 1.48 seconds
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
		public int WindowSize { get; private set; }

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