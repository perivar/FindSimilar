using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Comirva.Audio.Util.Maths;

namespace Wavelets
{
	/// <summary>
	/// Description of WaveletUtils.
	/// </summary>
	public static class WaveletUtils
	{
		public static void SaveWaveletImage(string imageInPath, string imageOutPath) {
			Image img = Image.FromFile(imageInPath);
			Bitmap bmp = new Bitmap(img);
			double[][] argb = new double[bmp.Height][];
			for (int i = 0; i < bmp.Height; i++)
			{
				argb[i] = new double[bmp.Width];
				for (int j = 0; j < bmp.Width; j++) argb[i][j] = bmp.GetPixel(j, i).ToArgb();
			}

			Image image = GetWaveletTransformedImage(argb, new StandardHaarWaveletDecomposition());
			image.Save(imageOutPath, ImageFormat.Jpeg);
			img.Dispose();
			bmp.Dispose();
			image.Dispose();
		}
		
		public static Image GetWaveletTransformedImage(double[][] image, IWaveletDecomposition wavelet)
		{
			int width = image[0].Length;
			int height = image.Length;
			
			//wavelet.DecomposeImageInPlace(image);
			
			Matrix matrix = new Matrix(image);
			Wavelets.Dwt dwt = new Wavelets.Dwt(2);
			Matrix dwtMatrix = dwt.DTW(matrix);
			
			Bitmap transformed = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
			for (int i = 0; i < transformed.Height; i++)
			{
				for (int j = 0; j < transformed.Width; j++)
				{
					transformed.SetPixel(j, i, Color.FromArgb((int)dwtMatrix.MatrixData[i][j]));
				}
			}

			return transformed;
		}
		
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
		
		public static void TestHaar() {
			
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
			Matrix dwtMatrix = dwt.DTW(matrix);
			dwtMatrix.Print();
			
			Console.Write("\n\nThe 2D IDWT method: ");
			Console.Write("\n");
			Matrix idwtMatrix = dwt.IDTW(dwtMatrix);
			idwtMatrix.Print();
			
			/*
			List<Matrix> matrices = new List<Matrix>();
			matrices.Add(matrix);
			IEnumerable<Matrix> dwt_result = dwt.Fwd(matrices);
			dwt_result.First().Print();
			
			IEnumerable<Matrix> idwt_result = dwt.Back(dwt_result);
			idwt_result.First().Print();
			 */
		}
	}
}
