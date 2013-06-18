namespace math.transform.jwave.exc
{
	///
	// * Marking failures for this package; failures that are recoverable
	// * 
	// * @date 19.05.2009 09:26:22
	// * @author Christian Scheiblich
	public class JWaveFailure : JWaveException
	{

		//   * Generated serial ID for this failure
		//   * 
		//   * @date 19.05.2009 09:27:18
		//   * @author Christian Scheiblich
		private const long serialVersionUID = 5471588833755939370L;

		//   * Constructor taking a failure message
		//   * 
		//   * @date 19.05.2009 09:26:22
		//   * @author Christian Scheiblich
		//   * @param message
		//   *          the stored failure message for this exception
		public JWaveFailure(string message) : base(message)
		{
		} // TransformFailure

	} // class

}