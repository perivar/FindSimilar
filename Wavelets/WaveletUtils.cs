using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Comirva.Audio.Util.Maths;
using CommonUtils;

namespace Wavelets
{
	/// <summary>
	/// Description of WaveletUtils.
	/// </summary>
	public static class WaveletUtils
	{
		private static Matrix HaarWaveletTransform(double[][] image) {
			Matrix imageMatrix = new Matrix(image);
			double[] imagePacked = imageMatrix.GetColumnPackedCopy();
			HaarTransform.haar_2d(imageMatrix.Rows, imageMatrix.Columns, imagePacked);
			Matrix haarMatrix = new Matrix(imagePacked, imageMatrix.Rows);
			return haarMatrix;
		}

		private static Matrix InverseHaarWaveletTransform(double[][] image) {
			Matrix imageMatrix = new Matrix(image);
			double[] imagePacked = imageMatrix.GetColumnPackedCopy();
			HaarTransform.haar_2d_inverse(imageMatrix.Rows, imageMatrix.Columns, imagePacked);
			Matrix inverseHaarMatrix = new Matrix(imagePacked, imageMatrix.Rows);
			return inverseHaarMatrix;
		}
		
		public static void TestDenoise(string imageInPath) {
			
			// Read Image
			Image img = Image.FromFile(imageInPath);
			Bitmap bmp = new Bitmap(img);
			double[][] image = new double[bmp.Height][];
			for (int i = 0; i < bmp.Height; i++)
			{
				image[i] = new double[bmp.Width];
				for (int j = 0; j < bmp.Width; j++) {
					//image[i][j] = bmp.GetPixel(j, i).ToArgb();
					image[i][j] = bmp.GetPixel(j, i).B; // use only blue channel
				}
			}

			//Matrix imageMatrix = new Matrix(image);
			//imageMatrix.WriteCSV("lena-blue.csv", ";");

			// Normalize the pixel values to the range 0..1.0. It does this by dividing all pixel values by the max value.
			double max = image.Max((b) => b.Max((v) => Math.Abs(v)));
			double[][] imageNormalized = image.Select(i => i.Select(j => j/max).ToArray()).ToArray();
			
			Matrix normalizedMatrix = new Matrix(imageNormalized);
			//normalizedMatrix.WriteCSV("lena-normalized.csv", ";");
			normalizedMatrix.DrawMatrixImage("lena-original.png", -1, -1, false);

			// Add Noise using normally distributed pseudorandom numbers
			// image_noisy = image_normalized + 0.1 * randn(size(image_normalized));
			TestSimpleRNG.SimpleRNG.SetSeedFromSystemTime();
			double[][] imageNoisy = imageNormalized.Select(i => i.Select(j => j + (0.1 * TestSimpleRNG.SimpleRNG.GetNormal())).ToArray()).ToArray();
			Matrix matrixNoisy = new Matrix(imageNoisy);
			matrixNoisy.DrawMatrixImage("lena-noisy.png", -1, -1, false);

			// Haar Wavelet Transform
			Matrix haarMatrix = HaarWaveletTransform(imageNoisy);

			// Thresholding
			double threshold = 0.15;
			double[][] yHard = Thresholding.perform_hard_thresholding(haarMatrix.MatrixData, threshold);
			double[][] ySoft = Thresholding.perform_soft_thresholding(haarMatrix.MatrixData, threshold);
			double[][] ySemisoft = Thresholding.perform_semisoft_thresholding(haarMatrix.MatrixData, threshold, threshold*2);
			double[][] ySemisoft2 = Thresholding.perform_semisoft_thresholding(haarMatrix.MatrixData, threshold, threshold*4);
			double[][] yStrict = Thresholding.perform_strict_thresholding(haarMatrix.MatrixData, 20);
			
			// Inverse 2D Haar Wavelet Transform
			Matrix zHard = InverseHaarWaveletTransform(yHard);
			Matrix zSoft = InverseHaarWaveletTransform(ySoft);
			Matrix zSemisoft = InverseHaarWaveletTransform(ySemisoft);
			Matrix zSemisoft2 = InverseHaarWaveletTransform(ySemisoft2);
			Matrix zStrict = InverseHaarWaveletTransform(yStrict);
			
			//zHard.WriteCSV("lena-thresholding-hard.csv", ";");

			// Output the images
			zHard.DrawMatrixImage("lena-thresholding-hard.png", -1, -1, false);
			zSoft.DrawMatrixImage("lena-thresholding-soft.png", -1, -1, false);
			zSemisoft.DrawMatrixImage("lena-thresholding-semisoft.png", -1, -1, false);
			zSemisoft2.DrawMatrixImage("lena-thresholding-semisoft2.png", -1, -1, false);
			zStrict.DrawMatrixImage("lena-thresholding-strict.png", -1, -1, false);
		}

