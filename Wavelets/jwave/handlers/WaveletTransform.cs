using WaveletInterface = math.transform.jwave.handlers.wavelets.WaveletInterface;

namespace math.transform.jwave.handlers
{

	///
	// * TODO pol explainMeShortly
	// * 
	// * @date 30 juin 2011 14:50:35
	// * @author Pol Kennel
	// 
	public abstract class WaveletTransform : BasicTransform
	{

		//----------------------------------- Attributes -------------------------------------------

		protected internal WaveletInterface _wavelet;

		protected internal int _iteration = -1;

		//----------------------------------- Constructors -----------------------------------------

		public WaveletTransform()
		{
		}

		public WaveletTransform(WaveletInterface wavelet)
		{
			_wavelet = wavelet;
		}

		public WaveletTransform(WaveletInterface wavelet, int iteration)
		{
			_wavelet = wavelet;
			_iteration = iteration;
		}

		//----------------------------------- Getters / Setters ------------------------------------

		public virtual WaveletInterface get_wavelet()
		{
			return _wavelet;
		}

		public virtual void set_wavelet(WaveletInterface _wavelet)
		{
			this._wavelet = _wavelet;
		}

		public virtual int get_iteration()
		{
			return _iteration;
		}

		public virtual void set_iteration(int _iteration)
		{
			this._iteration = _iteration;
		}

		//----------------------------------- Methods ---------------------------------------------

		public override double[] forward(double[] arrTime)
		{
			//    System.out.println("1D wave");
			if(_iteration == -1)
				return forwardWavelet(arrTime);
			else
				return forwardWavelet(arrTime, _iteration);
		}

		public override double[] reverse(double[] arrFreq)
		{
			if(_iteration == -1)
				return reverseWavelet(arrFreq);
			else
				return reverseWavelet(arrFreq, _iteration);
		}

		public override double[][] forward(double[][] matTime)
		{
			if(_iteration == -1)
				return base.forward(matTime);
			else
				return forwardWavelet(matTime, _iteration);
		}

		public override double[][] reverse(double[][] matFreq)
		{
			if(_iteration == -1)
				return base.reverse(matFreq);
			else
				return reverseWavelet(matFreq, _iteration);
		}

		public override double[][][] forward(double[][][] spcTime)
		{
			if(_iteration == -1)
				return base.forward(spcTime);
			else
				return forwardWavelet(spcTime, _iteration);
		}

		public override double[][][] reverse(double[][][] spcFreq)
		{
			if(_iteration == -1)
				return base.reverse(spcFreq);
			else
				return reverseWavelet(spcFreq, _iteration);
		}

		public abstract double[] forwardWavelet(double[] arrTime);

		//   * Performs the forward transform from time domain to frequency or Hilbert
		//   * domain for a given array depending on the used transform algorithm by
		//   * inheritance. The number of transformation levels applied is limited by
		//   * threshold.
		//   * 
		//   * @date 15.07.2010
		//   * @author Thomas Haider
		//   * @date 15.08.2010 00:32:09
		//   * @author Christian Scheiblich
		//   * @param arrTime
		//   *          coefficients of 1-D time domain
		//   * @param toLevel
		//   *          threshold for number of iterations
		//   * @return coefficients of 1-D frequency or Hilbert domain
		public abstract double[] forwardWavelet(double[] arrTime, int toLevel);

		public abstract double[] reverseWavelet(double[] arrTime);

		//   * Performs the reverse transform from frequency or Hilbert domain to time
		//   * domain for a given array depending on the used transform algorithm by
		//   * inheritance. The number of transformation levels applied is limited by
		//   * threshold.
		//   * 
		//   * @date 15.07.2010
		//   * @author Thomas Haider
		//   * @date 15.08.2010 00:32:24
		//   * @author Christian Scheiblich
		//   * @param arrFreq
		//   *          coefficients of 1-D frequency or Hilbert domain
		//   * @param fromLevel
		//   *          threshold for number of iterations
		//   * @return coefficients of 1-D time domain
		public abstract double[] reverseWavelet(double[] arrFreq, int fromLevel);

