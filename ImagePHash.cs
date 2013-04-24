using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using CommonUtils;

namespace Imghash
{
	// * pHash-like image hash.
	// * Author: Elliot Shepherd (elliot@jarofworms.com)
	// * Based On: http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html
	// * Converted from Java to C# by Per Ivar Nerseth (perivar@nerseth.com)
	public class ImagePHash
	{
		private int size = 32;			// 32
		private int smallerSize = 8; 	// 8
		
		public ImagePHash()
		{
		}

		public ImagePHash(int size, int smallerSize)
		{
			this.size = size;
			this.smallerSize = smallerSize;
		}
		
		public int HammingDistance(string s1, string s2)
		{
			int counter = 0;
			for (int k = 0; k < s1.Length;k++)
			{
				if(s1[k] != s2[k])
				{
					counter++;
				}
			}
			return counter;
		}

		// Returns a 'binary string' (aka bitstring) (like. 001010111011100010) which is easy to do a hamming distance on.
		public string GetHash(Bitmap img)
		{
			// 1. Reduce size.
			// Like Average Hash, pHash starts with a small image.
			// However, the image is larger than 8x8; 32x32 is a good size.
			// This is really done to simplify the DCT computation and not
			// because it is needed to reduce the high frequencies.
			img = CommonUtils.ImageUtils.Resize(img, size, size);
			
			#if DEBUG
			img.Save("ImagePHash 1-reduced.png");
			#endif
			
			// 2. Reduce color.
			// The image is reduced to a grayscale just to further simplify
			// the number of computations.
			img = CommonUtils.ImageUtils.MakeGrayscale(img);
			
			#if DEBUG
			img.Save("ImagePHash 2-grayscale.png");
			#endif

			double[,] vals = new double[size,size];
			for (int x = 0; x < img.Width; x++)
			{
				for (int y = 0; y < img.Height; y++)
				{
					// when the image is grayscale RGB has the same value
					vals[x,y] = img.GetPixel(x, y).B;
				}
			}

			#if DEBUG
			// create byte array to be able to save the image
			byte[] grayByteArray = new byte[size*size];
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					// add to byte array
					grayByteArray[x + (y * size)] = Convert.ToByte(vals[x,y]);
				}
			}
			Image grayscale = CommonUtils.ImageUtils.ByteArrayGrayscaleToImage(grayByteArray,size,size);
			grayscale.Save("ImagePHash 3-grayscale-array.png");
			#endif
			
			//  3. Compute the DCT.
			//	The DCT separates the image into a collection of frequencies
			//	and scalars. While JPEG uses an 8x8 DCT, this algorithm uses
			//	a 32x32 DCT.
			Dct2 dct = new Dct2(size);
			double[,] dctVals = dct.Dct(vals);
			
			#if DEBUG
			// create byte array to be able to save the image
			double dctMin = 0;
			double dctMax = 0;
			CommonUtils.MathUtils.ComputeMinAndMax(dctVals, out dctMin, out dctMax);
			byte[] dctByteArray = new byte[size*size];
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					// add to byte array
					dctByteArray[x + (y * size)] = Convert.ToByte(CommonUtils.MathUtils.ConvertAndMainainRatio(dctVals[x,y], dctMin, dctMax, 0, 255));
				}
			}
			Image dctImage = CommonUtils.ImageUtils.ByteArrayGrayscaleToImage(dctByteArray,size,size);
			dctImage.Save("ImagePHash 4-dct.png");
			#endif
			
			// 4. Reduce the DCT.
			// This is the magic step. While the DCT is 32x32, just keep the
			// top-left 8x8. Those represent the lowest frequencies in the
			// picture.
			double total = 0;
			for (int x = 0; x < smallerSize; x++)
			{
				for (int y = 0; y < smallerSize; y++)
				{
					total += dctVals[x,y];
				}
			}
			total -= dctVals[0,0];

			// 5. Compute the average value.
			// Like the Average Hash, compute the mean DCT value (using only
			// the 8x8 DCT low-frequency values and excluding the first term
			// since the DC coefficient can be significantly different from
			// the other values and will throw off the average).
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
						hash += (dctVals[x,y] > avg ? "1" : "0");
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
		/// <returns>The hash of the image.</returns>
		public ulong PHash(Bitmap img)
		{
			return StringUtils.BinaryStringToLong(GetHash(img));
		}
	}
}