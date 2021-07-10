using Sandbox.UI;

namespace MarchingCubes
{
	public partial class MarchingCubesHudEntity : Sandbox.HudEntity<RootPanel>
	{
		public MarchingCubesHudEntity()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "/UI/MarchingCubesHud.html" );
			}
		}
	}

}
