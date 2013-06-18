using System;
using TransformInterface = math.transform.jwave.handlers.TransformInterface;
using DiscreteFourierTransform = math.transform.jwave.handlers.DiscreteFourierTransform;
using FastWaveletTransform = math.transform.jwave.handlers.FastWaveletTransform;
using WaveletPacketTransform = math.transform.jwave.handlers.WaveletPacketTransform;
using DiscreteWaveletTransform = math.transform.jwave.handlers.DiscreteWaveletTransform;
using WaveletInterface = math.transform.jwave.handlers.wavelets.WaveletInterface;
using Coif06 = math.transform.jwave.handlers.wavelets.Coif06;
using Daub02 = math.transform.jwave.handlers.wavelets.Daub02;
using Daub03 = math.transform.jwave.handlers.wavelets.Daub03;
using Daub04 = math.transform.jwave.handlers.wavelets.Daub04;
using Haar02 = math.transform.jwave.handlers.wavelets.Haar02;
using Lege02 = math.transform.jwave.handlers.wavelets.Lege02;
using Lege04 = math.transform.jwave.handlers.wavelets.Lege04;
using Lege06 = math.transform.jwave.handlers.wavelets.Lege06;

namespace math.transform.jwave
{
	///
	// * Main class for doing little test runs for different transform types and
	// * different wavelets without JUnit.
	// * 
	// * @date 23.02.2010 14:26:47
	// * @author Christian Scheiblich
	// 
	public class JWave
	{

		//   * Constructor.
		//   * 
		//   * @date 23.02.2010 14:26:47
		//   * @author Christian Scheiblich
		//
		public JWave()
		{
		} // JWave

		//   * Main method for doing little test runs for different transform types and
		//   * different wavelets without JUnit. Requesting the transform type and the
		//   * type of wavelet to be used else usage is printed.
		//   * 
		//   * @date 23.02.2010 14:26:47
		//   * @author Christian Scheiblich
		//   * @param args
		//   *          [transformType] [waveletType]
		//
		public static void RunTests(string[] args)
		{
			string waveletTypeList = "Haar02, Lege02, Daub02, Lege04, Daub03, Lege06, Coif06, Daub04";

			if(args.Length < 2 || args.Length > 3)
			{
				System.Console.Error.WriteLine("usage: JWave [transformType] {waveletType} {noOfSteps}");
				System.Console.Error.WriteLine("");
				System.Console.Error.WriteLine("transformType: DFT, FWT, WPT, DWT");
				System.Console.Error.WriteLine("waveletType : " + waveletTypeList);
				System.Console.Error.WriteLine("noOfSteps : " + "no of steps forward and reverse; optional");
				return;
			} // if args

			string wType = args[1];
			WaveletInterface wavelet = null;
			if(wType.Equals("haar02", StringComparison.InvariantCultureIgnoreCase))
				wavelet = new Haar02();
			else if(wType.Equals("lege02", StringComparison.InvariantCultureIgnoreCase))
				wavelet = new Lege02();
			else if(wType.Equals("daub04", StringComparison.InvariantCultureIgnoreCase))
				wavelet = new Daub02();
			else if(wType.Equals("lege04", StringComparison.InvariantCultureIgnoreCase))
				wavelet = new Lege04();
			else if(wType.Equals("daub06", StringComparison.InvariantCultureIgnoreCase))
				wavelet = new Daub03();
			else if(wType.Equals("lege06", StringComparison.InvariantCultureIgnoreCase))
				wavelet = new Lege06();
			else if(wType.Equals("coif06", StringComparison.InvariantCultureIgnoreCase))
				wavelet = new Coif06();
			else if(wType.Equals("daub08", StringComparison.InvariantCultureIgnoreCase))
				wavelet = new Daub04();
			else
			{
				System.Console.Error.WriteLine("usage: JWave [transformType] {waveletType}");
				System.Console.Error.WriteLine("");
				System.Console.Error.WriteLine("available wavelets are " + waveletTypeList);
				return;
			} // if wType

			string tType = args[0];
			TransformInterface bWave = null;
			if(tType.Equals("dft", StringComparison.InvariantCultureIgnoreCase))
				bWave = new DiscreteFourierTransform();
			else if(tType.Equals("fwt", StringComparison.InvariantCultureIgnoreCase))
				bWave = new FastWaveletTransform(wavelet);
			else if(tType.Equals("wpt", StringComparison.InvariantCultureIgnoreCase))
				bWave = new WaveletPacketTransform(wavelet);
			else if(tType.Equals("dwt", StringComparison.InvariantCultureIgnoreCase))
				bWave = new DiscreteWaveletTransform(wavelet);
			else
			{
				System.Console.Error.WriteLine("usage: JWave [transformType] {waveletType}");
				System.Console.Error.WriteLine("");
				System.Console.Error.WriteLine("available transforms are DFT, FWT, WPT, DFT");
				return;
			} // if tType

			// instance of transform
			Transform t;

			if(args.Length > 2)
			{
				string argNoOfSteps = args[2];
				int noOfSteps = Convert.ToInt32(argNoOfSteps);

				t = new Transform(bWave, noOfSteps); // perform less steps than possible
			}
			else
			{
				t = new Transform(bWave); // perform all steps
			}

			double[] arrTime = { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };

			Console.WriteLine("");
			Console.WriteLine("time domain:");
			for(int p = 0; p < arrTime.Length; p++)
				Console.Write("{0,9:F6}", arrTime[p]);
			Console.WriteLine("");

			double[] arrFreqOrHilb = t.forward(arrTime); // 1-D forward transform

			if(bWave is DiscreteFourierTransform)
				Console.WriteLine("frequency domain:");
			else
				Console.WriteLine("Hilbert domain:");
			for(int p = 0; p < arrTime.Length; p++)
				Console.Write("{0,9:F6}", arrFreqOrHilb[p]);
			Console.WriteLine("");

			double[] arrReco = t.reverse(arrFreqOrHilb); // 1-D reverse transform

			Console.WriteLine("reconstruction:");
			for(int p = 0; p < arrTime.Length; p++)
				Console.Write("{0,9:F6}", arrReco[p]);
			Console.WriteLine("");

		} // main

	} // class

}