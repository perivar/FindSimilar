/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Mirage
{

	public class AudioFileReader
	{

		public static float[] Decode(string fileIn, int srate)
		{
			using (Process towav = new Process())
			{
				using (Process toraw = new Process())
				{
					fileIn = Regex.Replace(fileIn, "%20", " ");
					Timer t = new Timer();
					t.Start();
					String curdir = System.Environment.CurrentDirectory;
					Dbg.WriteLine("Decoding: " + fileIn);
					String wav = System.IO.Path.GetTempFileName();
					wav = wav + ".wav";
					String raw = System.IO.Path.GetTempFileName();
					Dbg.WriteLine("Temporary raw file: " + raw);

					if (fileIn.EndsWith("mp3") || fileIn.EndsWith("mp2"))
					{
						towav.StartInfo.FileName = "./NativeLibraries\\lame\\lame.exe";
						towav.StartInfo.Arguments = " --decode \"" + fileIn + "\" \"" + wav + "\"";
						towav.StartInfo.UseShellExecute = false;
						towav.StartInfo.RedirectStandardOutput = true;
						towav.Start();
						towav.WaitForExit();
					}
					else if (fileIn.EndsWith("aac") || fileIn.EndsWith("m4a"))
					{
						towav.StartInfo.FileName = "./NativeLibraries\\faad\\faad.exe";
						towav.StartInfo.Arguments = " -o \"" + wav + "\" \"" + fileIn + "\"";
						towav.StartInfo.UseShellExecute = false;
						towav.StartInfo.RedirectStandardOutput = true;
						towav.Start();
						towav.WaitForExit();

					}
					else if (fileIn.EndsWith("wav"))
					{
						wav = fileIn;
					}
					else
					{
						return null;
					}
					
					toraw.StartInfo.FileName = "./NativeLibraries\\sox\\sox.exe";
					//toraw.StartInfo.FileName = @"C:\Program Files (x86)\sox-14.4.1\sox.exe";
					
					// -t 			filetype
					// -c 			channels
					// -r 			samplerate
					// -b/-w/-l/-d	The sample data size is in bytes, 16-bit words,  32-bit long words, or 64-bit double long (long long) words.
					// -f 			The sample data encoding is Floating-point
					//toraw.StartInfo.Arguments = " -t wav \"" + wav + "\" -c 1 -r "+srate+" -l -f -t raw \"" + raw + "\"";
					
					// sox -t wav "C:\Users\perivar.nerseth\AppData\Local\Temp\tmp2B06.tmp.wav" -c 1 -r 11025 -e float -b 32 -t raw "C:\Users\perivar.nerseth\AppData\Local\Temp\tmp2B06.tmp"
					toraw.StartInfo.Arguments = " -t wav \"" + wav + "\" -c 1 -r "+srate+" -e float -b 32 -t raw \"" + raw + "\"";
					toraw.StartInfo.UseShellExecute = false;
					toraw.StartInfo.RedirectStandardOutput = true;
					//toraw.StartInfo.RedirectStandardError = true;
					//toraw.StartInfo.RedirectStandardInput = true;
					toraw.Start();
					toraw.WaitForExit();
					
					float[] floatBuffer;
					FileStream fs = null;
					try {
						File.Delete(wav);
						FileInfo fi = new FileInfo(raw);
						fs = fi.OpenRead();
						int bytes = (int)fi.Length;
						int samples = bytes/sizeof(float);
						if ((samples*sizeof(float)) != bytes)
							return null;
						
						if (bytes > 121*srate*sizeof(float)) {
							int seekto = (bytes/2) - (60*sizeof(float)*srate);
							Dbg.WriteLine("seekto="+seekto);
							bytes = 120*srate*sizeof(float);
							fs.Seek((samples/2-60*srate)*sizeof(float), SeekOrigin.Begin);
						}
						
						BinaryReader br = new BinaryReader(fs);
						
						byte[] bytesBuffer = new byte[bytes];
						br.Read(bytesBuffer, 0, bytesBuffer.Length);
						
						int items = (int)bytes/sizeof(float);
						floatBuffer = new float[items];
						
						// unsafe gain: 100ms
						for (int i = 0; i < items; i++) {
							floatBuffer[i] = BitConverter.ToSingle(bytesBuffer,
							                                       i * sizeof(float)) * 65536.0f;
						}
						
					} catch (System.IO.FileNotFoundException) {
						floatBuffer = null;
						
					} finally {
						if (fs != null)
							fs.Close();
						try
						{
							File.Delete(raw);
						}
						catch (IOException io)
						{
							Console.WriteLine(io);
						}
						Dbg.WriteLine("Mp3 Decoding Execution Time: " + t.Stop() + "ms");
					}
					
					return floatBuffer;
				}
			}
		}
	}
}
