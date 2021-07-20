
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
		
		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

		}
	}
}
