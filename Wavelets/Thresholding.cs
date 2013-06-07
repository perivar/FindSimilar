using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtils;

namespace Wavelets
{
	/// <summary>
	/// Description of Thresholding.
	/// </summary>
	public static class Thresholding
	{
		#region ThrowAway by Emil Mikulic
		public const int SIZE = 256;
		public const int MAX_ITER = 250;
		
		/// <summary>
		/// Returns the percentage of coefficients under <amount>
		/// </summary>
		/// <param name="data">coefficients</param>
		/// <param name="amount">threshold amount</param>
		/// <returns>the percentage of coefficients under amount</returns>
		/// <remarks>
		/// Copyright (c) 2003 Emil Mikulic.
		/// http://dmr.ath.cx/
		/// </remarks>
		public static double PercentUnder(double[][] data, double amount)
		{
			int num_thrown = 0;
			int x;
			int y;

			for (y = 0; y < SIZE; y++)
				for (x = 0; x < SIZE; x++)
					if (Math.Abs(data[y][x]) <= amount)
						num_thrown++;

			return (double)(100 * num_thrown) / (double)(SIZE * SIZE);
		}

		/// <summary>
		/// Throw away weakest <percentage>% of coefficients
		/// </summary>
		/// <param name="data">coefficients</param>
		/// <param name="percentage">how many percent to throw away</param>
		/// <remarks>
		/// Copyright (c) 2003 Emil Mikulic.
		/// http://dmr.ath.cx/
		/// </remarks>
		public static void ThrowAway(double[][] data, double percentage)
		{
			double low;
			double high;
			double thresh = 0;
			double loss;
			int i;
			int j;

			// find max
			low = high = 0.0;
			for (j =0; j < SIZE; j++)
				for (i =0; i < SIZE; i++)
					if (Math.Abs(data[j][i]) > high)
						high = Math.Abs(data[j][i]);

			// binary search
			for (i = 0; i < MAX_ITER; i++)
			{
				thresh = (low+high)/2.0;
				loss = PercentUnder(data, thresh);

				Console.Write("binary search: " + "iteration={0,4:D}, thresh={1,4:f}, loss={2,3:f2}%\r", i+1, thresh, loss);
				
				if (loss < percentage) {
					low = thresh;
				} else {
					high = thresh;
				}

				if (Math.Abs(loss - percentage) < 0.01)
					i = MAX_ITER;
				if (Math.Abs(low - high) < 0.0000001)
					i = MAX_ITER;
			}

			// zero out anything too low
			for (j = 0; j < SIZE; j++)
				for (i = 0; i < SIZE; i++)
					if (Math.Abs(data[j][i]) < thresh)
						data[j][i] = 0.0;

		}
		#endregion

		// Keep only the s largest coefficients in each column of X
		public static double[][] perform_strict_thresholding(double[][] x, int s) {
			// Copyright (c) 2006 Gabriel Peyre
			// v = sort(abs(x)); v = v(end:-1:1,:);
			// v = v(round(s),:);
			// v = repmat(v, [size(x,1) 1]);
			// X = X .* (abs(x)>=v);

			int rowCount = x.Length;
			int columnCount = x[0].Length;
			double[][] y = new double[rowCount][];
			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				double[] row = x[rowIndex];
				
				// Find top S coefficients-indexes by doing an absolute value sort
				int[] topIndexes = (from t in row.Select((n, i) => new { Index = i, Value = n })
				                    orderby Math.Abs(t.Value) descending
				                    select t.Index).Take(s).ToArray();

				// Empty all values except the top S coefficients
				y[rowIndex] = new double[row.Length];
				for (int i = 0; i < row.Length; i++) {
					if (topIndexes.Contains(i)) {
						y[rowIndex][i] = row[i];
					} else {
						y[rowIndex][i] = 0;
					}
				}
			}
			
			return y;
		}

		// Hard thresholding sets any coefficient less than or equal to the threshold to zero.
		public static double[][] perform_hard_thresholding(double[][] x, double thresh) {
			// Hard thresholding can be described as the usual process of setting to zero the elements whose absolute values are lower than the threshold.
			// The hard threshold signal is x if |x| > t, and is 0 if |x| <= t.

			// http://sfb649.wiwi.hu-berlin.de/fedc_homepage/xplore/tutorials/xlghtmlnode93.html
			// Copyright (c) 2006 Gabriel Peyre
			// t = t(1);
			// y = x .* (abs(x) > t);
			
			double[][] y = x.DeepCopy();
			
			for (int i = 0; i < y.Length; i++) {
				for (int j = 0; j < y[i].Length; j++) {
					if (Math.Abs(y[i][j]) <= thresh) {
						y[i][j] = 0.0;
					}
				}
			}
			return y;
		}

		// Soft thresholding sets any coefficient less than or equal to the threshold to zero.
		// The threshold is subtracted from any coefficient that is greater than the threshold.
		// This moves the time series toward zero.
		public static double[][] perform_soft_thresholding(double[][] x, double thresh) {
			// Soft thresholding is an extension of hard thresholding, first setting to zero the elements whose absolute values are lower than the threshold,
			// and then shrinking the nonzero coefficients towards 0
			// The soft threshold signal is sign(x)(|x| - t) if |x| > t and is 0 if |x| <= t.

			// http://sfb649.wiwi.hu-berlin.de/fedc_homepage/xplore/tutorials/xlghtmlnode93.html
			// Copyright (c) 2006 Gabriel Peyre
			// t = t(1);
			// s = abs(x) - t;
			// s = (s + abs(s))/2;
			// y = sign(x) .* s;

			double[][] y = x.DeepCopy();
			
			for (int i = 0; i < y.Length; i++) {
				for (int j = 0; j < y[i].Length; j++) {
					if (Math.Abs(y[i][j]) <= thresh) {
						y[i][j] = 0.0;
					} else {
						if (y[i][j] > 0) {
							y[i][j] = y[i][j] - thresh;
						} else if (y[i][j] < 0) {
							y[i][j] = y[i][j] + thresh;
						} else {
							y[i][j] = 0;
						}
					}
				}
			}
			return y;
		}
		