		//   * Performs the 2-D forward transform from time domain to frequency or Hilbert
		//   * domain for a given array depending on the used transform algorithm by
		//   * inheritance. The number of transformation levels applied is limited by
		//   * threshold.
		//   * 
		//   * @date 15.07.2010
		//   * @author Thomas Haider
		//   * @date 15.08.2010 00:32:52
		//   * @author Christian Scheiblich
		//   * @param matTime
		//   *          coefficients of 2-D time domain
		//   * @param toLevel
		//   *          threshold for number of iterations
		//   * @return coefficients of 2-D frequency or Hilbert domain
		public virtual double[][] forwardWavelet(double[][] matTime, int toLevel)
		{
			int noOfRows = matTime.Length;
			int noOfCols = matTime[0].Length;
			//double[][] matHilb = new double[noOfRows][noOfCols];
			double[][] matHilb = CommonUtils.MathUtils.CreateJaggedArray<double[][]>(noOfRows, noOfCols);

			for(int i = 0; i < noOfRows; i++) // rows
			{
				double[] arrTime = new double[noOfCols];
				for(int j = 0; j < noOfCols; j++)
					arrTime[j] = matTime[i][j];
				double[] arrHilb = forwardWavelet(arrTime, toLevel);
				for(int j = 0; j < noOfCols; j++)
					matHilb[i][j] = arrHilb[j];
			}

			for(int j = 0; j < noOfCols; j++) // cols
			{
				double[] arrTime = new double[noOfRows];
				for(int i = 0; i < noOfRows; i++)
					arrTime[i] = matHilb[i][j];
				double[] arrHilb = forwardWavelet(arrTime, toLevel);
				for(int i = 0; i < noOfRows; i++)
					matHilb[i][j] = arrHilb[i];
			}

			return matHilb;
		} // forward

		//   * Performs the 2-D reverse transform from frequency or Hilbert or time domain
		//   * to time domain for a given array depending on the used transform algorithm
		//   * by inheritance. The number of transformation levels applied is limited by
		//   * threshold.
		//   * 
		//   * @date 15.07.2010
		//   * @author Thomas Haider
		//   * @date 15.08.2010 00:33:10
		//   * @author Christian Scheiblich
		//   * @param matFreq
		//   *          coefficients of 2-D frequency or Hilbert domain
		//   * @param fromLevel
		//   *          threshold for number of iterations
		//   * @return coefficients of 2-D time domain
		public virtual double[][] reverseWavelet(double[][] matFreq, int fromLevel)
		{
			int noOfRows = matFreq.Length;
			int noOfCols = matFreq[0].Length;
			//double[][] matTime = new double[noOfRows][noOfCols];
			double[][] matTime = CommonUtils.MathUtils.CreateJaggedArray<double[][]>(noOfRows, noOfCols);

			for(int j = 0; j < noOfCols; j++) // cols
			{
				double[] arrFreq = new double[noOfRows];
				for(int i = 0; i < noOfRows; i++)
					arrFreq[i] = matFreq[i][j];
				double[] arrTime = reverseWavelet(arrFreq, fromLevel);
				for(int i = 0; i < noOfRows; i++)
					matTime[i][j] = arrTime[i];
			}

			for(int i = 0; i < noOfRows; i++) // rows
			{
				double[] arrFreq = new double[noOfCols];
				for(int j = 0; j < noOfCols; j++)
					arrFreq[j] = matTime[i][j];
				double[] arrTime = reverseWavelet(arrFreq, fromLevel);
				for(int j = 0; j < noOfCols; j++)
					matTime[i][j] = arrTime[j];
			}

			return matTime;
		}

