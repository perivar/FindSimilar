/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 *
 * Copyright (C) 2007-2008 Dominik Schnitzer <dominik@schnitzer.at>
 * Changed and enhanced by Per Ivar Nerseth <perivar@nerseth.com>
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA  02110-1301, USA.
 */

using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Comirva.Audio;
using Comirva.Audio.Extraction;
using Comirva.Audio.Feature;

using System.Globalization;

using CommonUtils;

// For drawing graph
using ZedGraph;
using System.Drawing;
using System.Drawing.Imaging;

using Wavelets;
using math.transform.jwave;
using math.transform.jwave.handlers;
using math.transform.jwave.handlers.wavelets;

using Soundfingerprinting;
using Soundfingerprinting.Audio.Services;
using Soundfingerprinting.Fingerprinting;
using Soundfingerprinting.Fingerprinting.FFT;
using Soundfingerprinting.Fingerprinting.Wavelets;
using Soundfingerprinting.Fingerprinting.Configuration;
using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
using Soundfingerprinting.Image;
using Soundfingerprinting.Audio.Models;
using Soundfingerprinting.Hashing;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.DbStorage.Entities;
using Soundfingerprinting.SoundTools;

// Heavily modified by perivar@nerseth.com
namespace Mirage
{
	public class Analyzer
	{
		public const bool DEBUG_INFO_VERBOSE = false;
		public const bool DEBUG_OUTPUT_TEXT = false;
		public const bool DEFAULT_DEBUG_INFO = false;
		
		public enum AnalysisMethod {
			SCMS = 1,
			MandelEllis = 2,
			AudioFingerprinting = 3
		}
		
		public const int SAMPLING_RATE = 44100; //22050;
		private const int WINDOW_SIZE = 2048; //2048 1024;
		private const int MEL_COEFFICIENTS = 40; // 36 filters (SPHINX-III uses 40)
		public const int MFCC_COEFFICIENTS = 20; //20
		public const int SECONDS_TO_ANALYZE = 60;
		
		// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
		// Matlab multiplies with 2^15 (32768)
		public const int AUDIO_MULTIPLIER = 32768;
		
		//private static MfccLessOptimized mfcc = new MfccLessOptimized(WINDOW_SIZE, SAMPLING_RATE, MEL_COEFFICIENTS, MFCC_COEFFICIENTS);
		private static MfccMirage mfccMirage = new MfccMirage(WINDOW_SIZE, SAMPLING_RATE, MEL_COEFFICIENTS, MFCC_COEFFICIENTS);

		#if DEBUG
		//private static Mfcc mfccOptimized = new Mfcc(WINDOW_SIZE, SAMPLING_RATE, MEL_COEFFICIENTS, MFCC_COEFFICIENTS);
		//private static MFCC mfccComirva = new MFCC(SAMPLING_RATE, WINDOW_SIZE, MFCC_COEFFICIENTS, true, 20.0, SAMPLING_RATE/2, MEL_COEFFICIENTS);
		#endif
		
		// http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting
		// The parameters used in the Duplicates-detector-via-audio-fingerprinting transformation steps
		// is equal to those that have been found work well in other audio fingerprinting studies
		// (specifically in A Highly Robust Audio Fingerprinting System):
		// audio frames that are 371 ms long (2048 samples), taken every 11.6 ms (64 samples),
		// thus having an overlap of 31/32
		//
		// parameters: samplerate: 5512 hz, overlap: 31/32, window length: 2048
		// slice (window) size: 2048 / 5512 * 1000 =  371 ms
		// distance between slices: 64 / 5512 * 1000 =  11.6 ms

		// parameters: samplerate: 44100 hz, overlap: 1024 samples, window length: 2048
		// slice (window) size: 2048 / 44100 * 1000 =  46.44 ms
		// distance between slices: 1024 / 44100 * 1000 =  23.22 ms
		
		// Create the STFS object with 50% overlap (half of the window size);
		//private static Stft stft = new Stft(WINDOW_SIZE, WINDOW_SIZE/2, new HannWindow());
		private static StftMirage stftMirage = new StftMirage(WINDOW_SIZE, WINDOW_SIZE/2, new HannWindow());

		// Create the Soundfingerprinting Service
		private static FingerprintService fingerprintService = GetSoundfingerprintingService();
		//private static IFingerprintingConfiguration fingerprintingConfig = new DefaultFingerprintingConfiguration();
		private static IFingerprintingConfiguration fingerprintingConfig = new FullFrequencyFingerprintingConfiguration();

		public static AudioFeature AnalyzeMandelEllis(FileInfo filePath, bool doOutputDebugInfo=DEFAULT_DEBUG_INFO)
		{
			DbgTimer t = new DbgTimer();
			t.Start ();

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return null;
			}
			
			#if DEBUG
			DrawGraph(MathUtils.FloatToDouble(audiodata), "waveform.png");
			#endif
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;
			
			// Normalize
			//MathUtils.NormalizeInPlace(audiodata);
			
			// Matlab multiplies with 2^15 (32768)
			// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			MathUtils.Multiply(ref audiodata, AUDIO_MULTIPLIER); // 65536
			
			MandelEllisExtractor extractor = new MandelEllisExtractor(SAMPLING_RATE, WINDOW_SIZE, MFCC_COEFFICIENTS, MEL_COEFFICIENTS);
			AudioFeature audioFeature = extractor.Calculate(MathUtils.FloatToDouble(audiodata));
			
