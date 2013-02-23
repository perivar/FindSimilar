using System;
using System.IO;
using System.Globalization;

//  Please feel free to use/modify this class.
//  If you give me credit by keeping this information or
//  by sending me an email before using it or by reporting bugs , i will be happy.
//  Email : gtiwari333@gmail.com,
//  Blog : http://ganeshtiwaridotcomdotnp.blogspot.com/
namespace SpeechRecognitionHMM
{
	// saves the array to file or console ...... supports various data types
	// @author Ganesh Tiwari
	public static class ArrayWriter
	{
		private static NumberFormatInfo numberFormat = new CultureInfo("en-US", false).NumberFormat;

		// saves the @param array to file : @param fileName
		// @param array input array
		// @param fileName output file
		public static void PrintIntArrayToFile(int[] array, string fileName)
		{
			TextWriter tw = File.CreateText(fileName);
			
			// write the array
			for (int i = 0; i < array.Length; i++)
			{
				tw.WriteLine(array[i].ToString(numberFormat));
			}
			tw.Flush();
			tw.Close();
		}

		// display @param array 's content to console
		// @param array input array
		public static void PrintIntArrayToConsole(int[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Console.WriteLine(array[i]);
			}
		}

		// saves the @param array to file : @param fileName
		// @param array input array
		// @param fileName output file
		public static void PrintDoubleArrayToFile(double[] array, string fileName)
		{
			TextWriter tw = File.CreateText(fileName);
			
			// write the array
			for (int i = 0; i < array.Length; i++)
			{
				tw.WriteLine(array[i].ToString(numberFormat));
			}
			tw.Flush();
			tw.Close();
		}

		// saves the @param array to file : @param fileName
		// @param array input array
		// @param fileName output file
		public static void Print2DDoubleArrayToFile(double[][] array, string fileName)
		{
			TextWriter tw = File.CreateText(fileName);
			
			// write the array
			for (int i = 0; i < array.Length; i++)
			{
				for (int j = 0; j < array[i].Length; j++)
				{
					tw.WriteLine(array[i][j].ToString(numberFormat));
				}
			}
			tw.Flush();
			tw.Close();
		}

		// display @param array 's content to console
		// @param array input array
		public static void PrintDoubleArrayToConsole(double[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Console.WriteLine(array[i]);
			}
		}

		// display @param array 's content to console
		// @param array input array
		public static void Print2DTabbedDoubleArrayToConsole(double[][] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				for (int j = 0; j < array[i].Length; j++)
				{
					Console.Write(array[i][j] + "\t");
				}
				Console.WriteLine();
			}

		}

		// display @param array 's content to console
		// @param array input array
		public static void PrintFrameWise2DDoubleArrayToConsole(double[][] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				for (int j = 0; j < array[i].Length; j++)
				{
					Console.WriteLine(array[j][i]);
				}
				Console.WriteLine();
			}
		}

		// display @param array 's content to console
		// @param array input array
		public static void Print2DDoubleArrayToConsole(double[][] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				for (int j = 0; j < array[i].Length; j++)
				{
					Console.WriteLine(array[i][j]);
				}
				Console.WriteLine();
			}
		}

		// saves the @param array to file : @param fileName
		// @param array input array
		// @param fileName output file
		public static void PrintStringArrayToFile(string[] array, string fileName)
		{
			TextWriter tw = File.CreateText(fileName);
			
			// write the array
			for (int i = 0; i < array.Length; i++)
			{
				tw.WriteLine(array[i]);
			}
			tw.Flush();
			tw.Close();
		}

		// display @param array 's content to console
		// @param array input array
		public static void PrintStringArrayToConsole(string[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Console.WriteLine(array[i]);
			}
		}

		// saves the @param array to file : @param fileName
		// @param array input array
		// @param fileName output file
		public static void PrintFloatArrayToFile(float[] array, string fileName)
		{
			TextWriter tw = File.CreateText(fileName);
			
			// write the array
			for (int i = 0; i < array.Length; i++)
			{
				tw.WriteLine(array[i].ToString(numberFormat));
			}
			tw.Flush();
			tw.Close();
		}

		// display @param array 's content to console
		// @param array input array
		public static void PrintFloatArrayToConsole(float[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Console.WriteLine(array[i]);
			}
		}
	}
}