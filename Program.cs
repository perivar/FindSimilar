using System;
using System.Drawing;
using CommonUtils;

namespace Imghash
{
	public class Program
	{
		// http://stackoverflow.com/questions/7395836/converting-from-bitstring-to-integer
		// http://stackoverflow.com/questions/4777070/hamming-distance-on-binary-strings-in-sql
		// http://www.fftw.org/doc/Real_002dto_002dReal-Transform-Kinds.html
		
		public static void HashTester(string[] args)
		{
			if(args.Length == 1)
			{
				Bitmap theImage = new Bitmap(1,1);

				try
				{
					theImage = new Bitmap(args[0]);
				}
				catch(Exception)
				{
					Console.WriteLine("Couldn't open the image " + args[0] + ".");
					return;
				}
				
				//ulong hash1 = ImageAverageHash.AverageHash(theImage);
				//Console.WriteLine(hash1.ToString("x16") + "\t" + args[0]);

				ImagePHash phash = new ImagePHash(64,16);
				string hash1s = phash.GetHash(theImage);
				Console.WriteLine(hash1s + "\t" + args[0]);
			}
			else if(args.Length == 2)
			{
				Bitmap theImage = new Bitmap(1,1);
				Bitmap theOtherImage  = new Bitmap(1,1);

				try
				{
					theImage = new Bitmap(args[0]);
				}
				catch(Exception)
				{
					Console.WriteLine("Couldn't open the image " + args[0] + ".");
					return;
				}
				try
				{
					theOtherImage = new Bitmap(args[1]);
				}
				catch(Exception)
				{
					Console.WriteLine("Couldn't open the image " + args[1] + ".");
					return;
				}
				
				/*
				ulong hash1 = ImageAverageHash.AverageHash(theImage);
				ulong hash2 = ImageAverageHash.AverageHash(theOtherImage);
				
				Console.WriteLine(hash1.ToString("x16") + "\t" + args[0]);
				Console.WriteLine(hash2.ToString("x16") + "\t" + args[1]);
				Console.WriteLine("Similarity: " + ImageAverageHash.Similarity(hash1, hash2) + "%");
				Console.WriteLine("\n\n");
				 */

				ImagePHash phash = new ImagePHash(64,16);
				string hash1s = phash.GetHash(theImage);
				string hash2s = phash.GetHash(theOtherImage);
				Console.WriteLine(hash1s + "\t" + args[0]);
				Console.WriteLine(hash2s + "\t" + args[1]);
				Console.WriteLine("Similarity: {0:00.00} % ", ImagePHash.Similarity(hash1s, hash2s));
				
				/*
				ulong hash1p = phash.PHash(theImage);
				ulong hash2p = phash.PHash(theOtherImage);
				Console.WriteLine(hash1p + "\t" + args[0]);
				Console.WriteLine(hash2p + "\t" + args[1]);
				Console.WriteLine("Similarity: " + BitCounter.Hamming(hash1p, hash2p) + "");
				Console.WriteLine("Similarity: " + ImageAverageHash.Similarity(hash1p, hash2p) + "%");
				 */
			}
			else
			{
				Console.WriteLine("To get the hash of an image: Imghash.exe <image name>");
				Console.WriteLine("To compare two images: Imghash.exe <image 1> <image 2>");
			}
		}
	}
}