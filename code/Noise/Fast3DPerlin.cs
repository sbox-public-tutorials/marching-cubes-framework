using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Sandbox;


namespace MarchingCubes
{
    class Fast3DPerlin
	{
		private int seed;
		public Fast3DPerlin(int seed)
		{
			seed = seed;
		}

		public float getValue(float x, float y, float z)
		{
			Log.Info( Vector<float>.Count );
			Vector<float> position = new Vector<float>( new float[] { x, y, z} );
			Vector<int> gridPosition = Vector.ConvertToInt32( position );


			return 0.0f;
		}

		private int getPositionalSeed( Vector<int> gridPosition)
		{
			//int h = seed;
			uint h = 2166136261;
			h *= 16777619;
			h ^= (uint)seed;
			for ( int i = 0; i < 3; i++ )
			{
				h *= 16777619;
				h ^= (uint)gridPosition[i];
			}
			return (int)h;
		}
	}
}