			if (audioFeature != null) {
				// Store duration
				audioFeature.Duration = (long) duration;
				
				// Store file name
				audioFeature.Name = filePath.Name;
			}
			
			Dbg.WriteLine ("MandelEllisExtractor - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);

			return audioFeature;
		}
		
		public static Scms AnalyzeScms(FileInfo filePath, bool doOutputDebugInfo=DEFAULT_DEBUG_INFO, bool useHaarWavelet = true)
		{
			DbgTimer t = new DbgTimer();
			t.Start ();
			FindSimilar.AudioProxies.BassProxy bass = FindSimilar.AudioProxies.BassProxy.Instance;

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return null;
			}

			// Name of file being processed
			string name = StringUtils.RemoveNonAsciiCharacters(Path.GetFileNameWithoutExtension(filePath.Name));
			
			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				if (DEBUG_OUTPUT_TEXT) WriteAscii(audiodata, name + "_audiodata.ascii");
				if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata, name + "_audiodata.txt");
			}
			#endif
			
			if (doOutputDebugInfo) {
				DrawGraph(MathUtils.FloatToDouble(audiodata), name + "_audiodata.png");
			}
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;
			
			// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// Matlab multiplies with 2^15 (32768)
			// e.g. if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			MathUtils.Multiply(ref audiodata, AUDIO_MULTIPLIER); // 65536
			
			// zero pad if the audio file is too short to perform a mfcc
			if (audiodata.Length < WINDOW_SIZE * 8)
			{
				int lenNew = WINDOW_SIZE * 8;
				Array.Resize<float>(ref audiodata, lenNew);
			}
			
			// 2. Windowing
			// 3. FFT
			Comirva.Audio.Util.Maths.Matrix stftdata = stftMirage.Apply(audiodata);

			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				if (DEBUG_OUTPUT_TEXT) {
					stftdata.WriteAscii(name + "_stftdata.ascii");
					stftdata.WriteCSV(name + "_stftdata.csv", ";");
				}
			}
			#endif

			if (doOutputDebugInfo) {
				// same as specgram(audio*32768, 2048, 44100, hanning(2048), 1024);
				stftdata.DrawMatrixImageLogValues(name + "_specgram.png", true);
				
				// spec gram with log values for the y axis (frequency)
				stftdata.DrawMatrixImageLogY(name + "_specgramlog.png", SAMPLING_RATE, 20, SAMPLING_RATE/2, 120, WINDOW_SIZE);
			}
			
			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE & false) {
				#region Inverse STFT
				double[] audiodata_inverse_stft = stftMirage.InverseStft(stftdata);
				
				// divide
				//MathUtils.Divide(ref audiodata_inverse_stft, AUDIO_MULTIPLIER);
				MathUtils.Normalize(ref audiodata_inverse_stft);

				if (DEBUG_OUTPUT_TEXT) {
					WriteAscii(audiodata_inverse_stft, name + "_audiodata_inverse_stft.ascii");
					WriteF3Formatted(audiodata_inverse_stft, name + "_audiodata_inverse_stft.txt");
				}
				
				DrawGraph(audiodata_inverse_stft, name + "_audiodata_inverse_stft.png");
				
				float[] audiodata_inverse_float = MathUtils.DoubleToFloat(audiodata_inverse_stft);
				bass.SaveFile(audiodata_inverse_float, name + "_inverse_stft.wav", Analyzer.SAMPLING_RATE);
				#endregion
			}
			#endif
			
			// 4. Mel Scale Filterbank
			// Mel-frequency is proportional to the logarithm of the linear frequency,
			// reflecting similar effects in the human's subjective aural perception)
			// 5. Take Logarithm
			// 6. DCT (Discrete Cosine Transform)

			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				#region Mel Scale and Log Values
				Comirva.Audio.Util.Maths.Matrix mellog = mfccMirage.ApplyMelScaleAndLog(ref stftdata);
				
				if (DEBUG_OUTPUT_TEXT) {
					mellog.WriteCSV(name + "_mel_log.csv", ";");
				}
				
				if (doOutputDebugInfo) {
					mellog.DrawMatrixImage(name + "_mel_log.png", 600, 400, true, true);
				}
				#endregion
				
				#region Inverse Mel Scale and Log Values
				if (false) {
					Comirva.Audio.Util.Maths.Matrix inverse_mellog = mfccMirage.InverseMelScaleAndLog(ref mellog);

					inverse_mellog.WriteCSV(name + "_mel_log_inverse.csv", ";");
					inverse_mellog.DrawMatrixImageLogValues(name + "_mel_log_inverse.png", true);
					
					double[] audiodata_inverse_mellog = stftMirage.InverseStft(inverse_mellog);
					//MathUtils.Divide(ref audiodata_inverse_mellog, AUDIO_MULTIPLIER/100);
					MathUtils.Normalize(ref audiodata_inverse_mellog);

					if (DEBUG_OUTPUT_TEXT) {
						WriteAscii(audiodata_inverse_mellog, name + "_audiodata_inverse_mellog.ascii");
						WriteF3Formatted(audiodata_inverse_mellog, name + "_audiodata_inverse_mellog.txt");
					}
					
					DrawGraph(audiodata_inverse_mellog, name + "_audiodata_inverse_mellog.png");
					
					float[] audiodata_inverse_mellog_float = MathUtils.DoubleToFloat(audiodata_inverse_mellog);
					bass.SaveFile(audiodata_inverse_mellog_float, name + "_inverse_mellog.wav", Analyzer.SAMPLING_RATE);
				}
				#endregion
			}
			#endif

			Comirva.Audio.Util.Maths.Matrix featureData = null;
			if (useHaarWavelet) {
				#region Wavelet Transform
				int lastHeight = 0;
				int lastWidth = 0;
				featureData = mfccMirage.ApplyMelScaleWaveletCompression(ref stftdata, out lastHeight, out lastWidth);

				#if DEBUG
				if (Analyzer.DEBUG_INFO_VERBOSE) {
					if (DEBUG_OUTPUT_TEXT) featureData.WriteAscii(name + "_waveletdata.ascii");
				}
				#endif

				if (doOutputDebugInfo) {
					featureData.DrawMatrixImageLogValues(name + "_waveletdata.png", true);
				}
				
				#if DEBUG
				if (Analyzer.DEBUG_INFO_VERBOSE  & false) {
					#region Inverse Wavelet
					// try to do an inverse wavelet transform
					Comirva.Audio.Util.Maths.Matrix stftdata_inverse_wavelet = mfccMirage.InverseMelScaleWaveletCompression(ref featureData, lastHeight, lastWidth);

					if (DEBUG_OUTPUT_TEXT) stftdata_inverse_wavelet.WriteCSV(name + "_specgramlog_inverse_wavelet.csv", ";");
					stftdata_inverse_wavelet.DrawMatrixImageLogValues(name + "_specgramlog_inverse_wavelet.png", true);
					
					double[] audiodata_inverse_wavelet = stftMirage.InverseStft(stftdata_inverse_wavelet);
					MathUtils.Normalize(ref audiodata_inverse_wavelet);
					
					if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata_inverse_wavelet, name + "_audiodata_inverse_wavelet.txt");
					DrawGraph(audiodata_inverse_wavelet, name + "_audiodata_inverse_wavelet.png");
					bass.SaveFile(MathUtils.DoubleToFloat(audiodata_inverse_wavelet), name + "_inverse_wavelet.wav", Analyzer.SAMPLING_RATE);
					#endregion
				}
				#endif
				#endregion
			} else {
				#region DCT Transform
				// It seems the Mirage way of applying the DCT is slightly faster than the
				// Comirva way due to less loops
				featureData = mfccMirage.ApplyMelScaleDCT(ref stftdata);
				//featureData = mfccMirage.ApplyComirvaWay(ref stftdata);

				#if DEBUG
				if (Analyzer.DEBUG_INFO_VERBOSE) {
					if (DEBUG_OUTPUT_TEXT) featureData.WriteAscii(name + "_mfccdata.ascii");
				}
				#endif

				if (doOutputDebugInfo) {
					featureData.DrawMatrixImageLogValues(name + "_mfccdata.png", true);
				}

				#if DEBUG
				if (Analyzer.DEBUG_INFO_VERBOSE & false) {
					#region Inverse MFCC
					// try to do an inverse mfcc
					Comirva.Audio.Util.Maths.Matrix stftdata_inverse_mfcc = mfccMirage.InverseMelScaleDCT(ref featureData);
					
					if (DEBUG_OUTPUT_TEXT) stftdata_inverse_mfcc.WriteCSV(name + "_stftdata_inverse_mfcc.csv", ";");
					stftdata_inverse_mfcc.DrawMatrixImageLogValues(name + "_specgramlog_inverse_mfcc.png", true);
					
					double[] audiodata_inverse_mfcc = stftMirage.InverseStft(stftdata_inverse_mfcc);
					MathUtils.Normalize(ref audiodata_inverse_mfcc);

					if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata_inverse_mfcc, name + "_audiodata_inverse_mfcc.txt");
					DrawGraph(audiodata_inverse_mfcc, name + "_audiodata_inverse_mfcc.png");
					bass.SaveFile(MathUtils.DoubleToFloat(audiodata_inverse_mfcc), name + "_inverse_mfcc.wav", Analyzer.SAMPLING_RATE);
					#endregion
				}
				#endif
				#endregion
			}
			
			// Store in a Statistical Cluster Model Similarity class.
			// A Gaussian representation of a song
			Scms audioFeature = Scms.GetScms(featureData, name);
			
			if (audioFeature != null) {
				
				// Store image if debugging
				if (doOutputDebugInfo) {
					audioFeature.Image = featureData.DrawMatrixImageLogValues(name + "_featuredata.png", true, false, 0, 0, true);
				}

				// Store bitstring hash as well
				string hashString = GetBitString(featureData);
				audioFeature.BitString = hashString;
				
				// Store duration
				audioFeature.Duration = (long) duration;
				
				// Store file name
				audioFeature.Name = filePath.FullName;
			}
			
			Dbg.WriteLine ("Mirage - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);

			return audioFeature;
		}
		
		public static AudioFeature AnalyzeSoundfingerprinting(FileInfo filePath, bool doOutputDebugInfo=DEFAULT_DEBUG_INFO, bool useHaarWavelet = true) {
			DbgTimer t = new DbgTimer();
			t.Start ();

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return null;
			}
			
			// Read TAGs using BASS
			FindSimilar.AudioProxies.BassProxy bass = FindSimilar.AudioProxies.BassProxy.Instance;
			Un4seen.Bass.AddOn.Tags.TAG_INFO tag_info = bass.GetTagInfoFromFile(filePath.FullName);

			// Name of file being processed
			string name = StringUtils.RemoveNonAsciiCharacters(Path.GetFileNameWithoutExtension(filePath.Name));
			
			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				if (DEBUG_OUTPUT_TEXT) WriteAscii(audiodata, name + "_audiodata.ascii");
				if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata, name + "_audiodata.txt");
			}
			#endif
			
			if (doOutputDebugInfo) {
				DrawGraph(MathUtils.FloatToDouble(audiodata), name + "_audiodata.png");
			}
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;
			
			// zero pad if the audio file is too short to perform a mfcc
			if (audiodata.Length < (fingerprintingConfig.WdftSize + fingerprintingConfig.Overlap))
			{
				int lenNew = fingerprintingConfig.WdftSize + fingerprintingConfig.Overlap;
				Array.Resize<float>(ref audiodata, lenNew);
			}
			
			// Get fingerprint signatures using the Soundfingerprinting methods
			
			// Get database
			DatabaseService databaseService = DatabaseService.Instance;

			IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms.csv", ",");
			Repository repository = new Repository(permutations, databaseService, fingerprintService);

			// Image Service
			ImageService imageService = new ImageService(
				fingerprintService.SpectrumService,
				fingerprintService.WaveletService);

			// work config
			WorkUnitParameterObject param = new WorkUnitParameterObject();
			param.FingerprintingConfiguration = fingerprintingConfig;
			param.AudioSamples = audiodata;
			param.PathToAudioFile = filePath.FullName;
			param.MillisecondsToProcess = SECONDS_TO_ANALYZE * 1000;
			param.StartAtMilliseconds = 0;

			// build track
			Track track = new Track();
			track.Title = name;
			track.TrackLengthMs = (int) duration;
			track.FilePath = filePath.FullName;
			track.Id = -1; // this will be set by the insert method
			
			#region parse tag_info
			if (tag_info != null) {
				Dictionary<string, string> tags = new Dictionary<string, string>();
				
				//if (tag_info.title != string.Empty) tags.Add("title", tag_info.title);
				if (tag_info.artist != string.Empty) tags.Add("artist", tag_info.artist);
				if (tag_info.album != string.Empty) tags.Add("album", tag_info.album);
				if (tag_info.albumartist != string.Empty) tags.Add("albumartist", tag_info.albumartist);
				if (tag_info.year != string.Empty) tags.Add("year", tag_info.year);
				if (tag_info.comment != string.Empty) tags.Add("comment", tag_info.comment);
				if (tag_info.genre != string.Empty) tags.Add("genre", tag_info.genre);
				if (tag_info.track != string.Empty) tags.Add("track", tag_info.track);
				if (tag_info.disc != string.Empty) tags.Add("disc", tag_info.disc);
				if (tag_info.copyright != string.Empty) tags.Add("copyright", tag_info.copyright);
				if (tag_info.encodedby != string.Empty) tags.Add("encodedby", tag_info.encodedby);
				if (tag_info.composer != string.Empty) tags.Add("composer", tag_info.composer);
				if (tag_info.publisher != string.Empty) tags.Add("publisher", tag_info.publisher);
				if (tag_info.lyricist != string.Empty) tags.Add("lyricist", tag_info.lyricist);
				if (tag_info.remixer != string.Empty) tags.Add("remixer", tag_info.remixer);
				if (tag_info.producer != string.Empty) tags.Add("producer", tag_info.producer);
				if (tag_info.bpm != string.Empty) tags.Add("bpm", tag_info.bpm);
				//if (tag_info.filename != string.Empty) tags.Add("filename", tag_info.filename);
				tags.Add("channelinfo", tag_info.channelinfo.ToString());
				//if (tag_info.duration > 0) tags.Add("duration", tag_info.duration.ToString());
				if (tag_info.bitrate > 0) tags.Add("bitrate", tag_info.bitrate.ToString());
				if (tag_info.replaygain_track_gain != -100f) tags.Add("replaygain_track_gain", tag_info.replaygain_track_gain.ToString());
				if (tag_info.replaygain_track_peak != -1f) tags.Add("replaygain_track_peak", tag_info.replaygain_track_peak.ToString());
				if (tag_info.conductor != string.Empty) tags.Add("conductor", tag_info.conductor);
				if (tag_info.grouping != string.Empty) tags.Add("grouping", tag_info.grouping);
				if (tag_info.mood != string.Empty) tags.Add("mood", tag_info.mood);
				if (tag_info.rating != string.Empty) tags.Add("rating", tag_info.rating);
				if (tag_info.isrc != string.Empty) tags.Add("isrc", tag_info.isrc);
				
				foreach(var nativeTag in tag_info.NativeTags) {
					string[] keyvalue = nativeTag.Split('=');
					tags.Add(keyvalue[0], keyvalue[1]);
				}
				track.Tags = tags;
			}
			#endregion
			
			AudioFeature audioFeature = null;
			double[][] logSpectrogram;
			if (repository.InsertTrackInDatabaseUsingSamples(track, 25, 4, param, out logSpectrogram)) {
				
				if (doOutputDebugInfo) {
					imageService.GetLogSpectralImages(logSpectrogram, fingerprintingConfig.Stride, fingerprintingConfig.FingerprintLength, fingerprintingConfig.Overlap, 2).Save(name + "_specgram_logimages.png");
					
					Comirva.Audio.Util.Maths.Matrix logSpectrogramMatrix = new Comirva.Audio.Util.Maths.Matrix(logSpectrogram);
					logSpectrogramMatrix = logSpectrogramMatrix.Transpose();
					logSpectrogramMatrix.DrawMatrixImageLogValues(name + "_specgram_logimage.png", true);
					
					if (DEBUG_OUTPUT_TEXT) {
						logSpectrogramMatrix.WriteCSV(name + "_specgram_log.csv", ";");
					}
				}
				
				audioFeature = new DummyAudioFeature();
				
				// Store duration
				audioFeature.Duration = (long) duration;
				
				// Store file name
				audioFeature.Name = filePath.FullName;
			} else {
				// failed
			}

			Dbg.WriteLine ("Soundfingerprinting - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);
			return audioFeature;
		}
		
		public static bool AnalyzeAndAdd(FileInfo filePath, Db db, DatabaseService databaseService, bool doOutputDebugInfo=DEFAULT_DEBUG_INFO, bool useHaarWavelet = true) {
			DbgTimer t = new DbgTimer();
			t.Start ();

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return false;
			}
			
			// Read TAGs using BASS
			FindSimilar.AudioProxies.BassProxy bass = FindSimilar.AudioProxies.BassProxy.Instance;
			Un4seen.Bass.AddOn.Tags.TAG_INFO tag_info = bass.GetTagInfoFromFile(filePath.FullName);

			// Name of file being processed
			string name = StringUtils.RemoveNonAsciiCharacters(Path.GetFileNameWithoutExtension(filePath.Name));
			
			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				if (DEBUG_OUTPUT_TEXT) WriteAscii(audiodata, name + "_audiodata.ascii");
				if (DEBUG_OUTPUT_TEXT) WriteF3Formatted(audiodata, name + "_audiodata.txt");
			}
			#endif
			
			if (doOutputDebugInfo) {
				DrawGraph(MathUtils.FloatToDouble(audiodata), name + "_audiodata.png");
			}
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;
			
			// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// Matlab multiplies with 2^15 (32768)
			// e.g. if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			MathUtils.Multiply(ref audiodata, AUDIO_MULTIPLIER); // 65536
			
			// zero pad if the audio file is too short to perform a mfcc
			if (audiodata.Length < (fingerprintingConfig.WdftSize + fingerprintingConfig.Overlap))
			{
				int lenNew = fingerprintingConfig.WdftSize + fingerprintingConfig.Overlap;
				Array.Resize<float>(ref audiodata, lenNew);
			}
			
			// Get fingerprint signatures using the Soundfingerprinting methods
			IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms.csv", ",");
			Repository repository = new Repository(permutations, databaseService, fingerprintService);

			// Image Service
			ImageService imageService = new ImageService(
				fingerprintService.SpectrumService,
				fingerprintService.WaveletService);

			// work config
			WorkUnitParameterObject param = new WorkUnitParameterObject();
			param.FingerprintingConfiguration = fingerprintingConfig;
			param.AudioSamples = audiodata;
			param.PathToAudioFile = filePath.FullName;
			param.MillisecondsToProcess = SECONDS_TO_ANALYZE * 1000;
			param.StartAtMilliseconds = 0;

			// build track
			Track track = new Track();
			track.Title = name;
			track.TrackLengthMs = (int) duration;
			track.FilePath = filePath.FullName;
			track.Id = -1; // this will be set by the insert method
			
			#region parse tag_info
			if (tag_info != null) {
				Dictionary<string, string> tags = new Dictionary<string, string>();
				
				//if (tag_info.title != string.Empty) tags.Add("title", tag_info.title);
				if (tag_info.artist != string.Empty) tags.Add("artist", tag_info.artist);
				if (tag_info.album != string.Empty) tags.Add("album", tag_info.album);
				if (tag_info.albumartist != string.Empty) tags.Add("albumartist", tag_info.albumartist);
				if (tag_info.year != string.Empty) tags.Add("year", tag_info.year);
				if (tag_info.comment != string.Empty) tags.Add("comment", tag_info.comment);
				if (tag_info.genre != string.Empty) tags.Add("genre", tag_info.genre);
				if (tag_info.track != string.Empty) tags.Add("track", tag_info.track);
				if (tag_info.disc != string.Empty) tags.Add("disc", tag_info.disc);
				if (tag_info.copyright != string.Empty) tags.Add("copyright", tag_info.copyright);
				if (tag_info.encodedby != string.Empty) tags.Add("encodedby", tag_info.encodedby);
				if (tag_info.composer != string.Empty) tags.Add("composer", tag_info.composer);
				if (tag_info.publisher != string.Empty) tags.Add("publisher", tag_info.publisher);
				if (tag_info.lyricist != string.Empty) tags.Add("lyricist", tag_info.lyricist);
				if (tag_info.remixer != string.Empty) tags.Add("remixer", tag_info.remixer);
				if (tag_info.producer != string.Empty) tags.Add("producer", tag_info.producer);
				if (tag_info.bpm != string.Empty) tags.Add("bpm", tag_info.bpm);
				//if (tag_info.filename != string.Empty) tags.Add("filename", tag_info.filename);
				tags.Add("channelinfo", tag_info.channelinfo.ToString());
				//if (tag_info.duration > 0) tags.Add("duration", tag_info.duration.ToString());
				if (tag_info.bitrate > 0) tags.Add("bitrate", tag_info.bitrate.ToString());
				if (tag_info.replaygain_track_gain != -100f) tags.Add("replaygain_track_gain", tag_info.replaygain_track_gain.ToString());
				if (tag_info.replaygain_track_peak != -1f) tags.Add("replaygain_track_peak", tag_info.replaygain_track_peak.ToString());
				if (tag_info.conductor != string.Empty) tags.Add("conductor", tag_info.conductor);
				if (tag_info.grouping != string.Empty) tags.Add("grouping", tag_info.grouping);
				if (tag_info.mood != string.Empty) tags.Add("mood", tag_info.mood);
				if (tag_info.rating != string.Empty) tags.Add("rating", tag_info.rating);
				if (tag_info.isrc != string.Empty) tags.Add("isrc", tag_info.isrc);
				
				foreach(var nativeTag in tag_info.NativeTags) {
					string[] keyvalue = nativeTag.Split('=');
					tags.Add(keyvalue[0], keyvalue[1]);
				}
				track.Tags = tags;
			}
			#endregion
			
			double[][] logSpectrogram;
			if (repository.InsertTrackInDatabaseUsingSamples(track, 25, 4, param, out logSpectrogram)) {

				// store logSpectrogram as Matrix
				Comirva.Audio.Util.Maths.Matrix logSpectrogramMatrix = new Comirva.Audio.Util.Maths.Matrix(logSpectrogram);
				logSpectrogramMatrix = logSpectrogramMatrix.Transpose();
				
				#region Debug for Soundfingerprinting Method
				if (doOutputDebugInfo) {
					imageService.GetLogSpectralImages(logSpectrogram, fingerprintingConfig.Stride, fingerprintingConfig.FingerprintLength, fingerprintingConfig.Overlap, 2).Save(name + "_specgram_logimages.png");
					
					logSpectrogramMatrix.DrawMatrixImageLogValues(name + "_specgram_logimage.png", true);
					
					if (DEBUG_OUTPUT_TEXT) {
						logSpectrogramMatrix.WriteCSV(name + "_specgram_log.csv", ";");
					}
				}
				#endregion
				
				#region Insert Statistical Cluster Model Similarity Audio Feature as well
				Comirva.Audio.Util.Maths.Matrix scmsMatrix = null;
				if (useHaarWavelet) {
					#region Wavelet Transform
					int lastHeight = 0;
					int lastWidth = 0;
					scmsMatrix = mfccMirage.ApplyWaveletCompression(ref logSpectrogramMatrix, out lastHeight, out lastWidth);

					#if DEBUG
					if (Analyzer.DEBUG_INFO_VERBOSE) {
						if (DEBUG_OUTPUT_TEXT) scmsMatrix.WriteAscii(name + "_waveletdata.ascii");
					}
					#endif

					if (doOutputDebugInfo) {
						scmsMatrix.DrawMatrixImageLogValues(name + "_waveletdata.png", true);
					}
					
					#if DEBUG
					if (Analyzer.DEBUG_INFO_VERBOSE) {
						#region Inverse Wavelet
						// try to do an inverse wavelet transform
						Comirva.Audio.Util.Maths.Matrix stftdata_inverse_wavelet = mfccMirage.InverseWaveletCompression(ref scmsMatrix, lastHeight, lastWidth, logSpectrogramMatrix.Rows, logSpectrogramMatrix.Columns);

						if (DEBUG_OUTPUT_TEXT) stftdata_inverse_wavelet.WriteCSV(name + "_specgramlog_inverse_wavelet.csv", ";");
						stftdata_inverse_wavelet.DrawMatrixImageLogValues(name + "_specgramlog_inverse_wavelet.png", true);
						#endregion
					}
					#endif
					#endregion
				} else {
					#region DCT Transform
					// It seems the Mirage way of applying the DCT is slightly faster than the
					// Comirva way due to less loops
					scmsMatrix = mfccMirage.ApplyDCT(ref logSpectrogramMatrix);

					#if DEBUG
					if (Analyzer.DEBUG_INFO_VERBOSE) {
						if (DEBUG_OUTPUT_TEXT) scmsMatrix.WriteAscii(name + "_mfccdata.ascii");
					}
					#endif

					if (doOutputDebugInfo) {
						scmsMatrix.DrawMatrixImageLogValues(name + "_mfccdata.png", true);
					}

					#if DEBUG
					if (Analyzer.DEBUG_INFO_VERBOSE) {
						#region Inverse MFCC
						// try to do an inverse mfcc
						Comirva.Audio.Util.Maths.Matrix stftdata_inverse_mfcc = mfccMirage.InverseDCT(ref scmsMatrix);
						
						if (DEBUG_OUTPUT_TEXT) stftdata_inverse_mfcc.WriteCSV(name + "_stftdata_inverse_mfcc.csv", ";");
						stftdata_inverse_mfcc.DrawMatrixImageLogValues(name + "_specgramlog_inverse_mfcc.png", true);
						#endregion
					}
					#endif
					#endregion
				}
				
				// Store in a Statistical Cluster Model Similarity class.
				// A Gaussian representation of a song
				Scms audioFeature = Scms.GetScms(scmsMatrix, name);
				
				if (audioFeature != null) {
					
					// Store image if debugging
					if (doOutputDebugInfo) {
						audioFeature.Image = scmsMatrix.DrawMatrixImageLogValues(name + "_featuredata.png", true, false, 0, 0, true);
					}

					// Store bitstring hash as well
					string hashString = GetBitString(scmsMatrix);
					audioFeature.BitString = hashString;
					
					// Store duration
					audioFeature.Duration = (long) duration;
					
					// Store file name
					audioFeature.Name = filePath.FullName;
					
					int id = track.Id;
					if (db.AddTrack(ref id, audioFeature) == -1) {
						Console.Out.WriteLine("Failed! Could not add audioFeature to database {0}!", name);
					}
				}
				#endregion
				
			} else {
				// failed
				return false;
			}

			Dbg.WriteLine ("AnalyzeAndAdd - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);
			return true;
		}
		
		// TODO: Rememeber to use another stride when querying
		public static Dictionary<Track, double> SimilarTracksSoundfingerprinting(FileInfo filePath) {
			DbgTimer t = new DbgTimer();
			t.Start ();
			FindSimilar.AudioProxies.BassProxy bass = FindSimilar.AudioProxies.BassProxy.Instance;

			float[] audiodata = AudioFileReader.Decode(filePath.FullName, SAMPLING_RATE, SECONDS_TO_ANALYZE);
			if (audiodata == null || audiodata.Length == 0)  {
				Dbg.WriteLine("Error! - No Audio Found");
				return null;
			}

			// Name of file being processed
			string name = StringUtils.RemoveNonAsciiCharacters(Path.GetFileNameWithoutExtension(filePath.Name));
			
			// Calculate duration in ms
			double duration = (double) audiodata.Length / SAMPLING_RATE * 1000;
			
			// Explode samples to the range of 16 bit shorts (–32,768 to 32,767)
			// Matlab multiplies with 2^15 (32768)
			// e.g. if( max(abs(speech))<=1 ), speech = speech * 2^15; end;
			MathUtils.Multiply(ref audiodata, AUDIO_MULTIPLIER); // 65536
			
			// zero pad if the audio file is too short to perform a mfcc
			if (audiodata.Length < (fingerprintingConfig.WdftSize + fingerprintingConfig.Overlap))
			{
				int lenNew = fingerprintingConfig.WdftSize + fingerprintingConfig.Overlap;
				Array.Resize<float>(ref audiodata, lenNew);
			}
			
			// Get fingerprint signatures using the Soundfingerprinting methods
			
			// Get database
			DatabaseService databaseService = DatabaseService.Instance;

			IPermutations permutations = new LocalPermutations("Soundfingerprinting\\perms.csv", ",");
			Repository repository = new Repository(permutations, databaseService, fingerprintService);

			// work config
			WorkUnitParameterObject param = new WorkUnitParameterObject();
			param.FingerprintingConfiguration = fingerprintingConfig;
			param.PathToAudioFile = filePath.FullName;
			param.AudioSamples = audiodata;
			param.MillisecondsToProcess = SECONDS_TO_ANALYZE * 1000;
			param.StartAtMilliseconds = 0;

			Dictionary<Track, double> candidates = repository.FindSimilarFromAudioSamples(25, 4, 2, param);
			return candidates;
			
			/*
			// Use var keyword to enumerate dictionary
			foreach (var pair in candidates)
			{
				Console.WriteLine("{0} - {1:0.00}",
				                  pair.Key.Title,
				                  pair.Value);
			}
			 */
			
			Dbg.WriteLine ("Soundfingerprinting - Total Execution Time: {0} ms", t.Stop().TotalMilliseconds);
		}
		
		/// <summary>
		/// Graphs an array of doubles varying between -1 and 1
		/// </summary>
		/// <param name="data">data</param>
		/// <param name="fileName">filename to save png to</param>
		/// <param name="onlyCanvas">true if no borders should be printed</param>
		public static void DrawGraph(double[] data, string fileName, bool onlyCanvas=false)
		{
			GraphPane myPane = new GraphPane( new RectangleF( 0, 0, 1200, 600 ), "", "", "" );
			
			if (onlyCanvas) {
				myPane.Chart.Border.IsVisible = false;
				myPane.Chart.Fill.IsVisible = false;
				myPane.Fill.Color = Color.Black;
				myPane.Margin.All = 0;
				myPane.Title.IsVisible = false;
				myPane.XAxis.IsVisible = false;
				myPane.YAxis.IsVisible = false;
			}
			myPane.XAxis.Scale.Max = data.Length - 1;
			myPane.XAxis.Scale.Min = 0;
			//myPane.YAxis.Scale.Max = 1;
			//myPane.YAxis.Scale.Min = -1;
			
			// add pretty stuff
			myPane.Fill = new Fill( Color.WhiteSmoke, Color.Lavender, 0F );
			myPane.Chart.Fill = new Fill( Color.FromArgb( 255, 255, 245 ),
			                             Color.FromArgb( 255, 255, 190 ), 90F );
			
			var timeData = Enumerable.Range(0, data.Length)
				.Select(i => (double) i)
				.ToArray();
			myPane.AddCurve(null, timeData, data, Color.Blue, SymbolType.None);
			
			Bitmap bm = new Bitmap( 1, 1 );
			using ( Graphics g = Graphics.FromImage( bm ) )
				myPane.AxisChange( g );
			
			myPane.GetImage().Save(fileName, ImageFormat.Png);
		}
		
		/// <summary>Writes the float array to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');</summary>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\data.ascii"</param>
		public static void WriteAscii(float[] data, string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write(" {0}\r", data[i].ToString("#.00000000e+000", CultureInfo.InvariantCulture));
			}
			pw.Close();
		}

		/// <summary>Writes the double array to an ascii-textfile that can be read by Matlab.
		/// Usage in Matlab: load('filename', '-ascii');</summary>
		/// <param name="filename">the name of the ascii file to create, e.g. "C:\\temp\\data.ascii"</param>
		public static void WriteAscii(double[] data, string filename)
		{
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write(" {0}\r", data[i].ToString("#.00000000e+000", CultureInfo.InvariantCulture));
			}
			pw.Close();
		}
		
		/// <summary>
		/// Write matrix to file using F3 formatting
		/// </summary>
		/// <param name="filename">filename</param>
		public static void WriteF3Formatted(float[] data, string filename) {
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write("{0}", data[i].ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) + " ");
				pw.Write("\r");
			}
			pw.Close();
		}
		
		/// <summary>
		/// Write matrix to file using F3 formatting
		/// </summary>
		/// <param name="filename">filename</param>
		public static void WriteF3Formatted(double[] data, string filename) {
			TextWriter pw = File.CreateText(filename);
			for(int i = 0; i < data.Length; i++)
			{
				pw.Write("{0}", data[i].ToString("F3", CultureInfo.InvariantCulture).PadLeft(10) + " ");
				pw.Write("\r");
			}
			pw.Close();
		}
		
		/// <summary>
		/// Computes the perceptual hash of an audio file using the mfcc matrix
		/// </summary>
		/// <param name="mfcc">mfcc Matrix</param>
		/// <returns>Returns a 'binary string' (aka bitstring) (like. 001010111011100010) which is easy to do a hamming distance on.</returns>
		private static string GetBitString(Comirva.Audio.Util.Maths.Matrix mfcc) {

			int rows = mfcc.Rows;
			int columns = mfcc.Columns;
			
			// 5. Compute the average value.
			// Compute the mean DCT value (using only
			// the 8x8 DCT low-frequency values and excluding the first term
			// since the DC coefficient can be significantly different from
			// the other values and will throw off the average).
			double total = 0;
			for (int x = 0; x < rows; x++) {
				for (int y = 0; y < columns; y++) {
					total += mfcc.MatrixData[x][y];
				}
			}
			total -= mfcc.MatrixData[0][0];
			
			double avg = total / (double)((rows * columns) - 1);

			// 6. Further reduce the DCT.
			// This is the magic step. Set the 64 hash bits to 0 or 1
			// depending on whether each of the 64 DCT values is above or
			// below the average value. The result doesn't tell us the
			// actual low frequencies; it just tells us the very-rough
			// relative scale of the frequencies to the mean. The result
			// will not vary as long as the overall structure of the image
			// remains the same; this can survive gamma and color histogram
			// adjustments without a problem.
			string hash = "";
			for (int x = 0; x < rows; x++) {
				for (int y = 0; y < columns; y++) {
					if (x != 0 && y != 0) {
						hash += (mfcc.MatrixData[x][y] > avg ? "1" : "0");
					}
				}
			}
			return hash;
		}
		
		private static FingerprintService GetSoundfingerprintingService() {

			// Audio service
			IAudioService audioService = new AudioService();
			
			// Fingerprint Descriptor
			FingerprintDescriptor fingerprintDescriptor = new FingerprintDescriptor();
			
			// SpectrumService
			SpectrumService spectrumService = new SpectrumService();
			
			// Wavelet Service
			IWaveletDecomposition waveletDecomposition = new Soundfingerprinting.Fingerprinting.Wavelets.StandardHaarWaveletDecomposition();
			IWaveletService waveletService = new WaveletService(waveletDecomposition);

			// Fingerprint Service
			FingerprintService fingerprintService = new FingerprintService(audioService,
			                                                               fingerprintDescriptor,
			                                                               spectrumService,
			                                                               waveletService);
			
			return fingerprintService;
		}
		
		private static List<bool[]> GetFingerprintSignatures(FingerprintService fingerprintService, float[] samples, string name) {
			
			Mirage.DbgTimer t = new Mirage.DbgTimer();
			t.Start();
			
			// work config
			WorkUnitParameterObject param = new WorkUnitParameterObject();
			param.FingerprintingConfiguration = fingerprintingConfig;
			
			// Get fingerprints
			double[][] LogSpectrogram;
			List<bool[]> fingerprints = fingerprintService.CreateFingerprintsFromAudioSamples(samples, param, out LogSpectrogram);

			#if DEBUG
			if (Analyzer.DEBUG_INFO_VERBOSE) {
				// Image Service
				ImageService imageService =
					new ImageService(fingerprintService.SpectrumService, fingerprintService.WaveletService);
				
				int width = param.FingerprintingConfiguration.FingerprintLength;
				int height = param.FingerprintingConfiguration.LogBins;
				imageService.GetImageForFingerprints(fingerprints, width, height, 2).Save(name + "_fingerprints.png");
			}
			#endif
			
			Mirage.Dbg.WriteLine("GetFingerprintSignatures Execution Time: " + t.Stop().TotalMilliseconds + " ms");
			return fingerprints;
		}
	}
}
