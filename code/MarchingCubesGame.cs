
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MarchingCubes
{
	[Library( "minimal" )]
	public partial class MarchingCubesGame : Sandbox.Game
	{
		public MarchingCubesGame()
		{
			if ( IsServer )
			{
				new MarchingCubesHudEntity();
			}

			if ( IsClient )
			{

			}
		}
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new MarchingCubesPlayer();
			client.Pawn = player;

			player.Respawn();
		}
	}

}
