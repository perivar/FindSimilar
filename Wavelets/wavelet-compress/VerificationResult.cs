using System;

namespace Wavelets.Compress
{
	/// Author: Linzhi Qi
	/// Converted from C++ by perivar@nerseth.com
	/// https://github.com/linzhi/wavelet-compress
	public static class VerificationResult
	{
		public static double MeanSquaredError(double[] file_in, double[] file_out, ref int file_size)
		{
			int i;
			double mse = 0;

			for (i = 0; i < file_size; i++)
			{
				mse += Math.Pow((file_in[i] - file_out[i]), 2.0);
			}

			mse = mse / file_size;

			return mse;
		}

		public static double PeakSignalToNoiseRatio(double mse)
		{
			double psnr = 0;

			psnr = 10.0 * Math.Log10((255.0 * 255.0) / mse);

			return psnr;
		}
	}
}
