using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Diagnostics;

namespace MarchingCubes
{
	partial class MarchingCubesPlayer : Player
	{
		private const int maxVertexCount = 1 << 16;
		private int count;

		public override void Respawn()
		{
			count = 0;
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new WalkController();

			Animator = new StandardPlayerAnimator();

			Camera = new ThirdPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			SimulateActiveChild( cl, ActiveChild );

			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				/*
				if(IsClient)
				{
					Stopwatch stopwatch = new Stopwatch();
					SimpleSlerpNoise ssn = new SimpleSlerpNoise( 0, new int[] { 8 }, new float[] { 1f } );
					stopwatch.Start();
					float summation = 0;
					for ( int i = 0; i < 500_000; i++ )
					{
						summation += ssn.getValue( i, 0, 0 );
					}
					stopwatch.Stop();
					Log.Info( summation );
					Log.Info( "1 Took: " + stopwatch.ElapsedMilliseconds + " ms." );
					Log.Info( "1 Timer took: " + ssn.timer.ElapsedMilliseconds + " ms." );

					Stopwatch stopwatch2 = new Stopwatch();
					stopwatch2.Start();
					float summation2 = 0;
					for ( int i = 0; i < 100_000; i++ )
					{
						summation2 += Noise.Perlin( (float)i / 100f, 0.0f, 0.0f );
					}
					stopwatch2.Stop();
					Log.Info( summation2 );
					Log.Info( "2 Took: " + stopwatch2.ElapsedMilliseconds + " ms." );

				}
				*/
				Vector3 position = EyePos + (EyeRot.Forward * 128);
				position = new Vector3( MathF.Floor( position.x ), MathF.Floor( position.y ), MathF.Floor( position.z ) );

				Stopwatch sw = new Stopwatch();
				Stopwatch sw2 = new Stopwatch();
				sw.Start();
				//NDimensionalPerlin perlin = new NDimensionalPerlin( 3, 271, 16 );
				count++;
				SimpleSlerpNoise ssn = new SimpleSlerpNoise( count, new int[] { 2, 8, 16 }, new float[] { 0.15f, 0.25f, 0.60f } );
				//SimpleSlerpNoise ssn = new SimpleSlerpNoise( count, new int[] { 4 }, new float[] { 1.0f } );
				bool[,,] points = new bool[16, 16, 16];
				float max = -999;
				float min = 999;
				for ( int i = 0; i < points.GetLength( 0 ); i++ )
				{
					for ( int j = 0; j < points.GetLength( 1 ); j++ )
					{
						for ( int k = 0; k < points.GetLength( 2 ); k++ )
						{
							//Just testing a bunch of ways to generate noise.

							//points[i, j, k] = Rand.Int( 0, 6 ) == 0;
							//points[i, j, k] = Noise.Perlin( Rand.Float() * i / 11f, Rand.Float() * j / 11f, Rand.Float() * k / 11f )  > 0.25f;
							//points[i, j, k] = Vector3.DistanceBetween( new Vector3( 5, 5, 5 ), new Vector3( i, j, k ) ) < 5;

							//points[i, j, k] = perlin.getValue( new double[] { i, j, k } ) > 0f;
							//points[i, j, k] = ssn.getValue( i, j, k ) < (1.0f / (k + 0.25f)) * 0.5f;
							
							float val = ssn.getValue( i, j, k );
							//float val = Noise.Perlin( i / 16f, j / 16f, k / 16f );
							//points[i, j, k] = Noise.Perlin( i / 100f, j / 100f, k / 100f ) < 0.2f;
							//Log.Info( val + " : " + (val > 0.5f) );

							points[i, j, k] = val > 0.0f;
							//DebugOverlay.Sphere( position + new Vector3( i, j, k ) * 64, 32, new Color(val, val, val), true, 20f );
						}
					}
				}
				sw.Stop();
				Log.Info( "Values took " + sw.ElapsedMilliseconds + "ms." );
				sw2.Start();

				Material material = Material.Load( "materials/dev/gray_25.vmat" );
				Mesh mesh = new Mesh( material );
				mesh.CreateVertexBuffer<SuperVertex>(
					maxVertexCount,
					new VertexAttribute[]{
						new VertexAttribute( VertexAttributeType.Position, VertexAttributeFormat.Float32, 3),
						new VertexAttribute( VertexAttributeType.Tangent,  VertexAttributeFormat.Float32, 3),
						new VertexAttribute( VertexAttributeType.Normal,   VertexAttributeFormat.Float32, 3),
						new VertexAttribute( VertexAttributeType.Color,    VertexAttributeFormat.Float32, 4),
						new VertexAttribute( VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2)
					}
				);
				List<SuperVertex> verticies = new List<SuperVertex>();
				List<Vector3> collisionVerticies = new List<Vector3>();
				int numTris = 0;

				for ( int i = 0; i < points.GetLength( 0 ) - 1; i++ )
				{
					for ( int j = 0; j < points.GetLength( 1 ) - 1; j++ )
					{
						for ( int k = 0; k < points.GetLength( 2 ) - 1; k++ )
						{
							int index = 0;

							if ( points[i + 1, j + 1, k + 0] )
							{
								index |= 1;
							}
							if ( points[i + 1, j + 0, k + 0] )
							{
								index |= 2;
							}
							if ( points[i + 0, j + 0, k + 0] )
							{
								index |= 4;
							}
							if ( points[i + 0, j + 1, k + 0] )
							{
								index |= 8;
							}

							if ( points[i + 1, j + 1, k + 1] )
							{
								index |= 16;
							}
							if ( points[i + 1, j + 0, k + 1] )
							{
								index |= 32;
							}
							if ( points[i + 0, j + 0, k + 1] )
							{
								index |= 64;
							}
							if ( points[i + 0, j + 1, k + 1] )
							{
								index |= 128;
							}

							bool flag = true;
							for ( int l = 0; l < 16 && flag; l++ )
							{
								int triIndex = Triangulation.vertexTable[index, l];
								//Log.Info( "triindex: " + triIndex )
								if ( triIndex == -1 )
								{
									flag = false;
								}
								else
								{
									//Log.Info( l );
									Vector3 pos = (Triangulation.offsets[triIndex] * 64) + (new Vector3(i, j, k) * 64);
									collisionVerticies.Add( pos );
									Vector3 norm = Triangulation.offsets[triIndex].Normal;
								
									Vector3 basePos = position + pos;
									//DebugOverlay.Line( basePos, basePos + (norm.Normal * 32), 9999f, true );

									//Vector3 tan = Vector3.Cross( norm, Vector3.Up );
									Vector3 tan;
									if(norm.Equals(Vector3.Up))
									{
										tan = Vector3.Cross( norm, Vector3.Right );
									}
									else
									{
										tan = Vector3.Cross( norm, Vector3.Up );
									}
									Color c = Color.White;
									Vector2 texCoords = randomVector2( Rand.Int( 0, 9999 ) );
									SuperVertex vertex = new SuperVertex( pos, tan, norm, c, texCoords );
									verticies.Add( vertex );
									numTris++;
								}
							}
						}
					}
				}
				//mesh.SetVertexBufferData<SuperVertex>( CollectionsMarshal.AsSpan<SuperVertex>( verticies ) );
				int[] indicies = new int[numTris];

				for ( int j = 0; j < numTris; j++ )
				{
					indicies[j] = j;
				}
				mesh.SetVertexBufferData<SuperVertex>( new Span<SuperVertex>( verticies.ToArray() ) );
				mesh.SetVertexRange( 0, numTris );
				Model model = new ModelBuilder()
					.AddMesh( mesh )
					.AddCollisionMesh( collisionVerticies.ToArray(), indicies )
					.WithMass( 10 )
					.Create();
				ModelEntity e = new ModelEntity();
				e.SetModel( model );
				e.Position = position;
				e.SetupPhysicsFromModel( PhysicsMotionType.Static );
				e.Spawn();

				sw2.Stop();
				Log.Info( "Rest of function took " + sw2.ElapsedMilliseconds + "ms." );
				
				/*
				Random r = new Random();
				int[,,] values = new int[50, 50, 50];
				for(int i = 0; i < values.GetLength(0); i++ )
				{
					for(int j = 0; j < values.GetLength(1); j++ )
					{
						for ( int k = 0; k < values.GetLength( 1 ); k++ )
						{
							values[k, j, i] = r.Next();
						}
					}
				}
				*/
				/*
				Vector3 position = EyePos + (EyeRot.Forward * 128);
				position = new Vector3( MathF.Floor( position.x ), MathF.Floor( position.y ), MathF.Floor( position.z ) );
				int valuesPerGrid = r.Next(1, 20);
				for ( int i = 0; i < 100; i++ ) // i = z
				{
					for ( int j = 0; j < 100; j++ ) // j = y
					{
						for ( int k = 0; k < 100; k++ ) // k = x
						{
							int gridX = k / valuesPerGrid;
							int gridY = j / valuesPerGrid;
							int gridZ = i / valuesPerGrid;

							int val000 = values[gridX, gridY, gridZ];
							int val100 = values[gridX + 1, gridY, gridZ];
							int val010 = values[gridX, gridY + 1, gridZ];
							int val110 = values[gridX + 1, gridY + 1, gridZ];
							int val001 = values[gridX, gridY, gridZ + 1];
							int val101 = values[gridX + 1, gridY, gridZ + 1];
							int val011 = values[gridX, gridY + 1, gridZ + 1];
							int val111 = values[gridX + 1, gridY + 1, gridZ + 1];

							float offsetX = NDimensionalPerlin.sCurve( (k - (gridX * valuesPerGrid)) / (float)valuesPerGrid );
							float offsetY = NDimensionalPerlin.sCurve( (j - (gridY * valuesPerGrid)) / (float)valuesPerGrid );
							float offsetZ = NDimensionalPerlin.sCurve( (i - (gridZ * valuesPerGrid)) / (float)valuesPerGrid );
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
							val /= int.MaxValue;

							float radius = val;
							if(radius > 0.85f)
							{
								Color c = new Color( radius, radius, radius, 1 );
								DebugOverlay.Sphere( position + new Vector3( k, j, i ) * 64, radius * 32, c, true, 15f );
							}
						}
					}
				}
				*/

				/*
				SimpleSlerpNoise ssn = new SimpleSlerpNoise( ((int)DateTime.Now.Ticks), new int[] { 2, 8, 16 }, new float[] { 0.15f, 0.25f, 0.60f } );
				//Color c = Color.White;
				Vector3 position = EyePos + (EyeRot.Forward * 128);
				for ( int i = 0; i < 16; i++ ) // i = z
				{
					for ( int j = 0; j < 64; j++ ) // j = y
					{
						for ( int k = 0; k < 64; k++ ) // k = x
						{
							float value = ssn.getValue( k, j, i );
							Color c = new Color( value, value, value );
							//Log.Info( value );
							if(value > 0.50f)
							{
								DebugOverlay.Sphere( position + new Vector3( k, j, i ) * 64, value * 32, c, true, 5f );
							}
						}
					}
				}
				*/
			}
		}

