using System;

namespace math.transform.jwave.blocks.exc
{
	///
	// * Failure class as an recoverable exception.
	// * 
	// * @date 11.06.2011 20:06:56
	// * @author Christian Scheiblich
	// 
	public class BlockFailure : BlockException
	{

		//   * Generated serial id.
		//   *
		//   * @date 11.06.2011 20:08:17
		//   * @author Christian Scheiblich
		//
		private const long serialVersionUID = -1020584447000514150L;

		//   * Constructor for failure message.
		//   * 
		//   * @date 11.06.2011 20:06:56
		//   * @author Christian Scheiblich
		//   * @param message
		//
		public BlockFailure(string message) : base(message)
		{
		}

		//   * Constructor taking any Java based exception.
		//   * 
		//   * @date 11.06.2011 20:06:56
		//   * @author Christian Scheiblich
		//   * @param e  object of type java.lang.Exception
		//
		public BlockFailure(Exception e) : base(e)
		{
		}

	} // class
}