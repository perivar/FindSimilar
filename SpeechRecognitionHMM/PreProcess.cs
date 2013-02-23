using System;

//
//  Please feel free to use/modify this class.
//  If you give me credit by keeping this information or
//  by sending me an email before using it or by reporting bugs , i will be happy.
//  Email : gtiwari333@gmail.com,
//  Blog : http://ganeshtiwaridotcomdotnp.blogspot.com/
// 
namespace SpeechRecognitionHMM
{
	// pre-processing steps
	// @author Ganesh Tiwari
	public class PreProcess
	{
		internal float[] originalSignal; // initial extracted PCM,
		internal float[] afterEndPtDetection; // after endPointDetection
		public int noOfFrames; // calculated total no of frames
		internal int samplePerFrame; // how many samples in one frame
		public float[][] framedSignal;
		internal float[] hammingWindow;
		internal EndPointDetection epd;
		internal int samplingRate;
		
		/// <summary>
		/// constructor, all steps are called frm here
		/// </summary>
		/// <param name="originalSignal">extracted PCM data</param>
		/// <param name="samplePerFrame">how many samples in one frame (660 << frameDuration = typically 30)</param>
		/// <param name="samplingRate">samplingFreq, typically 22Khz</param>
		public PreProcess(float[] originalSignal, int samplePerFrame, int samplingRate)
		{
			this.originalSignal = originalSignal;
			this.samplePerFrame = samplePerFrame;
			this.samplingRate = samplingRate;

			NormalizePCM();
			epd = new EndPointDetection(this.originalSignal, this.samplingRate);
			afterEndPtDetection = epd.DoEndPointDetection();
			
			// ArrayWriter.PrintFloatArrayToFile(afterEndPtDetection, "endPt.txt");
			
			DoFraming();
			DoWindowing();
		}

		private void NormalizePCM()
		{
			float max = originalSignal[0];
			for (int i = 1; i < originalSignal.Length; i++)
			{
				if (max < Math.Abs(originalSignal[i]))
				{
					max = Math.Abs(originalSignal[i]);
				}
			}
			
			// Console.Out.WriteLine("max PCM =  " + max);
			for (int i = 0; i < originalSignal.Length; i++)
			{
				originalSignal[i] = originalSignal[i] / max;
			}
		}

		// divides the whole signal into frames of samplerPerFrame
		private void DoFraming()
		{
			// calculate no of frames, for framing
			noOfFrames = 2 * afterEndPtDetection.Length / samplePerFrame - 1;
			Console.WriteLine("noOfFrames       "
			                  + noOfFrames + "  samplePerFrame     "
			                  + samplePerFrame + "  EPD length   "
			                  + afterEndPtDetection.Length);

			framedSignal = new float[noOfFrames][];
			for (int i = 0; i < noOfFrames; i++) {
				framedSignal[i] = new float[samplePerFrame];
			}
			
			for (int i = 0; i < noOfFrames; i++)
			{
				int startIndex = (i * samplePerFrame / 2);
				for (int j = 0; j < samplePerFrame; j++)
				{
					framedSignal[i][j] = afterEndPtDetection[startIndex + j];
				}
			}
		}

		// does hamming window on each frame
		private void DoWindowing()
		{
			// prepare hammingWindow
			hammingWindow = new float[samplePerFrame + 1];
			
			// prepare for through out the data
			for (int i = 1; i <= samplePerFrame; i++)
			{
				hammingWindow[i] = (float)(0.54 - 0.46 * (Math.Cos(2 * Math.PI * i / samplePerFrame)));
			}

			// do windowing
			for (int i = 0; i < noOfFrames; i++)
			{
				for (int j = 0; j < samplePerFrame; j++)
				{
					framedSignal[i][j] = framedSignal[i][j] * hammingWindow[j + 1];
				}
			}
		}
	}
}