		public static Vector3 randomVector3( int seed )
		{
			Random r = new Random( seed );
			double randomAngle = (r.NextDouble() * Math.PI) * 2;
			double randomDistance = r.NextDouble();
			float xAxis = (float)(Math.Cos( randomAngle ) * randomDistance);
			float yAxis = (float)(Math.Sin( randomAngle ) * randomDistance);
			float zAxis = (float)(Math.Sqrt( 1 - Math.Pow( randomDistance, 2.0 ) ));
			return new Vector3( xAxis, yAxis, zAxis );
		}

		public static Vector2 randomVector2( int seed )
		{
			Random r = new Random( seed );
			double randomAngle = (r.NextDouble() * Math.PI * 2);
			float xAxis = (float)(Math.Cos( randomAngle ));
			float yAxis = (float)(Math.Sin( randomAngle ));
			return new Vector2( xAxis, yAxis );
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct SuperVertex
		{
			public Vector3 Position;
			public Vector3 Tangent;
			public Vector3 Normal;
			public Color color;
			public Vector2 TexCoords;

			public SuperVertex( Vector3 position, Vector3 tangent, Vector3 normal, Color color, Vector2 texCoords )
			{
				this.Position = position;
				this.Tangent = tangent;
				this.Normal = normal;
				this.color = color;
				this.TexCoords = texCoords;
			}
		}

		private void debugPrintIndex( int index, Vector3 position)
		{
			//Log.Info( "index: " + index );
			DebugOverlay.Box( 100000.0f, position, position + (new Vector3( 64, 64, 64 )), Color.Blue, true );
			for (int i = 0; i < 16 && (Triangulation.vertexTable[index, i] != -1); i += 3 )
			{
				//Log.Info( Triangulation.vertexTable[ index, i ] );
				//Log.Info( Triangulation.vertexTable[index, i+1] );
				//Log.Info( Triangulation.vertexTable[index, i+2] );

				Vector3 pos1 = position + (Triangulation.offsets[Triangulation.vertexTable[index, i]] * 64);
				Vector3 pos2 = position + (Triangulation.offsets[Triangulation.vertexTable[index, i + 1]] * 64);
				Vector3 pos3 = position + (Triangulation.offsets[Triangulation.vertexTable[index, i + 2]] * 64);
				//Log.Info( (i + 0) + ": " + (Triangulation.offsets[Triangulation.vertexTable[index, i]] * 64));
				//Log.Info( (i + 1) + ": " + (Triangulation.offsets[Triangulation.vertexTable[index, i + 1]] * 64));
				//Log.Info( (i + 2) + ": " + (Triangulation.offsets[Triangulation.vertexTable[index, i + 2]] * 64));

				//DebugOverlay.Line( pos1, pos2, Color.Green, 100000.0f, true );
				//DebugOverlay.Line( pos2, pos3, Color.Green, 100000.0f, true );
				//DebugOverlay.Line( pos3, pos1, Color.Green, 100000.0f, true );
				
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
		}
	}
}
