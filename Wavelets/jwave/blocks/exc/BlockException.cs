using System;
using JWaveException = math.transform.jwave.exc.JWaveException;

namespace math.transform.jwave.blocks.exc
{
	///
	// * Exception class for the block package.
	// * 
	// * @date 11.06.2011 20:04:19
	// * @author Christian Scheiblich
	// 
	public class BlockException : JWaveException
	{

		//   * Generated serial id.
		//   * 
		//   * @date 11.06.2011 20:05:43
		//   * @author Christian Scheiblich
		//
		private const long serialVersionUID = 3348636428006375494L;

		//   * Constructor for exception message.
		//   * 
		//   * @date 11.06.2011 20:04:19
		//   * @author Christian Scheiblich
		//   * @param message
		//
		public BlockException(string message) : base(message)
		{
		}

		//   * Constructor taking any Java based exception.
		//   * 
		//   * @date 11.06.2011 20:04:19
		//   * @author Christian Scheiblich
		//   * @param e
		//   *          object of type java.lang.Exception
		//
		public BlockException(Exception e) : base(e)
		{
		}

	} // class
}