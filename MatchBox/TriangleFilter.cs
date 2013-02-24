using System;
using System.Globalization;
using System.IO;

//Copyright (c) 2011 Sebastian Böhm sebastian@sometimesfood.org
//                   Heinrich Fink hf@hfink.eu
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

namespace MatchBox
{
	/**
	 * A triangle-shaped filter that can be applied on an array of floats. This
	 * filter is windows the array of floats with the triangle and returns the
	 * sum of the windowed data, i.e. its the dot-product between the triangle
	 * and the data to be filtered. This class operators on array indices, i.e.
	 * a left and right border is defined by indices. This is useful when e.g.
	 * applying this filter on FFT bins. The filter uses optimized vectorized
	 * versions for filter creaetion and execution.
	 */
	public class TriangleFilter
	{
		private static NumberFormatInfo numberFormat = new CultureInfo("en-US", false).NumberFormat;
		
		private readonly int left_edge_;
		private readonly int right_edge_;
		private readonly float height_;
		private int size_;

		private float[] filter_data_;
		
		public int LeftEdge {
			get {
				return left_edge_;
			}
		}
		
		public int RightEdge {
			get {
				return right_edge_;
			}
		}

		public float Height {
			get {
				return height_;
			}
		}

		public int Size {
			get {
				return size_;
			}
		}

		public float[] FilterData {
			get {
				return filter_data_;
			}
		}
		
		/**
		 * Creates a new TriangleFilter.
		 * @param An index defining the left edge of the triangle. (including)
		 * @param An index defining the right edge of the triangle. (including)
		 * @param The height of the triangle.
		 *
		 * Note that the array passed to TriangleFilter::apply is required to be
		 * valid for index range defined by this constructor, i.e. it has to
		 * accomodate at least right_edge + 1 number of float values.
		 */
		public TriangleFilter(int left_edge, int right_edge, float height)
		{
			left_edge_ = left_edge;
			right_edge_ = right_edge;
			height_ = height;
			size_ = right_edge - left_edge + 1;
			filter_data_ = new float[size_];
			
			if ((left_edge >= right_edge) || (left_edge < 0) || (right_edge < 0))
			{
				throw new Exception(String.Format("TriangleFilter: edge values are invalid: left_edge = '{0}' right_edge = '{1}'.",left_edge, right_edge));
			}
			
			if (height == 0)
			{
				throw new Exception("Invalid height input: height == 0.");
			}
			
			int center = (int)((left_edge + right_edge) * 0.5f + 0.5f);
			
			// left rising part with positive slope, without setting center
			int left_side_length = (center - left_edge);
			float left_dx = height / left_side_length;
			float zero = 0;
			
			//vDSP_vramp(zero, left_dx, filter_data_.get(), 1, left_side_length);
			// vDSP_vramp = Cnk = a + n * b  (n = 0 -> N-1)
			for (int i = 0; i < left_side_length; i++) {
				filter_data_[i] = zero + i * left_dx;
			}
			
			// right falling part with negative slope, also setting center
			int right_side_length = right_edge - center;
			float right_dx = - height / right_side_length;
			
			//vDSP_vramp(height, right_dx, filter_data_.get()[size_-right_side_length-1], 1, right_side_length+1);
			// vDSP_vramp = Cnk = a + n * b  (n = 0 -> N-1)
			for (int i = 0; i < right_side_length+1; i++) {
				double val = height + i * right_dx;
				val = Math.Round(val, 6);
				filter_data_[size_-right_side_length-1 + i] = (float) val;
			}
		}

		/**
		 * Applies the filter to a float buffer.
		 * Note that the array passed to TriangleFilter::apply is required to be
		 * valid for index range defined by this constructor, i.e. it has to
		 * accomodate at least right_edge + 1 number of float values.
		 *
		 * @param buffer The array to be filter by this TriangleFilter.
		 * @return The result of the filtering operation, i.e. the dot-product
		 * between the triangle shape and the elements of the buffer as defined
		 * by the triangles left and right edge indices.
		 */
		public float Apply(float[] buffer)
		{
			//we can simply apply the filter as the dot product with the sample buffer
			//within its range
			
			//vDSP_dotpr(buffer[left_edge_], 1, filter_data_.get(), 1, result, size_);
			float result = 0;
			for(int i = 0; i < size_; i++)
			{
				result += buffer[left_edge_ + i] * filter_data_[i];
			}
			
			return result;
		}

		public override string ToString()
		{
			StringWriter writer = new StringWriter();
			Print(writer, this);
			writer.Close();
			return writer.ToString();
		}

		/**
		 * Used for debugging purposes.
		 */
		public static void Print(TextWriter @out, TriangleFilter f)
		{
			for (int i = 0; i < f.left_edge_; ++i)
			{
				if (i != 0)
					@out.Write(", ");
				@out.Write("0");
			}

			for (int i = 0; i < f.size_; ++i)
			{
				//@out.Write(", " + f.filter_data_[i].ToString("0.000", CultureInfo.InvariantCulture));
				@out.Write(", " + f.filter_data_[i].ToString(numberFormat));
			}
		}
	}
}

