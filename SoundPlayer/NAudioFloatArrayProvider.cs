using System;
using NAudio.Wave;
using CommonUtils.Audio.NAudio;

namespace FindSimilar.AudioProxies
{
	/// <summary>
	/// An Audio Array SoundProvider.
	/// </summary>
	public class NAudioFloatArrayProvider : WaveProvider32
	{
		public long Length { get { return AudioData.Length; } }
		public long Position { get; set; }

		public bool HasReachedEndOfStream {
			get {
				if (Length > 0 && Position > 0) {
					if (Length == Position) return true;
				}
				return false;
			}
		}
		
		public float[] AudioData {
			get; set;
		}

		public NAudioFloatArrayProvider(int sampleRate, float[] audioData, int channels) : base (sampleRate, channels)
		{
			AudioData = audioData;
		}
		
		public override int Read(float[] buffer, int offset, int samplesRequested)
		{
			// check if we have any samples left
			int samplesRemaining = (int) (AudioData.Length - Position);
			if (samplesRemaining == 0) {
				return 0;
			}
			
			int samplesToRead = samplesRequested;
			if (samplesToRead > samplesRemaining) {
				samplesToRead = samplesRemaining;
			}
			
			for (int n = 0; n < samplesToRead; n++)
			{
				buffer[n+offset] = AudioData[n+Position];
			}
			Position += samplesToRead;

			return samplesToRead;
		}
	}
}
