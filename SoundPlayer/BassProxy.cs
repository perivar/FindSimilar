using System;
using System.Collections.Generic;

using System.Windows;
using System.Diagnostics;

using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.Misc;

using System.ComponentModel;

namespace FindSimilar.AudioProxies
{
	/// <summary>
	///   Bass Proxy for Bass.Net API
	/// </summary>
	/// <remarks>
	///   BASS is an audio library for use in Windows and Mac OSX software.
	///   Its purpose is to provide developers with powerful and efficient sample, stream (MP3, MP2, MP1, OGG, WAV, AIFF, custom generated, and more via add-ons),
	///   MOD music (XM, IT, S3M, MOD, MTM, UMX), MO3 music (MP3/OGG compressed MODs), and recording functions.
	///   All in a tiny DLL, under 100KB* in size.
	/// </remarks>
	/// <example>
	/// using (BassProxy bass = new BassProxy())
	/// {
	/// 	string pathToRecoded = Path.GetFullPath(sfd.FileName);
	/// 	bass.RecodeTheFile(_tbPathToFile.Text, pathToRecoded, (int) _nudSampleRate.Value);
	/// }
	/// </example>
	/// <seealso cref="BassEngine.cs">BassEngine.cs from WPF Sound Visualization Library</seealso>
	/// <remarks>
	/// Originally from "Sound Fingerprinting framework"
	/// git://github.com/AddictedCS/soundfingerprinting.git
	/// Code license: CPOL v.1.02
	/// ciumac.sergiu@gmail.com
	/// Modified by perivar@nerseth.com
	/// </remarks>
	public class BassProxy : IAudio
	{
		private static BassProxy instance;
		
		private bool canPlay;
		private bool canPause;
		private bool canStop;
		private bool isPlaying;
		
		/// <summary>
		///   Default sample rate used at initialization
		/// </summary>
		private const int DEFAULT_SAMPLE_RATE = 44100;

		/// <summary>
		///   Shows whether the proxy is already disposed
		/// </summary>
		private bool _alreadyDisposed;

		/// <summary>
		///   Currently playing stream
		/// </summary>
		private int _playingStream;

