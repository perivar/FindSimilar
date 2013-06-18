using BlockException = math.transform.jwave.blocks.exc.BlockException;
using BlockFailure = math.transform.jwave.blocks.exc.BlockFailure;

namespace math.transform.jwave.blocks
{
	///
	// * Class for generating full blocks that keep information about position and
	// * size and allocates memory as double array of an array.
	// * 
	// * @date 11.06.2011 21:38:51
	// * @author Christian Scheiblich
	// 
	public class BlockFull : Block
	{
		//   * the internal matrix for keeping stored values
		private double[][] _matrix;

		//   * Constructor for a full block -- use BlockBuilder class for creating.
		//   * 
		//   * @date 11.06.2011 21:38:51
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#Block(int, int, int , int)
		protected internal BlockFull(int offSetRow, int offSetCol, int noOfRows, int noOfCols) : base(offSetRow, offSetCol, noOfRows, noOfCols)
		{
			// TODO tucker should implement this constructor
		}

		//   * Returns a requested entry if available.
		//   * 
		//   * @date 11.06.2011 21:38:51
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#get(int, int)
		public override double @get(int i, int j)
		{
			if(!_isMemAllocated)
				throw new BlockFailure("BlockFull#get -- no memory allocated");

			try
			{
				checkIndices(i, j); // check for correct indices
			}
			catch(BlockException blockException)
			{
				string exceptionMsg = blockException.getMessage();
				throw new BlockFailure("BlockFull#get -- " + exceptionMsg);
			}

			return _matrix[i][j];
		}

		//   * Returns all entries as an array of an array; matrix style.
		//   * 
		//   * @date 11.06.2011 22:52:34
		//   * @author tucker
		//   * @see math.transform.jwave.blocks.Block#get()
		public override double[][] @get()
		{
			if(!_isMemAllocated)
				throw new BlockFailure("BlockFull#get -- no memory allocated");

			//double[][] matrix = new double[_noOfRows][_noOfCols];
			double[][] matrix = CommonUtils.MathUtils.CreateJaggedArray<double[][]>(_noOfRows, _noOfCols);
			
			for(int i = 0; i < _noOfRows; i++) {
				for(int j = 0; j < _noOfCols; j++) {
					matrix[i][j] = _matrix[i][j];
				}
			}
			return matrix;
		}

		//   * Stores an entry if available.
		//   * 
		//   * @date 11.06.2011 21:38:51
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#set(int, int)
		public override void @set(int i, int j, double val)
		{
			if(!_isMemAllocated)
				throw new BlockFailure("BlockFull#set -- no memory allocated");

			try
			{
				checkIndices(i, j); // check for correct indices
			}
			catch(BlockException blockException)
			{
				string exceptionMsg = blockException.getMessage();
				throw new BlockFailure("BlockFull#set -- " + exceptionMsg);
			}

			_matrix[i][j] = val;
		}

		//   * Allocates memory as an array of an array -- matrix.
		//   * 
		//   * @date 11.06.2011 21:38:51
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#allocateMemory()
		public override void allocateMemory()
		{
			//_matrix = new double[_noOfRows][_noOfCols];
			_matrix = CommonUtils.MathUtils.CreateJaggedArray<double[][]>(_noOfRows, _noOfCols);
			_isMemAllocated = true;
		}

		//   * Erases the allocated memory by setting null pointer (and running java's
		//   * garbage collector).
		//   * 
		//   * @date 11.06.2011 21:38:51
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#eraseMemory()
		public override void eraseMemory()
		{
			_matrix = null;
			_isMemAllocated = false;
		}

	} // class

}