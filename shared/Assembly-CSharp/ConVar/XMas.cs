using UnityEngine;

namespace ConVar;

[Factory("xmas")]
public class XMas : ConsoleSystem
{
	private const string path = "assets/prefabs/misc/xmas/xmasrefill.prefab";

	[ServerVar(Help = "(Generated) Enables the Christmas event on the server, activating Christmas-themed loot spawns, trees, and holiday gift mechanics")]
	public static bool enabled = false;

	[ServerVar(Help = "(Generated) Radius in metres around each player within which Christmas gift entities are spawned during the xmas event refill")]
	public static float spawnRange = 40f;

	[ServerVar(Help = "(Generated) Number of spawn attempts made per player when trying to place Christmas gifts during refill; higher values increase fill reliability in cluttered areas")]
	public static int spawnAttempts = 5;

	[ServerVar(Help = "(Generated) Target number of gift entities to maintain per connected player during the xmas event; controls overall gift density on the server")]
	public static int giftsPerPlayer = 2;

	[ServerVar(Help = "(Generated) Manually triggers a Christmas gift spawn pass, filling the world with gifts up to the giftsPerPlayer target for all connected players")]
	public static void refill(Arg arg)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/misc/xmas/xmasrefill.prefab");
		if (Object.op_Implicit((Object)(object)baseEntity))
		{
			baseEntity.Spawn();
		}
	}
}