		#region Constructors
		static BassProxy()
		{
			//Call to avoid the freeware splash screen. Didn't see it, but maybe it will appear if the Forms are used :D
			BassNet.Registration("gleb.godonoga@gmail.com", "2X155323152222");
			
			//Dummy calls made for loading the assemblies
			int bassVersion = Bass.BASS_GetVersion();
			int bassMixVersion = BassMix.BASS_Mixer_GetVersion();
			int bassfxVersion = BassFx.BASS_FX_GetVersion();
			
			//Window mainWindow = Application.Current.MainWindow;
			//WindowInteropHelper interopHelper = new WindowInteropHelper(mainWindow);
			//if (!Bass.BASS_Init(-1, DEFAULT_SAMPLE_RATE, BASSInit.BASS_DEVICE_SPEAKERS, interopHelper.Handle))
			
			//Set Sample Rate / MONO
			if (Bass.BASS_Init(-1, DEFAULT_SAMPLE_RATE, BASSInit.BASS_DEVICE_SPEAKERS | BASSInit.BASS_DEVICE_DEFAULT | BASSInit.BASS_DEVICE_MONO, IntPtr.Zero))
			{
				// Load the plugins
				int pluginFlac = Bass.BASS_PluginLoad("bassflac.dll");
				int pluginAAC = Bass.BASS_PluginLoad("bass_aac.dll");
				int pluginMPC = Bass.BASS_PluginLoad("bass_mpc.dll");
				int pluginAC3 = Bass.BASS_PluginLoad("bass_ac3.dll");
				int pluginWMA = Bass.BASS_PluginLoad("basswma.dll");
				int pluginAPE = Bass.BASS_PluginLoad("bass_ape.dll");
				
				if (pluginFlac == 0
				    || pluginAAC == 0
				    || pluginMPC == 0
				    || pluginAC3 == 0
				    || pluginWMA == 0
				    || pluginAPE == 0)
					throw new Exception(Bass.BASS_ErrorGetCode().ToString());
				
				#if DEBUG
				BASS_INFO info = new BASS_INFO();
				Bass.BASS_GetInfo(info);
				Debug.WriteLine(info.ToString());
				
				string nativeSupport = Utils.BASSAddOnGetSupportedFileExtensions(null);
				Debug.WriteLine("Native Bass Supported Extensions: " + nativeSupport);
				
				BASS_PLUGININFO flacInfo = Bass.BASS_PluginGetInfo(pluginFlac);
				foreach (BASS_PLUGINFORM f in flacInfo.formats) {
					Debug.WriteLine("Type={0}, Name={1}, Exts={2}", f.ctype, f.name, f.exts);
				}
				BASS_PLUGININFO aacInfo = Bass.BASS_PluginGetInfo(pluginAAC);
				foreach (BASS_PLUGINFORM f in aacInfo.formats) {
					Debug.WriteLine("Type={0}, Name={1}, Exts={2}", f.ctype, f.name, f.exts);
				}
				BASS_PLUGININFO mpcInfo = Bass.BASS_PluginGetInfo(pluginMPC);
				foreach (BASS_PLUGINFORM f in mpcInfo.formats) {
					Debug.WriteLine("Type={0}, Name={1}, Exts={2}", f.ctype, f.name, f.exts);
				}
				BASS_PLUGININFO ac3Info = Bass.BASS_PluginGetInfo(pluginAC3);
				foreach (BASS_PLUGINFORM f in ac3Info.formats) {
					Debug.WriteLine("Type={0}, Name={1}, Exts={2}", f.ctype, f.name, f.exts);
				}
				BASS_PLUGININFO wmaInfo = Bass.BASS_PluginGetInfo(pluginWMA);
				foreach (BASS_PLUGINFORM f in wmaInfo.formats) {
					Debug.WriteLine("Type={0}, Name={1}, Exts={2}", f.ctype, f.name, f.exts);
				}
				BASS_PLUGININFO apeInfo = Bass.BASS_PluginGetInfo(pluginAPE);
				foreach (BASS_PLUGINFORM f in apeInfo.formats) {
					Debug.WriteLine("Type={0}, Name={1}, Exts={2}", f.ctype, f.name, f.exts);
				}

				Dictionary<int, string> loadedPlugIns = new Dictionary<int, string>();
				loadedPlugIns.Add(pluginFlac, "bassflac.dll");
				loadedPlugIns.Add(pluginAAC, "bass_aac.dll");
				loadedPlugIns.Add(pluginMPC, "bass_mpc.dll");
				loadedPlugIns.Add(pluginAC3, "bass_ac3.dll");
				loadedPlugIns.Add(pluginWMA, "basswma.dll");
				loadedPlugIns.Add(pluginAPE, "bass_ape.dll");
				
				string fileSupportedExtFilter = Utils.BASSAddOnGetPluginFileFilter(loadedPlugIns, "All supported Audio Files", true);
				Debug.WriteLine("Bass generated FileFilter: " + fileSupportedExtFilter);
				#endif
			} else {
				throw new Exception(Bass.BASS_ErrorGetCode().ToString());
			}
			
			if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_MIXER_FILTER, 50)) {
				/*Set filter for anti aliasing*/
				throw new Exception(Bass.BASS_ErrorGetCode().ToString());
			}
			
