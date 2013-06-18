using System.Collections.Generic;
using BlockError = math.transform.jwave.blocks.exc.BlockError;
using BlockException = math.transform.jwave.blocks.exc.BlockException;
using BlockFailure = math.transform.jwave.blocks.exc.BlockFailure;

namespace math.transform.jwave.blocks
{

	///
	// * A block that stores given data sparse to reduce memory costs.
	// * 
	// * @date 12.06.2011 23:45:37
	// * @author Christian Scheiblich
	// 
	public class BlockIndex : Block
	{
		//   * Array for storing the row indices of values.
		internal List<int> _arrI;

		//   * Array for storing the column indices of values.
		internal List<int> _arrJ;

		//   * Array for storing values.
		internal List<double> _arrVal;

		//   * Constructor taking position and dimension.
		//   * 
		//   * @date 12.06.2011 23:45:37
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#Block(int, int, int, int)
		public BlockIndex(int offSetRow, int offSetCol, int noOfRows, int noOfCols) : base(offSetRow, offSetCol, noOfRows, noOfCols)
		{
		}

		//   * Returns either 0 if there is no value stored for the pair (i,j) or the
		//   * value stored for the pair (i,j).
		//   * 
		//   * @date 12.06.2011 23:45:37
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#get(int, int)
		public override double @get(int i, int j)
		{
			if(!isMemAllocated())
				throw new BlockFailure("BlockIndex#get -- memory is not allocted");

			bool isOccupied = false;
			try
			{
				isOccupied = IsOccupied(i, j);
			}
			catch(BlockException blockException)
			{
				string exceptionMsg = blockException.getMessage();
				throw new BlockFailure("BlockIndex#get -- " + exceptionMsg);
			}

			double val = 0.0;

			if(isOccupied)
			{

				int pos = getOccupiedInternalArrayIndices(i, j);

				val = _arrVal[pos];

			}

			return val;
		}

		//   * Returns a complete matrix filled by zeros and the entries stored at pairs
		//   * (i,j).
		//   * 
		//   * @date 12.06.2011 23:45:37
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#get()
		public override double[][] @get()
		{
			//double[][] matrix = new double[_noOfRows][_noOfCols];
			double[][] matrix = CommonUtils.MathUtils.CreateJaggedArray<double[][]>(_noOfRows, _noOfCols);

			int noOf = _arrVal.Count;

			for(int p = 0; p < noOf; p++)
				matrix[_arrI[p]][_arrJ[p]] = _arrVal[p];

			return matrix;
		}

		//   * Add an entry to the array lists by extending those.
		//   * 
		//   * @date 12.06.2011 23:45:37
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#set(int, int, double)
		public override void @set(int i, int j, double val)
		{
			if(!isMemAllocated())
				throw new BlockFailure("BlockIndex#set -- memory is not allocted");

			bool isOccupied = false;
			try
			{
				isOccupied = IsOccupied(i, j);
			}
			catch(BlockException blockException)
			{
				string exceptionMsg = blockException.getMessage();
				throw new BlockFailure("BlockIndex#set -- " + exceptionMsg);
			}

			if(!isOccupied)
			{
				_arrI.Add(i);
				_arrJ.Add(j);
				_arrVal.Add(val);

			}
			else
			{
				int pos = getOccupiedInternalArrayIndices(i, j);
				_arrI[pos] = i;
				_arrJ[pos] = j;
				_arrVal[pos] = val;

			} // if

		}

		//   * Allocated memory for the index block as extendible array lists.
		//   * 
		//   * @date 12.06.2011 23:45:37
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#allocateMemory()
		public override void allocateMemory()
		{
			_arrI = new List<int>();
			_arrJ = new List<int>();
			_arrVal = new List<double>();

			_isMemAllocated = true;
		}

		//   * Erase the allocated memory by setting null pointers.
		//   * 
		//   * @date 12.06.2011 23:45:37
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#eraseMemory()
		public override void eraseMemory()
		{
			_arrI = null;
			_arrJ = null;
			_arrVal = null;

			_isMemAllocated = false;

		}

		//   * Checks if a given pair is already stored in the Block.
		//   * 
		//   * @date 13.06.2011 14:44:54
		//   * @author Christian Scheiblich
		//   * @param i
		//   *          local index for rows
		//   * @param j
		//   *          local index for columns
		//   * @return false if pair (i,j) is not stored yet other wise if pair (i,j) is
		//   *         stored true
		//   * @throws BlockException
		//   *           if indices are negative or out of bound or no memory is
		//   *           allocated.
		private bool IsOccupied(int i, int j)
		{
			if(!isMemAllocated())
				throw new BlockFailure("BlockIndex#isOccupied -- memory is not allocted");

			try
			{
				checkIndices(i, j); // check for correct indices
			}
			catch(BlockException blockException)
			{
				string exceptionMsg = blockException.getMessage();
				throw new BlockFailure("BlockIndex#isOccupied -- " + exceptionMsg);
			}

			bool isOccupied = false;

			int noOfIndicesI = _arrI.Count;
			int noOfIndicesJ = _arrJ.Count;

			if(noOfIndicesI != noOfIndicesJ)
				throw new BlockError("BlockIndex#isOccupied -- something wicked happend to internal sizes");

			for(int p = 0; p < noOfIndicesI; p++)
				if(i == _arrI[p] && j == _arrJ[p])
					isOccupied = true;

			return isOccupied;
		}

		//   * Searches and returns the internal index for both array lists for a
		//   * requested pair of indices.
		//   * 
		//   * @date 13.06.2011 15:31:47
		//   * @author Christian Scheiblich
		//   * @param i
		//   *          local index for rows
		//   * @param j
		//   *          local index for columns
		//   * @return an integer displaying the internal index of both array lists
		//   * @throws BlockException
		//   *           if given pair (i,j) is negative or out of bound or non memory is
		//   *           allocated.
		private int getOccupiedInternalArrayIndices(int i, int j)
		{
			if(!isMemAllocated())
				throw new BlockFailure("BlockIndex#getOccupiedInternalArrayIndices -- memory is not allocted");

			try
			{
				checkIndices(i, j); // check for correct indices
			}
			catch(BlockException blockException)
			{
				string exceptionMsg = blockException.getMessage();
				throw new BlockFailure("BlockIndex#getOccupiedIndexRow -- " + exceptionMsg);
			}

			int p = 0;

			bool isFound = false;

			while(!isFound && p < _arrI.Count)
			{
				if(i == _arrI[p])
				{
					if(j == _arrJ[p])
						isFound = true;
				} // if
				p++;
			} // whiles

			if(isFound)
			{
				if(p > 0)
					p--;

			}
			else
				p = -1;

			return p;
		}

	} // class

}