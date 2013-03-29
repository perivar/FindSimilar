using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;

/// <summary>
/// Naudio SoundPlayer Engine
/// Some of the code is taken from NAudioEngine created by Jacob Johnston.
/// </summary>
namespace FindSimilar
{
	/// <summary>
	/// NAudio SoundPlayer.
	/// </summary>
	public class SoundPlayer : INotifyPropertyChanged, IDisposable
	{
		public enum Output
		{
			WaveOut,
			DirectSound,
			Wasapi,
			Asio
		}

		private static SoundPlayer instance; // singleton
		private bool disposed;
		private bool canPlay;
		private bool canPause;
		private bool canStop;
		private bool isPlaying;
		private IWavePlayer waveOutDevice;
		private IWaveProvider waveProvider;

		/// <summary>
		/// Set output type to use (e.g. WaveOut or Asio ..)
		/// </summary>
		public Output OutputType { get; set; }
		
		// NAudio setup variables
		/// <summary>
		/// Asio Driver name, needed to use Asio rendering
		/// </summary>
		public string AsioDriverName { get; set; }

		/// <summary>
		/// Wasapi audio client driver Mode, shared if false, exclusive if true
		/// </summary>
		public bool WasapiExclusiveMode { get; set; }

		/// <summary>
		/// Desired Latency, used for Wavout, DirectSound and Wasapi
		/// by default, value is 250ms
		/// </summary>
		public int Latency { get; set; }

		#region Singleton Pattern
		public static SoundPlayer GetWaveOutInstance()
		{
			if (instance == null)
				instance = new SoundPlayer();
			return instance;
		}

		public static SoundPlayer GetAsioInstance(string asioDriverName, int latency)
		{
			if (instance == null) {
				instance = new SoundPlayer(asioDriverName, latency);
			}
			return instance;
		}
		#endregion

		#region Constructors
		
		/// <summary>
		/// Create a SoundPlayer object using WaveOut and 250 ms latency
		/// </summary>
		private SoundPlayer() : this(Output.WaveOut, 250)
		{
		}
		
		/// <summary>
		/// Create a SoundPlayer object using passed output type and 250 ms latency
		/// </summary>
		/// <param name="outputType">Output type (WaveOut, Asio etc)</param>
		private SoundPlayer(Output outputType) : this(outputType, 250)
		{
		}

		/// <summary>
		/// Create a SoundPlayer object using asio driver name
		/// </summary>
		/// <param name="asioDriverName"></param>
		/// <param name="latency"></param>
		private SoundPlayer(string asioDriverName, int latency)
		{
			OutputType = Output.Asio;
			AsioDriverName = asioDriverName;
			Latency = latency;
			
			Init();
		}

		/// <summary>
		/// Create a SoundPlayer object
		/// </summary>
		/// <param name="outputType">Output type (WaveOut, Asio etc)</param>
		/// <param name="latency">Latency</param>
		private SoundPlayer(Output outputType, int latency)
		{
			OutputType = outputType;
			Latency = latency;
			
			Init();
		}
		#endregion

