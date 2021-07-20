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
		public MarchingCubesPlayer() : base()
		{
		}

		public override void Respawn()
		{

			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new WalkController();
			((WalkController)this.Controller).GroundAngle = 60f;

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
				bool[] binData = new bool[1024];
				string s = "";
				Random r = new Random(0);
				for(int i = 0; i < binData.Length; i++ )
				{
					binData[i] = r.Next(0,2) == 0;
					s += binData[i] ? "1" : "0";
				}
				Log.Info( s );
				BinTree<bool> tree = binTreeize<bool>( binData );

				String undone = stringifyBinTree( tree );
				Log.Info( s );
				Log.Info( undone );
				Log.Info( s.Equals( undone ) );
				*/
				SimpleSlerpNoise ssn = new SimpleSlerpNoise( 0, new int[] { 1, 8, 16, 32, 64 }, new float[] { 0.01f, 0.09f, 0.2f, 0.2f, 0.5f } );

				bool[,,][,,] superMap = new bool[16, 16, 4][,,];
				Random r1 = new Random(0);
				for(int i = 0; i < superMap.GetLength( 0 ); i++ )
				{
					for ( int j = 0; j < superMap.GetLength( 1 ); j++ )
					{
						for ( int k = 0; k < superMap.GetLength( 2 ); k++ )
						{
							bool[,,] newMap = new bool[16, 16, 16];
							for ( int x = 0; x < newMap.GetLength( 0 ) - 1; x++ )
							{
								for ( int y = 0; y < newMap.GetLength( 1 ) - 1; y++ )
								{
									if(k>0)
									{
										continue;
									}
									int heightToStart = (int) ( (1 + ssn.getValue(x + (i*16),y + (j * 16),0)) * 16 / 2);
									for(int z = 0; z < 16; z++ )
									{
										newMap[x, y, z] = (z + (k*16)) <= heightToStart;
									}
								}
							}
							superMap[i, j, k] = newMap;
						}
					}
				}
				Stopwatch sw = new Stopwatch();
				Vector3 pos = EyePos + EyeRot.Forward * 128;
				for ( int i = 0; i < superMap.GetLength( 0 ); i++ )
				{
					for ( int j = 0; j < superMap.GetLength( 1 ); j++ )
					{
						for ( int k = 0; k < superMap.GetLength( 2 ); k++ )
						{
							sw.Start();
							march( pos + new Vector3(i,j,k) * 14f * 16f, superMap[i,j,k], 16f );
							sw.Stop();
						}
					}
				}
				Log.Info( sw.ElapsedMilliseconds );
			}
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct Vertex
		{
			public Vector3 Position;
			public Vector3 Tangent;
			public Vector3 Normal;
			public Color color;

			public Vertex( Vector3 position, Vector3 tangent, Vector3 normal, Color color )
			{
				this.Position = position;
				this.Tangent = tangent;
				this.Normal = normal;
				this.color = color;
			}
		}

		public void march(Vector3 pos, bool[,,] map, float voxelSize)
		{
			List<Vertex> verticies = new List<Vertex>();
			List<Vector3> collisionVerticies = new List<Vector3>();
			int numTris = 0;
			for ( int x = 0; x < map.GetLength( 0 ) - 1; x++ )
			{
				for ( int y = 0; y < map.GetLength( 1 ) - 1; y++ )
				{
					for ( int z = 0; z < map.GetLength( 2 ) - 1; z++ )
					{
						int index = 0;
						if ( map[z + 1, y + 1, x + 0] ) index |= 1;
						if ( map[z + 1, y + 0, x + 0] ) index |= 2;
						if ( map[z + 0, y + 0, x + 0] ) index |= 4;
						if ( map[z + 0, y + 1, x + 0] ) index |= 8;
						if ( map[z + 1, y + 1, x + 1] ) index |= 16;
						if ( map[z + 1, y + 0, x + 1] ) index |= 32;
						if ( map[z + 0, y + 0, x + 1] ) index |= 64;
						if ( map[z + 0, y + 1, x + 1] ) index |= 128;

						bool continueFlag = true;
						for(int i = 0; i < 16 && continueFlag; i++ )
						{
							int triIndex = Triangulation.vertexTable[index, i];
							if ( triIndex == -1 )
							{
								continueFlag = false;
							}
							else
							{
								Vector3 vertexPosition = (Triangulation.offsets[triIndex] + new Vector3( z, y, x )) * voxelSize;
								collisionVerticies.Add( vertexPosition );
								Vector3 vertexNormal = Triangulation.normals[index, i / 3];
								
								Vector3 vertexTangent;
								if ( vertexNormal.Equals( Vector3.Up ) || vertexNormal.Equals( Vector3.Down ) )
								{
									vertexTangent = Vector3.Cross( vertexNormal, Vector3.Right );
								}
								else
								{
									vertexTangent = Vector3.Cross( vertexNormal, Vector3.Up );
								}

								Color vertexColor = Color.White;
								//Color vertexColor = new Color( vertexNormal.x, vertexNormal.y, vertexNormal.z );
								Vertex vertex = new Vertex( vertexPosition, vertexTangent, vertexNormal, vertexColor );
								verticies.Add( vertex );
								numTris++;
							}
						}
					}
				}
			}

			Material material = Material.Load( "materials/default/vertex_color.vmat" );
			//Material material = Material.Load( "materials/dev/dev_measuregeneric01.vmat" );
			Mesh mesh = new Mesh( material );
			mesh.CreateVertexBuffer<Vertex>(
				1 << 16,
				new VertexAttribute[]{
						new VertexAttribute( VertexAttributeType.Position, VertexAttributeFormat.Float32, 3),
						new VertexAttribute( VertexAttributeType.Tangent,  VertexAttributeFormat.Float32, 3),
						new VertexAttribute( VertexAttributeType.Normal,   VertexAttributeFormat.Float32, 3),
						new VertexAttribute( VertexAttributeType.Color,    VertexAttributeFormat.Float32, 4)
				},
				new Span<Vertex>( verticies.ToArray() )
			);

			int[] indicies = new int[numTris];
			for ( int j = 0; j < numTris; j++ )
			{
				indicies[j] = j;
			}

			if (numTris > 0)
			{
				//new Span<Vertex>( verticies.ToArray() )
				mesh.SetVertexRange( 0, numTris );
				Model model = new ModelBuilder()
					.AddMesh( mesh )
					.AddCollisionMesh( collisionVerticies.ToArray(), indicies )
					.WithMass( 10 )
					.Create();
				ModelEntity e = new ModelEntity();
				e.SetModel( model );
				e.Position = pos;
				e.SetupPhysicsFromModel( PhysicsMotionType.Static );
				e.Spawn();
			}
		}

		public static string stringifyBinTree(BinTree<bool> tree)
		{
			return stringifyBinTree( tree, 0, 1023 );
		}

		public static string stringifyBinTree(BinTree<bool> tree, int start, int end)
		{
			if(tree.isLeaf)
			{
				string outstring = "";
				string component = tree.Data ? "1" : "0";
				for (int i = start; i <= end; i++ )
				{
					outstring += component;
				}
				return outstring;
			}
			else
			{
				int a = start;
				int b = start + ((end - start + 1) / 2) - 1;
				int c = start + ((end - start + 1) / 2);
				int d = end;
				return stringifyBinTree( tree.child0, a, b ) + stringifyBinTree( tree.child1, c, d );
			}
		}

		public static BinTree<T> binTreeize<T>( T[] arr)
		{
			return binTreeize<T>( arr, 0, arr.Length - 1 );
		}

		public static BinTree<T> binTreeize<T> (T[] arr, int start, int end)
		{
			//Log.Info( (start, end) );
			BinTree<T> thisTree = new BinTree<T>();
			if(!isContinuous<T>(arr, start, end))
			{
				thisTree.isLeaf = false;
				int a = start;
				int b = start + ((end - start + 1) / 2) - 1;
				int c = start + ((end - start + 1) / 2);
				int d = end;

				thisTree.child0 = binTreeize<T>( arr, a, b );
				thisTree.child1 = binTreeize<T>( arr, c, d );
			}
			else
			{
				//Log.Info( "continuous" );
				thisTree.isLeaf = true;
				thisTree.Data = arr[start];
			}
			return thisTree;
		}

		//Both inclusive
		public static bool isContinuous<T>( T[] arr, int start, int end)
		{
			T value1 = arr[start];
			for(int i = start+1; i <= end; i++ )
			{
				if(!arr[i].Equals(value1))
				{
					return false;
				}
			}
			return true;
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
		}
	}
}
