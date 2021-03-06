using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Diagnostics;

namespace MarchingCubes
{
	//TODO: Map destruction is 1: obviously inefficient.
	//2: Doesnt respawn nearby chunks, so you can have a gap in the mesh.
	partial class MarchingCubesPlayer : Player
	{
		private const int maxVertexCount = 1 << 16;
		Dictionary<(int, int), ModelEntity> generatedMap;
		Dictionary<(int, int, int), bool> overrideMap;

		public MarchingCubesPlayer() : base()
		{
			generatedMap = new Dictionary<(int, int), ModelEntity>();
			overrideMap = new Dictionary<(int, int, int), bool>();
		}

		public override void Respawn()
		{
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

			//Main loop to generate new undiscovered chunks
			if(true)
			{
				//Compute grid-coords for the noise
				int x = (int)(Position.x / 1984);
				int y = (int)(Position.y / 1984);
				int z = (int)(Position.z / 1984);

				//For each grid in a square 5 (2 in each direction)
				for ( int i = -2; i <= 2; i++ )
				{
					for ( int j = -2; j <= 2; j++ )
					{ 
						int xUsing = x + i;
						int yUsing = y + j;

						//Check if a mesh has been generated for that grid point.
						if ( !generatedMap.ContainsKey( (xUsing, yUsing) ) )
						{
							//If not, build the meshes, and add an entry signaling that the coordinate is built.
							ModelEntity e = generateMarchingCubes( new Vector3( xUsing * 1984, yUsing * 1984, 4096 ), (xUsing * 31) + 1, (yUsing * 31) + 1, 1 , (xUsing * 31) + 31, (yUsing * 31) + 31, 62 );
							
							generatedMap[(xUsing, yUsing)] = e;
						}

					}
				}
			}
			
			//Left click to destroy terrain.
			//BUG: this seems to not work when you click into negative positions.
			//Also using eyeRot sucks and doesn't align to player's crosshair.
			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				ThirdPersonCamera tpc = ((ThirdPersonCamera)this.Camera);
				Vector3 pos = tpc.Pos;
				Vector3 dir = tpc.Rot.Forward;

				TraceResult tr = Sandbox.Trace.Ray( EyePos + EyeRot.Forward * 64, EyePos + EyeRot.Forward * 9999 ).Run();

				if ( tr.Hit )
				{
					DebugOverlay.Sphere( tr.EndPos, 5f, Color.White, true, 0.25f );

					int x = (int)(tr.EndPos.x / 64);
					int y = (int)(tr.EndPos.y / 64);
					int z = (int)((tr.EndPos.z-4096) / 64);

					int worldMapX = (int)(tr.EndPos.x / 1984);
					int worldMapY = (int)(tr.EndPos.y / 1984);

					Vector3 thePos = new Vector3( worldMapX * 1984, worldMapY * 1984, 4096 );

					for(int i = -1; i <= 1; i++ )
					{
						for ( int j = -1; j <= 1; j++ )
						{
							for ( int k = -1; k <= 1; k++ )
							{
								overrideMap[(x + k, y + j, z + i)] = false;
							}
						}
					}

					ModelEntity a1 = generatedMap[(worldMapX, worldMapY)];

					ModelEntity e = generateMarchingCubes( thePos, (worldMapX * 31) + 1, (worldMapY * 31) + 1, 1, (worldMapX * 31) + 31, (worldMapY * 31) + 31, 62 );

					generatedMap[(worldMapX, worldMapY)].DeleteAsync( 0.0f );
					generatedMap.Remove( (worldMapX, worldMapY) );

					generatedMap[(worldMapX, worldMapY) ] = e;
				}
			}
		}

