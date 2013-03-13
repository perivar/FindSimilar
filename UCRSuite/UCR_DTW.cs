using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Dynamic Time Warping - UCR Suite in C#
// http://www.cs.ucr.edu/~eamonn/UCRsuite.html
// Ported to C# by Erez Robinson
// Modified and enhanced by perivar@nerseth.com

namespace UCRCSharp
{
	public partial class UCR
	{
		#region Helper Methods
		private static double dist(double x, double y)
		{
			return (x - y)*(x - y);
		}

		private static double min(double x, double y)
		{
			return x < y ? x : y;
		}

		private static double max(double x, double y)
		{
			return x > y ? x : y;
		}

		private static int max(int x, int y)
		{
			return x > y ? x : y;
		}
		#endregion

		/// Data structure (circular array) for finding minimum and maximum for LB_Keogh envolop
		private struct Deque
		{
			public int[] dq;
			public int size, capacity;
			public int f, r;

			public bool Empty
			{
				get { return size == 0; }
			}
		}

		static void bubble_sort_generic<T>(T[] array) where T : IComparable
		{
			long right_border = array.Length - 1;

			do
			{
				long last_exchange = 0;

				for (long i = 0; i < right_border; i++)
				{
					if (array[i].CompareTo(array[i + 1]) > 0)
					{
						T temp = array[i];
						array[i] = array[i + 1];
						array[i + 1] = temp;

						last_exchange = i;
					}
				}

				right_border = last_exchange;
			}
			while (right_border > 0);
		}
		
		#region Methods
		/// Initial the queue at the begining step of envelop calculation
		private static void init(ref Deque d, int capacity)
		{
			d.capacity = capacity;
			d.size = 0;
			d.dq = new int[d.capacity]; // (int *) malloc(sizeof(int)*d->capacity);
			d.f = 0;
			d.r = d.capacity - 1;
		}

		/// Insert to the queue at the back
		private static void push_back(ref Deque d, int v)
		{
			d.dq[d.r] = v;
			d.r--;
			if (d.r < 0)
				d.r = d.capacity - 1;
			d.size++;
		}

		/// Delete the current (front) element from queue
		private static void pop_front(ref Deque d)
		{
			d.f--;
			if (d.f < 0)
				d.f = d.capacity - 1;
			d.size--;
		}

		/// Delete the last element from queue
		private static void pop_back(ref Deque d)
		{
			d.r = (d.r + 1)%d.capacity;
			d.size--;
		}

		/// Get the value at the current position of the circular queue
		private static int front(ref Deque d)
		{
			int aux = d.f - 1;

			if (aux < 0)
				aux = d.capacity - 1;
			return d.dq[aux];
		}

		/// Get the value at the last position of the circular queueint back(struct deque *d)
		private static int back(ref Deque d)
		{
			int aux = (d.r + 1)%d.capacity;
			return d.dq[aux];
		}

		/// Finding the envelop of min and max value for LB_Keogh
		/// Implementation idea is intoruduced by Danial Lemire in his paper
		/// "Faster Retrieval with a Two-Pass Dynamic-Time-Warping Lower Bound", Pattern Recognition 42(9), 2009.
		private static void lower_upper_lemire(double[] t, int len, int r, double[] l, double[] u)
		{
			Deque du = new Deque();
			Deque dl = new Deque();

			init(ref du, 2*r + 2);
			init(ref dl, 2*r + 2);

			push_back(ref du, 0);
			push_back(ref dl, 0);

			for (int i = 1; i < len; i++)
			{
				if (i > r)
				{
					u[i - r - 1] = t[front(ref du)];
					l[i - r - 1] = t[front(ref dl)];
				}
				if (t[i] > t[i - 1])
				{
					pop_back(ref du);
					while (!du.Empty && t[i] > t[back(ref du)])
						pop_back(ref du);
				}
				else
				{
					pop_back(ref dl);
					while (!dl.Empty && t[i] < t[back(ref dl)])
						pop_back(ref dl);
				}
				push_back(ref du, i);
				push_back(ref dl, i);
				if (i == 2*r + 1 + front(ref du))
					pop_front(ref du);
				else if (i == 2*r + 1 + front(ref dl))
					pop_front(ref dl);
			}
			for (int i = len; i < len + r + 1; i++)
			{
				u[i - r - 1] = t[front(ref du)];
				l[i - r - 1] = t[front(ref dl)];
				if (i - front(ref du) >= 2*r + 1)
					pop_front(ref du);
				if (i - front(ref dl) >= 2*r + 1)
					pop_front(ref dl);
			}
		}

