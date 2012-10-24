using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

// http://itssmee.wordpress.com/2010/06/28/java-example-of-hamming-distance/
// http://stackoverflow.com/questions/4240490/problems-with-dct-and-idct-algorithm-in-java
// http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html
// http://pastebin.com/Pj9d8jt5
// https://github.com/jforshee/ImageHashing/blob/master/ImageHashing/ImageHashing.cs

public class ImageHash
{

	private const bool doSaveimages = false;
	
	/// <summary>
	/// Computes the average hash of an image according to the algorithm given by Dr. Neal Krawetz
	/// on his blog: http://www.hackerfactor.com/blog/index.php?/archives/432-Looks-Like-It.html.
	/// </summary>
	/// <param name="image">The image to hash.</param>
	/// <returns>The hash of the image.</returns>
	public static ulong AverageHash(Image image)
	{
		int smallerSize = 8;
		if (doSaveimages) image.Save(@"C:\Users\perivar.nerseth\Documents\My Projects\Hashing\1-orig.png");
		
		Bitmap squeezedImage = CommonUtils.ImageUtils.Resize(image, smallerSize, smallerSize);
		if (doSaveimages) squeezedImage.Save(@"C:\Users\perivar.nerseth\Documents\My Projects\Hashing\2-squeezed.png");

		uint averageValue = 0;
		byte[] grayscaleByteArray = CommonUtils.ImageUtils.ImageToByteArray8BitGrayscale(squeezedImage, out averageValue);
		if (doSaveimages) {
			Image fromBinary = CommonUtils.ImageUtils.ByteArray8BitGrayscaleToImage(grayscaleByteArray, smallerSize, smallerSize);
			fromBinary.Save(@"C:\Users\perivar.nerseth\Documents\My Projects\Hashing\3-grayFromArray.png");
			Bitmap grayscaleImage = CommonUtils.ImageUtils.MakeGrayscaleFastest(squeezedImage);
			grayscaleImage.Save(@"C:\Users\perivar.nerseth\Documents\My Projects\Hashing\4-grayscale.png");
		}
		
		// Compute the hash: each bit is a pixel
		// 1 = higher than average, 0 = lower than average
		ulong hash = 0;
		for(int i = 0; i < (squeezedImage.Width * squeezedImage.Height); i++)
		{
			//if(i % squeezedImage.Width == 0) {
			//	Console.WriteLine();
			//}

			if(grayscaleByteArray[i] >= averageValue) {
				hash |= (1UL << ((squeezedImage.Width * squeezedImage.Height - 1) - i));
				//	Console.Write(" ");
				//} else {
				//	Console.Write("#");
			}
		}
		//Console.WriteLine();
		//Console.WriteLine();

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
		return ((64 - BitCount(hash1 ^ hash2)) * 100) / 64.0;
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
	
	#region Private constants and utility methods
	
	
	// see also http://www.dotnetperls.com/bitcount
	// http://stackoverflow.com/questions/3815165/how-to-implement-bitcount-using-only-bitwise-operators
	
	// Count the number of 1-bits in a number.
	// We use a precomputed table to hopefully speed it up.
	// Made in Python as follows:
	// a = list()
	// a.append(0)
	// while len(a) <= 128:
	//  a.extend([b+1 for b in a])
	/// <summary>
	/// Bitcounts array used for BitCount method (used in Similarity comparisons).
	/// Don't try to read this or understand it, I certainly don't. Credit goes to
	/// David Oftedal of the University of Oslo, Norway for this.
	/// http://folk.uio.no/davidjo/computing.php
	/// </summary>
	private static byte[] bitCounts = {
		0,1,1,2,1,2,2,3,1,2,2,3,2,3,3,4,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,1,2,2,3,2,3,3,4,
		2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,
		2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,
		4,5,5,6,5,6,6,7,1,2,2,3,2,3,3,4,2,3,3,4,3,4,4,5,2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,
		2,3,3,4,3,4,4,5,3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,2,3,3,4,3,4,4,5,
		3,4,4,5,4,5,5,6,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,3,4,4,5,4,5,5,6,4,5,5,6,5,6,6,7,
		4,5,5,6,5,6,6,7,5,6,6,7,6,7,7,8
	};

	/// <summary>
	/// Counts bits (duh). Utility function for similarity.
	/// I wouldn't try to understand this. I just copy-pasta'd it
	/// from Oftedal's implementation. It works.
	/// </summary>
	/// <param name="num">The hash we are counting.</param>
	/// <returns>The total bit count.</returns>
	private static uint BitCount(ulong num)
	{
		uint count = 0;
		for (; num > 0; num >>= 8)
			count += bitCounts[(num & 0xff)];
		return count;
	}
	#endregion
}
