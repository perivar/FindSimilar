namespace math.transform.jwave.exc
{

	///
	// * Marking errors for this package; failures that are not recoverable
	// * 
	// * @date 19.05.2009 09:28:17
	// * @author Christian Scheiblich
	public class JWaveError : JWaveException
	{
		//   * Generated serial ID for this error
		//   * 
		//   * @date 19.05.2009 09:29:04
		//   * @author Christian Scheiblich
		private const long serialVersionUID = -2757378141408012245L;

		//   * constructor taking an error message
		//   * 
		//   * @date 19.05.2009 09:28:17
		//   * @author Christian Scheiblich
		//   * @param message
		//   *          stored message for this error
		public JWaveError(string message) : base(message)
		{
		} // TransformError

	} // class

}