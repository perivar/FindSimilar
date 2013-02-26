using System;
using System.Xml;
using System.Xml.Linq;

using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Comirva.Audio.Util.Maths;
using CommonUtils;

namespace Comirva.Audio.Feature
{
	/// <summary>
	/// CoMIRVA: Collection of Music Information Retrieval and Visualization Applications
	/// Ported from Java to C# by perivar@nerseth.com
	/// </summary>
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
			// a row vector
			internal Matrix mean;

			// the covariance matrix
			internal Matrix covarMatrix;

			// the inverted covarMatrix, stored for computational efficiency
			internal Matrix covarMatrixInv;
			
			public GmmMe(Matrix mean, Matrix covarMatrix)
			{
				this.mean = mean;
				this.covarMatrix = covarMatrix;

				// Inverse Covariance
				try {
					this.covarMatrixInv = covarMatrix.Inverse();
				} catch (Exception) {
					Console.Error.WriteLine("MatrixSingularException");
					return;
				}
			}
			
			public GmmMe(Matrix mean, Matrix covarMatrix, Matrix covarMatrixInv) {
				
				this.mean = mean;
				this.covarMatrix = covarMatrix;
				this.covarMatrixInv = covarMatrixInv;
			}
		}

		private GmmMe gmmMe; /// the feature
		
		private string name; // the name
		public override string Name {
			get {
				return name;
			}
			set {
				this.name = value;
			}
		}
		
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
		/// <seealso cref="">comirva.audio.feature.AudioFeature#GetDistance(comirva.audio.feature.AudioFeature)</seealso>
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
		/// <example>
		/// mandelEllis.WriteXML(new XmlTextWriter("mandelellis.xml", null));
		/// </example>
		public void WriteXML(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("feature");
			xmlWriter.WriteAttributeString("type", this.GetType().ToString());
			gmmMe.mean.WriteXML(xmlWriter, "mean");
			gmmMe.covarMatrix.WriteXML(xmlWriter, "cov");
			gmmMe.covarMatrixInv.WriteXML(xmlWriter, "icov");
			xmlWriter.WriteEndElement();
			xmlWriter.Close();
		}

		/// <summary>
		/// Reads the xml representation of an object form the xml input stream.<br>
		/// </summary>
		/// <param name="parser">XMLStreamReader the xml input stream</param>
		/// <example>
		/// mandelEllis.ReadXML(new XmlTextReader("mandelellis.xml"));
		/// </example>
		public void ReadXML(XmlTextReader xmlTextReader)
		{
			XDocument xdoc = XDocument.Load(xmlTextReader);
			
			XElement feature = xdoc.Element("feature");
			if (feature == null) {
				throw new MissingFieldException("Could not find feature section - no GmmMe Loaded!");
			}
			
			Matrix mean = new Matrix(0,0);
			mean.ReadXML(xdoc, "mean");

			Matrix covarMatrix = new Matrix(0,0);
			covarMatrix.ReadXML(xdoc, "cov");

			Matrix covarMatrixInv = new Matrix(0,0);
			covarMatrixInv.ReadXML(xdoc, "icov");
			
			this.gmmMe = new GmmMe(mean, covarMatrix, covarMatrixInv);
			xmlTextReader.Close();
		}
		
		public byte[] ToBytesCompressed() {
			using (var stream = new MemoryStream()) {
				gmmMe.mean.WriteBinary(stream);
				gmmMe.covarMatrix.WriteBinary(stream);
				gmmMe.covarMatrixInv.WriteBinary(stream);
				
				return stream.ToArray();
			}
		}

		public static AudioFeature FromBytesCompressed(byte[] byteArray)
		{
			using (Stream stream = new MemoryStream(byteArray)) {
				Matrix mean = Matrix.LoadBinary(stream);
				Matrix covarMatrix = Matrix.LoadBinary(stream);
				Matrix covarMatrixInv = Matrix.LoadBinary(stream);
				
				MandelEllis.GmmMe gmmme = new MandelEllis.GmmMe(mean, covarMatrix, covarMatrixInv);
				var mandelEllis = new MandelEllis(gmmme);
				return mandelEllis;
			}
		}
		
		/// <summary>
		/// Manual serialization of a AudioFeature object to a byte array
		/// </summary>
		/// <returns>byte array</returns>
		public override byte[] ToBytes()
		{
			//XmlWriterSettings settings = new XmlWriterSettings();
			//settings.Indent = true;
			/*
			StringBuilder builder = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(builder))
			{
				WriteXML(writer);
			}
			
			return StringUtils.GetBytes(builder.ToString());
			 */
			
			return ToBytesCompressed();
		}
		
		/// <summary>
		/// Manual deserialization of an AudioFeature from a LittleEndian byte array
		/// </summary>
		/// <param name="buf">byte array</param>
		public static AudioFeature FromBytes(byte[] buf)
		{
			/*
			String xmlData = StringUtils.GetString(buf);
			XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(xmlData));
			
			MandelEllis mandelEllis = new MandelEllis();
			mandelEllis.ReadXML(xmlTextReader);
			return mandelEllis;
			 */
			
			return FromBytesCompressed(buf);
		}
	}
}