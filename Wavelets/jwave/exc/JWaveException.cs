using System;

namespace math.transform.jwave.exc
{
	///
	// * Class to be generally thrown in this package to mark an exception
	// * 
	// * @date 16.10.2008 07:30:20
	// * @author Christian Scheiblich
	// 
	public class JWaveException : Exception
	{

		//   * Generated serial version ID for this exception
		//   * 
		//   * @date 27.05.2009 06:58:27
		//   * @author Christian Scheiblich
		//
		private const long serialVersionUID = -4165486739091019056L;

		//   * Member var for the stored exception message
		//
		protected internal string _message; // exception message

		//   * Constructor for storing a handed exception message
		//   * 
		//   * @date 27.05.2009 06:51:57
		//   * @author Christian Scheiblich
		//   * @param message
		//   *          this message should tell exactly what went wrong
		//
		public JWaveException(string message)
		{
			_message = "JWave"; // empty
			_message += ":"; // separator
			_message += message; // add message
			_message += "\n"; // break line
		} // TransformException

		//   * Copy constructor; use this for a quick fix of sub types
		//   * 
		//   * @date 29.07.2009 07:03:45
		//   * @author Christian Scheiblich
		//   * @param e
		//   *          an object of this class
		//
		public JWaveException(Exception e)
		{
			_message = e.Message;
		} // TransformException

		//   * Returns the stored exception message as a string
		//   * 
		//   * @date 27.05.2009 06:52:46
		//   * @author Christian Scheiblich
		//   * @return exception message that should tell exactly what went wrong
		//
		public string getMessage()
		{
			return _message;
		} // getMessage

		//   * Displays the stored exception message at console out
		//   * 
		//   * @date 27.05.2009 06:53:23
		//   * @author Christian Scheiblich
		//
		public virtual void showMessage()
		{
			Console.WriteLine(_message);
		} // showMessage

		//   * Nuke the run and print stack trace
		//   * 
		//   * @date 02.07.2009 05:07:42
		//   * @author Christian Scheiblich
		//
		public virtual void nuke()
		{
			Console.WriteLine("");
			Console.WriteLine("                  ____             ");
			Console.WriteLine("          __,-~~/~    `---.        ");
			Console.WriteLine("        _/_,---(      ,    )       ");
			Console.WriteLine("    __ /        NUKED     ) \\ __  ");
			Console.WriteLine("   ====------------------===;;;==  ");
			Console.WriteLine("      /  ~\"~\"~\"~\"~\"~~\"~)     ");
			Console.WriteLine("      (_ (      (     >    \\)     ");
			Console.WriteLine("       \\_( _ <         >_>\'      ");
			Console.WriteLine("           ~ `-i' ::>|--\"         ");
			Console.WriteLine("               I;|.|.|             ");
			Console.WriteLine("              <|i::|i|>            ");
			Console.WriteLine("               |[::|.|             ");
			Console.WriteLine("                ||: |              ");
			Console.WriteLine("");
			this.showMessage();
			Console.WriteLine(this.StackTrace);
		} // nuke
	} // class
}