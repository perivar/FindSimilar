using System.Collections.Generic;
using System;
using CommonUtils;

/**
 * @file WaveFile.cpp
 *
 * Handling WAVE files - implementation.
 *
 * WaveFile class enables .wav file header and data access, either
 * per sample or per frame.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 0.0.7
 */
namespace Aquila
{
	/**
	 * Wave file data access.
	 */
	public class WaveFile
	{
		/**
		 * .wav file header structure.
		 */
		public class WaveHeader
		{
			public string RIFF = "RIFF";
			public UInt32 DataLength = new UInt32();
			public string WAVE = "WAVE";
			public string fmt_ = "fmt ";
			public UInt32 SubBlockLength = new UInt32();
			public UInt16 formatTag = new UInt16();
			public UInt16 Channels = new UInt16();
			public UInt32 SampFreq = new UInt32();
			public UInt32 BytesPerSec = new UInt32();
			public UInt16 BytesPerSamp = new UInt16();
			public UInt16 BitsPerSamp = new UInt16();
			public string data = new string(new char[4]);
			public UInt32 WaveSize = new UInt32();
			
			public int WaveHeaderSize = 44;
		}

		/**
		 * Which channel to use when reading stereo recordings.
		 */
		public enum StereoDataSource
		{
			LEFT_CHANNEL,
			RIGHT_CHANNEL
		}
		
		/**
		 * Data from both channels.
		 */
		public List<short> LChTab = new List<short>();
		public List<short> RChTab = new List<short>();

		/**
		 * Full path of the .wav file.
		 */
		private string filename;

		/**
		 * Header structure.
		 */
		private WaveHeader hdr = new WaveHeader();

		/**
		 * Number of samples per frame.
		 */
		private int samplesPerFrame;

		/**
		 * Frame length (in milliseconds).
		 */
		private int frameLength;

		/**
		 * Overlap between frames - fraction of frame length (0 < overlap < 1).
		 */
		private double overlap;

		/**
		 * Next power of 2 larger than number of samples per frame.
		 */
		private int zeroPaddedLength;
		
		/**
		 * Pointers to signal frames.
		 */
		public List<Frame> frames = new List<Frame>();

		/**
		 * Creates the WaveFile object.
		 *
		 * By default, no frame division will be performed after loading data
		 * from file.
		 * If frameLengthMs is given and is not 0, all calls to load() will
		 * perform frame division.
		 * Adjacent frames can overlap each other, the default overlap length
		 * is 66% of frame length.
		 *
		 * @param frameLengthMs frame length in milliseconds (0 - dont use frames)
		 * @param frameOverlap overlap between adjacent frames
		 */
		public WaveFile(int frameLengthMs) : this(frameLengthMs, 0.66)
		{
		}

		public WaveFile() : this(0, 0.66)
		{
		}

		public WaveFile(int frameLengthMs, double frameOverlap)
		{
			frameLength = frameLengthMs;
			overlap = frameOverlap;
		}

		/**
		 * Deletes the WaveFile object.
		 *
		 * If there was any frame division, deletes all frames.
		 */
		public void Dispose()
		{
			if (frameLength != 0)
				ClearFrames();
		}

		/**
		 * Reads the header and channel data from given .wav file.
		 *
		 * Data are read into a temporary buffer and then converted to
		 * channel sample vectors. If source is a mono recording, samples
		 * are written to left channel.
		 *
		 * To improve performance, no format checking is performed.
		 *
		 * @param file full path to .wav file
		 */
		public void Load(string file)
		{
			filename = file;
			LChTab.Clear();
			RChTab.Clear();
			if (frameLength != 0)
				ClearFrames();
			
			// first we read header from the stream
			// then as we know now the data size, we create a temporary
			// buffer and read raw data into that buffer
			BinaryFile fs = new BinaryFile(filename);
			LoadHeader(ref fs);
			short[] data = new short[hdr.WaveSize/2];
			LoadRawData(ref fs, data, hdr.WaveSize/2);
			fs.Close();
			
			// initialize data channels (using right channel only in stereo mode)
			uint channelSize = hdr.WaveSize/hdr.BytesPerSamp;
			
			//LChTab.resize(channelSize);
			//if (2 == hdr.Channels)
			//RChTab.resize(channelSize);
			
			// most important conversion happens right here
			if (16 == hdr.BitsPerSamp) {
				if (2 == hdr.Channels) {
					Convert16Stereo(data, channelSize);
				} else {
					Convert16Mono(data, channelSize);
				}
			} else {
				if (2 == hdr.Channels) {
					Convert8Stereo(data, channelSize);
				} else {
					Convert8Mono(data, channelSize);
				}
			}
			
			// clear the buffer
			data = null;
			
			// when we have the data, it is possible to create frames
			if (frameLength != 0)
				DivideFrames(LChTab);
		}