		/// Calculate quick lower bound
		/// Usually, LB_Kim take time O(m) for finding top,bottom,fist and last.
		/// However, because of z-normalization the top and bottom cannot give siginifant benefits.
		/// And using the first and last points can be computed in constant time.
		/// The prunning power of LB_Kim is non-trivial, especially when the query is not long, say in length 128.
		private static double lb_kim_hierarchy(double[] t, double[] q, long j, int len, double mean, double std,
		                                       double bsf = double.PositiveInfinity)
		{
			/// 1 point at front and back
			double d, lb;
			double x0 = (t[j] - mean)/std;
			double y0 = (t[(len - 1 + j)] - mean)/std;
			lb = dist(x0, q[0]) + dist(y0, q[len - 1]);
			if (lb >= bsf) return lb;

			/// 2 points at front
			double x1 = (t[(j + 1)] - mean)/std;
			d = min(dist(x1, q[0]), dist(x0, q[1]));
			d = min(d, dist(x1, q[1]));
			lb += d;
			if (lb >= bsf) return lb;

			/// 2 points at back
			double y1 = (t[(len - 2 + j)] - mean)/std;
			d = min(dist(y1, q[len - 1]), dist(y0, q[len - 2]));
			d = min(d, dist(y1, q[len - 2]));
			lb += d;
			if (lb >= bsf) return lb;

			/// 3 points at front
			double x2 = (t[(j + 2)] - mean)/std;
			d = min(dist(x0, q[2]), dist(x1, q[2]));
			d = min(d, dist(x2, q[2]));
			d = min(d, dist(x2, q[1]));
			d = min(d, dist(x2, q[0]));
			lb += d;
			if (lb >= bsf) return lb;

			/// 3 points at back
			double y2 = (t[(len - 3 + j)] - mean)/std;
			d = min(dist(y0, q[len - 3]), dist(y1, q[len - 3]));
			d = min(d, dist(y2, q[len - 3]));
			d = min(d, dist(y2, q[len - 2]));
			d = min(d, dist(y2, q[len - 1]));
			lb += d;

			return lb;
		}

		/// LB_Keogh 1: Create Envelop for the query
		/// Note that because the query is known, envelop can be created once at the begenining.
		///
		/// Variable Explanation,
		/// order : sorted indices for the query.
		/// uo, lo: upper and lower envelops for the query, which already sorted.
		/// t     : a circular array keeping the current data.
		/// j     : index of the starting location in t
		/// cb    : (output) current bound at each position. It will be used later for early abandoning in DTW.
		private static double lb_keogh_cumulative(long[] order, double[] t, double[] uo, double[] lo, double[] cb,
		                                          long j, int len, double mean, double std,
		                                          double best_so_far = double.PositiveInfinity)
		{
			double lb = 0;
			double x, d;

			for (int i = 0; i < len && lb < best_so_far; i++)
			{
				x = (t[(order[i] + j)] - mean)/std;
				d = 0;
				if (x > uo[i])
					d = dist(x, uo[i]);
				else if (x < lo[i])
					d = dist(x, lo[i]);
				lb += d;
				cb[order[i]] = d;
			}
			return lb;
		}

