using System;
using System.Collections.Generic;
using System.Numerics;
using Sandbox;

namespace MarchingCubes
{
	public class NDimensionalPerlin
	{
		int dimensions;
		int seed;

		int[][] dimensionaloffsets;

		public NDimensionalPerlin( int dimensions, int seed )
		{
			this.dimensions = dimensions;
			this.seed = seed;

			dimensionaloffsets = generateAllBinaryStrings( dimensions );
		}

		private static int[] add(int[] a, int[] b)
		{
			int[] c = new int[a.Length];
			for ( int i = 0; i < a.Length; i++ )
			{
				c[i] = a[i] + b[i];
			}
			return c;
		}
		
		private static float[] sub(float[] a, int[] b)
		{
			float[] c = new float[a.Length];
			for ( int i = 0; i < a.Length; i++ )
			{
				c[i] = a[i] - b[i];
			}
			return c;
		}

		public float getValue( float[] position ) // dimensions {x,y,z,w. . . .}
		{
			//Vector<int> gridCoords = Vector.ConvertToInt32( position );
			int[] gridCoords = new int[dimensions];
			for(int i = 0; i < dimensions; i++ )
			{
				gridCoords[i] = (int)position[i];
			}


			//Max size of a matrix = 4x4
			//where n is dimensions
			//Number of vectors = 2^n offsets, 2^n gradients
			//Matricies per vector = ceil(n/4), offsets in the rows.
			//You can pack 4 vectors into each matrix. ceil(2^n / 4) * ceil(n / 4) per side of multiplication.

			//Matrix4x4 a = new Matrix4x4()
			float[] dots = new float[1 << dimensions];
			int size = (int)MathF.Ceiling( dimensions / 4.0f ); // num of matricies
			for (int i = 0; i < 1 << dimensions; i+=4 )
			{


				//Vector<int> pos1 = Vector.Add<int>( gridCoords, dimensionaloffsets[i] );
				//Vector<int> pos2 = Vector.Add<int>( gridCoords, dimensionaloffsets[i + 1] );
				//Vector<int> pos3 = Vector.Add<int>( gridCoords, dimensionaloffsets[i + 2] );
				//Vector<int> pos4 = Vector.Add<int>( gridCoords, dimensionaloffsets[i + 3] );
				int[] pos1 = add( gridCoords, dimensionaloffsets[i  ] );
				int[] pos2 = add( gridCoords, dimensionaloffsets[i+1] );
				int[] pos3 = add( gridCoords, dimensionaloffsets[i+2] );
				int[] pos4 = add( gridCoords, dimensionaloffsets[i+3] );

				//Vector<float>[] offsets = new Vector<float>[4];
				//offsets[0] = Vector.Subtract<float>( position, Vector.ConvertToSingle( pos1 ) );
				//offsets[1] = Vector.Subtract<float>( position, Vector.ConvertToSingle( pos2 ) );
				//offsets[2] = Vector.Subtract<float>( position, Vector.ConvertToSingle( pos3 ) );
				//offsets[3] = Vector.Subtract<float>( position, Vector.ConvertToSingle( pos4 ) );

				float[][] offsets = new float[4][];
				offsets[0] = sub( position, pos1 );
				offsets[1] = sub( position, pos2 );
				offsets[2] = sub( position, pos3 );
				offsets[3] = sub( position, pos4 );


				float[][] gradients = new float[4][];
				gradients[0] = generateNDimensionalVector( getPositionalSeed( pos1 ) );
				gradients[1] = generateNDimensionalVector( getPositionalSeed( pos2 ) );
				gradients[2] = generateNDimensionalVector( getPositionalSeed( pos3 ) );
				gradients[3] = generateNDimensionalVector( getPositionalSeed( pos4 ) );

				Matrix4x4[] horizontal= new Matrix4x4[size];
				Matrix4x4[] vertical  = new Matrix4x4[size];

				for(int j = 0; j < dimensions; j+=4 )
				{
					int whichMatrix = j / 4;
					horizontal[whichMatrix].M11 = offsets[0][j];
					horizontal[whichMatrix].M21 = offsets[1][j];
					horizontal[whichMatrix].M31 = offsets[2][j];
					horizontal[whichMatrix].M41 = offsets[3][j];

					vertical[whichMatrix].M11 = gradients[0][j];
					vertical[whichMatrix].M12 = gradients[1][j];
					vertical[whichMatrix].M13 = gradients[2][j];
					vertical[whichMatrix].M14 = gradients[3][j];

					if (j+1 < dimensions)
					{
						horizontal[whichMatrix].M12 = offsets[0][j + 1];
						horizontal[whichMatrix].M22 = offsets[1][j + 1];
						horizontal[whichMatrix].M32 = offsets[2][j + 1];
						horizontal[whichMatrix].M42 = offsets[3][j + 1];

						vertical[whichMatrix].M21 = gradients[0][j+1];
						vertical[whichMatrix].M22 = gradients[1][j+1];
						vertical[whichMatrix].M23 = gradients[2][j+1];
						vertical[whichMatrix].M24 = gradients[3][j+1];
					}
					else
					{
						horizontal[whichMatrix].M12 = 0;
						horizontal[whichMatrix].M22 = 0;
						horizontal[whichMatrix].M32 = 0;
						horizontal[whichMatrix].M42 = 0;

						vertical[whichMatrix].M21 = 0;
						vertical[whichMatrix].M22 = 0;
						vertical[whichMatrix].M23 = 0;
						vertical[whichMatrix].M24 = 0;
					}
					
					if(j+2 < dimensions)
					{
						horizontal[whichMatrix].M13 = offsets[0][j + 2];
						horizontal[whichMatrix].M23 = offsets[1][j + 2];
						horizontal[whichMatrix].M33 = offsets[2][j + 2];
						horizontal[whichMatrix].M43 = offsets[3][j + 2];

						vertical[whichMatrix].M31 = gradients[0][j + 2];
						vertical[whichMatrix].M32 = gradients[1][j + 2];
						vertical[whichMatrix].M33 = gradients[2][j + 2];
						vertical[whichMatrix].M34 = gradients[3][j + 2];
					}
					else
					{
						horizontal[whichMatrix].M13 = 0;
						horizontal[whichMatrix].M23 = 0;
						horizontal[whichMatrix].M33 = 0;
						horizontal[whichMatrix].M43 = 0;

						vertical[whichMatrix].M31 = 0;
						vertical[whichMatrix].M32 = 0;
						vertical[whichMatrix].M33 = 0;
						vertical[whichMatrix].M34 = 0;
					}
					
					if(j+3 < dimensions)
					{
						horizontal[whichMatrix].M14 = offsets[0][j + 3];
						horizontal[whichMatrix].M24 = offsets[1][j + 3];
						horizontal[whichMatrix].M34 = offsets[2][j + 3];
						horizontal[whichMatrix].M44 = offsets[3][j + 3];

						vertical[whichMatrix].M41 = gradients[0][j + 3];
						vertical[whichMatrix].M42 = gradients[1][j + 3];
						vertical[whichMatrix].M43 = gradients[2][j + 3];
						vertical[whichMatrix].M44 = gradients[3][j + 3];
					}
					else
					{
						horizontal[whichMatrix].M14 = 0;
						horizontal[whichMatrix].M24 = 0;
						horizontal[whichMatrix].M34 = 0;
						horizontal[whichMatrix].M44 = 0;

						vertical[whichMatrix].M41 = 0;
						vertical[whichMatrix].M42 = 0;
						vertical[whichMatrix].M43 = 0;
						vertical[whichMatrix].M44 = 0;
					}
				}

				for (int j = 0; j < size; j++ )
				{
					Matrix4x4 matrix = Matrix4x4.Multiply( horizontal[j], vertical[j] );
					dots[i  ] += matrix.M11;
					dots[i+1] += matrix.M22;
					dots[i+2] += matrix.M33;
					dots[i+3] += matrix.M44;
				}
			}

			for(int i = 0; i < dimensions; i++ )
			{
				float slerpAmount = sCurve( position[i] - MathF.Truncate(position[i]) );
				
				int index = 0;
				for(int j = 0; j < 1 << (dimensions - i); j += 2 )
				{
					dots[index++] = lerpTo( dots[j], dots[j + 1], slerpAmount );
				}
			}
			return dots[0];
		}
		private static float lerpTo(float x, float y, float w)
		{
			return (x * (1 - w)) + (y * w);
		}

