using System;
using System.Xml;
using System.Xml.Linq;

using Comirva.Audio.Util.Maths;

namespace Comirva.Audio.Feature
{
	public class MandelEllis : AudioFeature
	{
		/// <summary>
		/// "Gaussian Mixture Model for Mandel / Ellis algorithm" This class holds
		/// the features needed for the Mandel Ellis algorithm: One full covariance
		/// matrix, and the mean of all MFCCS.
		///	@author tim
		/// </summary>
		public class GmmMe
		{
			internal Matrix covarMatrix;
			/// the inverted covarMatrix, stored for computational efficiency
			internal Matrix covarMatrixInv;
			/// a row vector
			internal Matrix mean;

			public GmmMe(Matrix covarMatrix, Matrix mean)
			{
				this.covarMatrix = covarMatrix;
				this.mean = mean;
				this.covarMatrixInv = covarMatrix.Inverse();
			}
		}

		private GmmMe gmmMe; /// the feature

		public MandelEllis(GmmMe gmmMe) : this()
		{
			this.gmmMe = gmmMe;
		}

		protected internal MandelEllis() : base()
		{
		}

		/// <summary>
		/// Calculate the Kullback-Leibler (KL) distance between the two GmmMe. (Also
		/// known as relative entropy) To make the measure symmetric (ie. to obtain a
		/// divergence), the KL distance should be called twice, with exchanged
		/// parameters, and the result be added.
		/// Implementation according to the submission to the MIREX'05 by Mandel and Ellis.
		/// </summary>
		/// <param name="gmmMe1">ME features of song 1</param>
		/// <param name="gmmMe2">ME features of song 2</param>
		/// <returns>the KL distance from gmmMe1 to gmmMe2</returns>
		private float KullbackLeibler(GmmMe gmmMe1, GmmMe gmmMe2)
		{
			int dim = gmmMe1.covarMatrix.GetColumnDimension();

			/// calculate the trace-term:
			Matrix tr1 = gmmMe2.covarMatrixInv.Times(gmmMe1.covarMatrix);
			Matrix tr2 = gmmMe1.covarMatrixInv.Times(gmmMe2.covarMatrix);
			Matrix sum = tr1.Plus(tr2);
			float trace = (float)sum.Trace();

			/// "distance" between the two mean vectors:
			Matrix dist = gmmMe1.mean.Minus(gmmMe2.mean);

			/// calculate the second brace:
			Matrix secBra = gmmMe2.covarMatrixInv.Plus(gmmMe1.covarMatrixInv);

			Matrix tmp1 = dist.Transpose().Times(secBra);

			/// finally, the whole term:
			return 0.5f * (trace - 2*dim + (float)tmp1.Times(dist).Get(0, 0));
		}

		/// <summary>Get Distance</summary>
		/// <seealso cref="">comirva.audio.feature.AudioFeature#getDistance(comirva.audio.feature.AudioFeature)</seealso>
		public override double GetDistance(AudioFeature f)
		{
			if(!(f is MandelEllis))
			{
				new Exception("Can only handle AudioFeatures of type Mandel Ellis, not of: "+f);
				return -1;
			}
			MandelEllis other = (MandelEllis)f;
			return KullbackLeibler(this.gmmMe, other.gmmMe) + KullbackLeibler(other.gmmMe, this.gmmMe);
		}

		/// <summary>
		/// Writes the xml representation of this object to the xml ouput stream.<br>
		/// <br>
		/// There is the convetion, that each call to a <code>writeXML()</code> method
		/// results in one xml element in the output stream.
		/// </summary>
		/// <param name="writer">XMLStreamWriter the xml output stream</param>
		public void WriteXML(XmlTextWriter xmlTextWriter)
		{
			xmlTextWriter.WriteStartElement("feature");
			xmlTextWriter.WriteAttributeString("type", this.GetType().ToString());
			gmmMe.covarMatrix.WriteXML(xmlTextWriter, "covarMatrix");
			gmmMe.mean.WriteXML(xmlTextWriter, "mean");
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.Close();
		}

		/// <summary>
		/// Reads the xml representation of an object form the xml input stream.<br>
		/// </summary>
		/// <param name="parser">XMLStreamReader the xml input stream</param>
		public void ReadXML(XmlTextReader xmlTextReader)
		{
			XDocument xdoc = XDocument.Load(xmlTextReader);
			
			XElement feature = xdoc.Element("feature");
			if (feature == null) {
				throw new MissingFieldException("Could not find feature section - no GmmMe Loaded!");
			}
			
			Matrix covarMatrix = new Matrix(0,0);
			covarMatrix.ReadXML(xdoc, "covarMatrix");
			
			Matrix mean = new Matrix(0,0);
			mean.ReadXML(xdoc, "mean");

			this.gmmMe = new GmmMe(covarMatrix, mean);
			xmlTextReader.Close();
		}
	}
}
