using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Sandbox;

namespace MarchingCubes
{
	//This is the class I am using to generate noise values, technically anything works.
	//If you want to help with this, give a read through and maybe try looking at getGridValue()
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
			//timer.Start();
			//Random r = new Random(getPositionalSeed(gridX, gridY, gridZ));
			//int random = r.Next();
			//timer.Stop();

			//This function will be MUCH faster if you can remove the new Random() and still get relatively normal numbers
			//Perhaps implementing my own LCG function or something. Or a hash function that looks nearly-random.
			int positionalSeed = getPositionalSeed( gridX, gridY, gridZ );

			return (new Random( positionalSeed ).Next() - (int.MaxValue/2)) * 2; 
			//Have to do some math weirdness to get to [-maxInt, maxInt]

			//return positionalSeed;
		}

		public static float sCurve( float w )
		{
			return (1 * ((w * (w * 6.0f - 15.0f) + 10.0f) * w * w * w));
		}

		//FNV-1 Hash
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
