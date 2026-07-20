using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcSleepingComponent : EntityComponent<BaseEntity>, IAISleepable, IServerComponent
{
	public List<Component> componentsToSleep = new List<Component>();

	private bool sleeping;

	private AIInformationZone infoZone;

	public override void InitShared()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		base.InitShared();
		infoZone = AIInformationZone.GetForPoint(((Component)this).transform.position, fallBackToNearest: false);
		if ((Object)(object)infoZone != (Object)null)
		{
			infoZone.RegisterSleepableEntity(this);
		}
	}

	public override void DestroyShared()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)infoZone == (Object)null)
		{
			infoZone = AIInformationZone.GetForPoint(((Component)this).transform.position);
		}
		if ((Object)(object)infoZone != (Object)null)
		{
			infoZone.UnregisterSleepableEntity(this);
		}
		base.DestroyShared();
	}

	bool IAISleepable.AllowedToSleep()
	{
		return true;
	}

	void IAISleepable.SleepAI()
	{
		SetSleeping(newSleeping: true);
	}

	void IAISleepable.WakeAI()
	{
		SetSleeping(newSleeping: false);
	}

	private void SetSleeping(bool newSleeping)
	{
		if (sleeping == newSleeping)
		{
			return;
		}
		sleeping = newSleeping;
		foreach (Component item in componentsToSleep)
		{
			if ((Object)(object)item == (Object)null)
			{
				continue;
			}
			if (item is FSMComponent fSMComponent)
			{
				fSMComponent.SetFsmActive(!newSleeping);
				continue;
			}
			MonoBehaviour val = (MonoBehaviour)(object)((item is MonoBehaviour) ? item : null);
			if (val != null)
			{
				((Behaviour)val).enabled = !newSleeping;
			}
		}
		base.baseEntity.limitNetworking = newSleeping;
	}
}