		public static double[][] perform_semisoft_thresholding(double[][] x, double thresh1, double thresh2) {
			// Semi-soft thresholding is a family of non-linearities that interpolates between soft and hard thresholding.
			// It uses both a main threshold T and a secondary threshold T1=mu*T.
			// When mu=1, the semi-soft thresholding performs a hard thresholding,
			//vwhereas when mu=infinity, it performs a soft thresholding.
			
			// Copyright (c) 2006 Gabriel Peyre
			// if length(t)==1
			//     t = [t 2*t];
			// end
			// t = sort(t);
			// 
			// y = x;
			// y(abs(x) < t(1)) = 0;
			// I = find( abs(x) >= t(1) & abs(x) < t(2) );
			// y( I ) = sign(x(I)) .* t(2)/(t(2)-t(1)) .* (abs(x(I)) - t(1));
			
			double[][] y = x.DeepCopy();
			
			for (int i = 0; i < y.Length; i++) {
				for (int j = 0; j < y[i].Length; j++) {
					if (Math.Abs(y[i][j]) < thresh1) {
						y[i][j] = 0.0;
					}
				}
			}
			
			int rowCount = x.Length;
			int columnCount = x[0].Length;
			int[][] I = new int[rowCount][];
			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				double[] row = x[rowIndex];
				
				// find indices for those values that are between thresh1 and thresh2
				int[] indices = Enumerable.Range(0, row.Length)
					.Where(index => Math.Abs(row[index]) >= thresh1 && Math.Abs(row[index]) < thresh2 )
					.ToArray();
				
				I[rowIndex] = indices;
			}

			double threshold = thresh2/(thresh2-thresh1);
			
			for (int rowIndex = 0; rowIndex < I.Length; rowIndex++) {
				for (int columnIndex = 0; columnIndex < I[rowIndex].Length; columnIndex++) {
					int index = I[rowIndex][columnIndex];
					
					double xIndexValue = x[rowIndex][index];
					double fraction = (Math.Abs(xIndexValue) - thresh1);
					double interpolatedValue = threshold * fraction;

					if (xIndexValue > 0) {
						y[rowIndex][index] = interpolatedValue;
					} else if (xIndexValue < 0) {
						y[rowIndex][index] = interpolatedValue * -1;
					} else {
						y[rowIndex][index] = 0;
					}
				}
			}
			
			return y;
		}
		
		public static void RunTests() {
			// Run the following matlab test:
			// T = 1; % threshold value
			// v = linspace(-5,5,1024);
			// clf;
			// hold('on');
			// plot(v, perform_thresholding(v,T,'hard'), 'b--');
			// plot(v, perform_thresholding(v,T,'soft'), 'r--');
			// plot(v, perform_thresholding(v,[T 2*T],'semisoft'), 'g');
			// plot(v, perform_thresholding(v,[T 4*T],'semisoft'), 'g:');
			// plot(v, perform_thresholding(v',400,'strict'), 'r:');
			// legend('hard', 'soft', 'semisoft, \mu=2', 'semisoft, \mu=4', 'strict, 400');
			// hold('off');
			
			// linspace in c#
			double start = -5;
			double end = 5;
			double totalCount = 1024;
			
			double[][] v = new double[1][];
			v[0] = new double[(int) totalCount];
			
			int count = 0;
			for(double i = start; i < end; i += (end-start)/totalCount) {
				v[0][count] = i;
				count++;
			}
			
			// perform thresholding and plot
			int T = 1;
			
			double[][] hard = perform_hard_thresholding(v, T);
			Comirva.Audio.Util.Maths.Matrix mHard = new Comirva.Audio.Util.Maths.Matrix(hard);
			mHard.DrawMatrixGraph("thresholding-hard.png", false);

			double[][] soft = perform_soft_thresholding(v, T);
			Comirva.Audio.Util.Maths.Matrix mSoft = new Comirva.Audio.Util.Maths.Matrix(soft);
			mSoft.DrawMatrixGraph("thresholding-soft.png", false);

			double[][] semisoft1 = perform_semisoft_thresholding(v, T, 2*T);
			Comirva.Audio.Util.Maths.Matrix mSemiSoft1 = new Comirva.Audio.Util.Maths.Matrix(semisoft1);
			mSemiSoft1.DrawMatrixGraph("thresholding-semisoft1.png", false);

			double[][] semisoft2 = perform_semisoft_thresholding(v, T, 4*T);
			Comirva.Audio.Util.Maths.Matrix mSemiSoft2 = new Comirva.Audio.Util.Maths.Matrix(semisoft2);
			mSemiSoft2.DrawMatrixGraph("thresholding-semisoft2.png", false);
			
			double[][] strict = perform_strict_thresholding(v, 400);
			Comirva.Audio.Util.Maths.Matrix mStrict = new Comirva.Audio.Util.Maths.Matrix(strict);
			mStrict.DrawMatrixGraph("thresholding-strict.png", false);
		}
	}
}
