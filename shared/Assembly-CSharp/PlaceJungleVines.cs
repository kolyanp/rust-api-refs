using ConVar;
using UnityEngine;

public class PlaceJungleVines : PlaceDecorUniform
{
	public override void Process(uint seed)
	{
		if (!Server.spawnVineTrees)
		{
			Debug.LogWarning((object)"server.spawnVineTrees is disabled, skipping vine spawn...");
		}
		else
		{
			base.Process(seed);
		}
	}
}
