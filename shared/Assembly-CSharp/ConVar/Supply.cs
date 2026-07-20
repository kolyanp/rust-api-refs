using UnityEngine;

namespace ConVar;

[Factory("supply")]
public class Supply : ConsoleSystem
{
	private const string path = "assets/prefabs/npc/cargo plane/cargo_plane.prefab";

	[ServerVar(Help = "(Generated) Spawns a supply drop at the calling admin or player position; the drop falls from the sky with a parachute like a naturally occurring airdrop")]
	public static void drop(Arg arg)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			Debug.Log((object)"Supply Drop Inbound");
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/cargo plane/cargo_plane.prefab");
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				((Component)baseEntity).GetComponent<CargoPlane>().InitDropPosition(((Component)basePlayer).transform.position + new Vector3(0f, 10f, 0f));
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar(Help = "(Generated) Calls in a supply drop to a specific grid coordinate or position; useful for testing supply crate loot tables and airdrop pathing")]
	public static void call(Arg arg)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ArgEx.Player(arg)))
		{
			Debug.Log((object)"Supply Drop Inbound");
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/cargo plane/cargo_plane.prefab");
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				baseEntity.Spawn();
			}
		}
	}
}