		/**
		 * Returns the filename.
		 *
		 * @return full path to currently loaded file
		 */
		public string GetFilename()
		{
			return filename;
		}

		/**
		 * Returns number of channels.
		 *
		 * @return 1 for mono, 2 for stereo, other types are not recognized
		 */
		public int GetChannelsNum()
		{
			return hdr.Channels;
		}

		/**
		 * Returns signal sample frequency.
		 *
		 * @return sample frequency in Hz
		 */
		public UInt32 GetSampleFrequency()
		{
			return hdr.SampFreq;
		}

		/**
		 * Returns the number of bytes per second.
		 *
		 * @return product of sample frequency and bytes pare sample
		 */
		public UInt32 GetBytesPerSec()
		{
			return hdr.BytesPerSec;
		}

		/**
		 * Returns number of bytes per sample.
		 *
		 * @return 1 for 8b-mono, 2 for 8b-stereo or 16b-mono, 4 dor 16b-stereo
		 */
		public int GetBytesPerSamp()
		{
			return hdr.BytesPerSamp;
		}

		/**
		 * Returns number of bits per sample
		 *
		 * @return 8 or 16
		 */
		public int GetBitsPerSamp()
		{
			return hdr.BitsPerSamp;
		}

		/**
		 * Returns the recording size (without header).
		 *
		 * The return value is a raw byte count. To know the real sample count,
		 * it must be divided by bytes per sample.
		 *
		 * @return byte count
		 */
		public UInt32 GetWaveSize()
		{
			return hdr.WaveSize;
		}

		/**
		 * Returns the real data length.
		 *
		 * @return left channel vector size
		 */
		public int GetSamplesCount()
		{
			return LChTab.Count;
		}

		/**
		 * Returns the audio recording length
		 *
		 * @return recording length in milliseconds
		 */
		public int GetAudioLength()
		{
			return (int)(hdr.WaveSize / (double)(hdr.BytesPerSec) * 1000);
		}

		/**
		 * Returns a pointer to data table.
		 *
		 * Because vector guarantees to be contiguous in memory, we can
		 * return the address of the first element in the vector.
		 * It is valid only before next operation which modifies the vector,
		 * but as we use it only to copy that memory to another buffer,
		 * we can do that safely.
		 *
		 * @return address of the first element
		 */
		public int GetData()
		{
			return LChTab[0];
		}

		/**
		 * Returns a const reference to channel source.
		 *
		 * @param source which channel to use as a source
		 * @return source vector
		 */
		public List<short> GetDataVector()
		{
			return GetDataVector(StereoDataSource.LEFT_CHANNEL);
		}

		public List<short> GetDataVector(StereoDataSource source)
		{
			return (source == StereoDataSource.LEFT_CHANNEL) ? LChTab : RChTab;
		}