		/// LB_Keogh 2: Create Envelop for the data
		/// Note that the envelops have been created (in main function) when each data point has been read.
		///
		/// Variable Explanation,
		/// tz: Z-normalized data
		/// qo: sorted query
		/// cb: (output) current bound at each position. Used later for early abandoning in DTW.
		/// l,u: lower and upper envelop of the current data
		/// I: array pointer
		private static double lb_keogh_data_cumulative(long[] order, double[] tz, double[] qo, double[] cb, double[] l,
		                                               double[] u, int I , int len, double mean, double std,
		                                               double best_so_far = double.PositiveInfinity)
		{
			double lb = 0;
			double uu, ll, d;

			for (int i = 0; i < len && lb < best_so_far; i++)
			{
				uu = (u[order[i] + I] - mean)/std;
				ll = (l[order[i] + I] - mean)/std;
				d = 0;
				if (qo[i] > uu)
					d = dist(qo[i], uu);
				else
				{
					if (qo[i] < ll)
						d = dist(qo[i], ll);
				}
				lb += d;
				cb[order[i]] = d;
			}
			return lb;
		}

		/// Calculate Dynamic Time Wrapping distance
		/// A,B: data and query, respectively
		/// cb : cummulative bound used for early abandoning
		/// r  : size of Sakoe-Chiba warpping band
		private static double dtw(double[] A, double[] B, double[] cb, int m, int r,
		                          double bsf = double.PositiveInfinity)
		{

			double[] cost;
			double[] cost_prev;
			double[] cost_tmp;
			int i, j, k;
			double x, y, z, min_cost;

			/// Instead of using matrix of size O(m^2) or O(mr), we will reuse two array of size O(r).
			cost = new double[2*r + 1]; //(double*)malloc(sizeof(double)*(2*r+1));
			for (k = 0; k < 2*r + 1; k++) cost[k] = double.PositiveInfinity;

			cost_prev = new double[2*r + 1]; //(double*)malloc(sizeof(double)*(2*r+1));
			for (k = 0; k < 2*r + 1; k++) cost_prev[k] = double.PositiveInfinity;

			for (i = 0; i < m; i++)
			{
				k = max(0, r - i);
				min_cost = double.PositiveInfinity;

				for (j = max(0, i - r); j <= min(m - 1, i + r); j++, k++)
				{
					// Initialize all row and column
					if ((i == 0) && (j == 0))
					{
						cost[k] = dist(A[0], B[0]);
						min_cost = cost[k];
						continue;
					}

					if ((j - 1 < 0) || (k - 1 < 0)) y = double.PositiveInfinity;
					else y = cost[k - 1];
					if ((i - 1 < 0) || (k + 1 > 2*r)) x = double.PositiveInfinity;
					else x = cost_prev[k + 1];
					if ((i - 1 < 0) || (j - 1 < 0)) z = double.PositiveInfinity;
					else z = cost_prev[k];

					// Classic DTW calculation
					cost[k] = min(min(x, y), z) + dist(A[i], B[j]);

					// Find minimum cost in row for early abandoning (possibly to use column instead of row).
					if (cost[k] < min_cost)
					{
						min_cost = cost[k];
					}
				}

				// We can abandon early if the current cummulative distace with lower bound together are larger than bsf
				if (i + r < m - 1 && min_cost + cb[i + r + 1] >= bsf)
				{
					return min_cost + cb[i + r + 1];
				}

				// Move current array to previous array.
				cost_tmp = cost;
				cost = cost_prev;
				cost_prev = cost_tmp;
			}
			k--;

			// the DTW distance is in the last cell in the matrix of size O(m^2) or at the middle of our array.
			double final_dtw = cost_prev[k];

			return final_dtw;
		}

		/// Print function for debugging
		private void printArray(double[] x, int len)
		{
			for (int i = 0; i < len; i++)
				Console.Write(" {0:N2}", x[i]);
			Console.WriteLine(Environment.NewLine);
		}

		// If expected error happens, teminated the program.
		private void error_dtw(int id)
		{
			if (id == 1)
				Console.WriteLine("ERROR : Memory can't be allocated!!!\n");
			else if (id == 2)
				Console.WriteLine("ERROR : File not Found!!!\n");
			else if (id == 3)
				Console.WriteLine("ERROR : Can't create Output File!!!\n");
			else if (id == 4)
			{
				Console.WriteLine("ERROR : Invalid Number of Arguments!!!");
				Console.WriteLine("Command Usage:  UCR_DTW.exe  data-file  query-file   m   R\n");
				Console.WriteLine("For example  :  UCR_DTW.exe  data.txt   query.txt   128  0.05");
			}
			Environment.Exit(1);
		}
		#endregion

