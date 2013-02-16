/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 *
 * Copyright (C) 2007-2008 Dominik Schnitzer <dominik@schnitzer.at>
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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Comirva.Audio.Feature;
using Comirva.Audio.Util.Maths;

using NDtw;

namespace Mirage
{
	public class ScmsImpossibleException : Exception
	{
	}

	/** Statistical Cluster Model Similarity class. A Gaussian representation
	 *  of a song. The distance between two models is computed with the
	 *  symmetrized Kullback Leibler Divergence.
	 */
	public class Scms : AudioFeature
	{
		private float [] mean;
		private float [] cov;
		private float [] icov;
		private int dim;

		private string name; // the name
		public override string Name {
			get {
				return name;
			}
			set {
				this.name = value;
			}
		}
		
		public Scms(int dimension)
		{
			dim = dimension;
			int symDim = (dim * dim + dim) / 2;

			mean = new float [dim];
			cov  = new float [symDim];
			icov = new float [symDim];
		}

		// Computes a Scms model from the MFCC representation of a song.
		public static Scms GetScms(Matrix mfcc)
		{
			//mfcc.Print();
			DbgTimer t = new DbgTimer();
			t.Start();

			// Mean
			Vector m = mfcc.Mean();

			// Covariance
			Matrix c = mfcc.Covariance(m);

			// Inverse Covariance
			Matrix ic;
			try {
				ic = c.Inverse();
			} catch (MatrixSingularException) {
				//throw new ScmsImpossibleException();
				Dbg.WriteLine("MatrixSingularException - Scms failed!");
				return null;
			}

			// Store the Mean, Covariance, Inverse Covariance in an optimal format.
			int dim = m.rows;
			Scms s = new Scms(dim);
			int l = 0;
			for (int i = 0; i < dim; i++) {
				s.mean[i] = m.d[i, 0];
				for (int j = i; j < dim; j++) {
					s.cov[l] = c.d[i, j];
					s.icov[l] = ic.d[i, j];
					l++;
				}
			}

			long stop = 0;
			t.Stop(ref stop);
			Dbg.WriteLine("Mirage - scms created in: {0}ms", stop);

			return s;
		}

		public override double GetDistance(AudioFeature f)
		{
			if(!(f is Scms))
			{
				new Exception("Can only handle AudioFeatures of type Scms, not of: "+f);
				return -1;
			}
			Scms other = (Scms)f;
			
			Dtw dtw = new Dtw(this.GetArray(), other.GetArray(), DistanceMeasure.Euclidean, true, true, null, null, null);
			return dtw.GetCost();
			//return Distance(this, other, new ScmsConfiguration (Analyzer.MFCC_COEFFICIENTS));
		}
		
		public double[] GetArray() {
			
			double[] d = new double[mean.Length + cov.Length + icov.Length];

			int start = 0;
			for (int i = 0; i < mean.Length; i++) {
				d[start + i] = mean[i];
			}

			start += mean.Length;
			for (int i = 0; i < cov.Length; i++) {
				d[start + i] = cov[i];
			}

			start += cov.Length;
			for (int i = 0; i < icov.Length; i++) {
				d[start + i] = icov[i];
			}
			
			return d;
		}
		
		public static float Distance(byte [] a, byte [] b)
		{
			return Distance (
				FromBytes (a),
				FromBytes (b),
				new ScmsConfiguration (Analyzer.MFCC_COEFFICIENTS)
			);
		}

		/** Function to compute the spectral distance between two song models.
		 *  This is a fast implementation of the symmetrized Kullback Leibler
		 *  Divergence.
		 */
		public static float Distance(Scms s1, Scms s2, ScmsConfiguration c)
		{
			float val = 0;
			int i;
			int k;
			int idx = 0;
			int dim = c.Dimension;
			int covlen = c.CovarianceLength;
			float tmp1;

			unsafe {
				fixed (float* s1cov = s1.cov, s2icov = s2.icov,
				       s1icov = s1.icov, s2cov = s2.cov,
				       s1mean = s1.mean, s2mean = s2.mean,
				       mdiff = c.MeanDiff, aicov = c.AddInverseCovariance)
				{
					for (i = 0; i < covlen; i++) {
						aicov[i] = s1icov[i] + s2icov[i];
					}

					for (i = 0; i < dim; i++) {
						idx = i*dim - (i*i+i)/2;
						val += s1cov[idx+i] * s2icov[idx+i] +
							s2cov[idx+i] * s1icov[idx+i];

						for (k = i+1; k < dim; k++) {
							val += 2*s1cov[idx+k] * s2icov[idx+k] +
								2*s2cov[idx+k] * s1icov[idx+k];
						}
					}

					for (i = 0; i < dim; i++) {
						mdiff[i] = s1mean[i] - s2mean[i];
					}

					for (i = 0; i < dim; i++) {
						idx = i - dim;
						tmp1 = 0;

						for (k = 0; k <= i; k++) {
							idx += dim - k;
							tmp1 += aicov[idx] * mdiff[k];
						}
						for (k = i + 1; k < dim; k++) {
							idx++;
							tmp1 += aicov[idx] * mdiff[k];
						}
						val += tmp1 * mdiff[i];
					}
				}
			}

			// FIXME: fix the negative return values
			//val = Math.Max(0.0f, (val/2 - s1.dim));
			val = val / 4 - c.Dimension / 2;

			return val;
		}

		// Manual serialization of a Scms object to a byte array
		public override byte [] ToBytes()
		{
			using (var stream = new MemoryStream ()) {
				using (var bw = new BinaryWriter(stream)) {
					bw.Write ((Int32)dim);

					for (int i = 0; i < mean.Length; i++) {
						bw.Write (mean[i]);
					}

					for (int i = 0; i < cov.Length; i++) {
						bw.Write (cov[i]);
					}

					for (int i = 0; i < icov.Length; i++) {
						bw.Write (icov[i]);
					}

					return stream.ToArray ();
				}
			}
		}

		public static Scms FromBytes(byte [] buf)
		{
			var scms = new Scms(Analyzer.MFCC_COEFFICIENTS);
			FromBytes (buf, scms);
			return scms;
		}

		// Manual deserialization of an Scms from a LittleEndian byte array
		public static void FromBytes(byte[] buf, Scms s)
		{
			byte [] buf4 = new byte[4];
			int buf_i = 0;

			s.dim = GetInt32 (buf, buf_i, buf4);
			buf_i += 4;

			for (int i = 0; i < s.mean.Length; i++) {
				s.mean[i] = GetFloat (buf, buf_i, buf4);
				buf_i += 4;
			}

			for (int i = 0; i < s.cov.Length; i++) {
				s.cov[i] = GetFloat (buf, buf_i, buf4);
				buf_i += 4;
			}

			for (int i = 0; i < s.icov.Length; i++) {
				s.icov[i] = GetFloat (buf, buf_i, buf4);
				buf_i += 4;
			}
		}
	}
}