			if (!Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_FLOATDSP, true)) {
				/*Set floating parameters to be passed*/
				throw new Exception(Bass.BASS_ErrorGetCode().ToString());
			}
		}

		/// <summary>
		/// Private Constructor
		/// </summary>
		private BassProxy()
		{
		}
		#endregion

		#region Singleton Instance
		public static BassProxy Instance
		{
			get
			{
				if (instance == null)
					instance = new BassProxy();
				return instance;
			}
		}
		#endregion
		
		#region IAudio Members
		/// <summary>
		/// Read mono from file
		/// </summary>
		/// <param name="filename">Name of the file</param>
		/// <param name="samplerate">Output sample rate</param>
		/// <param name="milliseconds">Milliseconds to read</param>
		/// <param name="startmillisecond">Start millisecond</param>
		/// <returns>Array of samples</returns>
		/// <remarks>
		/// Seeking capabilities of Bass where not used because of the possible
		/// timing errors on different formats.
		/// </remarks>
		public float[] ReadMonoFromFile(string filename, int samplerate, int milliseconds, int startmillisecond)
		{
			int totalmilliseconds = milliseconds <= 0 ? Int32.MaxValue : milliseconds + startmillisecond;
			float[] data = null;
			//create streams for re-sampling
			int stream = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT); //Decode the stream
			if (stream == 0)
				throw new Exception(Bass.BASS_ErrorGetCode().ToString());
			int mixerStream = BassMix.BASS_Mixer_StreamCreate(samplerate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
			if (mixerStream == 0)
				throw new Exception(Bass.BASS_ErrorGetCode().ToString());

			if (BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
			{
				int bufferSize = samplerate * 20 * 4; /*read 10 seconds at each iteration*/
				float[] buffer = new float[bufferSize];
				List<float[]> chunks = new List<float[]>();
				int size = 0;
				while ((float)(size) / samplerate * 1000 < totalmilliseconds)
				{
					//get re-sampled/mono data
					int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, bufferSize);
					if (bytesRead == 0)
						break;
					float[] chunk = new float[bytesRead / 4]; //each float contains 4 bytes
					Array.Copy(buffer, chunk, bytesRead / 4);
					chunks.Add(chunk);
					size += bytesRead / 4; //size of the data
				}

				if ((float)(size) / samplerate * 1000 < (milliseconds + startmillisecond))
					return null; /*not enough samples to return the requested data*/
				int start = (int)((float)startmillisecond * samplerate / 1000);
				int end = (milliseconds <= 0) ? size : (int)((float)(startmillisecond + milliseconds) * samplerate / 1000);
				data = new float[size];
				int index = 0;
				/*Concatenate*/
				foreach (float[] chunk in chunks)
				{
					Array.Copy(chunk, 0, data, index, chunk.Length);
					index += chunk.Length;
				}
				/*Select specific part of the song*/
				if (start != 0 || end != size)
				{
					float[] temp = new float[end - start];
					Array.Copy(data, start, temp, 0, end - start);
					data = temp;
				}
			}
			else
				throw new Exception(Bass.BASS_ErrorGetCode().ToString());
			Bass.BASS_StreamFree(mixerStream);
			Bass.BASS_StreamFree(stream);
			return data;
		}
		#endregion

		#region Public Methods
		/// <summary>
		///   Read data from file
		/// </summary>
		/// <param name = "filename">Filename to be read</param>
		/// <param name = "samplerate">Sample rate at which to perform reading</param>
		/// <returns>Array with data</returns>
		public float[] ReadMonoFromFile(string filename, int samplerate)
		{
			return ReadMonoFromFile(filename, samplerate, 0, 0);
		}

		/// <summary>
		/// Get's tag info from file
		/// </summary>
		/// <param name = "filename">Filename to decode</param>
		/// <returns>TAG_INFO structure</returns>
		/// <remarks>
		///   The tags can be extracted using the following code:
		///   <code>
		///     tags.album
		///     tags.albumartist
		///     tags.artist
		///     tags.title
		///     tags.duration
		///     tags.genre, and so on.
		///   </code>
		/// </remarks>
		public TAG_INFO GetTagInfoFromFile(string filename)
		{
			return BassTags.BASS_TAG_GetFromFile(filename);
		}

		/// <summary>
		/// Return the duration in seconds
		/// </summary>
		/// <param name="filename">filename</param>
		/// <returns>duration in seconds</returns>
		public double GetDurationInSeconds(string filename) {
			
			double time = -1;
			int stream = Bass.BASS_StreamCreateFile(filename, 0L, 0L, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
			if (stream != 0) {
				
				// length in bytes
				long len = Bass.BASS_ChannelGetLength(stream, BASSMode.BASS_POS_BYTES);
				
				// the time length
				time = Bass.BASS_ChannelBytes2Seconds(stream, len);
				
				// free resource
				Bass.BASS_StreamFree(stream);
			}
			return time;
		}
		
		/// <summary>
		/// Recode the file
		/// </summary>
		/// <param name="fileName">Initial file</param>
		/// <param name="outFileName">Target file</param>
		/// <param name="targetSampleRate">Target sample rate</param>
		public void RecodeTheFile(string fileName, string outFileName, int targetSampleRate)
		{
			int stream = Bass.BASS_StreamCreateFile(fileName, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
			TAG_INFO tags = new TAG_INFO();
			BassTags.BASS_TAG_GetFromFile(stream, tags);
			int mixerStream = BassMix.BASS_Mixer_StreamCreate(targetSampleRate, 1, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_MONO | BASSFlag.BASS_SAMPLE_FLOAT);
			if (BassMix.BASS_Mixer_StreamAddChannel(mixerStream, stream, BASSFlag.BASS_MIXER_FILTER))
			{
				WaveWriter waveWriter = new WaveWriter(outFileName, mixerStream, true);
				const int length = 5512 * 10 * 4;
				float[] buffer = new float[length];
				while (true)
				{
					int bytesRead = Bass.BASS_ChannelGetData(mixerStream, buffer, length);
					if (bytesRead == 0)
						break;
					waveWriter.Write(buffer, bytesRead);
				}
				waveWriter.Close();
			}
			else
				throw new Exception(Bass.BASS_ErrorGetCode().ToString());
		}
		#endregion
		
		#region IDisposable
		/// <summary>
		///   Dispose the unmanaged resource. Free bass.dll.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
			_alreadyDisposed = true;
			GC.SuppressFinalize(this);
		}
		
		/// <summary>
		///   Dispose the resources
		/// </summary>
		/// <param name = "isDisposing">If value is disposing</param>
		protected virtual void Dispose(bool isDisposing)
		{
			if (!_alreadyDisposed)
			{
				if (!isDisposing)
				{
					//release managed resources
				}
				// Bass.BASS_Free();
			}
		}

		/// <summary>
		///   Finalizer
		/// </summary>
		~BassProxy()
		{
			Dispose(true);
		}
		#endregion
		
		#region Open methods
		public void OpenFile(string path) {
			// BASS_STREAM_PRESCAN = Pre-scan the file for accurate seek points and length reading in MP3/MP2/MP1 files
			// and chained OGG files (has no effect on normal OGG files). This can significantly increase the time taken to create the stream, particularly with a large file and/or slow storage media.
			// BASS_SAMPLE_FLOAT = Use 32-bit floating-point sample data.
			int stream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
			//int stream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_DEFAULT);
			_playingStream = stream;
			CanPlay = true;
		}
		#endregion
		
		#region Public Play, Pause and Stop Methods
		public void Play() {
			if (CanPlay)
			{
				if (_playingStream != 0) {
					Bass.BASS_ChannelPlay(_playingStream, false);
				}
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
				if (_playingStream != 0) {
					Bass.BASS_ChannelPause(_playingStream);
				}
				IsPlaying = false;
				CanPlay = true;
				CanPause = false;
			}
		}
		
		public void Stop()
		{
			if (_playingStream != 0) {
				Bass.BASS_StreamFree(_playingStream);
				//Bass.BASS_Stop();
			}
			IsPlaying = false;
			CanStop = false;
			CanPlay = true;
			CanPause = false;
		}
		#endregion
		
		#region Public Properties
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