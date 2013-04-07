// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
using System;
using System.ComponentModel;

namespace FindSimilar.AudioProxies
{
	/// <summary>
	///   Digital signal processing proxy
	/// </summary>
	public interface IAudio : INotifyPropertyChanged, IDisposable
	{
		// Public Properties
		bool CanPlay { get; }
		bool CanPause { get; }
		bool CanStop { get; }
		bool IsPlaying { get; }
		
		/// <summary>
		/// Read from file at a specific frequency rate
		/// </summary>
		/// <param name="filename">Filename to read from</param>
		/// <param name="samplerate">Sample rate</param>
		/// <param name="milliseconds">Milliseconds to read</param>
		/// <param name="startmilliseconds">Start at a specific millisecond range</param>
		/// <returns>Array with data</returns>
		float[] ReadMonoFromFile(string filename, int samplerate, int milliseconds, int startmilliseconds);
		
		/// <summary>
		/// Open File using passed path
		/// </summary>
		/// <param name="path">path to audio file</param>
		void OpenFile(string path);
		
		/// <summary>
		/// Play Audio
		/// </summary>
		void Play();
		
		/// <summary>
		/// Pause the Audio
		/// </summary>
		void Pause();
		
		/// <summary>
		/// Stop the Audio
		/// </summary>
		void Stop();
	}
}