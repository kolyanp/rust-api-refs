using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/MaterialEffect")]
public class MaterialEffect : ScriptableObject
{
	[Serializable]
	public class Entry
	{
		public PhysicsMaterial Material;

		public GameObjectRef Effect;

		public SoundDefinition SoundDefinition;

		[NonSerialized]
		public EffectSpawnRule EffectRule;
	}

	public enum EffectSpawnRule
	{
		Unknown,
		Always,
		WhenMusicIsOn,
		Never
	}

	public struct RayGround
	{
		public PhysicsMaterial Material;

		public Vector3 Normal;

		public bool IsTerrain;
	}

	public GameObjectRef DefaultEffect;

	[NonSerialized]
	private EffectSpawnRule defaultEffectRule;

	public SoundDefinition DefaultSoundDefinition;

	public Entry[] Entries;

	public int waterFootstepIndex = -1;

	public Entry deepWaterEntry;

	public float deepWaterDepth = -1f;

	public Entry submergedWaterEntry;

	public float submergedWaterDepth = -1f;

	public bool ScaleVolumeWithSpeed;

	public AnimationCurve SpeedGainCurve;

	public Entry GetEntryFromMaterial(PhysicsMaterial mat)
	{
		Entry[] entries = Entries;
		foreach (Entry entry in entries)
		{
			if ((Object)(object)entry.Material == (Object)(object)mat)
			{
				return entry;
			}
		}
		return null;
	}

	public Entry GetWaterEntry()
	{
		if (waterFootstepIndex == -1)
		{
			for (int i = 0; i < Entries.Length; i++)
			{
				if (((Object)Entries[i].Material).name == "Water")
				{
					waterFootstepIndex = i;
					break;
				}
			}
		}
		if (waterFootstepIndex != -1)
		{
			return Entries[waterFootstepIndex];
		}
		Debug.LogWarning((object)("Unable to find water effect for :" + ((Object)this).name));
		return null;
	}

	public RayGround SpawnOnRay(Ray ray, int mask, float length = 0.5f, Vector3 forward = default(Vector3), float speed = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		if (!GamePhysics.Trace(ray, 0f, out var hitInfo, length, mask, (QueryTriggerInteraction)0))
		{
			if (ShouldSpawnEffect(DefaultEffect, ref defaultEffectRule))
			{
				Effect.client.Run(DefaultEffect.resourcePath, ray.origin, ray.direction * -1f, forward);
			}
			return default;
		}
		WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(ray.origin, waves: true, volumes: false);
		if (waterInfo.isValid)
		{
			Vector3 val = new Vector3(ray.origin.x, waterInfo.surfaceLevel, ray.origin.z);
			Entry waterEntry = GetWaterEntry();
			if (submergedWaterDepth > 0f && waterInfo.currentDepth >= submergedWaterDepth)
			{
				waterEntry = submergedWaterEntry;
			}
			else if (deepWaterDepth > 0f && waterInfo.currentDepth >= deepWaterDepth)
			{
				waterEntry = deepWaterEntry;
			}
			if (waterEntry == null)
			{
				return default;
			}
			Effect.client.Run(waterEntry.Effect.resourcePath, val, Vector3.up);
			if ((Object)(object)waterEntry.SoundDefinition != (Object)null)
			{
				PlaySound(waterEntry.SoundDefinition, val, speed);
			}
			return default;
		}
		PhysicsMaterial materialAt = ColliderEx.GetMaterialAt(hitInfo.collider, hitInfo.point);
		SpawnOnMaterial(materialAt, hitInfo.point, hitInfo.normal, forward, speed);
		return new RayGround
		{
			Material = materialAt,
			Normal = hitInfo.normal,
			IsTerrain = (hitInfo.collider is TerrainCollider)
		};
	}

	public void SpawnOnTerrain(Vector3 position, Vector3 forward, float speed)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		position.y = TerrainMeta.HeightMap.GetHeight(position);
		SpawnOnMaterial(TerrainMeta.Physics.GetMaterial(position), position, TerrainMeta.HeightMap.GetNormal(position), forward, speed);
	}

	public void SpawnOnMaterial(PhysicsMaterial material, Vector3 point, Vector3 normal, Vector3 forward, float speed)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Entry entryFromMaterial = GetEntryFromMaterial(material);
		if (entryFromMaterial == null)
		{
			if (ShouldSpawnEffect(DefaultEffect, ref defaultEffectRule))
			{
				Effect.client.Run(DefaultEffect.resourcePath, point, normal, forward);
			}
			if ((Object)(object)DefaultSoundDefinition != (Object)null)
			{
				PlaySound(DefaultSoundDefinition, point, speed);
			}
		}
		else
		{
			if (ShouldSpawnEffect(entryFromMaterial.Effect, ref entryFromMaterial.EffectRule))
			{
				Effect.client.Run(entryFromMaterial.Effect.resourcePath, point, normal, forward);
			}
			if ((Object)(object)entryFromMaterial.SoundDefinition != (Object)null)
			{
				PlaySound(entryFromMaterial.SoundDefinition, point, speed);
			}
		}
	}

	private static bool ShouldSpawnEffect(GameObjectRef effect, ref EffectSpawnRule rule)
	{
		return true;
	}

	public void PlaySound(SoundDefinition definition, Vector3 position, float velocity = 0f)
	{
	}
}