		//   * Performs the 3-D forward transform from time domain to frequency or Hilbert
		//   * domain for a given array depending on the used transform algorithm by
		//   * inheritance. The number of transformation levels applied is limited by
		//   * threshold.
		//   * 
		//   * @date 15.07.2010
		//   * @author Thomas Haider
		//   * @date 15.08.2010 00:33:30
		//   * @author Christian Scheiblich
		//   * @param matrixFreq
		//   *          coefficients of 3-D frequency or Hilbert domain
		//   * @param toLevel
		//   *          threshold for number of iterations
		//   * @return coefficients of 3-D time domain
		public virtual double[][][] forwardWavelet(double[][][] spcTime, int toLevel)
		{
			int noOfRows = spcTime.Length; // first dimension
			int noOfCols = spcTime[0].Length; // second dimension
			int noOfHigh = spcTime[0][0].Length; // third dimension

			//double[][][] spcHilb = new double[noOfRows][noOfCols][noOfHigh];
			double[][][] spcHilb = CommonUtils.MathUtils.CreateJaggedArray<double[][][]>(noOfRows, noOfCols, noOfHigh);

			for(int i = 0; i < noOfRows; i++)
			{
				//double[][] matTime = new double[noOfCols][noOfHigh];
				double[][] matTime = CommonUtils.MathUtils.CreateJaggedArray<double[][]>(noOfCols, noOfHigh);

				for(int j = 0; j < noOfCols; j++)
				{
					for(int k = 0; k < noOfHigh; k++)
					{

						matTime[j][k] = spcTime[i][j][k];

					} // high

				} // cols

				double[][] matHilb = forwardWavelet(matTime, toLevel); // 2-D forward

				for(int j = 0; j < noOfCols; j++)
				{
					for(int k = 0; k < noOfHigh; k++)
					{
						spcHilb[i][j][k] = matHilb[j][k];

					} // high

				} // cols

			} // rows

			for(int j = 0; j < noOfCols; j++)
			{
				for(int k = 0; k < noOfHigh; k++)
				{
					double[] arrTime = new double[noOfRows];

					for(int i = 0; i < noOfRows; i++)
						arrTime[i] = spcHilb[i][j][k];

					double[] arrHilb = forwardWavelet(arrTime, toLevel); // 1-D forward

					for(int i = 0; i < noOfRows; i++)
						spcHilb[i][j][k] = arrHilb[i];

				} // high

			} // cols

			return spcHilb;
		} // forward

		//   * Performs the 3-D reverse transform from frequency or Hilbert domain to time
		//   * domain for a given array depending on the used transform algorithm by
		//   * inheritance. The number of transformation levels applied is limited by
		//   * threshold.
		//   * 
		//   * @date 15.07.2010
		//   * @author Thomas Haider
		//   * @date 15.08.2010 00:33:44
		//   * @author Christian Scheiblich
		//   * @param matrixFreq
		//   *          coefficients of 3-D frequency or Hilbert domain
		//   * @param threshold
		//   *          threshold for number of iterations
		//   * @return coefficients of 3-D time domain
		public virtual double[][][] reverseWavelet(double[][][] spcHilb, int fromLevel)
		{
			int noOfRows = spcHilb.Length; // first dimension
			int noOfCols = spcHilb[0].Length; // second dimension
			int noOfHigh = spcHilb[0][0].Length; // third dimension

			//double[][][] spcTime = new double[noOfRows][noOfCols][noOfHigh];
			double[][][] spcTime = CommonUtils.MathUtils.CreateJaggedArray<double[][][]>(noOfRows, noOfCols, noOfHigh);

			for(int i = 0; i < noOfRows; i++)
			{
				//double[][] matHilb = new double[noOfCols][noOfHigh];
				double[][] matHilb = CommonUtils.MathUtils.CreateJaggedArray<double[][]>(noOfCols, noOfHigh);

				for(int j = 0; j < noOfCols; j++)
				{
					for(int k = 0; k < noOfHigh; k++)
					{
						matHilb[j][k] = spcHilb[i][j][k];
					} // high
				} // cols

				double[][] matTime = reverseWavelet(matHilb, fromLevel); // 2-D reverse

				for(int j = 0; j < noOfCols; j++)
				{
					for(int k = 0; k < noOfHigh; k++)
					{
						spcTime[i][j][k] = matTime[j][k];
					} // high
				} // cols

			} // rows

			for(int j = 0; j < noOfCols; j++)
			{
				for(int k = 0; k < noOfHigh; k++)
				{
					double[] arrHilb = new double[noOfRows];

					for(int i = 0; i < noOfRows; i++)
						arrHilb[i] = spcTime[i][j][k];

					double[] arrTime = reverseWavelet(arrHilb, fromLevel); // 1-D reverse

					for(int i = 0; i < noOfRows; i++)
						spcTime[i][j][k] = arrTime[i];

				} // high
			} // cols

			return spcTime;
		} // reverse
	}
}