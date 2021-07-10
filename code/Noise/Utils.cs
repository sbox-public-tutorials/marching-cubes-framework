using System;
using Sandbox;

namespace MarchingCubes
{
	public static class Utils
	{
		

		public static double magnitude(double[] vector)
		{
			double accumulator = 0;
			for(int i = 0; i < vector.Length; i++ )
			{
				accumulator += vector[i] * vector[i];
			}
			accumulator = Math.Sqrt( accumulator );
			return accumulator;
		}
	}
}
