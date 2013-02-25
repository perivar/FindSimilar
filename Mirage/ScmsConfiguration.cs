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

namespace Mirage
{
	/// <summary>
	/// Utility class storing a cache and Configuration variables for the Scms
	/// distance computation.
	/// </summary>
	public class ScmsConfiguration
	{
		private int dim;
		private int covlen;
		private float[] mdiff;
		private float[] aicov;

		public ScmsConfiguration(int dimension)
		{
			dim = dimension;
			covlen = (dim*dim + dim)/2;
			mdiff = new float[dim];
			aicov = new float[covlen];
		}

		public int Dimension {
			get { return dim; }
		}

		public int CovarianceLength {
			get { return covlen; }
		}

		public float [] AddInverseCovariance {
			get { return aicov;  }
		}

		public float[] MeanDiff {
			get {  return mdiff; }
		}
	}
}