		/**
		 * Saves selected frame span to a new file.
		 *
		 * @param filename where to save frames
		 * @param begin number of the first frame
		 * @param end number of the last frame
		 * @throw FormatException not allowed to save 8b-mono files
		 */
		public void SaveFrames(string filename, int begin, int end)
		{
			if (1 == hdr.Channels && 8 == hdr.BitsPerSamp)
			{
				throw new FormatException("Save error: 8-bit mono files are not supported yet!");
			}
			int samples = GetSamplesPerFrame();
			
			// calculate the boundaries of a fragment of the source channel
			// which correspond to given frame numbers
			int startPos = (int)(begin * samples * (1 - overlap));
			int endPos = (int)((end + 1) * samples * (1 - overlap) + samples * overlap);
			if (endPos > LChTab.Count)
				endPos = LChTab.Count;
			
			// number of data bytes in the resulting wave file
			UInt32 waveSize = (UInt32) (endPos - startPos) * hdr.BytesPerSamp;
			
			// prepare a new header and write it to file stream
			WaveHeader newHdr = new WaveHeader();
			//std.strncpy(newHdr.RIFF, hdr.RIFF, 4);
			newHdr.DataLength = (UInt32) (waveSize + newHdr.WaveHeaderSize);
			//std.strncpy(newHdr.WAVE, hdr.WAVE, 4);
			//std.strncpy(newHdr.fmt_, hdr.fmt_, 4);
			newHdr.SubBlockLength = hdr.SubBlockLength;
			newHdr.formatTag = hdr.formatTag;
			newHdr.Channels = hdr.Channels;
			newHdr.SampFreq = hdr.SampFreq;
			newHdr.BytesPerSec = hdr.BytesPerSec;
			newHdr.BytesPerSamp = hdr.BytesPerSamp;
			newHdr.BitsPerSamp = hdr.BitsPerSamp;
			//std.strncpy(newHdr.data, hdr.data, 4);
			newHdr.WaveSize = waveSize;
			
			BinaryFile fs = new BinaryFile(filename);
			//fs.write((string) newHdr, sizeof(WaveHeader));
			fs.Write(newHdr.RIFF);
			fs.Write(newHdr.DataLength);
			fs.Write(newHdr.WAVE);
			fs.Write(newHdr.fmt_);
			fs.Write(newHdr.SubBlockLength);
			fs.Write(newHdr.formatTag);
			fs.Write(newHdr.Channels);
			fs.Write(newHdr.SampFreq);
			fs.Write(newHdr.BytesPerSec);
			fs.Write(newHdr.BytesPerSamp);
			fs.Write(newHdr.BitsPerSamp);
			fs.Write(newHdr.data);
			fs.Write(newHdr.WaveSize);
			
			// convert our data from source channels to a temporary buffer
			short[] data = new short[waveSize/2];
			for (int i = startPos, di = 0; i < endPos; ++i, ++di)
			{
				if (16 == hdr.BitsPerSamp)
				{
					if (2 == hdr.Channels)
					{
						data[2 *di] = LChTab[i];
						data[2 *di+1] = RChTab[i];
					}
					else
					{
						data[di] = LChTab[i];
					}
				}
				else
				{
					if (2 == hdr.Channels)
					{
						//data[di/2] = ((RChTab[i] + 128) << 8) | (LChTab[i] + 128);
					}
				}
			}
			
			// write the raw data to file and clean the buffer
			for (int i = 0; i < data.Length; i++) {
				fs.Write(data[i]);
			}
			fs.Close();
			data = null;
		}

		/**
		 * Returns number of frames in the file.
		 *
		 * @return frame vector length
		 */
		public int GetFramesCount()
		{
			return frames.Count;
		}

		/**
		 * Returns number of samples in a single frame.
		 *
		 * @return samples per frame = bytes per frame / bytes per sample
		 */
		public int GetSamplesPerFrame()
		{
			int bytesPerFrame = (int)(hdr.BytesPerSec * frameLength / 1000.0);
			return bytesPerFrame / hdr.BytesPerSamp;
		}

		/**
		 * Returns frame length (in samples) after zero padding (ZP).
		 *
		 * @return padded frame length is a power of 2
		 */
		public int GetSamplesPerFrameZP()
		{
			return zeroPaddedLength;
		}

		/**
		 * Recalculates frame division, taking new arguments into consideration.
		 *
		 * @param newFrameLength new frame length in milliseconds
		 * @param newOverlap new overlap value
		 */
		public void Recalculate(int newFrameLength)
		{
			Recalculate(newFrameLength, 0.66);
		}

		public void Recalculate()
		{
			Recalculate(0, 0.66);
		}

		public void Recalculate(int newFrameLength, double newOverlap)
		{
			if (newFrameLength != 0)
				frameLength = newFrameLength;
			
			overlap = newOverlap;
			
			ClearFrames();
			DivideFrames(LChTab);
		}

		/**
		 * Reads file header into the struct.
		 *
		 * @param fs input file stream
		 * @see WaveFile::hdr
		 */
		private void LoadHeader(ref BinaryFile fs)
		{
			hdr.RIFF = fs.ReadString(4);
			hdr.DataLength = fs.ReadUInt32();
			hdr.WAVE = fs.ReadString(4);
			hdr.fmt_ = fs.ReadString(4);
			hdr.SubBlockLength = fs.ReadUInt32();
			hdr.formatTag = fs.ReadUInt16();
			hdr.Channels = fs.ReadUInt16();
			hdr.SampFreq = fs.ReadUInt32();
			hdr.BytesPerSec = fs.ReadUInt32();
			hdr.BytesPerSamp = fs.ReadUInt16();
			hdr.BitsPerSamp = fs.ReadUInt16();
			hdr.data = fs.ReadString(4);
			hdr.WaveSize = fs.ReadUInt32();
		}

