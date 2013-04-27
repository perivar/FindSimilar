using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using CommonUtils;
using DiscreteCosineTransform;

namespace Imghash
{
	// * pHash-like image hash.
	// * Author: Elliot Shepherd (elliot@jarofworms.com)
	// * Based On: http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html
	// * Converted from Java to C# by Per Ivar Nerseth (perivar@nerseth.com)
	public class ImagePHash
	{
		private int size = 32;			// 32 (64 usable?)
		private int smallerSize = 8; 	// 8  (16 usable?)
		
		public ImagePHash()
		{
		}

		public ImagePHash(int size, int smallerSize)
		{
			this.size = size;
			this.smallerSize = smallerSize;
		}
		
		/// <summary>
		/// Return similar (lower values = more similar)
		/// </summary>
		/// <param name="s1">binary string 1 (aka bitstring)</param>
		/// <param name="s2">binary string 2 (aka bitstring)</param>
		/// <returns>Number of different bits (lower values equals more similar)</returns>
		public static int HammingDistance(string s1, string s2)
		{
			// use shortest string
			int lengthS1 = s1.Length;
			int lengthS2 = s2.Length;
			int length = lengthS2 > lengthS1 ? lengthS1 : lengthS2;
			
			int counter = 0;
			for (int k = 0; k < length;k++)
			{
				if(s1[k] != s2[k])
				{
					counter++;
				}
			}
			return counter;
		}

		/// <summary>
		/// Return percentage similar
		/// </summary>
		/// <param name="s1">binary string 1 (aka bitstring)</param>
		/// <param name="s2">binary string 2 (aka bitstring)</param>
		/// <returns>Percentage Similar</returns>
		public static double Similarity(string s1, string s2) {
			int different = HammingDistance(s1, s2);
			double perc = (double) different / (double) s1.Length;
			return 100 * (1 - perc);
		}
		
		/// <summary>
		/// Computes the perceptual hash of an image according to the algorithm given by Dr. Neal Krawetz
		/// on his blog: http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html.
		/// </summary>
		/// <param name="image">The image to hash.</param>
		/// <returns>Returns a 'binary string' (aka bitstring) (like. 001010111011100010) which is easy to do a hamming distance on.</returns>
		public string GetHash(Bitmap img)
		{
			#if DEBUG
			img.Save("ImagePHash 1-orig.png");
			#endif
			
			// 1. Reduce size.
			// Like Average Hash, pHash starts with a small image.
			// However, the image is larger than 8x8; 32x32 is a good size.
			// This is really done to simplify the DCT computation and not
			// because it is needed to reduce the high frequencies.
			img = CommonUtils.ImageUtils.Resize(img, size, size);
			
			#if DEBUG
			img.Save("ImagePHash 2-reduced.png");
			#endif
			
			// 2. Reduce color.
			// The image is reduced to a grayscale just to further simplify
			// the number of computations.
			img = CommonUtils.ImageUtils.MakeGrayscale(img);
			
			#if DEBUG
			img.Save("ImagePHash 3-grayscale.png");
			#endif

			double[][] vals = new double[size][];
			for (int x = 0; x < img.Width; x++) {
				vals[x] = new double[size];
				for (int y = 0; y < img.Height; y++) {
					// when the image is grayscale RGB has the same value
					vals[x][y] = img.GetPixel(x, y).B;
				}
			}

			#if DEBUG
			// create byte array to be able to save the image
			byte[] grayscaleByteArray = new byte[size*size];
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					// add to byte array
					grayscaleByteArray[x + (y * size)] = Convert.ToByte(vals[x][y]);
				}
			}
			Image grayscale = CommonUtils.ImageUtils.ByteArrayGrayscaleToImage(grayscaleByteArray,size,size);
			grayscale.Save("ImagePHash 4-grayscale-array.png");
			#endif
			
			//  3. Compute the DCT.
			//	The DCT separates the image into a collection of frequencies
			//	and scalars. While JPEG uses an 8x8 DCT, this algorithm uses
			//	a 32x32 DCT.
			double[][] dctVals = DctMethods.dct2(vals);
			
			#if DEBUG
			// create image array to be able to save DCT image

			// array to keep only the highest frequency items
			double[][] dctValsOnlyHighFreq = new double[size][];

