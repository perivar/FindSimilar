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
		public static float[] Decode(string fileIn, int srate)
		{
			fileIn = Regex.Replace(fileIn, "%20", " ");
			float[] floatBuffer = null;
			
			// check if sox can read it
			using (Process checkSox = new Process())
			{
				//checkSox.StartInfo.FileName = "./NativeLibraries\\sox\\sox.exe";
				checkSox.StartInfo.FileName = @"C:\Program Files (x86)\sox-14.4.1\sox.exe";
				checkSox.StartInfo.Arguments = " --i \"" + fileIn + "\"";
				checkSox.StartInfo.UseShellExecute = false;
				checkSox.StartInfo.RedirectStandardOutput = true;
				checkSox.StartInfo.RedirectStandardError = true;
				checkSox.Start();
				checkSox.WaitForExit();
				
				int exitCode = checkSox.ExitCode;
				// 0 = succesfull
				// 1 = partially succesful
				// 2 = failed
				if (exitCode == 0) {
					// sox can read the file
					Console.Out.WriteLine("Using SOX to decode the file ...");
					floatBuffer = DecodeUsingSox(fileIn, srate);
				} else {
					// use mplayer to read it
					Console.Out.WriteLine("Using MPlayer to decode the file ...");
					floatBuffer = DecodeUsingMplayer(fileIn, srate);
				}
			}
			return floatBuffer;
		}

		public static float[] DecodeUsingSox(string fileIn, int srate) {
			
			using (Process toraw = new Process())
			{
				fileIn = Regex.Replace(fileIn, "%20", " ");
				Timer t = new Timer();
				t.Start();
				String curdir = System.Environment.CurrentDirectory;
				Dbg.WriteLine("Decoding: " + fileIn);
				String tempFile = System.IO.Path.GetTempFileName();
				String raw = tempFile + ".wav";
				Dbg.WriteLine("Temporary raw file: " + raw);
				
				//toraw.StartInfo.FileName = "./NativeLibraries\\sox\\sox.exe";
				toraw.StartInfo.FileName = @"C:\Program Files (x86)\sox-14.4.1\sox.exe";
				toraw.StartInfo.Arguments = " \"" + fileIn + "\" -c 1 -r "+srate+" -e float -b 32 -G -t raw \"" + raw + "\"";
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

					BinaryReader br = new BinaryReader(fs);
					
					byte[] bytesBuffer = new byte[bytes];
					br.Read(bytesBuffer, 0, bytesBuffer.Length);
					
					int items = (int)bytes/sizeof(float);
					floatBuffer = new float[items];
					
					for (int i = 0; i < items; i++) {
						floatBuffer[i] = BitConverter.ToSingle(bytesBuffer, i * sizeof(float));
					}
					
				} catch (System.IO.FileNotFoundException) {
					floatBuffer = null;
					
				} finally {
					if (fs != null)
						fs.Close();
					try
					{
						File.Delete(tempFile);
						File.Delete(raw);
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
				
				//toraw.StartInfo.FileName = "./NativeLibraries\\mplayer\\mplayer.exe";
				towav.StartInfo.FileName = @"C:\Program Files (x86)\mplayer-svn-35908\mplayer.exe";
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
					File.Delete(wav);
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
