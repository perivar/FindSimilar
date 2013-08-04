using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;

using Comirva.Audio.Util.Maths;
using CommonUtils;

using math.transform.jwave;
using math.transform.jwave.handlers;
using math.transform.jwave.handlers.wavelets;

namespace Wavelets
{
	public enum WaveletMethod : int {
		Dwt = 1,
		Haar = 2,
		HaarTransformTensor = 3,
		HaarWaveletDecompositionTensor = 4,
		HaarWaveletDecomposition = 5,
		NonStandardHaarWaveletDecomposition = 6,
		JWaveTensor = 7
	}
	
	/// <summary>
	/// Description of WaveletUtils.
	/// </summary>
	public static class WaveletUtils
	{
		/// <summary>
		/// Haar Transform of a 2D image to a Matrix.
		/// This is using the tensor product layout.
		/// Performance is also quite fast.
		/// </summary>
		/// <param name="image">2D array</param>
		/// <returns>Matrix with haar transform</returns>
		public static Matrix HaarWaveletTransform(double[][] image) {
			Matrix imageMatrix = new Matrix(image);
			double[] imagePacked = imageMatrix.GetColumnPackedCopy();
			HaarTransform.haar_2d(imageMatrix.Rows, imageMatrix.Columns, imagePacked);
			Matrix haarMatrix = new Matrix(imagePacked, imageMatrix.Rows);
			return haarMatrix;
		}

