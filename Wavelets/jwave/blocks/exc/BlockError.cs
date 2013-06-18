using System;

namespace math.transform.jwave.blocks.exc
{

	///
	// * Error class as an non-recoverable exception.
	// *
	// * @date 11.06.2011 20:08:51
	// * @author Christian Scheiblich
	// 
	public class BlockError : BlockException
	{

		//   * Generated serial id.
		//   *
		//   * @date 11.06.2011 20:09:56
		//   * @author Christian Scheiblich
		//
		private const long serialVersionUID = 3813313081473155788L;

		//   * Constructor for error message.
		//   *
		//   * @date 11.06.2011 20:08:51
		//   * @author Christian Scheiblich
		//   * @param message
		//
		public BlockError(string message) : base(message)
		{
		}

		//   * Constructor taking any Java based exception.
		//   * 
		//   * @date 11.06.2011 20:08:51
		//   * @author Christian Scheiblich
		//   * @param e
		//   *          object of type java.lang.Exception
		//
		public BlockError(Exception e) : base(e)
		{
		}

	} // class
}