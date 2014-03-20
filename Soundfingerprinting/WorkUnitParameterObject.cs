using System.Collections;
using System.Collections.Generic;
using Soundfingerprinting.Fingerprinting.Configuration;

namespace Soundfingerprinting.Fingerprinting.WorkUnitBuilder
{
	public class WorkUnitParameterObject
	{
		public string PathToAudioFile { get; set; }

		public float[] AudioSamples { get; set; }

		public IFingerprintingConfiguration FingerprintingConfiguration { get; set; }

		public int StartAtMilliseconds { get; set; }

		public int MillisecondsToProcess { get; set; }
		
		/// <summary>
		/// a filename without an extension
		/// </summary>
		public string FileName { get; set; }
		
		public double DurationInMs { get; set; }
		
		public Dictionary<string, string> Tags { get; set; }
	}
}