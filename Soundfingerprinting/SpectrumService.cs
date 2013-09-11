namespace Soundfingerprinting.Fingerprinting.FFT
{
	using System;
	using System.Collections.Generic;

	using Soundfingerprinting.Audio.Strides;

	public class SpectrumService : ISpectrumService
	{
		public List<double[][]> CutLogarithmizedSpectrum(
			double[][] logarithmizedSpectrum, IStride strideBetweenConsecutiveImages, int fingerprintImageLength, int overlap)
		{
			int start = strideBetweenConsecutiveImages.FirstStrideSize / overlap;
			int logarithmicBins = logarithmizedSpectrum[0].Length;
			List<double[][]> spectralImages = new List<double[][]>();

			int width = logarithmizedSpectrum.GetLength(0);
			
			while (start + fingerprintImageLength < width)
			{
				double[][] spectralImage = this.AllocateMemoryForFingerprintImage(fingerprintImageLength, logarithmicBins);
				for (int i = 0; i < fingerprintImageLength; i++)
				{
					Array.Copy(logarithmizedSpectrum[start + i], spectralImage[i], logarithmicBins);
				}

				start += fingerprintImageLength + (strideBetweenConsecutiveImages.StrideSize / overlap);
				spectralImages.Add(spectralImage);
			}

			return spectralImages;
		}

		private double[][] AllocateMemoryForFingerprintImage(int fingerprintLength, int logBins)
		{
			double[][] frames = new double[fingerprintLength][];
			for (int i = 0; i < fingerprintLength; i++)
			{
				frames[i] = new double[logBins];
			}

			return frames;
		}
	}
}