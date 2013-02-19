using System.Collections.Generic;
using System;


/**
 * @file Window.cpp
 *
 * Window functions - implementation.
 *
 * A signal frame should be multiplied by a time window before processing,
 * to decrease the magnitude of side lobes.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.0
 * @since 0.5.4
 */
namespace Aquila
{
	/**
	 * Possible window functions.
	 */
	public enum WindowType
	{
		WIN_RECT,
		WIN_HAMMING,
		WIN_HANN,
		WIN_BARLETT,
		WIN_BLACKMAN,
		WIN_FLATTOP
	}

	/**
	 * Class defining different window types as static methods.
	 *
	 * Windows are cached with window type combined with its length
	 * as a key. It is an efficient way to reduce calls to math functions.
	 */
	public class Window
	{
		// PI
		public const double M_PI = Math.PI;

		/**
		 * Returns window value for a given window type, size and position.
		 *
		 * Window is first looked up in cache. If it doesn't exist,
		 * it is generated. The cached value is then returned.
		 *
		 * @param type window function type
		 * @param n sample position in the window
		 * @param N window length
		 * @return window value for n-th sample
		 */
		public static double Apply(WindowType type, int n, int N)
		{
			KeyValuePair<WindowType, int> key = new KeyValuePair<WindowType, int>(type, N);
			
			if (!windowsCache.ContainsKey(key))
				CreateWindow(key);
			
			return windowsCache[key][n];
		}

		/**
		 * Window cache implemented as a static map.
		 */
		private static Dictionary<KeyValuePair<WindowType, int>, List<double>> windowsCache = new Dictionary<KeyValuePair<WindowType, int>, List<double>>();
		//public static Window.windowsCacheType windowsCache = new Window.windowsCacheType();
		
		/**
		 * Generates new window vector for a given type and size.
		 *
		 * Rectangular window is handled separately because it does not need
		 * any additional computation.
		 *
		 * @param windowKey a cache key
		 */
		private static void CreateWindow(KeyValuePair<WindowType, int> windowKey)
		{
			WindowType type = windowKey.Key;
			int N = windowKey.Value;
			
			if (type != WindowType.WIN_RECT)
			{
				WinGenerator generator = new WinGenerator(type, N);
				List<double> window = new List<double>();
				for (int i = 0; i < N; i++) {
					double val = generator.windowMethod.Invoke(i, N);
					window.Add(val);
				}
				windowsCache.Add(windowKey, window);
			}
			else
			{
				List<double> window = new List<double>();
				for (int i = 0; i < N; i++) {
					window.Add(1.0);
				}
				windowsCache.Add(windowKey, window);
			}
		}

		/**
		 * Hamming window.
		 *
		 * @param n sample position
		 * @param N window size
		 * @return n-th window sample value
		 */
		private static double Hamming(int n, int N)
		{
			return 0.53836 - 0.46164 * Math.Cos(2.0 * M_PI * n / (double)(N - 1));
		}

		/**
		 * Hann window.
		 *
		 * @param n sample position
		 * @param N window size
		 * @return n-th window sample value
		 */
		private static double Hann(int n, int N)
		{
			return 0.5 * (1.0 - Math.Cos(2.0 * M_PI * n / (double)(N - 1)));
		}

		/**
		 * Barlett (triangular) window.
		 *
		 * @param n sample position
		 * @param N window size
		 * @return n-th window sample value
		 */
		private static double Barlett(int n, int N)
		{
			return 1.0 - (2.0 * Math.Abs(n - (N - 1) / 2.0)) / ((double)(N - 1));
		}

		/**
		 * Blackman window.
		 *
		 * @param n sample position
		 * @param N window size
		 * @return n-th window sample value
		 */
		private static double Blackman(int n, int N)
		{
			return 0.42 - 0.5 * Math.Cos(2.0 * M_PI * n / (double)(N - 1)) + 0.08 * Math.Cos(4.0 * M_PI * n / (double)(N - 1));
		}

		/**
		 * Flat-top window.
		 *
		 * @param n sample position
		 * @param N window size
		 * @return n-th window sample value
		 */
		private static double Flattop(int n, int N)
		{
			return 1.0 - 1.93 * Math.Cos(2.0 * M_PI * n / (double)(N - 1)) + 1.29 * Math.Cos(4.0 * M_PI * n / (double)(N - 1)) - 0.388 * Math.Cos(6.0 * M_PI * n / (double)(N - 1)) + 0.0322 * Math.Cos(8.0 * M_PI * n / (double)(N - 1));
		}

		/**
		 * Private functor class for window generation.
		 */
		private class WinGenerator
		{
			/**
			 * Creates the generator functor.
			 *
			 * @param type window function type
			 * @param N window size
			 */
			public WinGenerator(WindowType type, int N)
			{
				_N = N;
				switch (type)
				{
					case WindowType.WIN_HAMMING:
						windowMethod = Window.Hamming;
						break;
					case WindowType.WIN_HANN:
						windowMethod = Window.Hann;
						break;
					case WindowType.WIN_BARLETT:
						windowMethod = Window.Barlett;
						break;
					case WindowType.WIN_BLACKMAN:
						windowMethod = Window.Blackman;
						break;
					case WindowType.WIN_FLATTOP:
						windowMethod = Window.Flattop;
						break;
					default:
						windowMethod = Window.Hamming;
						break;
				}
			}

			/**
			 * Window size.
			 */
			private int _N;

			/**
			 * Pointer to window function.
			 */
			public delegate double windowMethodDelegate(int NamelessParameter1, int NamelessParameter2);
			public windowMethodDelegate windowMethod;
		}
	}
}



