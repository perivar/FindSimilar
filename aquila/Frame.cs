using System;
using System.Collections;
using System.Collections.Generic;

/**
 * @file Frame.cpp
 *
 * Handling signal frames - implementation.
 *
 * The Frame class wraps a signal frame (short fragment of a signal).
 * Frame samples are accessed by STL-compatible iterators.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 0.2.2
 */
namespace Aquila
{
	/**
	 * An ecapsulation of a single frame of the signal.
	 */
	public class Frame : IEnumerable<short>
	{
		/**
		 * First and last sample of this frame in the data array/vector.
		 */
		private int _begin;
		private int _end;

		/**
		 * A const reference to signal source (audio channel).
		 */
		private readonly List<short> sourceChannel;
		
		/**
		 * Creates the frame object - sets signal source and frame boundaries.
		 *
		 * Frame should not change original data, so the source is a const
		 * reference.
		 *
		 * @param source const reference to signal source
		 * @param indexBegin position of first sample of this frame in the source
		 * @param indexEnd position of last sample of this frame in the source
		 */
		public Frame(List<short> source, int indexBegin, int indexEnd)
		{
			_begin = indexBegin;
			_end = indexEnd;
			sourceChannel = source;
		}
		
		/**
		 * Returns the frame length.
		 *
		 * @return frame length as a number of samples
		 */
		public int GetLength()
		{
			return _end - _begin;
		}

		/**
		 * Returns an iterator pointing to the first sample in the frame.
		 */
		public IEnumerator<short> GetEnumerator()
		{
			return GetFrameList().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		/// <summary>
		/// Returns frame
		/// </summary>
		/// <returns></returns>
		private List<short> GetFrameList() {
			return sourceChannel.GetRange(_begin, GetLength());
		}
	}
}
