using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CommonUtils;

namespace Mirage
{
	public class AudioFileReader
	{
		public static float[] Decode(string fileIn, int srate, int secondsToAnalyze)
		{
			fileIn = Regex.Replace(fileIn, "%20", " ");
			float[] floatBuffer = null;
			
			// check if sox can read it
			using (Process checkSoxReadable = new Process())
			{
				checkSoxReadable.StartInfo.FileName = "./NativeLibraries\\sox\\sox.exe";
				//checkSox.StartInfo.FileName = @"C:\Program Files (x86)\sox-14.4.1\sox.exe";
				checkSoxReadable.StartInfo.Arguments = " --i \"" + fileIn + "\"";
				checkSoxReadable.StartInfo.UseShellExecute = false;
				checkSoxReadable.StartInfo.RedirectStandardOutput = true;
				checkSoxReadable.StartInfo.RedirectStandardError = true;
				checkSoxReadable.Start();
				checkSoxReadable.WaitForExit();
				
				int exitCode = checkSoxReadable.ExitCode;
				// 0 = succesfull
				// 1 = partially succesful
				// 2 = failed
				if (exitCode == 0) {
					// sox can read the file
					Console.Out.WriteLine("Using SOX to decode the file ...");
					floatBuffer = DecodeUsingSox(fileIn, srate, secondsToAnalyze);
				} else {
					// use mplayer to first convert it, then sox to read it
					Console.Out.WriteLine("Using MPlayer and SOX to decode the file ...");
					floatBuffer = DecodeUsingMplayerAndSox(fileIn, srate, secondsToAnalyze);
				}
			}
			return floatBuffer;
		}

		public static float[] DecodeUsingSox(string fileIn, int srate, int secondsToAnalyze) {
			
			using (Process toraw = new Process())
			{
				fileIn = Regex.Replace(fileIn, "%20", " ");
				Timer t = new Timer();
				t.Start();
				String curdir = System.Environment.CurrentDirectory;
				Dbg.WriteLine("Decoding: " + fileIn);
				String tempFile = System.IO.Path.GetTempFileName();
				String raw = tempFile + "_raw.wav";
				Dbg.WriteLine("Temporary raw file: " + raw);
				
				toraw.StartInfo.FileName = "./NativeLibraries\\sox\\sox.exe";
				//toraw.StartInfo.FileName = @"C:\Program Files (x86)\sox-14.4.1\sox.exe";
				toraw.StartInfo.Arguments = " \"" + fileIn + "\" -r "+srate+" -e float -b 32 -G -t raw \"" + raw + "\" channels 1";
				toraw.StartInfo.UseShellExecute = false;
				toraw.StartInfo.RedirectStandardOutput = true;
				toraw.StartInfo.RedirectStandardError = true;
				toraw.Start();
				toraw.WaitForExit();
				
				int exitCode = toraw.ExitCode;
				// 0 = succesfull
				// 1 = partially succesful
				// 2 = failed
				if (exitCode != 0) {
					string standardError = toraw.StandardError.ReadToEnd();
					Console.Out.WriteLine(standardError);
					return null;
				}
				
				#if DEBUG
				string standardOutput = toraw.StandardOutput.ReadToEnd();
				Console.Out.WriteLine(standardOutput);
				#endif
				
				float[] floatBuffer;
				FileStream fs = null;
				try {
					FileInfo fi = new FileInfo(raw);
					fs = fi.OpenRead();
					int bytes = (int)fi.Length;
					int samples = bytes/sizeof(float);
					if ((samples*sizeof(float)) != bytes)
						return null;

					// If very long files, just use a part
					if (bytes > (secondsToAnalyze+1)*srate*sizeof(float)) {
						int seekto = (bytes/2) - ((secondsToAnalyze/2)*sizeof(float)*srate);
						Dbg.WriteLine("seekto="+seekto);
						bytes = (secondsToAnalyze)*srate*sizeof(float);
						fs.Seek((samples/2-(secondsToAnalyze/2)*srate)*sizeof(float), SeekOrigin.Begin);
					}
					
					BinaryReader br = new BinaryReader(fs);
					
					byte[] bytesBuffer = new byte[bytes];
					br.Read(bytesBuffer, 0, bytesBuffer.Length);
					
					int items = (int)bytes/sizeof(float);
					floatBuffer = new float[items];
					
					for (int i = 0; i < items; i++) {
						floatBuffer[i] = BitConverter.ToSingle(bytesBuffer, i * sizeof(float)) * 65536.0f;
					}
					
				} catch (System.IO.FileNotFoundException) {
					floatBuffer = null;
					
				} finally {
					if (fs != null)
						fs.Close();
					try
					{
						File.Delete(tempFile);
						//File.Delete(raw);
					}
					catch (IOException io)
					{
						Console.WriteLine(io);
					}
					Dbg.WriteLine("Decoding Execution Time: " + t.Stop() + "ms");
				}
				return floatBuffer;
			}
		}

