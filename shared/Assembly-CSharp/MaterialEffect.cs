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
	}

	public GameObjectRef DefaultEffect;

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

	public void SpawnOnRay(Ray ray, int mask, float length = 0.5f, Vector3 forward = default(Vector3), float speed = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		if (!GamePhysics.Trace(ray, 0f, out var hitInfo, length, mask, (QueryTriggerInteraction)0))
		{
			Effect.client.Run(DefaultEffect.resourcePath, ((Ray)(ref ray)).origin, ((Ray)(ref ray)).direction * -1f, forward);
			if ((Object)(object)DefaultSoundDefinition != (Object)null)
			{
				PlaySound(DefaultSoundDefinition, ((RaycastHit)(ref hitInfo)).point, speed);
			}
			return;
		}
		WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(((Ray)(ref ray)).origin, waves: true, volumes: false);
		if (waterInfo.isValid)
		{
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(((Ray)(ref ray)).origin.x, waterInfo.surfaceLevel, ((Ray)(ref ray)).origin.z);
			Entry waterEntry = GetWaterEntry();
			if (submergedWaterDepth > 0f && waterInfo.currentDepth >= submergedWaterDepth)
			{
				waterEntry = submergedWaterEntry;
			}
			else if (deepWaterDepth > 0f && waterInfo.currentDepth >= deepWaterDepth)
			{
				waterEntry = deepWaterEntry;
			}
			if (waterEntry != null)
			{
				Effect.client.Run(waterEntry.Effect.resourcePath, val, Vector3.up);
				if ((Object)(object)waterEntry.SoundDefinition != (Object)null)
				{
					PlaySound(waterEntry.SoundDefinition, val, speed);
				}
			}
			return;
		}
		PhysicsMaterial materialAt = ColliderEx.GetMaterialAt(((RaycastHit)(ref hitInfo)).collider, ((RaycastHit)(ref hitInfo)).point);
		Entry entryFromMaterial = GetEntryFromMaterial(materialAt);
		if (entryFromMaterial == null)
		{
			Effect.client.Run(DefaultEffect.resourcePath, ((RaycastHit)(ref hitInfo)).point, ((RaycastHit)(ref hitInfo)).normal, forward);
			if ((Object)(object)DefaultSoundDefinition != (Object)null)
			{
				PlaySound(DefaultSoundDefinition, ((RaycastHit)(ref hitInfo)).point, speed);
			}
		}
		else
		{
			Effect.client.Run(entryFromMaterial.Effect.resourcePath, ((RaycastHit)(ref hitInfo)).point, ((RaycastHit)(ref hitInfo)).normal, forward);
			if ((Object)(object)entryFromMaterial.SoundDefinition != (Object)null)
			{
				PlaySound(entryFromMaterial.SoundDefinition, ((RaycastHit)(ref hitInfo)).point, speed);
			}
		}
	}

	public void PlaySound(SoundDefinition definition, Vector3 position, float velocity = 0f)
	{
	}
}
