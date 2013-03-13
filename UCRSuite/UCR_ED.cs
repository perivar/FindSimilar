using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;

// Dynamic Time Warping - UCR Suite in C#
// http://www.cs.ucr.edu/~eamonn/UCRsuite.html
// Ported to C# by Erez Robinson
// Modified and enhanced by perivar@nerseth.com

namespace UCRCSharp
{
	public partial class UCR
	{
		/// Data structure for sorting the query.
		internal struct IndexValue : IComparable, IComparable<IndexValue>, IComparer<IndexValue>
		{
			public double Value;
			public int Index;

			#region Implementation of IComparable<in IndexValue>

			public int CompareTo(object other)
			{
				return Comp(this, (IndexValue) other);
			}

			public int CompareTo(IndexValue other)
			{
				return Comp(this, other);
			}

			#endregion

			#region Implementation of IComparer<in IndexValue>

			public int Compare(IndexValue x, IndexValue y)
			{
				return Comp(x, y);
			}

			#endregion
		}

		/// Comparison function for sorting the query.
		/// The query will be sorted by absolute z-normalization value, |z_norm(Q[i])| from high to low.
		private static int Comp(IndexValue a, IndexValue b)
		{
			return Convert.ToInt16(Math.Abs(b.Value) - Math.Abs(a.Value));
		}

		/// Main function for calculating ED distance between the query, Q, and current data, T.
		/// Note that Q is already sorted by absolute z-normalization value, |z_norm(Q[i])|
		///double distance(const double * const Q, const double * const T , const int& j , const int& m , const double& mean , const double& std , const int* const order, const double& bsf)
		private static double distance(double[] Q, double[] T, long j, long m, double mean, double std, long[] order, double bsf)
		{
			int i;
			double sum = 0;
			for (i = 0; i < m && sum < bsf; i++)
			{
				double x = (T[(order[i] + j)] - mean)/std;
				sum += (x - Q[i])*(x - Q[i]);
			}
			return sum;
		}

		/// If serious error happens, terminate the program.
		private static void error_ed(int id)
		{
			if (id == 1)
				Console.WriteLine("ERROR : Memory can't be allocated!!!\n");
			else if (id == 2)
				Console.WriteLine("ERROR : File not Found!!!\n");
			else if (id == 3)
				Console.WriteLine("ERROR : Can't create Output File!!!\n");
			else if (id == 4)
			{
				Console.WriteLine("ERROR: Invalid Number of Arguments!!!");
				Console.WriteLine("Command Usage:   UCR_ED.exe  data_file  query_file   m   ");
				Console.WriteLine("For example  :   UCR_ED.exe  data.txt   query.txt   128  ");
			}
			Environment.Exit(1);
		}

		public static void ED(string inputFileName, string queryFileName, int queryLength)
		{
			FileStream fp = null;
			FileStream qp = null;
			double[] Q = null; // query array
			double[] T = null; // array of current data
			long[] order; // ordering of query by |z(q_i)|
			double bsf; // best-so-far
			int m; // length of query
			long loc = 0; // answer: location of the best-so-far match

			double d;
			int i, j;
			double ex, ex2, mean, std;

			double t1, t2;

			t1 = DateTime.Now.Ticks;

			bsf = double.PositiveInfinity;
			i = 0;
			j = 0;
			ex = ex2 = 0;

			try
			{
				using (qp = File.OpenRead(queryFileName))
				{
					m = queryLength;

					// Array for keeping the query data
					Q = new double[m];
					// Read the query data from input file and calculate its statistic such as mean, std

					using (TextReader reader = new StreamReader(qp))
					{
						string line;
						while ((line = reader.ReadLine()) != null)
						{
							string[] strs = line.Split(' ');

							for (int k = 0; k < strs.Length && i<m; k++)
							{
								
								if(String.IsNullOrEmpty(strs[k]))
									continue;
								d = double.Parse(strs[k]);

								ex += d;
								ex2 += d*d;
								Q[i] = d;
								i++;
							}
						}
					}

					mean = ex/m;
					std = ex2/m;
					std = (double)Math.Sqrt(std - mean*mean);
				}
				
				// Do z_normalixation on query data
				for (i = 0; i < m; i++)
					Q[i] = (Q[i] - mean)/std;

				
				// Array for keeping the query data
				order = new long[m]; // (double *)malloc(sizeof(double)*m);

				IndexValue[] Q_tmp = new IndexValue[m];

				for (i = 0; i < m; i++)
				{
					Q_tmp[i].Value = Q[i];
					Q_tmp[i].Index = i;
				}

				// Sort the query data
				Array.Sort(Q_tmp, Comp);

				for (i = 0; i < m; i++)
				{
					Q[i] = Q_tmp[i].Value;
					order[i] = Q_tmp[i].Index;
				}

				// Array for keeping the current data; Twice the size for removing modulo (circulation) in distance calculation
				T = new double[2*m]; //(double *)malloc(sizeof(double)*2*m);

				double dist = 0;
				i = 0;
				j = 0;
				ex = ex2 = 0;

				using (fp = File.OpenRead(inputFileName))
				{
					// Read data file, one row at a time
					using (TextReader reader = new StreamReader(fp))
					{
						string line;
						while ((line = reader.ReadLine()) != null)
						{
							string[] strs = line.Split(' ');

							for (int k = 0; k < strs.Length; k++)
							{
								if (String.IsNullOrEmpty(strs[k]))
									continue;
								d = double.Parse(strs[k]);

								ex += d;
								ex2 += d*d;
								T[i%m] = d;
								T[(i%m) + m] = d;

								// If there is enough data in T, the ED distance can be calculated
								if (i >= m - 1)
								{
									// the current starting location of T
									j = (i + 1)%m;

									// Z_norm(T[i]) will be calculated on the fly
									mean = ex/m;
									std = ex2/m;
									std = (double)Math.Sqrt(std - mean*mean);

									// Calculate ED distance
									dist = distance(Q, T, j, m, mean, std, order, bsf);
									if (dist < bsf)
									{
										bsf = dist;
										loc = i - m + 1;
									}
									ex -= T[j];
									ex2 -= T[j]*T[j];
								}
								i++;
							}
						}
					}
				}
				t2 = DateTime.Now.Ticks;

				Console.WriteLine("Location : " + loc);
				Console.WriteLine("Distance : " + Math.Sqrt(bsf));
				Console.WriteLine("Data Scanned : " + i);
				Console.WriteLine("Total Execution Time : " + (t2 - t1)/TimeSpan.TicksPerSecond + " sec");

			}
			catch (OutOfMemoryException)
			{
				error_ed(1);
			}
			catch (FileNotFoundException)
			{
				error_ed(2);
			}
			catch (Exception)
			{
				Environment.Exit(2);
			}
		}
	}
}
