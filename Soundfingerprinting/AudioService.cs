// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
// http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting

using System;
using System.Diagnostics;
using System.Linq;

using Soundfingerprinting.Audio.Models;
using Mirage;

using Lomont;
using FindSimilar.AudioProxies;
using CommonUtils;

namespace Soundfingerprinting.Audio.Services
{
	public class AudioService : IAudioService
	{
		private readonly LomontFFT lomonFFT;
		
		public AudioService()
		{
			lomonFFT = new Lomont.LomontFFT();
		}

		// normalize power (volume) of an audio file.
		// minimum and maximum rms to normalize from.
		// these values has been detected empirically
		private const double MinRms = 0.1f;

		private const double MaxRms = 3;

		public void Dispose()
		{
			BassProxy.Instance.Dispose();
		}
		
		/// <summary>
		/// Read audio from file at a specific frequency rate
		/// </summary>
		/// <param name = "pathToFile">Filename to read from</param>
		/// <param name = "sampleRate">Sample rate</param>
		/// <param name = "milliSeconds">Milliseconds to read</param>
		/// <param name = "startMilliSeconds">Start at a specific millisecond</param>
		/// <returns>Array with data samples</returns>
		public float[] ReadMonoFromFile(
			string pathToFile, int sampleRate, int milliSeconds, int startMilliSeconds) {
			return BassProxy.Instance.ReadMonoFromFile(pathToFile, sampleRate, milliSeconds, startMilliSeconds);
		}

		/// <summary>
		/// Read data from file
		/// </summary>
		/// <param name = "pathToFile">Filename to be read</param>
		/// <param name = "sampleRate">Sample rate at which to perform reading</param>
		/// <returns>Array with data</returns>
		public float[] ReadMonoFromFile(string pathToFile, int sampleRate)
		{
			return ReadMonoFromFile(pathToFile, sampleRate, 0, 0);
		}
		
		public double[][] CreateSpectrogram(string pathToFilename, IWindowFunction windowFunction, int sampleRate, int overlap, int wdftSize)
		{
			// read 5512 Hz, Mono, PCM, with a specific proxy
			float[] samples = ReadMonoFromFile(pathToFilename, sampleRate, 0, 0);

			// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// Matlab multiplies with 2^15 (32768)
			// e.g. if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			//MathUtils.Multiply(ref samples, Analyzer.AUDIO_MULTIPLIER); // 65536
			
			NormalizeInPlace(samples);

			int width = (samples.Length - wdftSize) / overlap; /*width of the image*/
			double[][] frames = new double[width][];
			double[] complexSignal = new double[2 * wdftSize]; /*even - Re, odd - Img, thats how Exocortex works*/
			//double[] window = windowFunction.GetWindow(wdftSize);
			double[] window = windowFunction.GetWindow();
			for (int i = 0; i < width; i++)
			{
				// take 371 ms each 11.6 ms (2048 samples each 64 samples)
				for (int j = 0; j < wdftSize; j++)
				{
					// Weight by Hann Window
					complexSignal[2 * j] = window[j] * samples[(i * overlap) + j];

					// need to clear out as fft modifies buffer (phase)
					complexSignal[(2 * j) + 1] = 0;
				}

				lomonFFT.TableFFT(complexSignal, true);

				// When the input is purely real, its transform is Hermitian,
				// i.e., the component at frequency f_k is the complex conjugate of the component
				// at frequency -f_k, which means that for real inputs there is no information
				// in the negative frequency components that is not already available from the
				// positive frequency components.
				// Thus, n input points produce n/2+1 complex output points.
				// The inverses of this family assumes the same symmetry of its input,
				// and for an output of n points uses n/2+1 input points.
				
				// Transform output contains, for a transform of size N,
				// N/2+1 complex numbers, i.e. 2*(N/2+1) real numbers
				// our transform is of size N+1, because the histogram has n+1 bins
				double[] band = new double[(wdftSize / 2)]; // Don't add te last band, i.e. + 1 is removed
				for (int j = 0; j < (wdftSize / 2); j++)	// Don't add te last band, i.e. + 1 is removed
				{
					double re = complexSignal[2 * j];
					double img = complexSignal[(2 * j) + 1];

					band[j] = Math.Sqrt( ((re * re) + (img * img)) * wdftSize);
				}

				frames[i] = band;
			}

			return frames;
		}

		public double[][] CreateLogSpectrogram(string pathToFile, IWindowFunction windowFunction, AudioServiceConfiguration configuration)
		{
			float[] samples = ReadMonoFromFile(pathToFile, configuration.SampleRate, 0, 0);
			return CreateLogSpectrogram(samples, windowFunction, configuration);
		}

		public double[][] CreateLogSpectrogram(
			float[] samples, IWindowFunction windowFunction, AudioServiceConfiguration configuration)
		{
			// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// Matlab multiplies with 2^15 (32768)
			// e.g. if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			//MathUtils.Multiply(ref samples, Analyzer.AUDIO_MULTIPLIER); // 65536
			
			if (configuration.NormalizeSignal)
			{
				NormalizeInPlace(samples);
			}

			int width = (samples.Length - configuration.WdftSize) / configuration.Overlap; /*width of the image*/
			double[][] frames = new double[width][];
			int[] logFrequenciesIndexes = GenerateLogFrequencies(configuration);
			//double[] window = windowFunction.GetWindow(configuration.WdftSize);
			double[] window = windowFunction.GetWindow();
			for (int i = 0; i < width; i++)
			{
				double[] complexSignal = new double[2 * configuration.WdftSize]; /*even - Re, odd - Img, thats how Exocortex works*/

				// take 371 ms each 11.6 ms (2048 samples each 64 samples)
				for (int j = 0; j < configuration.WdftSize; j++)
				{
					// Weight by Hann Window
					complexSignal[2 * j] = window[j] * samples[(i * configuration.Overlap) + j];
					
					// need to clear out as fft modifies buffer (phase)
					complexSignal[(2 * j) + 1] = 0;
				}
				
				lomonFFT.TableFFT(complexSignal, true);
				
				frames[i] = ExtractLogBins(complexSignal, logFrequenciesIndexes, configuration.LogBins);
			}
			
			return frames;
		}

