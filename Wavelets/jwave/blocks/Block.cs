using System;
using BlockException = math.transform.jwave.blocks.exc.BlockException;
using BlockFailure = math.transform.jwave.blocks.exc.BlockFailure;

namespace math.transform.jwave.blocks
{
	///
	// * A Block that keeps some matrix based values inside stored in different ways.
	// * 
	// * @date 11.06.2011 19:53:52
	// * @author Christian Scheiblich
	// 
	public abstract class Block
	{

		//   * One index of upper left corner of the block in global area.
		protected internal int _offSetRow;

		//   * One index of upper left corner of the block in global area.
		protected internal int _offSetCol;

		//   * One dimension of the block like M matrix.
		protected internal int _noOfRows;

		//   * One dimension of the block like M matrix.
		protected internal int _noOfCols;

		//   * Set to false if block has no memory allocated otherwise to true if block
		//   * has memory allocated.
		protected internal bool _isMemAllocated;

		//   * Constructor taking the information of the upper left corner of the block:
		//   * e.g. the block is placed at (i,j) == (25,25) in a greater global area.
		//   * Furthermore, it is taking the information about the dimensions of the block
		//   * in common to (M,N) by a matrix.
		//   * 
		//   * @date 11.06.2011 20:14:27
		//   * @author Christian Scheiblich
		//   * @param offSetRow
		//   *          upper left corner of block -- index i
		//   * @param offSetCol
		//   *          upper left corner of block -- index j
		//   * @param noOfRows
		//   *          the dimension of the block in rows -- dim M
		//   * @param noOfCols
		//   *          the dimension of the block in columns -- dim N
		//   * @throws BlockException
		//   *           if the given values are negative or in case of noOfRows and
		//   *           noOfCols are equal to zero.
		public Block(int offSetRow, int offSetCol, int noOfRows, int noOfCols)
		{
			if(offSetRow < 0)
				throw new BlockFailure("Block#Block -- offSetRow is negative ~8>");
			if(offSetCol < 0)
				throw new BlockFailure("Block#Block -- offSetCol is negative ~8>");

			if(noOfRows < 0)
				throw new BlockFailure("Block#Block -- noOfRows is negative ~8>");
			if(noOfCols < 0)
				throw new BlockFailure("Block#Block -- noOfCols is negative ~8>");

			if(noOfRows == 0)
				throw new BlockFailure("Block#Block -- noOfRows is zero; must be at least 1 or greater ~8>");
			if(noOfCols == 0)
				throw new BlockFailure("Block#Block -- noOfCols is zero; must be at least 1 or greater ~8>");

			_offSetRow = offSetRow;
			_offSetCol = offSetCol;

			_noOfRows = noOfRows;
			_noOfCols = noOfCols;

			_isMemAllocated = false;

		} // Block

		//   * Barely answers the question to be or not to be. ~8>
		//   * 
		//   * @date 11.06.2011 20:35:21
		//   * @author Christian Scheiblich
		//   * @return returns false if no memory is allocated otherwise true memory is
		//   *         allocated.
		public virtual bool isMemAllocated()
		{
			return _isMemAllocated;
		}

		//   * Returns the number of rows -- M.
		//   * 
		//   * @date 11.06.2011 20:38:20
		//   * @author Christian Scheiblich
		//   * @return the M of a matrix as know by the size (M,N)
		public virtual int getNoOfRows()
		{
			return _noOfRows;
		}

		//   * Returns the number of columns -- N.
		//   * 
		//   * @date 11.06.2011 20:40:51
		//   * @author Christian Scheiblich
		//   * @return the N of a matrix as know by the size (M,N)
		public virtual int getNoOfCols()
		{
			return _noOfCols;
		}

		//   * Returns the number of stored entries; for memory counting e.g.
		//   * 
		//   * @date 11.06.2011 21:00:03
		//   * @author Christian Scheiblich
		//   * @return the number of stored entries in the block
		public virtual int getNoOfEntries()
		{
			return _noOfRows * _noOfCols;
		}

		//   * Returns the row offset of the upper left corner -- i.
		//   * 
		//   * @date 11.06.2011 20:41:20
		//   * @author Christian Scheiblich
		//   * @return the i of the upper left corner as an offset
		public virtual int getOffSetRow()
		{
			return _offSetRow;
		}

