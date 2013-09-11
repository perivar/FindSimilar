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

	public class FingerprintService
	{
		public readonly ISpectrumService spectrumService;
		public readonly IWaveletService waveletService;
		public readonly IFingerprintDescriptor fingerprintDescriptor;
		public readonly IAudioService audioService;

		public FingerprintService(
			IAudioService audioService,
			IFingerprintDescriptor fingerprintDescriptor,
			ISpectrumService spectrumService,
			IWaveletService waveletService)
		{
			this.spectrumService = spectrumService;
			this.waveletService = waveletService;
			this.fingerprintDescriptor = fingerprintDescriptor;
			this.audioService = audioService;
		}

		public List<bool[]> CreateFingerprintsFromAudioFile(WorkUnitParameterObject param)
		{
			float[] samples = audioService.ReadMonoFromFile(
				param.PathToAudioFile,
				param.FingerprintingConfiguration.SampleRate,
				param.MillisecondsToProcess,
				param.StartAtMilliseconds);

			return CreateFingerprintsFromAudioSamples(samples, param);
		}

		public List<bool[]> CreateFingerprintsFromAudioSamples(float[] samples, WorkUnitParameterObject param)
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
				WdftSize = configuration.WdftSize,
				NormalizeSignal = configuration.NormalizeSignal,
				UseDynamicLogBase = configuration.UseDynamicLogBase
			};

			double[][] spectrum = audioService.CreateLogSpectrogram(
				samples, configuration.WindowFunction, audioServiceConfiguration);
			
			return this.CreateFingerprintsFromLogSpectrum(
				spectrum,
				configuration.Stride,
				configuration.FingerprintLength,
				configuration.Overlap,
				configuration.TopWavelets);
		}

		private List<bool[]> CreateFingerprintsFromLogSpectrum(
			double[][] logarithmizedSpectrum, IStride stride, int fingerprintLength, int overlap, int topWavelets)
		{
			List<double[][]> spectralImages = spectrumService.CutLogarithmizedSpectrum(
				logarithmizedSpectrum, stride, fingerprintLength, overlap);

			waveletService.ApplyWaveletTransformInPlace(spectralImages);
			List<bool[]> fingerprints = new List<bool[]>();

			foreach (var spectralImage in spectralImages)
			{
				bool[] image = fingerprintDescriptor.ExtractTopWavelets(spectralImage, topWavelets);
				fingerprints.Add(image);
			}

			return fingerprints;
		}
	}
}