		#region IDisposable
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				if(disposing)
				{
					StopAndCloseStream();
				}
				disposed = true;
			}
		}
		#endregion
		
		#region Static Methods
		/// <summary>
		/// Return an array of the installed Asio drivers
		/// </summary>
		/// <returns>the asio driver name array</returns>
		public static string[] GetAsioDriverNames() {
			return AsioOut.GetDriverNames();
		}
		#endregion
		
		#region Initialize Methods
		public void Init()
		{
			CreateWaveOut();
		}

		private void CreateWaveOut()
		{
			switch (OutputType)
			{
				case Output.WaveOut:
					var callbackInfo = WaveCallbackInfo.FunctionCallback();
					var outputDevice= new WaveOut(callbackInfo) {DesiredLatency = Latency};
					waveOutDevice = outputDevice;
					break;
				case Output.DirectSound:
					waveOutDevice = new DirectSoundOut(Latency);
					break;
				case Output.Wasapi:
					waveOutDevice = new WasapiOut(WasapiExclusiveMode?AudioClientShareMode.Exclusive:AudioClientShareMode.Shared,Latency);
					break;
				case Output.Asio:
					waveOutDevice = new AsioOut(AsioDriverName);
					break;
			}
		}
		#endregion
		
		#region Open methods
		public void OpenWaveProvider(IWaveProvider waveProvider)
		{
			try
			{
				StopAndCloseStream(); // Dispose
				Init(); // and reinitialize the waveoutdevice

				WaveProvider = waveProvider;
				waveOutDevice.Init(waveProvider);
				CanPlay = true;
			}
			catch
			{
				waveProvider = null;
				CanPlay = false;
			}
		}
		
		public void OpenSampleProvider(ISampleProvider sampleProvider)
		{
			try
			{
				StopAndCloseStream(); // Dispose
				Init(); // and reinitialize the waveoutdevice

				WaveProvider = new SampleToWaveProvider(sampleProvider);
				waveOutDevice.Init(waveProvider);
				CanPlay = true;
			}
			catch
			{
				waveProvider = null;
				CanPlay = false;
			}
		}
		
		public void OpenFile(string path)
		{
			if (System.IO.File.Exists(path))
			{
				try
				{
					StopAndCloseStream(); // Dispose
					Init(); // and reinitialize the waveoutdevice
					
					//WaveStream waveStream = (WaveStream) new AudioFileReader(path);
					//WaveChannel32 waveChannel32 = new WaveChannel32(waveStream);
					//waveOutDevice.Init(waveChannel32);
					
					ISampleProvider sampleProvider = (ISampleProvider) new AudioFileReader(path);
					WaveProvider = new SampleToWaveProvider(sampleProvider);
					waveOutDevice.Init(waveProvider);
					
					CanPlay = true;
				}
				catch
				{
					waveProvider = null;
					CanPlay = false;
				}
			}
		}
		#endregion
		
		#region Private Utility Methods
		private void StopAndCloseStream()
		{
			if (waveProvider != null)
			{
				waveProvider = null;
			}
			if (waveOutDevice != null)
			{
				waveOutDevice.Stop();
			}
			if (waveOutDevice != null)
			{
				waveOutDevice.Dispose();
				waveOutDevice = null;
			}
		}
		#endregion
		
		#region Public Play, Pause and Stop Methods
		public void Play()
		{
			if (CanPlay)
			{
				waveOutDevice.Play();
				IsPlaying = true;
				CanPause = true;
				CanPlay = false;
				CanStop = true;
			}
		}

		public void Pause()
		{
			if (IsPlaying && CanPause)
			{
				waveOutDevice.Pause();
				IsPlaying = false;
				CanPlay = true;
				CanPause = false;
			}
		}
		
		public void Stop()
		{
			if (waveOutDevice != null)
			{
				waveOutDevice.Stop();
			}
			IsPlaying = false;
			CanStop = false;
			CanPlay = true;
			CanPause = false;
		}
		#endregion
		
		#region Public Properties
		public IWaveProvider WaveProvider {
			get { return waveProvider; }
			protected set
			{
				IWaveProvider oldValue = waveProvider;
				waveProvider = value;
				if (oldValue != waveProvider)
					NotifyPropertyChanged("WaveProvider");
			}
		}
		
		public bool CanPlay
		{
			get { return canPlay; }
			protected set
			{
				bool oldValue = canPlay;
				canPlay = value;
				if (oldValue != canPlay)
					NotifyPropertyChanged("CanPlay");
			}
		}

		public bool CanPause
		{
			get { return canPause; }
			protected set
			{
				bool oldValue = canPause;
				canPause = value;
				if (oldValue != canPause)
					NotifyPropertyChanged("CanPause");
			}
		}

		public bool CanStop
		{
			get { return canStop; }
			protected set
			{
				bool oldValue = canStop;
				canStop = value;
				if (oldValue != canStop)
					NotifyPropertyChanged("CanStop");
			}
		}

		public bool IsPlaying
		{
			get { return isPlaying; }
			protected set
			{
				bool oldValue = isPlaying;
				isPlaying = value;
				if (oldValue != isPlaying)
					NotifyPropertyChanged("IsPlaying");
			}
		}
		
		public int SampleRate {
			get {
				if (waveProvider != null)
					return waveProvider.WaveFormat.SampleRate;
				else
					return 44100; // Assume a default 44.1 kHz sample rate.
			}
		}
		#endregion

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion
	}
}