		//   * Returns the row offset of the upper left corner -- j.
		//   * 
		//   * @date 11.06.2011 20:42:25
		//   * @author Christian Scheiblich
		//   * @return the j of the upper left corner as an offset
		public virtual int getOffSetCol()
		{
			return _offSetCol;
		}

		//   * Returns the i-th row filled with entries.
		//   * 
		//   * @date 11.06.2011 20:47:39
		//   * @author Christian Scheiblich
		//   * @param i
		//   *          the row index of the requested row
		//   * @return double array keeping the values of the i-th row
		//   * @throws BlockException
		//   *           if i is out of bound or no memory is allocated for this block
		public virtual double[] getRow(int i)
		{
			double[] row = new double[_noOfCols];

			for(int j = 0; j < _noOfCols; j++)
				row[j] = @get(i, j);

			return row;
		}

		//   * Returns the j-th column filled with entries.
		//   * 
		//   * @date 11.06.2011 20:54:08
		//   * @author Christian Scheiblich
		//   * @param j
		//   *          the column index of the requested column
		//   * @return double array keeping the values of the j-th column
		//   * @throws BlockException
		//   *           if j is out of bound or no memory is allocated for this block
		public virtual double[] getCol(int j)
		{
			double[] col = new double[_noOfRows];

			for(int i = 0; i < _noOfRows; i++)
				col[i] = @get(i, j);

			return col;
		}

		//   * Checks for the indices being not out of bound.
		//   * 
		//   * @date 11.06.2011 21:48:32
		//   * @author Christian Scheiblich
		//   * @param i
		//   *          the row index
		//   * @param j
		//   *          the column index
		//   * @throws BlockException
		//   *           if an index is out of bound
		protected internal virtual void checkIndices(int i, int j)
		{
			if(i < 0)
				throw new BlockFailure("Block#checkIndices -- index i is negative");

			if(j < 0)
				throw new BlockFailure("Block#checkIndices -- index j is negative");

			if(i >= _noOfRows)
				throw new BlockFailure("Block#checkIndices -- index i is out of bound");

			if(j >= _noOfCols)
				throw new BlockFailure("Block#checkIndices -- index j is out of bound");

		}

		//   * Returns the stored entry at LOCAL position (i,j) of the block.
		//   * 
		//   * @date 11.06.2011 20:44:05
		//   * @author Christian Scheiblich
		//   * @param i
		//   *          row index as known by matrices
		//   * @param j
		//   *          column index as known by matrices
		//   * @return stored entry as double value
		//   * @throws BlockException
		//   *           if the pair (i,j) is out of bound or no memory is allocated
		abstract public double @get(int i, int j);

		//   * Returns a matrix keeping the values, if values are missing. like in index
		//   * storage, the places are filled by zero entries.
		//   * 
		//   * @date 11.06.2011 22:49:45
		//   * @author Christian Scheiblich
		//   * @return all stored values as an array of an array; matrix style
		//   * @throws BlockException
		//   *           if no memory is allocated
		abstract public double[][] @get();

		//   * Sets an entry in the block at LOCAL position (i,j) of the block.
		//   * 
		//   * @date 11.06.2011 21:15:42
		//   * @author Christian Scheiblich
		//   * @param i
		//   *          row index as known by matrices
		//   * @param j
		//   *          column index as known by matrices
		//   * @param val
		//   *          value as double to be stored at position (i,j)
		//   * @throws BlockException
		//   *           if the pair (i,j) is out of bound or no memory is allocated
		abstract public void @set(int i, int j, double val);

		//   * Allocates memory in case of the instantiated block type.
		//   * 
		//   * @date 11.06.2011 20:18:33
		//   * @author Christian Scheiblich
		//   * @throws BlockException
		//   *           if allocations fails due to some reason
		abstract public void allocateMemory();

		//   * Erases memory in case of the instantiated block type.
		//   * 
		//   * @date 11.06.2011 20:20:16
		//   * @author Christian Scheiblich
		//   * @throws BlockException
		//   *           if erasing fails due to some reason
		abstract public void eraseMemory();

	} // class

}