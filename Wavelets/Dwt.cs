using System;
using System.Collections.Generic;
using System.Linq;

using Comirva.Audio.Util.Maths;

namespace Wavelets
{
	// Discreete Haar Wavelet Transform
	// http://dfyz-stuff.googlecode.com/svn-history/r47/trunk/ImageCompression/Task1/Dwt.cs
	public class Dwt
	{
		public Dwt(int steps)
		{
			this.steps = steps;
			this.padding = Math.Max(analysisLowPass.Length, analysisHighPass.Length);
		}

		public IEnumerable<Matrix> Fwd(IEnumerable<Matrix> input)
		{
			return input.Select(Transform);
		}

		public IEnumerable<Matrix> Back(IEnumerable<Matrix> output)
		{
			return output.Select(TransformBack);
		}

		public Matrix Transform(Matrix m)
		{
			var cols = m.Columns;
			var rows = m.Rows;
			var res = m.Copy();
			var stepsLeft = steps;
			while (stepsLeft-- > 0)
			{
				for (var r = 0; r < rows; r++)
				{
					var tempIn = new double[cols + padding];
					for (var c = 0; c < cols; c++)
						tempIn[c] = res.MatrixData[r][c];
					var tempOut = TransformStep(tempIn, cols);
					for (var c = 0; c < cols; c++)
						res.MatrixData[r][c] = tempOut[c];
				}
				for (var c = 0; c < cols; c++)
				{
					var tempIn = new double[rows + padding];
					for (var r = 0; r < rows; r++)
						tempIn[r] = res.MatrixData[r][c];
					var tempOut = TransformStep(tempIn, rows);
					for (var r = 0; r < rows; r++)
						res.MatrixData[r][c] = tempOut[r];
				}
				cols /= 2;
				rows /= 2;
			}
			return res;
		}

		public Matrix TransformBack(Matrix m)
		{
			var res = m.Copy();
			var sizes = new int[steps, 2];
			sizes[0, 0] = m.Rows;
			sizes[0, 1] = m.Columns;
			for (var i = 1; i < steps; i++)
			{
				sizes[i, 0] = sizes[i - 1, 0]/2;
				sizes[i, 1] = sizes[i - 1, 1]/2;
			}
			var stepsLeft = steps;
			while (stepsLeft-- > 0)
			{
				var rows = sizes[stepsLeft, 0];
				var cols = sizes[stepsLeft, 1];
				for (var c = 0; c < cols; c++)
				{
					var tempIn = new double[rows];
					for (var r = 0; r < rows; r++)
						tempIn[r] = res.MatrixData[r][c];
					var tempOut = TransformBackStep(tempIn);
					for (var r = 0; r < rows; r++)
						res.MatrixData[r][c] = tempOut[r];
				}
				for (var r = 0; r < rows; r++)
				{
					var tempIn = new double[cols];
					for (var c = 0; c < cols; c++)
						tempIn[c] = res.MatrixData[r][c];
					var tempOut = TransformBackStep(tempIn);
					for (var c = 0; c < cols; c++)
						res.MatrixData[r][c] = tempOut[c];
				}
			}
			return res;
		}

		double[] TransformStep(double[] input, int signalSize)
		{
			ExtendPeriodically(input, signalSize);
			var halfSignalSize = signalSize/2;
			var res = new double[signalSize];
			for (var i = 0; i < halfSignalSize; i++)
			{
				for (var j = 0; j < analysisLowPass.Length; j++)
					res[i] += input[2*i + j]*GetElementReversed(analysisLowPass, j);
				for (var j = 0; j < analysisHighPass.Length; j++)
					res[i + halfSignalSize] += input[2*i + j]*GetElementReversed(analysisHighPass, j);
			}
			return res;
		}

		double[] TransformBackStep(double[] input)
		{
			var signalSize = input.Length;
			var doubleSignalSize = signalSize*2;
			var lowPart = new double[doubleSignalSize];
			for (var i = 0; i < signalSize/2; i++)
				lowPart[i*2] = input[i];
			ExtendPeriodically(lowPart, signalSize);
			var highPart = new double[doubleSignalSize];
			for (var i = 0; i < signalSize/2; i++)
				highPart[i*2] = input[i + signalSize/2];
			ExtendPeriodically(highPart, signalSize);

			var b = new double[signalSize];
			for (var i = 0; i < signalSize; i++)
			{
				for (var j = 0; j < analysisLowPass.Length; j++)
					b[i] += lowPart[i + j]*GetElementReversed(synthesisLowPass, j);
			}
			var c = new double[signalSize];
			for (var i = 0; i < signalSize; i++)
			{
				for (var j = 0; j < analysisLowPass.Length; j++)
					c[i] += highPart[i + j]*GetElementReversed(synthesisHighPass, j);
			}
			var res = new double[signalSize];
			for (var i = 0; i < signalSize; i++)
				res[i] = b[i] + c[i];
			return res;
		}

		static void ExtendPeriodically(double[] input, int signalSize)
		{
			for (var i = signalSize; i < input.Length; i++)
				input[i] = input[i - signalSize];
		}

		static double[] DivBySqrt2(IEnumerable<double> arr)
		{
			return arr.Select(x => x/Math.Sqrt(2)).ToArray();
		}

		static double GetElementReversed(double[] arr, int index)
		{
			return arr[arr.Length - 1 - index];
		}

		//readonly double[] analysisLowPass = new[] { 0.0267, -0.0168, -0.0782, 0.2668, 0.6029, 0.2668, -0.0782, -0.0168, 0.0267 };
		//readonly double[] analysisHighPass = new[] { 0.0912, -0.0575, -0.5912, 1.1150, -0.5912, -0.0575, 0.0912 };
		readonly double[] analysisLowPass = DivBySqrt2(new double[] { 1, 1 });
		readonly double[] analysisHighPass = DivBySqrt2(new double[] { -1, 1 });
		readonly double[] synthesisLowPass = DivBySqrt2(new double[] { 1, 1 });
		readonly double[] synthesisHighPass = DivBySqrt2(new double[] { 1, -1 });
		readonly int padding;
		readonly int steps;
	}
}