		public static void SaveWaveletImage(string imageInPath, string imageOutPath, bool useStandardHaarWaveletDecomposition) {

			// Read Image
			Image img = Image.FromFile(imageInPath);
			Bitmap bmp = new Bitmap(img);
			double[][] image = new double[bmp.Height][];
			for (int i = 0; i < bmp.Height; i++)
			{
				image[i] = new double[bmp.Width];
				for (int j = 0; j < bmp.Width; j++) {
					//image[i][j] = bmp.GetPixel(j, i).ToArgb();
					image[i][j] = bmp.GetPixel(j, i).B; // use only blue channel
				}
			}

			// Normalize the pixel values to the range 0..1.0. It does this by dividing all pixel values by the max value.
			double max = image.Max((b) => b.Max((v) => (v)));
			double[][] imageNormalized = image.Select(i => i.Select(j => j/max).ToArray()).ToArray();
			//Matrix normalizedMatrix = new Matrix(imageNormalized);
			//normalizedMatrix.WriteCSV("ImageNormalized.csv", ";");
			
			Matrix bitmap = GetWaveletTransformedMatrix(imageNormalized, useStandardHaarWaveletDecomposition);
			bitmap.DrawMatrixImage(imageOutPath, -1, -1, false);

			img.Dispose();
			bmp.Dispose();
			bitmap = null;
		}
		
		public static Matrix GetWaveletTransformedMatrix(double[][] image, bool useStandardHaarWaveletDecomposition)
		{
			int width = image[0].Length;
			int height = image.Length;

			Matrix dwtMatrix = null;
			if (useStandardHaarWaveletDecomposition) {
				IWaveletDecomposition haar = new StandardHaarWaveletDecomposition();
				haar.DecomposeImageInPlace(image);
				dwtMatrix = new Matrix(image);
			} else {
				//Matrix matrix = new Matrix(image);
				//Wavelets.Dwt dwt = new Wavelets.Dwt(2);
				//dwtMatrix = dwt.DWT(matrix);
				dwtMatrix = HaarWaveletTransform(image);
			}
			
			//dwtMatrix.WriteCSV("HaarImageNormalized.csv", ";");
			
			// increase all values
			double[][] haarImageNormalized5k = dwtMatrix.MatrixData.Select(i => i.Select(j => j*5000).ToArray()).ToArray();
			//Matrix haarImageNormalized5kMatrix = new Matrix(haarImageNormalized5k);
			//haarImageNormalized5kMatrix.WriteCSV("HaarImageNormalized5k.csv", ";");
			
			// convert to byte values (0 - 255)
			double[][] uint8 = new double[haarImageNormalized5k.Length][];
			for (int i = 0; i < haarImageNormalized5k.Length; i++) {
				uint8[i] = new double[haarImageNormalized5k.Length];
				for (int j = 0; j < haarImageNormalized5k[i].Length; j++) {
					double v = haarImageNormalized5k[i][j];
					if (v > 255) {
						uint8[i][j] = 255;
					} else if (v < 0) {
						uint8[i][j] = 0;
					} else {
						uint8[i][j] = (byte) haarImageNormalized5k[i][j];
					}
				}
			}
			
			Matrix uint8Matrix = new Matrix(uint8);
			//uint8Matrix.WriteCSV("Uint8HaarImageNormalized5k.csv", ";");
			return uint8Matrix;
		}
		
