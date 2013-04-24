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
		/*
		public static void Main(string[] args)
		{
			//DCTTester();
			HashTester(args);
			
			// TODO: remove this
			Console.ReadKey();
		}
		 */
		
		public static void DCTTester(bool doRandom) {

			int N = 0;
			double[,] f = new double[N,N];
			double[,] F = new double[N,N];
			
			Console.WriteLine("Original values");
			Console.WriteLine("-----------");

			if (doRandom) {
				// Generate random integers between 0 and 255
				N = 4;
				Random generator = new Random();

				int val;
				for (int x=0;x<N;x++)
				{
					for (int y=0;y<N;y++)
					{
						val = generator.Next(255);
						f[x,y] = val;
						Console.WriteLine(f[x,y]+" => f["+x+"]["+y+"]");
					}
				}
			} else {
				// Set some integers between 0 and 255
				N = 4;
				f = new double[,] {
					{54.0	, 35.0	, 16.0	, 32.0},
					{128.0	, 185.0	, 1.0	, 89.0},
					{14.0	, 96.0	, 156.0	, 45.0},
					{45.0	, 223.0	, 6.0	, 12.0}
				};
				// octave3.6.4_gcc4.6.2
				// > format short g
				// > X = [54.0, 35.0, 16.0, 32.0; 128.0, 185.0, 1.0, 89.0; 14.0, 96.0, 156.0, 45.0; 45.0, 223.0, 6.0, 12.0]
				for (int x=0;x<N;x++)
				{
					for (int y=0;y<N;y++)
					{
						Console.WriteLine(f[x,y]+" => f["+x+"]["+y+"]");
					}
				}
			}
			
			//DctInterface dctApplied = new Dct2(N);
			DctInterface dctApplied = new DctComirva(N, N);
			F = dctApplied.Dct(f);
			
			Console.WriteLine("\nFrom f to F");
			Console.WriteLine("-----------");
			for (int x=0;x<N;x++)
			{
				for (int y=0;y<N;y++)
				{
					try
					{
						Console.WriteLine(F[x,y]+" => F["+x+"]["+y+"]");
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
			}

			f = dctApplied.InverseDct(F);
			
			Console.WriteLine("\nBack to f");
			Console.WriteLine("---------");
			for (int y=0;y<N;y++)
			{
				for (int z=0;z<N;z++)
				{
					Console.WriteLine(f[y,z]+" => f["+y+"]["+z+"]");
				}
			}
		}
		
		public static void HashTester(string[] argv)
		{
			if(argv.Length == 1)
			{
				Bitmap theImage = new Bitmap(1,1);

				try
				{
					theImage = new Bitmap(argv[0]);
				}
				catch(Exception)
				{
					Console.WriteLine("Couldn't open the image " + argv[0] + ".");
				}

				Console.WriteLine(ImageAverageHash.AverageHash(theImage).ToString("x16") + "\t" + argv[0]);
			}
			else if(argv.Length == 2)
			{
				Bitmap theImage = new Bitmap(1,1);
				Bitmap theOtherImage  = new Bitmap(1,1);

				try
				{
					theImage = new Bitmap(argv[0]);
				}
				catch(Exception)
				{
					Console.WriteLine("Couldn't open the image " + argv[0] + ".");
				}
				try
				{
					theOtherImage = new Bitmap(argv[1]);
				}
				catch(Exception)
				{
					Console.WriteLine("Couldn't open the image " + argv[1] + ".");
				}

				
				ulong hash1 = ImageAverageHash.AverageHash(theImage);
				ulong hash2 = ImageAverageHash.AverageHash(theOtherImage);
				
				Console.WriteLine(hash1.ToString("x16") + "\t" + argv[0]);
				Console.WriteLine(hash2.ToString("x16") + "\t" + argv[1]);
				Console.WriteLine("Similarity: " + ImageAverageHash.Similarity(hash1, hash2) + "%");
				Console.WriteLine("\n\n");

				ImagePHash phash = new ImagePHash();
				string hash1s = phash.GetHash(theImage);
				string hash2s = phash.GetHash(theOtherImage);
				Console.WriteLine(hash1s + "\t" + argv[0]);
				Console.WriteLine(hash2s + "\t" + argv[1]);
				Console.WriteLine("Similarity: " + phash.HammingDistance(hash1s, hash2s) + "");
				
				ulong hash1p = phash.PHash(theImage);
				ulong hash2p = phash.PHash(theOtherImage);
				Console.WriteLine(hash1p + "\t" + argv[0]);
				Console.WriteLine(hash2p + "\t" + argv[1]);
				Console.WriteLine("Similarity: " + BitCounter.Hamming(hash1p, hash2p) + "");
				Console.WriteLine("Similarity: " + ImageAverageHash.Similarity(hash1p, hash2p) + "%");
			}
			else
			{
				Console.WriteLine("To get the hash of an image: Imghash.exe <image name>");
				Console.WriteLine("To compare two images: Imghash.exe <image 1> <image 2>");
			}
		}
	}
}