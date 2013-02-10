using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

using Comirva.Audio;
using Comirva.Audio.Util.Maths;
using Comirva.Audio.Feature;

namespace Comirva.Audio.Extraction
{
	public class MandelEllisExtractor
	{
		public int skipIntroSeconds = 30; //number of seconds to skip at the beginning of the song
		public int skipFinalSeconds = 30; //number of seconds to skip at the end of the song
		public int minimumStreamLength = 30; //minimal number of seconds of audio data to return a vaild result
		public static float DEFAULT_SAMPLE_RATE = 11025.0f;
		
		protected internal MFCC mfcc;

		public MandelEllisExtractor() : this(30, 30, 30)
		{
		}

		public MandelEllisExtractor(int skipIntro, int skipEnd, int minimumLength)
		{
			this.mfcc = new MFCC(DEFAULT_SAMPLE_RATE);

			if(skipIntro < 0 || skipEnd < 0 || minimumStreamLength < 1)
				throw new ArgumentException("illegal parametes;");

			this.skipIntroSeconds = skipIntro;
			this.skipFinalSeconds = skipEnd;
			this.minimumStreamLength = minimumLength;
		}

		public AudioFeature Calculate(double[] input, int sampleRate)
		{
			//skip the intro part
			//preProcessor.fastSkip((int) AudioPreProcessor.DEFAULT_SAMPLE_RATE * skipIntroSeconds);

			//pack the mfccs into a pointlist
			double[][] mfccCoefficients = mfcc.Process(input);

			//check if element 0 exists
			if(mfccCoefficients.Length == 0)
				throw new ArgumentException("the input stream ist to short to process;");

			//compute number of samples to skip at the end
			int skip = (int)((skipFinalSeconds * sampleRate)/(mfcc.GetWindowSize()/2));

			//check if the resulting point list has the required minimum length and skip the last few samples
			//if(mfccCoefficients.Length - skip > ((minimumStreamLength * sampleRate)/(mfcc.GetWindowSize()/2))) {
			//mfccCoefficients = new List<double[]>(mfccCoefficients.subList(0, mfccCoefficients.Length - skip - 1));
			//} else {
			//	throw new ArgumentException("the input stream ist to short to process;");
			//}

			//create mfcc matrix
			Matrix mfccs = new Matrix(mfccCoefficients);

			//create covariance matrix
			Matrix covarMatrix = mfccs.Cov();

			//compute mean
			Matrix mean = mfccs.Mean(1).Transpose();

			//mfccs.WriteAscii("mfccs.ascii");
			mfccs.Write(File.CreateText("mfccs.xml"));
			mfccs.Read(new StreamReader("mfccs.xml"));

			MandelEllis.GmmMe gmmMe = new MandelEllis.GmmMe(covarMatrix, mean);
			MandelEllis mandelEllis = new MandelEllis(gmmMe);
			//mandelEllis.WriteXML(new XmlTextWriter("mandelellis.xml", null));
			//mandelEllis.ReadXML(new XmlTextReader("mandelellis.xml"));
			
			return mandelEllis;
		}

		public virtual int GetAttributeType()
		{
			return typeof(MandelEllis).Name.GetHashCode();
		}

		public override string ToString()
		{
			return "Mandel Ellis";
		}
	}
}