		/**
		 * Reads raw data into the buffer.
		 *
		 * @param fs input file stream
		 * @param buffer pointer to data array
		 * @param bufferLength data buffer size
		 */
		private void LoadRawData(ref BinaryFile fs, short[] buffer, UInt32 bufferLength)
		{
			for (int i = 0; i < bufferLength; i++) {
				buffer[i] = (short) fs.ReadInt16();
			}
		}

		/**
		 * Converts the buffer to 16b stereo channels.
		 *
		 * @param data pointer to data buffer
		 * @param channelSize length of the channels
		 */
		private void Convert16Stereo(short[] data, uint channelSize)
		{
			for (int i = 0; i < channelSize; ++i)
			{
				//LChTab[i] = data[2 * i];
				//RChTab[i] = data[2 * i + 1];
				LChTab.Add(data[2 * i]);
				RChTab.Add(data[2 * i + 1]);
			}
		}

		/**
		 * Converts the buffer to 16b mono channel.
		 *
		 * @param data pointer to data buffer
		 * @param channelSize length of the channel
		 */
		private void Convert16Mono(short[] data, uint channelSize)
		{
			for (int i = 0; i < channelSize; ++i)
			{
				LChTab[i] = data[i];
			}
		}

		/**
		 * Converts the buffer to 8b stereo channels.
		 *
		 * @param data pointer to data buffer
		 * @param channelSize length of the channels
		 */
		private void Convert8Stereo(short[] data, uint channelSize)
		{
			// low byte and high byte of a 16b word
			byte lb = 0;
			byte hb = 0;
			for (int i = 0; i < channelSize; ++i)
			{
				SplitBytes(data[i/2], ref lb, ref hb);
				
				// left channel is in low byte, right in high
				// values are unipolar, so we move them by half
				// of the dynamic range
				LChTab[i] = (short) (lb - 128);
				RChTab[i] = (short) (hb - 128);
			}
		}

		/**
		 * Converts the buffer to 8b mono channel.
		 *
		 * @param data pointer to data buffer
		 * @param channelSize length of the channel
		 */
		private void Convert8Mono(short[] data, uint channelSize)
		{
			// low byte and high byte of a 16b word
			byte lb = 0;
			byte hb = 0;
			for (int i = 0; i < channelSize; ++i)
			{
				SplitBytes(data[i/2], ref lb, ref hb);
				
				// only the left channel collects samples
				LChTab[i] = (short) (lb - 128);
			}
		}

		/**
		 * Splits a 16-b number to lower and upper byte.
		 *
		 * @param twoBytes number to split
		 * @param lb lower byte (by reference)
		 * @param hb upper byte (by reference)
		 */
		private void SplitBytes(short twoBytes, ref byte lb, ref byte hb)
		{
			lb = (byte) (twoBytes & 0x00FF);
			hb = (byte) ((twoBytes >> 8) & 0x00FF);
		}

		/**
		 * Deletes all frame objects and clears the vector.
		 */
		private void ClearFrames()
		{
			frames.Clear();
		}

		/**
		 * Executes frame division, using overlap.
		 *
		 * Number of samples in an individual frame does not depend on the
		 * overlap value. The overlap affects total number of frames.
		 *
		 * @param source const reference to source chanel
		 */
		public void DivideFrames(List<short> source)
		{
			// calculate how many samples are in the part of the frame
			// which does NOT overlap, and use that value to find out
			// total number of frames; also set zero-padded length
			samplesPerFrame = GetSamplesPerFrame();
			int samplesPerNonOverlap = (int)(samplesPerFrame * (1 - overlap));
			int framesCount = (int)(hdr.WaveSize / hdr.BytesPerSamp) / samplesPerNonOverlap;
			int power = (int)(Math.Log((double)samplesPerFrame)/Math.Log(2.0));
			zeroPaddedLength = (int) ((byte)1 << (byte)(power + 1));

			int indexBegin = 0;
			int indexEnd = 0;
			for (int i = 0, size = source.Count; i < framesCount; ++i)
			{
				// calculate frame boundaries in the source channel
				// when frame end exceeds channel size, break out
				indexBegin = i * samplesPerNonOverlap;
				indexEnd = indexBegin + samplesPerFrame;
				if (indexEnd < size) {
					//frames.push_back(new Frame(source, indexBegin, indexEnd));
					frames.Add(new Frame(source, indexBegin, indexEnd));
				} else {
					break;
				}
			}
		}
	}
}