namespace Soundfingerprinting.Fingerprinting
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	using Soundfingerprinting.Audio.Models;
	using Soundfingerprinting.Audio.Services;
	using Soundfingerprinting.Audio.Strides;
	using Soundfingerprinting.Fingerprinting.FFT;
	using Soundfingerprinting.Fingerprinting.Wavelets;
	using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
	using Soundfingerprinting.Fingerprinting.Configuration;

	using Mirage; // for debug
	
	public class FingerprintService
	{
		public readonly SpectrumService SpectrumService;
		public readonly IWaveletService WaveletService;
		public readonly FingerprintDescriptor FingerprintDescriptor;
		public readonly IAudioService AudioService;

		public FingerprintService(
			IAudioService audioService,
			FingerprintDescriptor fingerprintDescriptor,
			SpectrumService spectrumService,
			IWaveletService waveletService)
		{
			this.SpectrumService = spectrumService;
			this.WaveletService = waveletService;
			this.FingerprintDescriptor = fingerprintDescriptor;
			this.AudioService = audioService;
		}

		public List<bool[]> CreateFingerprintsFromAudioFile(WorkUnitParameterObject param, out double[][] logSpectrogram)
		{
			float[] samples = AudioService.ReadMonoFromFile(
				param.PathToAudioFile,
				param.FingerprintingConfiguration.SampleRate,
				param.MillisecondsToProcess,
				param.StartAtMilliseconds);

			return CreateFingerprintsFromAudioSamples(samples, param, out logSpectrogram);
		}

		public List<bool[]> CreateFingerprintsFromAudioSamples(float[] samples, WorkUnitParameterObject param, out double[][] logSpectrogram)
		{
			IFingerprintingConfiguration configuration = param.FingerprintingConfiguration;
			AudioServiceConfiguration audioServiceConfiguration = new AudioServiceConfiguration
			{
				LogBins = configuration.LogBins,
				LogBase = configuration.LogBase,
				MaxFrequency = configuration.MaxFrequency,
				MinFrequency = configuration.MinFrequency,
				Overlap = configuration.Overlap,
				SampleRate = configuration.SampleRate,
				WindowSize = configuration.WindowSize,
				NormalizeSignal = configuration.NormalizeSignal,
				UseDynamicLogBase = configuration.UseDynamicLogBase
			};
			
			// store the log spectrogram in the out variable
			logSpectrogram = AudioService.CreateLogSpectrogram(
				samples, configuration.WindowFunction, audioServiceConfiguration);
			
			return this.CreateFingerprintsFromLogSpectrum(
				logSpectrogram,
				configuration.Stride,
				configuration.FingerprintLength,
				configuration.Overlap,
				configuration.TopWavelets);
		}

		public List<bool[]> CreateFingerprintsFromLogSpectrum(
			double[][] logarithmizedSpectrum, IStride stride, int fingerprintLength, int overlap, int topWavelets)
		{
			DbgTimer t = new DbgTimer();
			t.Start ();

			// Cut the logaritmic spectrogram into smaller spectrograms with one stride between each
			List<double[][]> spectralImages = SpectrumService.CutLogarithmizedSpectrum(logarithmizedSpectrum, stride, fingerprintLength, overlap);

			// Then apply the wavelet transform on them to later reduce the resolution
			// do this in place
			WaveletService.ApplyWaveletTransformInPlace(spectralImages);
			
			// Then for each of the wavelet reduce the resolution by only keeping the top wavelets
			// and ignore the magnitude of the top wavelets.
			// Instead, we can simply keep the sign of it (+/-).
			// This information is enough to keep the extract perceptual characteristics of a song.
			List<bool[]> fingerprints = new List<bool[]>();
			foreach (var spectralImage in spectralImages)
			{
				bool[] image = FingerprintDescriptor.ExtractTopWavelets(spectralImage, topWavelets);
				fingerprints.Add(image);
			}

			Dbg.WriteLine ("Created {1} Fingerprints from Log Spectrum - Execution Time: {0} ms", t.Stop().TotalMilliseconds, fingerprints.Count);
			return fingerprints;
		}
	}
}