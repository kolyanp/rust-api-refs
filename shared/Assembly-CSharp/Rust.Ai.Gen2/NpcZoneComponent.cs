using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcZoneComponent : EntityComponent<BaseEntity>, IServerComponent
{
	private bool hasAbandonnedZone;

	public NpcZone zone { get; private set; }

	public override void InitShared()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (base.baseEntity.isServer)
		{
			zone = NpcZone.GetForPoint(base.baseEntity, base.baseEntity.CenterPoint());
			base.InitShared();
		}
	}

	public void AbandonZone()
	{
		hasAbandonnedZone = true;
	}

	public bool IsPointInsideZone(Vector3 point)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)zone == (Object)null)
		{
			return true;
		}
		if (hasAbandonnedZone)
		{
			return true;
		}
		return zone.IsPointInside(base.baseEntity, point);
	}

	public bool IsInSameZone(BaseEntity other)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)zone == (Object)null || (Object)(object)other == (Object)null)
		{
			return false;
		}
		NpcZoneComponent npcZoneComponent = default(NpcZoneComponent);
		if (((Component)other).TryGetComponent<NpcZoneComponent>(ref npcZoneComponent))
		{
			return (Object)(object)zone == (Object)(object)npcZoneComponent.zone;
		}
		NpcZone forPoint = NpcZone.GetForPoint(other, other.CenterPoint());
		return (Object)(object)zone == (Object)(object)forPoint;
	}
}