			// Compressing Range By taking Log
			double[][] dctLogVals = new double[dctVals.Length][];
			for (int x = 0; x < size; x++) {
				dctValsOnlyHighFreq[x] = new double[dctVals[x].Length];
				dctLogVals[x] = new double[dctVals[x].Length];
				for (int y = 0; y < size; y++) {
					dctLogVals[x][y] = Math.Log(1 + Math.Abs((int)dctVals[x][y]));
				}
			}
			
			// Normalizing Array
			double dctMin = 0;
			double dctMax = 0;
			CommonUtils.MathUtils.ComputeMinAndMax(dctLogVals, out dctMin, out dctMax);
			byte[] dctPixels = new byte[size*size];
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					// Constrain Range between 0 and 255
					int dctPixelVal = (int)(((float)(dctLogVals[x][y] - dctMin) / (float)(dctMax - dctMin)) * 255);
					dctPixels[x + (y * size)] = Convert.ToByte(dctPixelVal);
				}
			}

			Image dctImage = CommonUtils.ImageUtils.ByteArrayGrayscaleToImage(dctPixels,size,size);
			dctImage.Save("ImagePHash 5-dct.png");
			#endif
			
			// 4. Reduce the DCT.
			// This is the magic step. While the DCT is 32x32, just keep the
			// top-left 8x8. Those represent the lowest frequencies in the
			// picture.

			#if DEBUG
			// create image array to be able to save DCT image
			byte[] dctPixelsSmallerSize = new byte[smallerSize*smallerSize];
			#endif

			// 5 a) Compute the average value.
			// Like the Average Hash, compute the mean DCT value (using only
			// the 8x8 DCT low-frequency values and excluding the first term
			// since the DC coefficient can be significantly different from
			// the other values and will throw off the average).
			double total = 0;
			for (int x = 0; x < smallerSize; x++) {
				for (int y = 0; y < smallerSize; y++) {
					#if DEBUG
					dctValsOnlyHighFreq[x][y] = dctVals[x][y]; // store in new dct val array

					// convert to pixel values
					int dctPixelSmallerVal = (int)(((float)(dctLogVals[x][y] - dctMin) / (float)(dctMax - dctMin)) * 255);
					dctPixelsSmallerSize[x + (y * smallerSize)] = Convert.ToByte(dctPixelSmallerVal);
					#endif
					total += dctVals[x][y];
				}
			}
			total -= dctVals[0][0];

			#if DEBUG
			Image dctImageSmaller = CommonUtils.ImageUtils.ByteArrayGrayscaleToImage(dctPixelsSmallerSize,smallerSize,smallerSize);
			dctImageSmaller.Save("ImagePHash 6-dct-smaller.png");
			
			// Inverse DCT
			double[][] inverseDctVals = DctMethods.idct2(dctValsOnlyHighFreq);

			byte[] idctPixels = new byte[size*size];
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					double inverseDctVal = inverseDctVals[x][y];
					inverseDctVal = inverseDctVal < 0 ? 0 : inverseDctVal;
					inverseDctVal = inverseDctVal > 255 ? 255 : inverseDctVal;
					idctPixels[x + (y * size)] = Convert.ToByte(inverseDctVal);
				}
			}
			Image idctImage = CommonUtils.ImageUtils.ByteArrayGrayscaleToImage(idctPixels,size,size);
			idctImage.Save("ImagePHash 7-idct.png");
			#endif
			
			// 5. b) Compute the average value.
			double avg = total / (double)((smallerSize * smallerSize) - 1);

			// 6. Further reduce the DCT.
			// This is the magic step. Set the 64 hash bits to 0 or 1
			// depending on whether each of the 64 DCT values is above or
			// below the average value. The result doesn't tell us the
			// actual low frequencies; it just tells us the very-rough
			// relative scale of the frequencies to the mean. The result
			// will not vary as long as the overall structure of the image
			// remains the same; this can survive gamma and color histogram
			// adjustments without a problem.
			string hash = "";
			for (int x = 0; x < smallerSize; x++)
			{
				for (int y = 0; y < smallerSize; y++)
				{
					if (x != 0 && y != 0)
					{
						hash += (dctVals[x][y] > avg ? "1" : "0");
					}
				}
			}

			return hash;
		}

		/// <summary>
		/// Computes the perceptual hash of an image according to the algorithm given by Dr. Neal Krawetz
		/// on his blog: http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html.
		/// </summary>
		/// <param name="image">The image to hash.</param>
		/// <returns>The hash of the image as an ulong.</returns>
		public ulong PHash(Bitmap img)
		{
			return StringUtils.BinaryStringToLong(GetHash(img));
		}
	}
}