		/// <summary>
		/// Original Main method that uses files
		/// </summary>
		/// <param name="inputFileName">input file with time series data</param>
		/// <param name="queryFileName">input file with time series data</param>
		/// <param name="queryLength">length of query (e.g. when to stop reading from file)</param>
		/// <param name="wrappingWindow">Normally between 0 - 15</param>
		/// <remarks>
		/// How does changing the width of the warping effect the speed-up?
		/// In brief, it makes very little difference.
		/// Over the range of 0 to 15, which would include the best accuracy setting
		/// for the vast majority of the UCR archive problems,
		/// the difference is bearly perceptable
		/// </remarks>
		public static void DTW(string inputFileName, string queryFileName, int queryLength, double wrappingWindow = 0)
		{
			FileStream fp = null; //Data File Pointer;
			FileStream qp = null; //Query File Pointer
			double bsf; /// best-so-far
			double[] t, q; /// data array and query array
			long[] order; ///new order of the query
			double[] u, l, qo, uo, lo, tz, cb, cb1, cb2, u_d, l_d;
			double d;
			int i, j;
			double ex, ex2, mean, std;
			int m = -1, r = -1;
			long loc = 0;
			double t1, t2;
			int kim = 0, keogh = 0, keogh2 = 0;
			double dist = 0, lb_kim = 0, lb_k = 0, lb_k2 = 0;
			double[] buffer, u_buff, l_buff;
			IndexValue[] Q_tmp;

			// For every EPOCH points, all cummulative values, such as ex (sum), ex2 (sum square), will be restarted for reducing the doubleing point error.
			int EPOCH = 100000;

			// read size of the query
			m = queryLength;

			// read warping windows
			if (wrappingWindow >= 0)
			{
				double R = wrappingWindow;
				if (R <= 1)
					r = (int) Math.Floor(R*m);
				else
					r = (int) Math.Floor(R);
			}

			// start the clock
			t1 = DateTime.Now.Ticks;

			// malloc everything here
			q = new double[m];
			qo = new double[m];
			uo = new double[m];
			lo = new double[m];
			order = new long[m];
			Q_tmp = new IndexValue[m];
			u = new double[m];
			l = new double[m];
			cb = new double[m];
			cb1 = new double[m];
			cb2 = new double[m];
			u_d = new double[m];
			l_d = new double[m];
			t = new double[m*2];
			tz = new double[m];
			buffer = new double[EPOCH];
			u_buff = new double[EPOCH];
			l_buff = new double[EPOCH];

			// Read query file
			bsf = double.PositiveInfinity;
			i = 0;
			j = 0;
			ex = ex2 = 0;

			using (qp = File.OpenRead(queryFileName))
			{

				using (TextReader reader = new StreamReader(qp))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						string[] strs = line.Split(' ');

						for (int itt = 0; itt < strs.Length && i < m; itt++)
						{
							if (String.IsNullOrEmpty(strs[itt]))
								continue;
							d = double.Parse(strs[itt]);
							ex += d;
							ex2 += d*d;
							q[i] = d;
							i++;
						}
					}
				}
			}

			// Do z-normalize the query, keep in same array, q
			mean = ex/m;
			std = ex2/m;
			std = (double)Math.Sqrt(std - mean*mean);
			for (i = 0; i < m; i++)
				q[i] = (q[i] - mean)/std;

			// Create envelop of the query: lower envelop, l, and upper envelop, u
			lower_upper_lemire(q, m, r, l, u);

			// Sort the query one time by abs(z-norm(q[i]))
			for (i = 0; i < m; i++)
			{
				Q_tmp[i] = new IndexValue {Index = i, Value = q[i]};
				// Q_tmp[i].Value = q[i];
				// Q_tmp[i].Index = i;
			}
			
			Array.Sort(Q_tmp,Comp);
			
