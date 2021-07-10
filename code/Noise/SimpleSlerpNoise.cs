using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Sandbox;

namespace MarchingCubes
{
	//This is the class I am using to generate noise values.
	class SimpleSlerpNoise
	{
		int seed;
		int octaves;
		int[] valuesPerGrid;
		float[] percentagesPerOctave;
		public Stopwatch timer;
		public SimpleSlerpNoise( int seed, int[] valuesPerGrid, float[] percentagesPerOctave)
		{
			if ( valuesPerGrid.Length != percentagesPerOctave.Length )
			{
				throw new Exception( "Array sizes must be equal" );
			}
			this.seed = seed;
			this.octaves = valuesPerGrid.Length;
			this.valuesPerGrid = valuesPerGrid;
			this.percentagesPerOctave = percentagesPerOctave;
			timer = new Stopwatch();
		}
		public float getValue(int x, int y, int z)
		{
			float accumulation = 0;
			for(int octaveNum = 0; octaveNum < octaves; octaveNum++ )
			{
				int octaveValuesPerGrid = valuesPerGrid[octaveNum];
				int gridX = x / octaveValuesPerGrid;
				int gridY = y / octaveValuesPerGrid;
				int gridZ = z / octaveValuesPerGrid;

				int val000 = getGridValue(gridX, gridY, gridZ);
				int val100 = getGridValue(gridX + 1, gridY, gridZ);
				int val010 = getGridValue(gridX, gridY + 1, gridZ);
				int val110 = getGridValue(gridX + 1, gridY + 1, gridZ);
				int val001 = getGridValue(gridX, gridY, gridZ + 1);
				int val101 = getGridValue(gridX + 1, gridY, gridZ + 1);
				int val011 = getGridValue(gridX, gridY + 1, gridZ + 1);
				int val111 = getGridValue(gridX + 1, gridY + 1, gridZ + 1);

				float offsetX = NDimensionalPerlin.sCurve( (x - (gridX * octaveValuesPerGrid)) / (float)octaveValuesPerGrid );
				float offsetY = NDimensionalPerlin.sCurve( (y - (gridY * octaveValuesPerGrid)) / (float)octaveValuesPerGrid );
				float offsetZ = NDimensionalPerlin.sCurve( (z - (gridZ * octaveValuesPerGrid)) / (float)octaveValuesPerGrid );
				float inverseOffsetX = 1 - offsetX;
				float inverseOffsetY = 1 - offsetY;
				float inverseOffsetZ = 1 - offsetZ;

				float val00 = (val000 * inverseOffsetX) + (val100 * offsetX);
				float val10 = (val010 * inverseOffsetX) + (val110 * offsetX);
				float val01 = (val001 * inverseOffsetX) + (val101 * offsetX);
				float val11 = (val011 * inverseOffsetX) + (val111 * offsetX);

				float val0 = (val00 * inverseOffsetY) + (val10 * offsetY);
				float val1 = (val01 * inverseOffsetY) + (val11 * offsetY);

				float val = (val0 * inverseOffsetZ) + (val1 * offsetZ);
				//This value will be between [-2147m, 2147m]
				val /= int.MaxValue;
				//Convert to  [-1,1]
				accumulation += val * percentagesPerOctave[octaveNum];
			}
			return accumulation;
		}

		private int getGridValue(int gridX, int gridY, int gridZ)
		{
			return lcg( getPositionalSeed( gridX, gridY, gridZ ) );
		}

		public int lcg( int seed )
		{
			//Random prime numbers seem to work
			return lcg( 102191, 104047, 3, seed );
		}

		//Linear congruent generator
		public int lcg( int a, int c, int n, int n0 )
		{
			if ( n == 0 )
			{
				return n0;
			}
			else
			{
				return ((a * lcg( a, c, n - 1, n0 )) + c);
			}
		}

		//For inputs between x=[0,1] returns a smooth curve between y=[0,1]
		//With x=0 and x=1 having slopes of zero
		public static float sCurve( float x )
		{
			return (1 * ((x * (x * 6.0f - 15.0f) + 10.0f) * x * x * x));
		}

		//FNV-1 Hash
		//Good for getting different values for each position,
		//but positions that are close on some axis get similar values,
		//so for randomness the LCG must be used as well. with this as a seed.
		private int getPositionalSeed( int x, int y, int z )
		{
			uint h = 2166136261;
			h *= 16777619;
			h ^= (uint)seed; 
			h *= 16777619;
			h ^= (uint)x;
			h *= 16777619;
			h ^= (uint)y;
			h *= 16777619;
			h ^= (uint)z;
			return (int)h;
		}
	}
}
