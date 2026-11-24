using System.Collections.Generic;
using UnityEngine;

public class ConvarTimedPrefabSpawner : BaseMonoBehaviour, IServerComponent
{
	[Tooltip("A random prefab from this list will be spawned each interval")]
	public List<GameObjectRef> prefabsToSpawn = new List<GameObjectRef>();

	[HideInInspector]
	[ServerVar(Help = "Time in seconds between prefab spawns. Set to 0 to disable spawning.")]
	public static float prefab_spawn_interval = 0f;

	[HideInInspector]
	[ServerVar(Saved = true, Help = "Variance in seconds to add/subtract from the spawn interval")]
	public static float prefab_spawn_interval_variance = 3f;

	[HideInInspector]
	[ServerVar(Saved = true, Help = "If true, spawned prefabs will have a random rotation")]
	public static bool prefab_spawn_random_rotation = true;

	[HideInInspector]
	[ServerVar(Saved = true, Help = "Maximum random offset (sphere radius) from the spawner's position when spawning prefabs")]
	public static float prefab_spawn_random_position_offset = 0f;

	[HideInInspector]
	[ServerVar(Saved = true, Help = "Time in seconds before spawned prefabs are despawned. Set to 0 to disable despawning.")]
	public static float prefab_despawn_time = 20f;

	[HideInInspector]
	[ServerVar(Saved = true, Help = "Smallest random uniform scale applied to spawned prefabs")]
	public static float prefab_spawn_scale_min = 1f;

	[ServerVar(Saved = true, Help = "Largest random uniform scale applied to spawned prefabs")]
	[HideInInspector]
	public static float prefab_spawn_scale_max = 1f;

	private TimeSince timeSinceLastSpawn;

	private float randomIntervalOffset;

	public void FixedUpdate()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if (prefab_spawn_interval == 0f || prefabsToSpawn.Count == 0 || !(TimeSince.op_Implicit(timeSinceLastSpawn) >= prefab_spawn_interval + randomIntervalOffset))
		{
			return;
		}
		timeSinceLastSpawn = TimeSince.op_Implicit(0f);
		randomIntervalOffset = Random.Range(0f - prefab_spawn_interval_variance, prefab_spawn_interval_variance);
		GameObjectRef gameObjectRef = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)];
		Vector3 pos = ((Component)this).transform.position + Random.insideUnitSphere * prefab_spawn_random_position_offset;
		Quaternion rot = (prefab_spawn_random_rotation ? Random.rotation : ((Component)this).transform.rotation);
		float num = Random.Range(prefab_spawn_scale_min, prefab_spawn_scale_max);
		if (!SpawnHandler.CheckBounds(gameObjectRef.Get(), pos, rot, Vector3.one * num, LayerMask.op_Implicit(32768)))
		{
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(gameObjectRef.resourcePath, pos, rot);
		if (!((Object)(object)baseEntity != (Object)null))
		{
			return;
		}
		if (num != 1f)
		{
			((Component)baseEntity).transform.localScale = Vector3.one * num;
			baseEntity.networkEntityScale = true;
		}
		baseEntity.Spawn();
		if (prefab_despawn_time > 0f)
		{
			EntityTimedDestroy entityTimedDestroy = ((Component)baseEntity).gameObject.AddComponent<EntityTimedDestroy>();
			if ((Object)(object)entityTimedDestroy != (Object)null)
			{
				entityTimedDestroy.SetTime(prefab_despawn_time);
			}
		}
	}
}