		public static float sCurve( float w )
		{
			return (1 * ((w * (w * 6.0f - 15.0f) + 10.0f) * w * w * w));
		}

		public static int[][] generateAllBinaryStrings(int dimensions)
		{
			long totalCount = 1 << dimensions;

			int[][] binaryStrings = new int[totalCount][];

			for( int i = 0; i < totalCount; i++ )
			{
				int[] thingToFill = new int[dimensions];
				for(byte j = 0; j < dimensions; j++ )
				{
					thingToFill[j] = (i & (1 << j)) >> j;
				}
				binaryStrings[i] = thingToFill;

				//binaryStrings[i] = new int[dimensions];
			}
			return binaryStrings;
		}

		/*
		private int[] sumIntVectors(int[] a, int[] b)
		{
			int[] c = new int[a.Length];
			for(int i = 0; i < a.Length; i++ )
			{
				c[i] = a[i] + b[i];
			}
			return c;
		}

		private float dotGradient( float[] position, int[] gridPosition )
		{
			float[] vector = generateNDimensionalVector( getPositionalSeed( gridPosition ), dimensions );

			float[] offsets = new float[dimensions];
			for(int i = 0; i < dimensions; i++ )
			{
				//Log.Info( "" );
				offsets[i] = ((float)((gridPosition[i] * gridSize) - position[i])) / maxLengthOfGrid;
				//Log.Info( gridPosition[i]*gridSize + " vs " + position[i] + " equals : " + offsets[i] );
			}

			return dot( offsets, vector );
		}

		public static float dot( float[] a, float[] b )
		{
			float accumulator = 0;
			for ( int i = 0; i < a.Length; i++ )
			{
				accumulator += a[i] * b[i];
			}
			return accumulator;
		}
		*/
		private int getPositionalSeed( int[] gridPosition )
		{
			//int h = seed;
			uint h = 2166136261;
			h *= 16777619;
			h ^= (uint) seed;
			for ( int i = 0; i < dimensions; i++ )
			{
				h *= 16777619;
				h ^= (uint) gridPosition[i];
			}
			return (int) h;
		}

		public float[] generateNDimensionalVector( int seed )
		{
			Random r = new Random( seed );
			if ( dimensions == 1 )
			{
				return new float[] { (r.Next() - 0.5f) * 2 };
				//return new Vector<float>( (r.Next() - 0.5f) * 2 );
			}
			float[] vector = new float[dimensions];
			float[] angles = new float[dimensions - 1];
			for ( int i = 0; i < angles.Length; i++ )
			{
				angles[i] = r.Next() * 2 * MathF.PI;
			}

			for ( int i = 1; i <= dimensions; i++ ) // [1,n]
			{
				float accumulator = 1;

				for ( int j = 1; j <= i; j++ )
				{
					if ( i == j )
					{
						accumulator *= MathF.Cos( angles[j - 1] );
					}
					else if ( i == dimensions && j == dimensions - 1 )
					{
						accumulator *= MathF.Sin( angles[j - 1] );
						break;
					}
					else
					{
						accumulator *= MathF.Sin( angles[j - 1] );
					}
				}
				vector[i - 1] = accumulator;
			}
			return vector;
		}
	}
}
