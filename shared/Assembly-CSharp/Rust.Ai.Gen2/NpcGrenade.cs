using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcGrenade : BaseEntity
{
	public GameObjectRef explosionEffect;

	public GameObjectRef fireballPrefab;

	public float speed = 10f;

	[NonSerialized]
	public NpcGrenadePositionHint grenadeHint;

	private double spawnTime;

	public override void ServerInit()
	{
		base.ServerInit();
		spawnTime = Time.realtimeSinceStartupAsDouble;
	}

	private void Update()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer && !base.IsDestroyed)
		{
			Vector3 val = ((Component)grenadeHint).transform.position + 1.8f * Vector3.up;
			Vector3 position = grenadeHint.landingPoint.position;
			Vector3 val2 = Vector3Ex.WithY((val + position) * 0.5f, Mathf.Max(val.y, position.y) + grenadeHint.apexHeight);
			float num = Vector3Ex.MagnitudeXZ(position - val) / speed;
			float num2 = (float)(Time.realtimeSinceStartupAsDouble - spawnTime) / num;
			num2 = Mathf.Clamp01(num2);
			((Component)this).transform.position = Vector3.Lerp(Vector3.Lerp(val, val2, num2), Vector3.Lerp(val2, position, num2), num2);
			if (num2 >= 1f)
			{
				FlameExplode();
				Kill();
			}
		}
	}

	public void FlameExplode(int numToCreate = 5)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position + Vector3.up * 0.3f;
		Effect.server.Run(explosionEffect.resourcePath, val, Vector3.up, null, broadcast: true);
		Collider component = ((Component)this).GetComponent<Collider>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.enabled = false;
		}
		SpawnFireball(val);
		for (int i = 0; i < numToCreate; i++)
		{
			Vector3 val2 = Quaternion.Euler(0f, (float)i / (float)numToCreate * 360f, 0f) * Vector3.forward * 1.8f * Random.Range(0.8f, 1.2f);
			Vector3 spawnPos = val + val2;
			if (GamePhysics.Trace(new Ray(val, val2), 0f, out var hitInfo, ((Vector3)(ref val2)).magnitude, 1237003025, (QueryTriggerInteraction)0))
			{
				spawnPos = ((RaycastHit)(ref hitInfo)).point - ((Vector3)(ref val2)).normalized * 0.5f;
			}
			SpawnFireball(spawnPos);
		}
	}

	private void SpawnFireball(Vector3 spawnPos)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameManager.server.CreateEntity(fireballPrefab.resourcePath, spawnPos);
		if (Object.op_Implicit((Object)(object)baseEntity))
		{
			float num = Random.Range(0f, 360f);
			Quaternion val = Quaternion.Euler(0f, num, 0f);
			((Component)baseEntity).transform.SetPositionAndRotation(spawnPos, val);
			baseEntity.creatorEntity = (((Object)(object)creatorEntity == (Object)null) ? baseEntity : creatorEntity);
			baseEntity.Spawn();
		}
	}

	public static bool SimulatePositionAtTime(Vector3 startPos, Vector3 endPos, float speed, float elapsedTime, out Vector3 pos, float gravity = -9.81f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = endPos - startPos;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(val.x, val.z);
		float magnitude = ((Vector2)(ref val2)).magnitude;
		float num;
		Vector3 val3;
		if (magnitude < 0.001f)
		{
			num = 0.25f;
			val3 = Vector3.zero;
		}
		else
		{
			Vector3 val4 = new Vector3(val.x, 0f, val.z);
			val3 = ((Vector3)(ref val4)).normalized;
			num = Mathf.Max(0.0001f, magnitude / speed);
		}
		float num2 = Mathf.Min(elapsedTime, num);
		pos = startPos;
		pos += val3 * (speed * num2);
		float num3 = (endPos.y - startPos.y - 0.5f * gravity * num * num) / num;
		pos.y = startPos.y + num3 * num2 + 0.5f * gravity * num2 * num2;
		return elapsedTime >= num;
	}
}