		private void NormalizeInPlace(float[] samples)
		{
			double squares = samples.AsParallel().Aggregate<float, double>(0, (current, t) => current + (t * t));

			double rms = (double)Math.Sqrt(squares / samples.Length) * 10;

			Debug.WriteLine("10 RMS: {0}", rms);
			
			if (rms < MinRms)
			{
				rms = MinRms;
			}

			if (rms > MaxRms)
			{
				rms = MaxRms;
			}

			for (int i = 0; i < samples.Length; i++)
			{
				samples[i] /= (float) rms;
				samples[i] = Math.Min(samples[i], 1);
				samples[i] = Math.Max(samples[i], -1);
			}
		}

		private double[] ExtractLogBins(double[] spectrum, int[] logFrequenciesIndex, int logBins)
		{
			int width = spectrum.Length / 2;
			double[] sumFreq = new double[logBins]; /*32*/
			for (int i = 0; i < logBins; i++)
			{
				int lowBound = logFrequenciesIndex[i];
				int higherBound = logFrequenciesIndex[i + 1];

				for (int k = lowBound; k < higherBound; k++)
				{
					//double re = spectrum[2 * k] / ((double)width / 2);
					//double img = spectrum[(2 * k) + 1] / ((double)width / 2);
					//sumFreq[i] += (double)((re * re) + (img * img));

					double re = spectrum[2 * k];
					double img = spectrum[(2 * k) + 1];
					
					sumFreq[i] += Math.Sqrt( ((re * re) + (img * img)) * width);
				}

				sumFreq[i] /= higherBound - lowBound;
			}

			return sumFreq;
		}

		private int[] GenerateLogFrequenciesDynamicBase(AudioServiceConfiguration configuration)
		{
			double logBase =
				Math.Exp(
					Math.Log((double)configuration.MaxFrequency / configuration.MinFrequency) / configuration.LogBins);
			double mincoef = (double)configuration.WdftSize / configuration.SampleRate * configuration.MinFrequency;
			int[] indexes = new int[configuration.LogBins + 1];
			for (int j = 0; j < configuration.LogBins + 1; j++)
			{
				int start = (int)((Math.Pow(logBase, j) - 1.0) * mincoef);
				int end = (int)((Math.Pow(logBase, j + 1.0f) - 1.0) * mincoef);
				indexes[j] = start + (int)mincoef;
			}

			return indexes;
		}

		/// <summary>
		/// Get logarithmically spaced indices
		/// </summary>
		/// <param name="configuration">
		/// The configuration for log frequencies
		/// </param>
		/// <returns>
		/// Log indexes
		/// </returns>
		private int[] GenerateLogFrequencies(AudioServiceConfiguration configuration)
		{
			if(configuration.UseDynamicLogBase)
			{
				return GenerateLogFrequenciesDynamicBase(configuration);
			}

			return GenerateStaticLogFrequencies(configuration);
		}

		private int[] GenerateStaticLogFrequencies(AudioServiceConfiguration configuration)
		{
			double logMin = Math.Log(configuration.MinFrequency, configuration.LogBase);
			double logMax = Math.Log(configuration.MaxFrequency, configuration.LogBase);

			double delta = (logMax - logMin) / configuration.LogBins;

			int[] indexes = new int[configuration.LogBins + 1];
			double accDelta = 0;
			for (int i = 0; i <= configuration.LogBins /*32 octaves*/; ++i)
			{
				double freq = (double)Math.Pow(configuration.LogBase, logMin + accDelta);
				accDelta += delta; // accDelta = delta * i
				/*Find the start index in array from which to start the summation*/
				indexes[i] = FreqToIndex(freq, configuration.SampleRate, configuration.WdftSize);
			}

			return indexes;
		}

		/*
		 * An array of WDFT [0, 2048], contains a range of [0, 5512] frequency components.
		 * Only 1024 contain actual data. In order to find the Index, the fraction is found by dividing the frequency by max frequency
		 */

		/// <summary>
		///   Gets the index in the spectrum vector from according to the starting frequency specified as the parameter
		/// </summary>
		/// <param name = "freq">Frequency to be found in the spectrum vector [E.g. 300Hz]</param>
		/// <param name = "sampleRate">Frequency rate at which the signal was processed [E.g. 5512Hz]</param>
		/// <param name = "spectrumLength">Length of the spectrum [2048 elements generated by WDFT from which only 1024 are with the actual data]</param>
		/// <returns>Index of the frequency in the spectrum array</returns>
		/// <remarks>
		/// The Bandwidth of the spectrum runs from 0 until SampleRate / 2 [E.g. 5512 / 2]
		///   Important to remember:
		///   N points in time domain correspond to N/2 + 1 points in frequency domain
		///   E.g. 300 Hz applies to 112'th element in the array
		/// </remarks>
		private int FreqToIndex(double freq, int sampleRate, int spectrumLength)
		{
			/*N sampled points in time correspond to [0, N/2] frequency range */
			double fraction = freq / ((double)sampleRate / 2);
			/*DFT N points defines [N/2 + 1] frequency points*/
			int i = (int)Math.Round(((spectrumLength / 2) + 1) * fraction);
			return i;
		}
	}
}