using System;
using BlockError = math.transform.jwave.blocks.exc.BlockError;
using BlockException = math.transform.jwave.blocks.exc.BlockException;

namespace math.transform.jwave.blocks
{
	///
	// * Creates Block objects
	// * 
	// * @date 11.06.2011 21:25:17
	// * @author Christian Scheiblich
	// 
	public class BlockController
	{
		public static Block create(BlockType blockType, int offSetRow, int offSetCol, int noOfRows, int noOfCols)
		{
			Block block = null;

			switch(blockType)
			{
				case BlockType.Dummy :

					block = new BlockDummy(offSetRow, offSetCol, noOfRows, noOfCols);

					break;

				case BlockType.Full :

					block = new BlockFull(offSetRow, offSetCol, noOfRows, noOfCols);

					break;

				case BlockType.Index :

					block = new BlockIndex(offSetRow, offSetCol, noOfRows, noOfCols);

					break;

					default :

						throw new BlockError("BlockBuilder#create -- given BlockType is not defined");

			} // switch

			return block;

		}

		//   * Convert a block to a different type of block as a copy.
		//   * 
		//   * @date 12.06.2011 23:33:50
		//   * @author Christian Scheiblich
		//   * @param blockType
		//   *          the type of block to convert to
		//   * @param block
		//   *          the pattern block keeping memory or not
		//   * @return a new block object as a copy for the the requested type
		//   * @throws BlockException
		//   *           if off sets or sizes are negative or out of bound
		public static Block convert(BlockType blockType, Block block)
		{
			Block newBlock = null;

			newBlock = create(blockType, block.getOffSetRow(), block.getOffSetCol(), block.getNoOfRows(), block.getNoOfCols());

			if(block.isMemAllocated())
			{
				newBlock.allocateMemory();

				double[][] matrix = block.get();

				for(int i = 0; i < block.getNoOfRows(); i++)
				{
					for(int j = 0; j < block.getNoOfCols(); j++)
					{
						double val = matrix[i][j];

						if(val != 0.0)
						{
							newBlock.set(i, j, val);
						}
					} // for
				} // for
			} // if

			return newBlock;
		}
	} // class
}