/***********************************************************************
 
Copyright (c) 2011 Alex Zaitsev
All rights reserved.
 
DCT & IDCT C# implementation
 
You can use this code freely for any commercial or non-commercial
purpose. However if you use this code in your program, you should
add the string "Contains code by Alex Zaitsev, www.az3749.narod.ru"
in your copyright notice text.
 ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

public class DctLessOptimized {
	double[][] c;
	double[] w;
	IEnumerable<int> Rn;
	
	public DctLessOptimized(int N)
	{
		Rn = Enumerable.Range(0, N);
		var N2 = N * 2;
		c = Rn.Select(n => Rn.Select(k => Math.Cos((Math.PI * ((2 * n + 1) * k)) / N2)).ToArray()).ToArray();
		w = Enumerable.Repeat(Math.Sqrt(2.0 / N), N).ToArray();
		w[0] = Math.Sqrt(1.0 / N);
	}
	
	public double[] forward(double[] y)
	{
		var t = Rn.Select(k => w[k] * Rn.Select(n => c[n][k] * y[n]).Sum()).ToArray();
		return t;
	}
	
	public double[] backward(double[] y)
	{
		var t = Rn.Select(k => Rn.Select(n => w[n] * c[k][n] * y[n]).Sum()).ToArray();
		return t;
	}
	
	public double[] Filter(double[] filter, double[] y)
	{
		var fo = forward(y);
		var filtered = Rn.Select(i => fo[i] * filter[i]).ToArray();
		return backward(filtered);
	}
	
	// statics
	public static double[] Forward(double[] y)
	{
		return GetDct(y).forward(y);
	}
	
	public static double[] Backward(double[] y)
	{
		return GetDct(y).backward(y);
	}
	
	static Dictionary<int, DctLessOptimized> Dcts = new Dictionary<int, DctLessOptimized>();
	private static DctLessOptimized GetDct(double[] y)
	{
		if (Dcts == null)
			Dcts = new Dictionary<int, DctLessOptimized>();
		var N = y.Length;
		if (!Dcts.ContainsKey(N))
			Dcts[N] = new DctLessOptimized(N);
		return Dcts[N];
	}
	
	public static double[] HighPassFilter(double[] y, int n)
	{
		var fil = Enumerable.Repeat(0.0, n).Concat(Enumerable.Repeat(1.0, y.Length-n)).ToArray();
		return GetDct(y).Filter(fil,y);
	}
	
	public static double[] LowPassFilter(double[] y, int n)
	{
		var fil = Enumerable.Repeat(1.0, n).Concat(Enumerable.Repeat(0.0, y.Length - n)).ToArray();
		return GetDct(y).Filter(fil, y);
	}
	
	public static void TestDctLessOptimized()
	{
		var y = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		//var dct = new Dct(y.Length);
		//var f = dct.forward(y); // dct
		//var b = dct.backward(y); // idct
		var hy = DctLessOptimized.HighPassFilter(y, 3);
	}
};

