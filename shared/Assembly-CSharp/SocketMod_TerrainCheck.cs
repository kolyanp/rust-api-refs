using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SocketMod_TerrainCheck : SocketMod
{
	public bool wantsInTerrain = true;

	public bool preventWorldLayerInMonuments;

	private static Phrase lastError = new Phrase("", "");

	protected override Phrase ErrorPhrase => lastError;

	public static bool IsInTerrain(Vector3 vPoint, bool worldLayerInMonuments)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		if (TerrainMeta.OutOfBounds(vPoint) && !DeepSeaManager.IsInsideDeepSea(vPoint))
		{
			Ray val = default(Ray);
			((Ray)(ref val))._002Ector(vPoint + Vector3.up * 3f, Vector3.down);
			if (TerrainMeta.IsPointWithinTutorialBounds(vPoint))
			{
				return Physics.Raycast(val, 3f, 65536);
			}
			return false;
		}
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAllUnordered(new Ray(vPoint + Vector3.up * 3f, Vector3.down), 0f, list, 3f, 65536, (QueryTriggerInteraction)0);
		using (List<RaycastHit>.Enumerator enumerator = list.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				RaycastHit current = enumerator.Current;
				if (worldLayerInMonuments)
				{
					Pool.FreeUnmanaged<RaycastHit>(ref list);
					return true;
				}
				if (((Component)((RaycastHit)(ref current)).collider).gameObject.HasCustomTag(GameObjectTag.BlockBarricadePlacement))
				{
					lastError = ConstructionErrors.CantPlaceOnMonument;
					Pool.FreeUnmanaged<RaycastHit>(ref list);
					return false;
				}
				if (((Component)((RaycastHit)(ref current)).collider).gameObject.HasCustomTag(GameObjectTag.AllowBarricadePlacement))
				{
					Pool.FreeUnmanaged<RaycastHit>(ref list);
					return true;
				}
				bool num = (Object)(object)ColliderEx.GetMonument(((RaycastHit)(ref current)).collider) != (Object)null;
				bool flag = (Object)(object)((RaycastHit)(ref current)).collider != (Object)null && GameObjectEx.ToBaseEntity(((RaycastHit)(ref current)).collider) is NPCDwelling;
				if (!num && !flag)
				{
					Pool.FreeUnmanaged<RaycastHit>(ref list);
					return true;
				}
				lastError = ConstructionErrors.CantPlaceOnMonument;
				Pool.FreeUnmanaged<RaycastHit>(ref list);
				return false;
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		if ((!Object.op_Implicit((Object)(object)TerrainMeta.Collision) || !TerrainMeta.Collision.GetIgnore(vPoint)) && TerrainMeta.SampleTerrainMeshHeight(vPoint) > vPoint.y)
		{
			return true;
		}
		if (DeepSeaManager.IsInsideDeepSea(vPoint))
		{
			Ray ray = default(Ray);
			((Ray)(ref ray))._002Ector(vPoint + Vector3.up * 3f, Vector3.down);
			if (DeepSeaManager.IsInsideDeepSea(vPoint) && GamePhysics.Trace(ray, 0f, out var hitInfo, 3f, 8388608, (QueryTriggerInteraction)0))
			{
				return GameObjectEx.ToBaseEntity(((RaycastHit)(ref hitInfo)).collider) is DeepSeaIsland;
			}
		}
		return false;
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vPoint = place.position + place.rotation * worldPosition;
		lastError = null;
		if (IsInTerrain(vPoint, !preventWorldLayerInMonuments) == wantsInTerrain)
		{
			return true;
		}
		if (lastError == null)
		{
			lastError = ConstructionErrors.NotInTerrain;
		}
		return false;
	}
}