		/*
		 * mat = [5, 6, 1, 2; 4, 2, 5, 5; 3, 1, 7, 1; 6, 3, 5, 1]
		 */
		private static double[][] Get2DTestData() {
			
			double[][] mat = new double[4][];
			for(int m = 0; m < 4; m++) {
				mat[m] = new double[4];
			}
			mat[0][0] = 5;
			mat[0][1] = 6;
			mat[0][2] = 1;
			mat[0][3] = 2;

			mat[1][0] = 4;
			mat[1][1] = 2;
			mat[1][2] = 5;
			mat[1][3] = 5;

			mat[2][0] = 3;
			mat[2][1] = 1;
			mat[2][2] = 7;
			mat[2][3] = 1;

			mat[3][0] = 6;
			mat[3][1] = 3;
			mat[3][2] = 5;
			mat[3][3] = 1;
			
			return mat;
		}

		public static void TestHaar1d() {
			
			int i = 0;
			double[] vec3 = { 4, 2, 5, 5 };

			Haar.Haar1d(vec3, 4);

			Console.Write("The 1D Haar Transform: ");
			Console.Write("\n");
			for(i = 0; i < 4; i++)
			{
				Console.Write(vec3[i]);
				Console.Write(" ");
			}
			Console.Write("\n");
		}
		
		public static void TestHaar2d() {
			
			Console.Write("\n\nThe 2D Haar Transform: ");
			Console.Write("\n");
			
			double[][] mat = Get2DTestData();
			Haar.Haar2d(mat, 4, 4);

			Matrix result = new Matrix(mat);
			result.Print();
		}
		
		public static void TestHaarWaveletDecomposition() {
			
			Console.Write("\n\nThe 2D HaarWaveletDecomposition method: ");
			Console.Write("\n");
			
			IWaveletDecomposition haar = new StandardHaarWaveletDecomposition();
			
			double[][] mat = Get2DTestData();
			
			haar.DecomposeImageInPlace(mat);

			Matrix result = new Matrix(mat);
			result.Print();
		}
		
		public static void TestDwt() {
			
			double[][] mat = Get2DTestData();
			Matrix matrix = new Matrix(mat);
			Wavelets.Dwt dwt = new Wavelets.Dwt(2);

			Console.Write("\n\nThe 2D DWT method: ");
			Console.Write("\n");
			Matrix dwtMatrix = dwt.DWT(matrix);
			dwtMatrix.Print();
			
			Console.Write("\n\nThe 2D IDWT method: ");
			Console.Write("\n");
			Matrix idwtMatrix = dwt.IDWT(dwtMatrix);
			idwtMatrix.Print();
		}
		
		public static void TestHaarTransform() {
			
			double[][] mat = Get2DTestData();
			Matrix matrix = new Matrix(mat);
			//matrix.Print();
			
			double[] packed = matrix.GetColumnPackedCopy();
			HaarTransform.r8mat_print (matrix.Rows, matrix.Columns, packed, "  Input array packed:");

			HaarTransform.haar_2d(matrix.Rows, matrix.Columns, packed);
			HaarTransform.r8mat_print (matrix.Rows, matrix.Columns, packed, "  Transformed array packed:");
			
			double[] w = HaarTransform.r8mat_copy_new(matrix.Rows, matrix.Columns, packed);

			HaarTransform.haar_2d_inverse (matrix.Rows, matrix.Columns, w);
			HaarTransform.r8mat_print (matrix.Rows, matrix.Columns, w, "  Recovered array W:");
			
			Matrix m = new Matrix(w, matrix.Rows);
			//m.Print();
		}
	}
}