			// also create another arrays for keeping sorted envelop
			for (i = 0; i < m; i++)
			{
				long o = Q_tmp[i].Index;
				order[i] = o;
				qo[i] = q[o];
				uo[i] = u[o];
				lo[i] = l[o];
			}

			// Initial the cummulative lower bound
			for (i = 0; i < m; i++)
			{
				cb[i] = 0;
				cb1[i] = 0;
				cb2[i] = 0;
			}

			i = 0; /// current index of the data in current chunk of size EPOCH
			j = 0; /// the starting index of the data in the circular array, t
			ex = ex2 = 0;
			bool done = false;
			int it = 0, ep = 0, k = 0;
			int I; /// the starting index of the data in current chunk of size EPOCH

			Queue<double> data = new Queue<double>();
			// double[] data = null;
			
			using (fp = File.OpenRead(inputFileName))
			{
				using (TextReader reader = new StreamReader(fp))
				{
					string str = reader.ReadToEnd();
					string[] strarr = str.Split(' ');
					
					foreach (var s in strarr)
					{
						if (string.IsNullOrEmpty(s))
							continue;
						data.Enqueue(double.Parse(s));
					}
				}
			}

			while (!done)
			{
				// Read first m-1 points
				ep = 0;
				if (it == 0)
				{
					for (k = 0; k < m - 1; k++)
					{
						if(data.Count > 0)
						{
							d = data.Dequeue();
							buffer[k] = d;
						}
					}
				}
				else
				{
					for (k = 0; k < m - 1; k++)
						buffer[k] = buffer[EPOCH - m + 1 + k];
				}

				// Read buffer of size EPOCH or when all data has been read.
				ep = m - 1;
				while (ep < EPOCH)
				{
					if (data.Count == 0)
						break;
					d = data.Dequeue();
					buffer[ep] = d;
					ep++;
				}

				// Data are read in chunk of size EPOCH.
				// When there is nothing to read, the loop is end.
				if (ep <= m - 1)
				{
					done = true;
				}
				else
				{
					lower_upper_lemire(buffer, ep, r, l_buff, u_buff);

					// Just for printing a dot for approximate a million point. Not much accurate.
					//if (it%(1000000/(EPOCH - m + 1)) == 0)
					//    fprintf(stderr, ".");

					/// Do main task here..
					ex = 0;
					ex2 = 0;
					for (i = 0; i < ep; i++)
					{
						// A bunch of data has been read and pick one of them at a time to use
						d = buffer[i];

						// Calcualte sum and sum square
						ex += d;
						ex2 += d*d;

						// t is a circular array for keeping current data
						t[i%m] = d;

						// double the size for avoiding using modulo "%" operator
						t[(i%m) + m] = d;

						// Start the task when there are more than m-1 points in the current chunk
						if (i >= m - 1)
						{
							mean = ex/m;
							std = ex2/m;
							std = (double)Math.Sqrt(std - mean*mean);

							// compute the start location of the data in the current circular array, t
							j = (i + 1)%m;
							// the start location of the data in the current chunk
							I = i - (m - 1);

							// Use a constant lower bound to prune the obvious subsequence
							lb_kim = lb_kim_hierarchy(t, q, j, m, mean, std, bsf);

							if (lb_kim < bsf)
							{
								// Use a linear time lower bound to prune; z_normalization of t will be computed on the fly.
								// uo, lo are envelop of the query.
								lb_k = lb_keogh_cumulative(order, t, uo, lo, cb1, j, m, mean, std, bsf);
								if (lb_k < bsf)
								{
									// Take another linear time to compute z_normalization of t.
									// Note that for better optimization, this can merge to the previous function.
									for (k = 0; k < m; k++)
									{
										tz[k] = (t[(k + j)] - mean)/std;
									}

									// Use another lb_keogh to prune
									// qo is the sorted query. tz is unsorted z_normalized data.
									// l_buff, u_buff are big envelop for all data in this chunk

									//ArraySegment<double> l_buff_partial = new ArraySegment<double>(l_buff, 0, I);
									//ArraySegment<double> u_buff_partial = new ArraySegment<double>(u_buff, 0, I);
									
									lb_k2 = lb_keogh_data_cumulative(order, tz, qo, cb2,l_buff,
									                                 u_buff, I, m, mean,
									                                 std, bsf);
									
									if (lb_k2 < bsf)
									{
										// Choose better lower bound between lb_keogh and lb_keogh2 to be used in early abandoning DTW
										// Note that cb and cb2 will be cumulative summed here.
										if (lb_k > lb_k2)
										{
											cb[m - 1] = cb1[m - 1];
											for (k = m - 2; k >= 0; k--)
												cb[k] = cb[k + 1] + cb1[k];
										}
										else
										{
											cb[m - 1] = cb2[m - 1];
											for (k = m - 2; k >= 0; k--)
												cb[k] = cb[k + 1] + cb2[k];
										}

										// Compute DTW and early abandoning if possible
										dist = dtw(tz, q, cb, m, r, bsf);

										if (dist < bsf)
										{
											// Update bsf
											// loc is the real starting location of the nearest neighbor in the file
											bsf = dist;
											loc = (it)*(EPOCH - m + 1) + i - m + 1;
										}
									}
									else
										keogh2++;
								}
								else
									keogh++;
							}
							else
								kim++;

							// Reduce obsolute points from sum and sum square
							ex -= t[j];
							ex2 -= t[j]*t[j];
						}
					}

					// If the size of last chunk is less then EPOCH, then no more data and terminate.
					if (ep < EPOCH)
						done = true;
					else
						it++;
				}
			}