		public ModelEntity generateMarchingCubes(Vector3 position, int x0, int y0, int z0, int x1, int y1, int z1)
		{
			//SimpleSlerpNoise ssn = new SimpleSlerpNoise( count, new int[] { 2, 8, 16 }, new float[] { 0.15f, 0.25f, 0.60f } );
			SimpleSlerpNoise ssn = new SimpleSlerpNoise( 0, new int[] { 1, 8, 16 }, new float[] { 0.01f, 0.25f, 0.74f } );
			bool[,,] points = new bool[x1 - x0 + 2, y1 - y0 + 2, z1 - z0 + 2];

			//Uncomment to visualize the bounds of the generated area
			//DebugOverlay.Box( 9999f, position, position + (new Vector3( points.GetLength( 0 ), points.GetLength( 1 ), points.GetLength( 2 ) ) * 64), Color.White, true);
			Stopwatch a = new Stopwatch();
			//a.Start();

			for ( int i = 0; i < points.GetLength( 0 ); i++ )
			{
				for ( int j = 0; j < points.GetLength( 1 ); j++ )
				{
					for ( int k = 0; k < points.GetLength( 2 ); k++ )
					{
						//Global grid coordinates for each voxel.
						int globalX = i + x0 - 1;
						int globalY = j + y0 - 1;
						int globalZ = k + z0 - 1;
						//Log.Info( "Checking: " + globalX + ", " + globalY + ", " + globalZ );

						if ( overrideMap.ContainsKey( (globalX, globalY, globalZ) ) )
						{
							//Log.Info( "Found a hit: " + globalX + ", " + globalY + ", " + globalZ + " : " + overrideMap[(globalX, globalY, globalZ)] );
							points[i, j, k] = overrideMap[(globalX, globalY, globalZ)];
							continue;
						}
						else
						{
							//Noise.Perlin is still noticably faster. 
							//Hopefully something can be done about this.

							//float val = Noise.Perlin( i / 8f, j / 8f, k / 8f );
							float val = ssn.getValue( globalX, globalY, globalZ );
							//Convert [-1,1] range to [0,1]
							val += 1f;
							val /= 2f;

							points[i, j, k] = val < ((1.0f / ((z0 + k) + 5f)) * 7f) - 0.11f;

							//Uncomment this line if you want a giant ball of laggy spheres to visualize the noise.
							//DebugOverlay.Sphere( position + new Vector3( i, j, k ) * 64, 32, new Color(val, val, val), true, 5f );
						}
					}
				}
			}

			//Material material = Material.Load( "materials/dev/gray_25.vmat" );
			Material material = Material.Load( "materials/dev/dev_measuregeneric01.vmat" );
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
			//SuperVertex list is used to build the mesh.
			List<SuperVertex> verticies = new List<SuperVertex>();
			//Vector3 list is used for collisions.
			List<Vector3> collisionVerticies = new List<Vector3>();
			int numTris = 0;

			for ( int i = 0; i < points.GetLength( 0 ) - 1; i++ )
			{
				for ( int j = 0; j < points.GetLength( 1 ) - 1; j++ )
				{
					for ( int k = 0; k < points.GetLength( 2 ) - 1; k++ )
					{
						//The table in Triangulation.vertexTable is composed of
						//256 entries, which is represented as 8 bits, each 
						//representing whether or not the corresponding 3d
						//grid position is active.

						int index = 0;
						//Note: |= 1, 2, etc sets single bits, 0b00000001, 0b00000010
						//while maintaining the other values, in order.
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

						Color debugC = Color.Black;
						Vector3 normalToDraw = Vector3.Zero;

						bool flag = true;
						for ( int l = 0; l < 16 && flag; l++ )
						{
							int triIndex = Triangulation.vertexTable[index, l];
							if ( triIndex == -1 )
							{
								flag = false;
							}
							else
							{
								if ( l % 3 == 0 )
								{
									debugC = new Color( (float)Rand.Double(), (float)Rand.Double(), (float)Rand.Double() );
									Vector3 pos1 = (Triangulation.offsets[Triangulation.vertexTable[index, l + 0]] * 64);
									Vector3 pos2 = (Triangulation.offsets[Triangulation.vertexTable[index, l + 1]] * 64);
									Vector3 pos3 = (Triangulation.offsets[Triangulation.vertexTable[index, l + 2]] * 64);
									normalToDraw = Vector3.Cross(pos2 - pos1, pos3 - pos1).Normal;

									//Uncomment these lines to visualize all face normals.
									/*
									if(i%32 < 16 && j%32 < 16) //You can change this to rendering lag
									{
										Vector3 avgOffset = (pos1 + pos2 + pos3) / 3f;
										Vector3 bp = position + (new Vector3( i, j, k ) * 64);
										DebugOverlay.Line( bp + avgOffset, bp + avgOffset + (normalToDraw * 32), debugC, 9999f, true );
									}
									*/
								}

								Vector3 pos = (Triangulation.offsets[triIndex] * 64) + (new Vector3( i, j, k ) * 64);
								collisionVerticies.Add( pos );
								Vector3 norm = normalToDraw;

								//Uncomment these to visualize all vertex normals.
								//Vector3 basePos = position + pos;
								//DebugOverlay.Line( basePos, basePos + (norm * 32), debugC, 9999f, true );

								Vector3 tan;
								//Since sometimes the normal can be straight up,
								//we cant always cross the normal with Up to get
								//a tangent. So we have to check.
								if ( norm.Equals( Vector3.Up ) || norm.Equals( Vector3.Down ))
								{
									tan = Vector3.Cross( norm, Vector3.Right );
								}
								else
								{
									tan = Vector3.Cross( norm, Vector3.Up );
								}
								Color c = Color.White;
								//Texcoords have yet to be worked out, I figure eventually it will need to be fixed.
								//But at the moment solid colors are fine so I am not going to worry about it.
								Vector2 texCoords = Vector2.Zero;
								SuperVertex vertex = new SuperVertex( pos, tan, norm, c, texCoords );
								verticies.Add( vertex );
								numTris++;
							}
						}
					}
				}
			}
			//Since the collisionVerticies list is in the same order as
			//the visual mesh is, it is in the correct order.
			//Therefore, the indicies for each is just 0, 1, 2. . . 
			//For every vertex.
			int[] indicies = new int[numTris];
			for ( int j = 0; j < numTris; j++ )
			{
				indicies[j] = j;
			}
			//Sometimes there can be no verticies, if this is the case then
			//Building the entity will fail, we have to make sure this doesn't happen.
			if(verticies.Count > 0)
			{
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
				return e;
			}
			return null;

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

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
		}
	}
}
