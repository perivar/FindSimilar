using System;

namespace Wavelets
{
	// Methods to test the HaarTransform Class
	public static class HaarTransformTest
	{
		public static void RunTests()
		{
			HaarTransform.timestamp();

			Console.Write("\n");
			Console.Write("HaarTransformTest\n");
			Console.Write("  C# version\n");
			Console.Write("  Test the HAAR library.\n");

			test01();
			test02();
			
			//
			//  Terminate.
			//
			Console.Write("\n");
			Console.Write("HaarTransformTest\n");
			Console.Write("  Normal end of execution.\n");

			Console.Write("\n");
			HaarTransform.timestamp();

			return;
		}
		
		/// <summary>
		/// TEST01 tests HAAR_1D.
		/// </summary>
		public static void test01()
		{
			int i;
			int n;
			int seed;
			double[] u;
			double[] v;
			double[] w;

			Console.Write("\n");
			Console.Write("TEST01\n");
			Console.Write("  HAAR_1D computes the Haar transform of a vector.\n");

			//
			//  Random data.
			//
			n = 16;
			seed = 123456789;
			u = HaarTransform.r8vec_uniform_01_new (n, ref seed);
			v = HaarTransform.r8vec_copy_new (n, u);

			HaarTransform.haar_1d (n, v);

			w = HaarTransform.r8vec_copy_new (n, v);
			HaarTransform.haar_1d_inverse (n, w);

			Console.Write("\n");
			Console.Write("   i    U(i)          H(U)(i)       Hinv(H(U))(i)\n");
			Console.Write("\n");
			for (i = 0; i < n; i++)
			{
				Console.Write("  ");
				Console.Write("{0,2}", i);
				Console.Write("{0,2}", "  ");
				Console.Write("{0,10:N6}", u[i]);
				Console.Write("{0,4}", "  ");
				Console.Write("{0,10:N6}", v[i]);
				Console.Write("{0,4}", "  ");
				Console.Write("{0,10:N6}", w[i]);
				Console.Write("{0,4}", "\n");
			}
			u = null;
			v = null;
			w = null;

			//
			//  Constant signal.
			//
			n = 8;
			u = HaarTransform.r8vec_ones_new (n);
			v = HaarTransform.r8vec_copy_new (n, u);

			HaarTransform.haar_1d (n, v);

			w = HaarTransform.r8vec_copy_new (n, v);
			HaarTransform.haar_1d_inverse (n, w);

			Console.Write("\n");
			Console.Write("   i    U(i)          H(U)(i)       Hinv(H(U))(i)\n");
			Console.Write("\n");
			for (i = 0; i < n; i++)
			{
				Console.Write("  ");
				Console.Write("{0,2}", i);
				Console.Write("{0,2}", "  ");
				Console.Write("{0,10:N6}", u[i]);
				Console.Write("{0,4}", "  ");
				Console.Write("{0,10:N6}", v[i]);
				Console.Write("{0,4}", "  ");
				Console.Write("{0,10:N6}", w[i]);
				Console.Write("{0,4}", "\n");
			}
			u = null;
			v = null;
			w = null;

			//
			//  Linear signal.
			//
			n = 16;
			u = HaarTransform.r8vec_linspace_new (n, 1.0, (double) n);
			v = HaarTransform.r8vec_copy_new (n, u);

			HaarTransform.haar_1d (n, v);

			w = HaarTransform.r8vec_copy_new (n, v);
			HaarTransform.haar_1d_inverse (n, w);

			Console.Write("\n");
			Console.Write("   i    U(i)          H(U)(i)       Hinv(H(U))(i)\n");
			Console.Write("\n");
			for (i = 0; i < n; i++)
			{
				Console.Write("  ");
				Console.Write("{0,2}", i);
				Console.Write("{0,2}", "  ");
				Console.Write("{0,10:N6}", u[i]);
				Console.Write("{0,4}", "  ");
				Console.Write("{0,10:N6}", v[i]);
				Console.Write("{0,4}", "  ");
				Console.Write("{0,10:N6}", w[i]);
				Console.Write("{0,4}", "\n");
			}
			u = null;
			v = null;
			w = null;

			//
			//  Quadratic data.
			//
			n = 8;
			u = new double[n];
			u[0] = 25.0;
			u[1] = 16.0;
			u[2] = 9.0;
			u[3] = 4.0;
			u[4] = 1.0;
			u[5] = 0.0;
			u[6] = 1.0;
			u[7] = 4.0;
			v = HaarTransform.r8vec_copy_new (n, u);

			HaarTransform.haar_1d (n, v);

			w = HaarTransform.r8vec_copy_new (n, v);
			HaarTransform.haar_1d_inverse (n, w);

			Console.Write("\n");
			Console.Write("   i    U(i)          H(U)(i)       Hinv(H(U))(i)\n");
			Console.Write("\n");
			for (i = 0; i < n; i++)
			{
				Console.Write("  ");
				Console.Write("{0,2}", i);
				Console.Write("{0,2}", "  ");
				Console.Write("{0,10:N6}", u[i]);
				Console.Write("{0,4}", "  ");
				Console.Write("{0,10:N6}", v[i]);
				Console.Write("{0,4}", "  ");
				Console.Write("{0,10:N6}", w[i]);
				Console.Write("{0,4}", "\n");
			}
			u = null;
			v = null;
			w = null;

			return;
		}

		/// <summary>
		/// TEST02 tests HAAR_2D and HAAR_2D_INVERSE.
		/// </summary>
		public static void test02()
		{
			int m = 16;
			int n = 4;
			int seed;
			double[] u;
			double[] v;
			double[] w;

			Console.Write("\n");
			Console.Write("TEST02\n");
			Console.Write("  HAAR_2D computes the Haar transform of an array.\n");
			Console.Write("  HAAR_2D_INVERSE inverts the transform.\n");
			
			//
			//  Demonstrate successful inversion.
			//
			seed = 123456789;
			u = HaarTransform.r8mat_uniform_01_new (m, n, ref seed);

			HaarTransform.r8mat_print (m, n, u, "  Input array U:");

			v = HaarTransform.r8mat_copy_new (m, n, u);

			HaarTransform.haar_2d (m, n, v);

			HaarTransform.r8mat_print (m, n, v, "  Transformed array V:");

			w = HaarTransform.r8mat_copy_new (m, n, v);

			HaarTransform.haar_2d_inverse (m, n, w);

			HaarTransform.r8mat_print (m, n, w, "  Recovered array W:");

			u = null;
			v = null;
			w = null;

			return;
		}
	}
}