			i = (it)*(EPOCH - m + 1) + ep;

			t2 = DateTime.Now.Ticks;
			Console.WriteLine();

			// Note that loc and i are long long.
			Console.WriteLine("Location : " + loc);
			Console.WriteLine("Distance : " + Math.Sqrt(bsf));
			Console.WriteLine("Data Scanned : " + i);
			Console.WriteLine("Total Execution Time : " + (t2 - t1)/TimeSpan.TicksPerSecond + " sec");

			// printf is just easier for formating ;)
			Console.WriteLine();
			Console.WriteLine("Pruned by LB_Kim    : {0:P2}", ((double) kim/i));
			Console.WriteLine("Pruned by LB_Keogh  : {0:P2}", ((double) keogh/i));
			Console.WriteLine("Pruned by LB_Keogh2 : {0:P2}", ((double) keogh2/i));
			Console.WriteLine("DTW Calculation     : {0:P2}", 1 - (((double) kim + keogh + keogh2)/i));
		}

		/// <summary>
		/// Perform a DTW and return the distance
		/// </summary>
		/// <param name="inputArray">input array</param>
		/// <param name="queryArray">query array</param>
		/// <param name="wrappingWindow">Normally between 0 - 15</param>
		/// <remarks>
		/// How does changing the width of the warping effect the speed-up?
		/// In brief, it makes very little difference.
		/// Over the range of 0 to 15, which would include the best accuracy setting
		/// for the vast majority of the UCR archive problems,
		/// the difference is bearly perceptable
		/// </remarks>
		/// <returns>The distance between the arrays</returns>
		public static double DTW(double[] inputArray, double[] queryArray, double wrappingWindow = 0)
		{
			double bsf; /// best-so-far
			double[] t, q; /// data array and query array
			long[] order; ///new order of the query
			double[] u, l, qo, uo, lo, tz, cb, cb1, cb2, u_d, l_d;
			double d;
			int i, j;
			double ex, ex2, mean, std;
			int m = -1, r = -1;
			long loc = 0;
			double t1;
			int kim = 0, keogh = 0, keogh2 = 0;
			double dist = 0, lb_kim = 0, lb_k = 0, lb_k2 = 0;
			double[] buffer, u_buff, l_buff;
			IndexValue[] Q_tmp;

			// For every EPOCH points, all cummulative values, such as ex (sum), ex2 (sum square), will be restarted for reducing the doubleing point error.
			int EPOCH = 100000;

			// read size of the query
			m = queryArray.Length;

			// read warping windows
			if (wrappingWindow >= 0)
			{
				double R = wrappingWindow;
				if (R <= 1)
					r = (int) Math.Floor(R*m);
				else
					r = (int) Math.Floor(R);
			}

			// start the clock
			t1 = DateTime.Now.Ticks;

			// malloc everything here
			q = new double[m];
			qo = new double[m];
			uo = new double[m];
			lo = new double[m];
			order = new long[m];
			Q_tmp = new IndexValue[m];
			u = new double[m];
			l = new double[m];
			cb = new double[m];
			cb1 = new double[m];
			cb2 = new double[m];
			u_d = new double[m];
			l_d = new double[m];
			t = new double[m*2];
			tz = new double[m];
			buffer = new double[EPOCH];
			u_buff = new double[EPOCH];
			l_buff = new double[EPOCH];

			// Read query file
			bsf = double.PositiveInfinity;
			i = 0;
			j = 0;
			ex = ex2 = 0;

			for (int itt = 0; itt < queryArray.Length; itt++)
			{
				d = queryArray[itt];
				ex += d;
				ex2 += d*d;
				q[i] = d;
				i++;
			}

			// Do z-normalize the query, keep in same array, q
			mean = ex/m;
			std = ex2/m;
			std = (double)Math.Sqrt(std - mean*mean);
			for (i = 0; i < m; i++)
				q[i] = (q[i] - mean)/std;

			// Create envelop of the query: lower envelop, l, and upper envelop, u
			lower_upper_lemire(q, m, r, l, u);

			// Sort the query one time by abs(z-norm(q[i]))
			for (i = 0; i < m; i++)
			{
				Q_tmp[i] = new IndexValue {Index = i, Value = q[i]};
				// Q_tmp[i].Value = q[i];
				// Q_tmp[i].Index = i;
			}
			
			Array.Sort(Q_tmp,Comp);
			
			// also create another arrays for keeping sorted envelop
			for (i = 0; i < m; i++)
			{
				long o = Q_tmp[i].Index;
				order[i] = o;
				qo[i] = q[o];
				uo[i] = u[o];
				lo[i] = l[o];
			}

			// Initial the cummulative lower bound
			for (i = 0; i < m; i++)
			{
				cb[i] = 0;
				cb1[i] = 0;
				cb2[i] = 0;
			}

			i = 0; /// current index of the data in current chunk of size EPOCH
			j = 0; /// the starting index of the data in the circular array, t
			ex = ex2 = 0;
			bool done = false;
			int it = 0, ep = 0, k = 0;
			int I; /// the starting index of the data in current chunk of size EPOCH

			Queue<double> data = new Queue<double>();
			// double[] data = null;
			
			for (int idata = 0; idata < inputArray.Length; idata++)
			{
				data.Enqueue(inputArray[idata]);
			}
			
			while (!done)
			{
				// Read first m-1 points
				ep = 0;
				if (it == 0)
				{
					for (k = 0; k < m - 1; k++)
					{
						if(data.Count > 0)
						{
							d = data.Dequeue();
							buffer[k] = d;
						}
					}
				}
				else
				{
					for (k = 0; k < m - 1; k++)
						buffer[k] = buffer[EPOCH - m + 1 + k];
				}

				// Read buffer of size EPOCH or when all data has been read.
				ep = m - 1;
				while (ep < EPOCH)
				{
					if (data.Count == 0)
						break;
					d = data.Dequeue();
					buffer[ep] = d;
					ep++;
				}

				// Data are read in chunk of size EPOCH.
				// When there is nothing to read, the loop is end.
				if (ep <= m - 1)
				{
					done = true;
				}
				else
				{
					lower_upper_lemire(buffer, ep, r, l_buff, u_buff);

					// Just for printing a dot for approximate a million point. Not much accurate.
					//if (it%(1000000/(EPOCH - m + 1)) == 0)
					//    fprintf(stderr, ".");

					/// Do main task here..
					ex = 0;
					ex2 = 0;
					for (i = 0; i < ep; i++)
					{
						// A bunch of data has been read and pick one of them at a time to use
						d = buffer[i];

						// Calcualte sum and sum square
						ex += d;
						ex2 += d*d;

						// t is a circular array for keeping current data
						t[i%m] = d;

						// double the size for avoiding using modulo "%" operator
						t[(i%m) + m] = d;

						// Start the task when there are more than m-1 points in the current chunk
						if (i >= m - 1)
						{
							mean = ex/m;
							std = ex2/m;
							std = (double)Math.Sqrt(std - mean*mean);

							// compute the start location of the data in the current circular array, t
							j = (i + 1)%m;
							// the start location of the data in the current chunk
							I = i - (m - 1);

							// Use a constant lower bound to prune the obvious subsequence
							lb_kim = lb_kim_hierarchy(t, q, j, m, mean, std, bsf);

							if (lb_kim < bsf)
							{
								// Use a linear time lower bound to prune; z_normalization of t will be computed on the fly.
								// uo, lo are envelop of the query.
								lb_k = lb_keogh_cumulative(order, t, uo, lo, cb1, j, m, mean, std, bsf);
								if (lb_k < bsf)
								{
									// Take another linear time to compute z_normalization of t.
									// Note that for better optimization, this can merge to the previous function.
									for (k = 0; k < m; k++)
									{
										tz[k] = (t[(k + j)] - mean)/std;
									}

									// Use another lb_keogh to prune
									// qo is the sorted query. tz is unsorted z_normalized data.
									// l_buff, u_buff are big envelop for all data in this chunk

									//ArraySegment<double> l_buff_partial = new ArraySegment<double>(l_buff, 0, I);
									//ArraySegment<double> u_buff_partial = new ArraySegment<double>(u_buff, 0, I);
									
									lb_k2 = lb_keogh_data_cumulative(order, tz, qo, cb2,l_buff,
									                                 u_buff, I, m, mean,
									                                 std, bsf);
									
									if (lb_k2 < bsf)
									{
										// Choose better lower bound between lb_keogh and lb_keogh2 to be used in early abandoning DTW
										// Note that cb and cb2 will be cumulative summed here.
										if (lb_k > lb_k2)
										{
											cb[m - 1] = cb1[m - 1];
											for (k = m - 2; k >= 0; k--)
												cb[k] = cb[k + 1] + cb1[k];
										}
										else
										{
											cb[m - 1] = cb2[m - 1];
											for (k = m - 2; k >= 0; k--)
												cb[k] = cb[k + 1] + cb2[k];
										}

										// Compute DTW and early abandoning if possible
										dist = dtw(tz, q, cb, m, r, bsf);

										if (dist < bsf)
										{
											// Update bsf
											// loc is the real starting location of the nearest neighbor in the file
											bsf = dist;
											loc = (it)*(EPOCH - m + 1) + i - m + 1;
										}
									}
									else
										keogh2++;
								}
								else
									keogh++;
							}
							else
								kim++;

							// Reduce obsolute points from sum and sum square
							ex -= t[j];
							ex2 -= t[j]*t[j];
						}
					}

					// If the size of last chunk is less then EPOCH, then no more data and terminate.
					if (ep < EPOCH)
						done = true;
					else
						it++;
				}
			}

			i = (it)*(EPOCH - m + 1) + ep;

			/*
			#if DEBUG
			t2 = DateTime.Now.Ticks;
			Console.WriteLine();

			// Note that loc and i are long long.
			Console.WriteLine("Location : " + loc);
			Console.WriteLine("Distance : " + Math.Sqrt(bsf));
			Console.WriteLine("Data Scanned : " + i);
			Console.WriteLine("Total Execution Time : " + (t2 - t1)/TimeSpan.TicksPerSecond + " sec");

			// printf is just easier for formating ;)
			Console.WriteLine();
			Console.WriteLine("Pruned by LB_Kim    : {0:P2}", ((double) kim/i));
			Console.WriteLine("Pruned by LB_Keogh  : {0:P2}", ((double) keogh/i));
			Console.WriteLine("Pruned by LB_Keogh2 : {0:P2}", ((double) keogh2/i));
			Console.WriteLine("DTW Calculation     : {0:P2}", 1 - (((double) kim + keogh + keogh2)/i));
			#endif
			 */
			
			return Math.Sqrt(bsf);
		}
	}
}