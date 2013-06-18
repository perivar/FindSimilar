using BlockException = math.transform.jwave.blocks.exc.BlockException;
using BlockFailure = math.transform.jwave.blocks.exc.BlockFailure;

namespace math.transform.jwave.blocks
{
	///
	// * Class for generating dummy blocks that keep information about position and
	// * size but never allocate memory.
	// * 
	// * @date 11.06.2011 21:31:12
	// * @author Christian Scheiblich
	public class BlockDummy : Block
	{
		//   * Constructor for a dummy block -- use BlockBuilder class for creating.
		//   * 
		//   * @date 11.06.2011 21:31:12
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#Block(int, int, int , int)
		protected internal BlockDummy(int offSetRow, int offSetCol, int noOfRows, int noOfCols) : base(offSetRow, offSetCol, noOfRows, noOfCols)
		{
		}

		//   * Method is not available in case of a dummy.
		//   * 
		//   * @date 11.06.2011 21:31:12
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#get(int, int)
		public override double @get(int i, int j)
		{
			throw new BlockFailure("BlockDummy#get -- method not available");
		}

		//   * Method is not available in case of a dummy.
		//   * 
		//   * @date 11.06.2011 22:51:59
		//   * @author tucker
		//   * @see math.transform.jwave.blocks.Block#get()
		public override double[][] @get()
		{
			throw new BlockFailure("BlockDummy#get -- method not available");
		}

		//   * Method is not available in case of a dummy.
		//   * 
		//   * @date 11.06.2011 21:31:12
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#set(int, int)
		public override void @set(int i, int j, double val)
		{
			throw new BlockFailure("BlockDummy#set -- method not available");
		}

		//   * Method is not available in case of a dummy.
		//   * 
		//   * @date 11.06.2011 21:31:12
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#allocateMemory()
		public override void allocateMemory()
		{
			throw new BlockFailure("BlockDummy#allocateMemory -- method not available");
		}

		//   * Method is not available in case of a dummy.
		//   * 
		//   * @date 11.06.2011 21:31:12
		//   * @author Christian Scheiblich
		//   * @see math.transform.jwave.blocks.Block#eraseMemory()
		public override void eraseMemory()
		{
			throw new BlockFailure("BlockDummy#eraseMemory -- method not available");
		}

	} // class

}