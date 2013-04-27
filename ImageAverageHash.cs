using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using CommonUtils;
using Imghash;

namespace Imghash
{
	// http://stackoverflow.com/questions/4240490/problems-with-dct-and-idct-algorithm-in-java
	// http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html
	// http://pastebin.com/Pj9d8jt5
	// https://github.com/jforshee/ImageHashing/blob/master/ImageHashing/ImageHashing.cs
	public class ImageAverageHash
	{
		private static BitCounter bitCounter = new BitCounter(8);
		
		/// <summary>
		/// Computes the average hash of an image according to the algorithm given by Dr. Neal Krawetz
		/// on his blog: http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html.
		/// </summary>
		/// <param name="image">The image to hash.</param>
		/// <returns>The hash of the image.</returns>
		public static ulong AverageHash(Image image)
		{
			int smallerSize = 8;
			string fileSavePrefix = "AverageHash (" + StringUtils.GetCurrentTimestamp() + ") ";
			
			#if DEBUG
			image.Save(fileSavePrefix + "1-orig.png");
			#endif
			
			Bitmap squeezedImage = CommonUtils.ImageUtils.Resize(image, smallerSize, smallerSize);
			
			#if DEBUG
			squeezedImage.Save(fileSavePrefix + "2-squeezed.png");
			#endif

			uint averageValue = 0;
			byte[] grayscaleByteArray = CommonUtils.ImageUtils.ImageToByteArray8BitGrayscale(squeezedImage, out averageValue);

			#if DEBUG
			Image fromBinary = CommonUtils.ImageUtils.ByteArray8BitGrayscaleToImage(grayscaleByteArray, smallerSize, smallerSize);
			fromBinary.Save(fileSavePrefix + "3-grayFromArray.png");

			Bitmap grayscaleImage = CommonUtils.ImageUtils.MakeGrayscaleFastest(squeezedImage);
			grayscaleImage.Save(fileSavePrefix + "4-grayscale.png");
			#endif
			
			// Compute the hash: each bit is a pixel
			// 1 = higher than average, 0 = lower than average
			ulong hash = 0; // = Unsigned 64-bit integer
			for(int i = 0; i < (squeezedImage.Width * squeezedImage.Height); i++)
			{
				if(grayscaleByteArray[i] >= averageValue) {
					hash |= (1UL << ((squeezedImage.Width * squeezedImage.Height - 1) - i));
				}
			}
			return hash;
		}

		/// <summary>
		/// Computes the average hash of the image content in the given file.
		/// </summary>
		/// <param name="path">Path to the input file.</param>
		/// <returns>The hash of the input file's image content.</returns>
		public static ulong AverageHash(String path)
		{
			Bitmap bmp = new Bitmap(path);
			return AverageHash(bmp);
		}

		/// <summary>
		/// Returns a percentage-based similarity value between the two given hashes. The higher
		/// the percentage, the closer the hashes are to being identical.
		/// </summary>
		/// <param name="hash1">The first hash.</param>
		/// <param name="hash2">The second hash.</param>
		/// <returns>The similarity percentage.</returns>
		public static double Similarity(ulong hash1, ulong hash2)
		{
			int s1 = bitCounter.CountOnesWithPrecomputation(hash1 ^ hash2);
			//int s2 = BitCounter.BitCount(hash1 ^ hash2);
			//int s3 = BitCounter.Hamming(hash1, hash2);
			
			return ((64 - s1) * 100) / 64.0;
			//return ((64 - bitCounter.CountOnesWithPrecomputation(hash1 ^ hash2)) * 100) / 64.0;
		}

		/// <summary>
		/// Returns a percentage-based similarity value between the two given images. The higher
		/// the percentage, the closer the images are to being identical.
		/// </summary>
		/// <param name="image1">The first image.</param>
		/// <param name="image2">The second image.</param>
		/// <returns>The similarity percentage.</returns>
		public static double Similarity(Image image1, Image image2)
		{
			ulong hash1 = AverageHash(image1);
			ulong hash2 = AverageHash(image2);
			return Similarity(hash1, hash2);
		}

		/// <summary>
		/// Returns a percentage-based similarity value between the image content of the two given
		/// files. The higher the percentage, the closer the image contents are to being identical.
		/// </summary>
		/// <param name="image1">The first image file.</param>
		/// <param name="image2">The second image file.</param>
		/// <returns>The similarity percentage.</returns>
		public static double Similarity(String path1, String path2)
		{
			ulong hash1 = AverageHash(path1);
			ulong hash2 = AverageHash(path2);
			return Similarity(hash1, hash2);
		}
	}
}