		/// <summary>
		/// Inverse Haar Transform of a 2D image to a Matrix.
		/// This is using the tensor product layout.
		/// Performance is also quite fast.
		/// </summary>
		/// <param name="image">2D array</param>
		/// <returns>Matrix with inverse haar transform</returns>
		public static Matrix InverseHaarWaveletTransform(double[][] image) {
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

		//Func<int, int, double> window
		public static void SaveWaveletImage(string imageInPath, string imageOutPath, WaveletMethod waveletMethod) {

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
			
			Matrix bitmap = GetWaveletTransformedMatrix(imageNormalized, waveletMethod);
			bitmap.DrawMatrixImage(imageOutPath, -1, -1, false);

			img.Dispose();
			bmp.Dispose();
			bitmap = null;
		}
		
		public static Matrix GetWaveletTransformedMatrix(double[][] image, WaveletMethod waveletMethod)
		{
			int width = image[0].Length;
			int height = image.Length;

			Matrix dwtMatrix = null;

			Stopwatch stopWatch = Stopwatch.StartNew();
			long startS = stopWatch.ElapsedTicks;

			switch(waveletMethod) {
				case WaveletMethod.Dwt:
					Wavelets.Dwt dwt = new Wavelets.Dwt(8);
					Matrix imageMatrix = new Matrix(image);
					dwtMatrix = dwt.Transform(imageMatrix);
					break;
				case WaveletMethod.Haar:
					Haar.Haar2d(image, height, width);
					dwtMatrix = new Matrix(image);
					break;
				case WaveletMethod.HaarTransformTensor: // This is using the tensor product layout
					dwtMatrix = HaarWaveletTransform(image);
					break;
				case WaveletMethod.HaarWaveletDecompositionTensor: // This is using the tensor product layout
					StandardHaarWaveletDecomposition haar = new StandardHaarWaveletDecomposition();
					haar.DecomposeImageInPlace(image);
					dwtMatrix = new Matrix(image);
					break;
				case WaveletMethod.HaarWaveletDecomposition:
					StandardHaarWaveletDecomposition haarNew = new StandardHaarWaveletDecomposition(false);
					haarNew.DecomposeImageInPlace(image);
					dwtMatrix = new Matrix(image);
					break;
				case WaveletMethod.NonStandardHaarWaveletDecomposition:
					NonStandardHaarWaveletDecomposition haarNonStandard = new NonStandardHaarWaveletDecomposition();
					haarNonStandard.DecomposeImageInPlace(image);
					dwtMatrix = new Matrix(image);
					break;
				case WaveletMethod.JWaveTensor: // This is using the tensor product layout
					WaveletInterface wavelet = null;
					wavelet = new Haar02();
					//wavelet = new Daub02();
					TransformInterface bWave = null;
					bWave = new FastWaveletTransform(wavelet);
					//bWave = new WaveletPacketTransform(wavelet);
					//bWave = new DiscreteWaveletTransform(wavelet);
					Transform t = new Transform(bWave); // perform all steps
					double[][] dwtArray = t.forward(image);
					dwtMatrix = new Matrix(dwtArray);
					break;
				default:
					break;
			}

			long endS = stopWatch.ElapsedTicks;
			Console.WriteLine("WaveletMethod: {0} Time in ticks: {1}", Enum.GetName(typeof(WaveletMethod), waveletMethod), (endS - startS));

			//dwtMatrix.WriteCSV("HaarImageNormalized.csv", ";");
			
			// increase all values
			double[][] haarImageNormalized5k = dwtMatrix.MatrixData.Select(i => i.Select(j => j*5000).ToArray()).ToArray();
			//Matrix haarImageNormalized5kMatrix = new Matrix(haarImageNormalized5k);
			//haarImageNormalized5kMatrix.WriteCSV("HaarImageNormalized5k.csv", ";");
			
			// convert to byte values (0 - 255)
			// duplicate the octave/ matlab method uint8
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
			
			Console.Write("\n\nThe Standard 2D HaarWaveletDecomposition method: ");
			Console.Write("\n");
			
			StandardHaarWaveletDecomposition haar = new StandardHaarWaveletDecomposition();
			
			double[][] mat = Get2DTestData();
			
			haar.DecomposeImageInPlace(mat);

			Matrix result = new Matrix(mat);
			result.Print();

			Console.Write("\n\nThe New Standard 2D HaarWaveletDecomposition method: ");
			Console.Write("\n");
			
			StandardHaarWaveletDecomposition haarNew = new StandardHaarWaveletDecomposition(false);
			
			mat = Get2DTestData();
			haarNew.DecomposeImageInPlace(mat);

			Matrix resultNew = new Matrix(mat);
			resultNew.Print();

			Console.Write("\n\nThe Non Standard 2D HaarWaveletDecomposition method: ");
			Console.Write("\n");
			
			NonStandardHaarWaveletDecomposition haarNonStandard = new NonStandardHaarWaveletDecomposition();
			
			mat = Get2DTestData();
			haarNonStandard.DecomposeImageInPlace(mat);

			Matrix resultNonStandard = new Matrix(mat);
			resultNonStandard.Print();
		}
		
		public static void TestDwt() {
			
			double[][] mat = Get2DTestData();
			Matrix matrix = new Matrix(mat);
			Wavelets.Dwt dwt = new Wavelets.Dwt(2);

			Console.Write("\n\nThe 2D DWT method: ");
			Console.Write("\n");
			Matrix dwtMatrix = dwt.Transform(matrix);
			dwtMatrix.Print();
			
			Console.Write("\n\nThe 2D IDWT method: ");
			Console.Write("\n");
			Matrix idwtMatrix = dwt.TransformBack(dwtMatrix);
			idwtMatrix.Print();
		}
		
		public static void TestJWave() {

			double[][] mat = Get2DTestData();

			WaveletInterface wavelet = null;
			wavelet = new Haar02();
			TransformInterface bWave = null;
			//bWave = new FastWaveletTransform(wavelet);
			//bWave = new WaveletPacketTransform(wavelet);
			bWave = new DiscreteWaveletTransform(wavelet);
			Transform t = new Transform(bWave); // perform all steps
			
			Console.Write("\n\nThe 2D JWave Haar02 Dwt method: ");
			Console.Write("\n");
			double[][] dwtArray = t.forward(mat);
			
			Matrix dwtMatrix = new Matrix(dwtArray);
			dwtMatrix.Print();
			
			Console.Write("\n\nThe 2D JWave Haar02 Inverse Dwt method: ");
			Console.Write("\n");
			double[][] idwtArray = t.reverse(dwtArray);

			Matrix idwtMatrix = new Matrix(idwtArray);
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