		public static float[] DecodeUsingMplayerAndSox(string fileIn, int srate, int secondsToAnalyze) {
			
			using (Process tosoxreadable = new Process())
			{
				fileIn = Regex.Replace(fileIn, "%20", " ");
				Timer t = new Timer();
				t.Start();
				String curdir = System.Environment.CurrentDirectory;
				Dbg.WriteLine("Decoding: " + fileIn);
				String tempFile = System.IO.Path.GetTempFileName();
				String soxreadablewav = tempFile + ".wav";
				Dbg.WriteLine("Temporary wav file: " + soxreadablewav);
				
				tosoxreadable.StartInfo.FileName = "./NativeLibraries\\mplayer\\mplayer.exe";
				//tosoxreadable.StartInfo.FileName = @"C:\Program Files (x86)\mplayer-svn-35908\mplayer.exe";

				tosoxreadable.StartInfo.Arguments = " -quiet -vc null -vo null -ao pcm:fast:waveheader \""+fileIn+"\" -ao pcm:file=\\\""+soxreadablewav+"\\\"";
				tosoxreadable.StartInfo.UseShellExecute = false;
				tosoxreadable.StartInfo.RedirectStandardOutput = true;
				tosoxreadable.StartInfo.RedirectStandardError = true;
				tosoxreadable.Start();
				tosoxreadable.WaitForExit();
				
				int exitCode = tosoxreadable.ExitCode;
				// 0 = succesfull
				// 1 = partially succesful
				// 2 = failed
				if (exitCode != 0) {
					string standardError = tosoxreadable.StandardError.ReadToEnd();
					Console.Out.WriteLine(standardError);
					return null;
				}
				
				#if DEBUG
				string standardOutput = tosoxreadable.StandardOutput.ReadToEnd();
				Console.Out.WriteLine(standardOutput);
				#endif

				float[] floatBuffer = DecodeUsingSox(soxreadablewav, srate, secondsToAnalyze);
				try
				{
					File.Delete(tempFile);
					//File.Delete(soxreadablewav);
				}
				catch (IOException io)
				{
					Console.WriteLine(io);
				}
				
				Dbg.WriteLine("Decoding Execution Time: " + t.Stop() + "ms");
				return floatBuffer;
			}
		}
		
		public static float[] DecodeUsingMplayer(string fileIn, int srate) {
			
			using (Process towav = new Process())
			{
				fileIn = Regex.Replace(fileIn, "%20", " ");
				Timer t = new Timer();
				t.Start();
				String curdir = System.Environment.CurrentDirectory;
				Dbg.WriteLine("Decoding: " + fileIn);
				String tempFile = System.IO.Path.GetTempFileName();
				String wav = tempFile + ".wav";
				Dbg.WriteLine("Temporary wav file: " + wav);
				
				towav.StartInfo.FileName = "./NativeLibraries\\mplayer\\mplayer.exe";
				//towav.StartInfo.FileName = @"C:\Program Files (x86)\mplayer-svn-35908\mplayer.exe";
				towav.StartInfo.Arguments = " -quiet -ao pcm:fast:waveheader \""+fileIn+"\" -format floatle -af resample="+srate+":0:2,pan=1:0.5:0.5 -channels 1 -vo null -vc null -ao pcm:file=\\\""+wav+"\\\"";
				towav.StartInfo.UseShellExecute = false;
				towav.StartInfo.RedirectStandardOutput = true;
				towav.StartInfo.RedirectStandardError = true;
				towav.Start();
				towav.WaitForExit();
				
				int exitCode = towav.ExitCode;
				// 0 = succesfull
				// 1 = partially succesful
				// 2 = failed
				if (exitCode != 0) {
					string standardError = towav.StandardError.ReadToEnd();
					Console.Out.WriteLine(standardError);
					return null;
				}
				
				#if DEBUG
				string standardOutput = towav.StandardOutput.ReadToEnd();
				Console.Out.WriteLine(standardOutput);
				#endif
				
				RiffRead riff = new RiffRead(wav);
				riff.Process();
				float[] floatBuffer = riff.SoundData[0];
				try
				{
					File.Delete(tempFile);
					//File.Delete(wav);
				}
				catch (IOException io)
				{
					Console.WriteLine(io);
				}
				Dbg.WriteLine("Decoding Execution Time: " + t.Stop() + "ms");
				return floatBuffer;
			}
		}
		
	}
}
