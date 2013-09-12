namespace Soundfingerprinting.Image
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Linq;

	using Soundfingerprinting.Audio.Strides;
	using Soundfingerprinting.Fingerprinting.FFT;
	using Soundfingerprinting.Fingerprinting.Wavelets;
	
	using CommonUtils;

	public class ImageService
	{
		private const int SpaceBetweenImages = 10; /*10 pixel space between fingerprint images*/

		private readonly ISpectrumService spectrumService;

		private readonly IWaveletService waveletService;
		
		public ImageService(ISpectrumService spectrumService, IWaveletService waveletService)
		{
			this.spectrumService = spectrumService;
			this.waveletService = waveletService;
		}

		public Image GetImageForFingerprint(bool[] data, int width, int height)
		{
			Bitmap image = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
			this.DrawFingerprintInImage(image, data, width, height, 0, 0);
			return image;
		}

		public Image GetImageForFingerprints(List<bool[]> fingerprints, int width, int height, int fingerprintsPerRow)
		{
			int imagesPerRow = fingerprintsPerRow; /*5 bitmap images per line*/
			int fingersCount = fingerprints.Count;
			int rowCount = (int)Math.Ceiling((float)fingersCount / imagesPerRow);
			int imageWidth = (imagesPerRow * (width + SpaceBetweenImages)) + SpaceBetweenImages;
			int imageHeight = (rowCount * (height + SpaceBetweenImages)) + SpaceBetweenImages;

			Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);
			this.SetBackground(image, Color.White);

			int verticalOffset = SpaceBetweenImages;
			int horizontalOffset = SpaceBetweenImages;
			int count = 0;
			foreach (bool[] fingerprint in fingerprints)
			{
				DrawFingerprintInImage(image, fingerprint, width, height, horizontalOffset, verticalOffset);
				count++;
				if (count % imagesPerRow == 0)
				{
					verticalOffset += height + SpaceBetweenImages;
					horizontalOffset = SpaceBetweenImages;
				}
				else
				{
					horizontalOffset += width + SpaceBetweenImages;
				}
			}

			return image;
		}

		public Image GetSignalImage(float[] data, int width, int height)
		{
			Bitmap image = new Bitmap(width, height);
			Graphics graphics = Graphics.FromImage(image);

			FillBackgroundColor(width, height, graphics, Color.Black);
			DrawGridlines(width, height, graphics);

			int center = height / 2;
			/*Draw lines*/
			using (Pen pen = new Pen(Color.MediumSpringGreen, 1))
			{
				/*Find delta X, by which the lines will be drawn*/
				double deltaX = (double)width / data.Length;
				double normalizeFactor = data.Max(a => Math.Abs(a)) / ((double)height / 2);
				for (int i = 0, n = data.Length; i < n; i++)
				{
					graphics.DrawLine(
						pen,
						(float)(i * deltaX),
						center,
						(float)(i * deltaX),
						(float)(center - (data[i] / normalizeFactor)));
				}
			}

			using (Pen pen = new Pen(Color.DarkGreen, 1))
			{
				/*Draw center line*/
				graphics.DrawLine(pen, 0, center, width, center);
			}

			//DrawCopyrightInfo(graphics, 10, 10);
			return image;
		}

		public Image GetSpectrogramImage(double[][] spectrum, int width, int height)
		{
			Bitmap image = new Bitmap(width, height);
			Graphics graphics = Graphics.FromImage(image);
			FillBackgroundColor(width, height, graphics, Color.Black);

			int bands = spectrum[0].Length;
			double deltaX = (double)(width - 1) / spectrum.Length; /*By how much the image will move to the left*/
			double deltaY = (double)(height - 1) / (bands + 1); /*By how much the image will move upward*/
			int prevX = -1;
			
			double maxValue = spectrum.Max((b) => b.Max((v) => Math.Abs(v)));
			maxValue = 20 * Math.Log10(maxValue);
			
			for (int i = 0, n = spectrum.Length; i < n; i++)
			{
				double x = i * deltaX;
				if ((int)x == prevX)
				{
					continue;
				}

				//double average = spectrum[i].Average(v => Math.Abs(v));
				for (int j = 0, m = spectrum[0].Length; j < m; j++)
				{
					//Color color = ValueToBlackWhiteColor(spectrum[i][j], average);
					//image.SetPixel((int)x, height - (int)(deltaY * j) - 1, color);
					
					double val = 20 * Math.Log10(spectrum[i][j]);
					Color color = Color.White;
					if (!double.IsNaN(val) && !double.IsInfinity(val)) {
						color = ValueToBlackWhiteColor(val, maxValue);
					}
					Brush brush = new SolidBrush(color);
					// draw a small square
					graphics.FillRectangle(brush, (int)x, height - (int)(deltaY * j) - 1, (int)deltaX + 1, 1);
				}

				prevX = (int)x;
			}

			//DrawCopyrightInfo(graphics, 10, 10);
			image = ColorUtils.Colorize(image, 255, ColorUtils.ColorPaletteType.MATLAB);
			return image;
		}

		public Image GetLogSpectralImages(
			double[][] spectrum,
			IStride strideBetweenConsecutiveImages,
			int fingerprintLength,
			int overlap,
			int imagesPerRow)
		{
			List<double[][]> spectralImages = spectrumService.CutLogarithmizedSpectrum(
				spectrum, strideBetweenConsecutiveImages, fingerprintLength, overlap);
			
			int blockSizeX = 4;
			int blockSizeY = 4;
			
			int width = spectralImages[0].GetLength(0);
			int height = spectralImages[0][0].Length;
			int fingersCount = spectralImages.Count;
			int rowCount = (int)Math.Ceiling((float)fingersCount / imagesPerRow);
			int imageWidth = (blockSizeX * imagesPerRow * (width + SpaceBetweenImages)) + SpaceBetweenImages;
			int imageHeight = (blockSizeY * rowCount * (height + SpaceBetweenImages)) + SpaceBetweenImages;
			//Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);
			Bitmap image = new Bitmap(imageWidth, imageHeight);
			Graphics graphics = Graphics.FromImage(image);

			SetBackground(image, Color.White);

			int verticalOffset = SpaceBetweenImages;
			int horizontalOffset = SpaceBetweenImages;
			int count = 0;
			foreach (double[][] spectralImage in spectralImages)
			{
				double maxValue = spectralImage.Max((b) => b.Max((v) => Math.Abs(v)));
				maxValue = 20 * Math.Log10(maxValue);

				//double average = spectralImage.Average(col => col.Average(v => Math.Abs(v)));
				for (int i = 0; i < width /*128*/; i++)
				{
					for (int j = 0; j < height /*32*/; j++)
					{
						//Color color = ValueToBlackWhiteColor(spectralImage[i][j], average);
						//image.SetPixel(i + horizontalOffset, j + verticalOffset, color);

						double val = 20 * Math.Log10(spectralImage[i][j]);
						Color color = Color.White;
						if (!double.IsNaN(val) && !double.IsInfinity(val)) {
							color = ValueToBlackWhiteColor(val, maxValue);
						}
						Brush brush = new SolidBrush(color);
						//graphics.FillRectangle(brush, i + horizontalOffset, j + verticalOffset, (int)deltaX + 1, 1);
						//image.SetPixel(i + horizontalOffset, j + verticalOffset, color);
						graphics.FillRectangle(brush, (i + horizontalOffset)*blockSizeX, (height - j + verticalOffset + 1)*blockSizeY, blockSizeX, blockSizeY);
					}
				}

				count++;
				if (count % imagesPerRow == 0)
				{
					verticalOffset += height + SpaceBetweenImages;
					horizontalOffset = SpaceBetweenImages;
				}
				else
				{
					horizontalOffset += width + SpaceBetweenImages;
				}
			}

			image = ColorUtils.Colorize(image, 255, ColorUtils.ColorPaletteType.MATLAB);
			return image;
		}

		public Image GetWaveletsImages(
			double[][] spectrum,
			IStride strideBetweenConsecutiveImages,
			int fingerprintLength,
			int overlap,
			int imagesPerRow)
		{
			List<double[][]> spectralImages = spectrumService.CutLogarithmizedSpectrum(
				spectrum, strideBetweenConsecutiveImages, fingerprintLength, overlap);
			waveletService.ApplyWaveletTransformInPlace(spectralImages);

			int width = spectralImages[0].GetLength(0);
			int height = spectralImages[0][0].Length;
			int fingersCount = spectralImages.Count;
			int rowCount = (int)Math.Ceiling((float)fingersCount / imagesPerRow);
			int imageWidth = (imagesPerRow * (width + SpaceBetweenImages)) + SpaceBetweenImages;
			int imageHeight = (rowCount * (height + SpaceBetweenImages)) + SpaceBetweenImages;
			Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);

			SetBackground(image, Color.White);

			int verticalOffset = SpaceBetweenImages;
			int horizontalOffset = SpaceBetweenImages;
			int count = 0;
			foreach (double[][] spectralImage in spectralImages)
			{
				double average = spectralImage.Average(col => col.Average(v => Math.Abs(v)));
				for (int i = 0; i < width /*128*/; i++)
				{
					for (int j = 0; j < height /*32*/; j++)
					{
						Color color = ValueToBlackWhiteColor(spectralImage[i][j], average);
						image.SetPixel(i + horizontalOffset, j + verticalOffset, color);
					}
				}

				count++;
				if (count % imagesPerRow == 0)
				{
					verticalOffset += height + SpaceBetweenImages;
					horizontalOffset = SpaceBetweenImages;
				}
				else
				{
					horizontalOffset += width + SpaceBetweenImages;
				}
			}

			return image;
		}

		public Image GetWaveletTransformedImage(double[][] image, IWaveletDecomposition wavelet)
		{
			int width = image[0].Length;
			int height = image.Length;
			wavelet.DecomposeImageInPlace(image);
			Bitmap transformed = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
			for (int i = 0; i < transformed.Height; i++)
			{
				for (int j = 0; j < transformed.Width; j++)
				{
					transformed.SetPixel(j, i, Color.FromArgb((int)image[i][j]));
				}
			}

			return transformed;
		}

		private Color ValueToBlackWhiteColor(double value, double maxValue)
		{
			int color = (int)(Math.Abs(value) * 255 / Math.Abs(maxValue));
			if (color > 255)
			{
				color = 255;
			}

			return Color.FromArgb(color, color, color);
		}

		private void FillBackgroundColor(int width, int height, Graphics graphics, Color color)
		{
			using (Brush brush = new SolidBrush(color))
			{
				graphics.FillRectangle(brush, new Rectangle(0, 0, width, height));
			}
		}

		private void DrawCopyrightInfo(Graphics graphics, int x, int y)
		{
			FontFamily fontFamily = new FontFamily("Courier New");
			Font font = new Font(fontFamily, 10);
			Brush textbrush = Brushes.White;
			Point coordinate = new Point(x, y);
			graphics.DrawString("https://github.com/AddictedCS/soundfingerprinting", font, textbrush, coordinate);
		}

		private void DrawGridlines(int width, int height, Graphics graphics)
		{
			const int Gridline = 50; /*Every 50 pixels draw gridline*/
			/*Draw gridlines*/
			using (Pen pen = new Pen(Color.Red, 1))
			{
				/*Draw horizontal gridlines*/
				for (int i = 1; i < height / Gridline; i++)
				{
					graphics.DrawLine(pen, 0, i * Gridline, width, i * Gridline);
				}

				/*Draw vertical gridlines*/
				for (int i = 1; i < width / Gridline; i++)
				{
					graphics.DrawLine(pen, i * Gridline, 0, i * Gridline, height);
				}
			}
		}

		private void DrawFingerprintInImage(
			Bitmap image, bool[] fingerprint, int fingerprintWidth, int fingerprintHeight, int xOffset, int yOffset)
		{
			// Scale the fingerprints and write to image
			for (int i = 0; i < fingerprintWidth /*128*/; i++)
			{
				for (int j = 0; j < fingerprintHeight /*32*/; j++)
				{
					// if 10 or 01 element then its white
					Color color = fingerprint[(2 * fingerprintHeight * i) + (2 * j)]
						|| fingerprint[(2 * fingerprintHeight * i) + (2 * j) + 1]
						? Color.White
						: Color.Black;
					image.SetPixel(xOffset + i, yOffset + j, color);
				}
			}
		}

		private void SetBackground(Bitmap image, Color color)
		{
			for (int i = 0; i < image.Width; i++)
			{
				for (int j = 0; j < image.Height; j++)
				{
					image.SetPixel(i, j, color);
				}
			}
		}
	}
}