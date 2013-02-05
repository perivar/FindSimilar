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

namespace Mirage
{
	[Serializable]
	public class CovarianceMatrix
	{
		public float[] d;
		public int dim;
		
		/// create a symmetric square matrix
		public CovarianceMatrix(int dim)
		{
			this.dim = dim;
			int length = (dim * dim + dim) / 2;
			d = new float[length];
		}

		/// create a symmetric square matrix using an existing Matrix
		public CovarianceMatrix(Matrix m)
		{
			this.dim = m.rows;
			int length = (dim * dim + dim) / 2;
			d = new float[length];
			
			int l = 0;
			for (int i = 0; i < m.rows; i++) {
				for (int j = i; j < m.columns; j++) {
					d[l] = m.d[i, j];
					l++;
				}
			}
		}
	}
}
