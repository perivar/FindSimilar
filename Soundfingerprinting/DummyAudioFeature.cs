using System;
using Comirva.Audio.Feature;

namespace Soundfingerprinting
{
	/// <summary>
	/// Description of DummyAudioFeature.
	/// </summary>
	public class DummyAudioFeature : AudioFeature
	{
		public DummyAudioFeature()
		{
		}
		
		public override byte[] ToBytes()
		{
			throw new NotImplementedException();
		}
		
		private string name;
		public override string Name {
			get {
				return name;
			}
			set {
				this.name = value;
			}
		}
		
		public override double GetDistance(AudioFeature f, AudioFeature.DistanceType t)
		{
			throw new NotImplementedException();
		}
		
		public override double GetDistance(AudioFeature f)
		{
			throw new NotImplementedException();
		}
	}
}
