
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MarchingCubes
{
	[Library( "minimal" )]
	public partial class MarchingCubesGame : Sandbox.Game
	{
		[Net] List<bool> Map { get; set; }
		float timer;
		public MarchingCubesGame()
		{
			if ( IsServer )
			{
				new MarchingCubesHudEntity();
				timer = Time.Now;
				Map = generateRandomMap();
			}

			if ( IsClient )
			{

			}
		}

		public static int get3Dto1DarrayIndex(int x, int y, int z, int xSize, int ySize, int zSize)
		{
			return (z * xSize * ySize) + (y * xSize) + x;
		}

		private static List<bool> generateRandomMap()
		{
			List<bool> newMap = new List<bool>( 32 * 32 * 32 );
			Log.Info( newMap.Count );

			SimpleSlerpNoise ssn = new SimpleSlerpNoise( 0, new int[] { 1, 8, 16 }, new float[] { 0.01f, 0.25f, 0.74f } );

			for ( int z = 0; z < 32; z++ )
			{
				for ( int y = 0; y < 32; y++ )
				{
					for ( int x = 0; x < 32; x++ )
					{
						//float val = ssn.getValue( x, y, z );
						//bool output = val < ((1.0f / ((z) + 5f)) * 6.3f) - 0.05f;
						newMap.Add( false );
					}
				}
			}
			return newMap;
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new MarchingCubesPlayer();
			client.Pawn = player;

			player.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
			if(IsClient)
			{

			}
			if(IsServer)
			{
				if ( Time.Now - timer > 0.5f )
				{
					//Flip a random bit every 5 seconds
					int x = Rand.Int( 0, 127 );
					int y = Rand.Int( 0, 127 );
					int z = Rand.Int( 0, 127 );
					int index = get3Dto1DarrayIndex( x, y, z, 128, 128, 128 );
					Map[index] = !Map[index];
					Log.Info( "change" );
					timer = Time.Now;
				}
			}
		}

		private void OnMapChanged()
		{
			Log.Info( "chang1" );
		}
	}

}
