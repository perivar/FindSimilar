using System;
using BasicTransform = math.transform.jwave.handlers.BasicTransform;
using TransformInterface = math.transform.jwave.handlers.TransformInterface;
using WaveletTransform = math.transform.jwave.handlers.WaveletTransform;
using Complex = math.transform.jwave.types.Complex;

namespace math.transform.jwave
{
	///
	// * Base class for transforms like DiscreteFourierTransform,
	// * FastWaveletTransform, and WaveletPacketTransform.
	// * 
	// * @date 19.05.2009 09:43:40
	// * @author Christian Scheiblich
	// 
	public class Transform
	{
		//   * Transform object of type base class
		protected internal TransformInterface _transform;

		//   * Constructor; needs some object like DiscreteFourierTransform,
		//   * FastWaveletTransform, WaveletPacketTransfom, ...
		//   * 
		//   * @date 19.05.2009 09:50:24
		//   * @author Christian Scheiblich
		//   * @param transform Transform object
		public Transform(TransformInterface transform)
		{
			_transform = transform;
		} // Transform

		//   * Constructor; needs some object like DiscreteFourierTransform,
		//   * FastWaveletTransform, WaveletPacketTransfom, ... It take also a number of iteration for decomposition
		//   * 
		//   * @date 19.05.2009 09:50:24
		//   * @author Christian Scheiblich
		public Transform(TransformInterface transform, int iteration)
		{
			if(transform is WaveletTransform)
			{
				_transform = transform;
				((WaveletTransform)_transform).set_iteration(iteration);
			}
			else
			{
				throw new ArgumentException("Can't use transform :" + transform.GetType() + " with a specific level decomposition ;" + " use Transform( TransformI transform ) constructor instead.");
			}
		} // Transform

		//   * Performs the forward transform of the specified BasicWave object.
		//   * 
		//   * @date 10.02.2010 09:41:01
		//   * @author Christian Scheiblich
		//   * @param arrTime
		//   *          coefficients of time domain
		//   * @return coefficients of frequency or Hilbert domain
		public virtual double[] forward(double[] arrTime)
		{
			return _transform.forward(arrTime);
		} // forward

		//   * Performs the reverse transform of the specified BasicWave object.
		//   * 
		//   * @date 10.02.2010 09:42:18
		//   * @author Christian Scheiblich
		//   * @param arrFreq
		//   *          coefficients of frequency or Hilbert domain
		//   * @return coefficients of time domain
		public virtual double[] reverse(double[] arrFreq)
		{
			return _transform.reverse(arrFreq);
		} // reverse


		//   * Performs the forward transform from time domain to frequency or Hilbert
		//   * domain for a given array depending on the used transform algorithm by
		//   * inheritance.
		//   * 
		//   * @date 23.11.2010 19:19:24
		//   * @author Christian Scheiblich
		//   * @param arrTime
		//   *          coefficients of 1-D time domain
		//   * @return coefficients of 1-D frequency or Hilbert domain
		public virtual Complex[] forward(Complex[] arrTime)
		{
			return ((BasicTransform)_transform).forward(arrTime);
		} // forward

		//   * Performs the reverse transform from frequency or Hilbert domain to time
		//   * domain for a given array depending on the used transform algorithm by
		//   * inheritance.
		//   * 
		//   * @date 23.11.2010 19:19:33
		//   * @author Christian Scheiblich
		//   * @param arrFreq
		//   *          coefficients of 1-D frequency or Hilbert domain
		//   * @return coefficients of 1-D time domain
		public virtual Complex[] reverse(Complex[] arrFreq)
		{
			return ((BasicTransform)_transform).reverse(arrFreq);
		} // reverse

		//   * Performs the 2-D forward transform of the specified BasicWave object.
		//   * 
		//   * @date 10.02.2010 10:58:54
		//   * @author Christian Scheiblich
		//   * @param matrixTime
		//   *          coefficients of 2-D time domain
		//   * @return coefficients of 2-D frequency or Hilbert domain
		public virtual double[][] forward(double[][] matrixTime)
		{
			return _transform.forward(matrixTime);
		} // forward

		//   * Performs the 2-D reverse transform of the specified BasicWave object.
		//   * 
		//   * @date 10.02.2010 10:59:32
		//   * @author Christian Scheiblich
		//   * @param matrixFreq
		//   *          coefficients of 2-D frequency or Hilbert domain
		//   * @return coefficients of 2-D time domain
		public virtual double[][] reverse(double[][] matrixFreq)
		{
			return _transform.reverse(matrixFreq);
		} // reverse

		//   * Performs the 3-D forward transform of the specified BasicWave object.
		//   * 
		//   * @date 10.07.2010 18:15:22
		//   * @author Christian Scheiblich
		//   * @param matrixTime
		//   *          coefficients of 2-D time domain
		//   * @return coefficients of 2-D frequency or Hilbert domain
		public virtual double[][][] forward(double[][][] spaceTime)
		{
			return _transform.forward(spaceTime);
		} // forward

		//   * Performs the 3-D reverse transform of the specified BasicWave object.
		//   * 
		//   * @date 10.07.2010 18:15:33
		//   * @author Christian Scheiblich
		//   * @param matrixFreq
		//   *          coefficients of 2-D frequency or Hilbert domain
		//   * @return coefficients of 2-D time domain
		public virtual double[][][] reverse(double[][][] spaceFreq)
		{
			return _transform.reverse(spaceFreq);
		} // reverse

	} // class

}