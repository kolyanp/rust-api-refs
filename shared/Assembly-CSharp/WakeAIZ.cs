using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class WakeAIZ : EntityComponent<BaseEntity>, IServerComponent
{
	[Header("Base")]
	public float sleepDelaySeconds;

	public bool isBox;

	public Vector3 size;

	public List<AIInformationZone> zones;

	private AIInformationZone aiz;

	private Action sleepAI;

	private bool hadContents;

	private float radius;

	private OBB obb;

	private Vector3 spherePos;

	private float r2;

	private Action tickWakeAIZ;

	private Func<BasePlayer, bool> gridIgnoreFilter;

	private Func<BasePlayer, bool> gridQueryFilter;

	private bool foundEmptyCoarseGrid;

	private float TimeBetweenTicksActiveZone => sleepDelaySeconds * 0.1f;

	private float TimeBetweenTicksInactiveZone => 0.15f;

	public override void InitShared()
	{
		Init();
	}

	public void Init(AIInformationZone zone = null)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)zone != (Object)null)
		{
			aiz = zone;
		}
		else if (zones == null || zones.Count == 0)
		{
			Transform val = ((Component)this).transform.parent;
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)this).transform;
			}
			aiz = ((Component)val).GetComponentInChildren<AIInformationZone>();
		}
		if ((Object)(object)aiz != (Object)null && !aiz.wakeZones.Contains(this))
		{
			aiz.wakeZones.Add(this);
		}
		SetZonesSleeping(flag: true);
		Vector3 val2 = default(Vector3);
		Quaternion val3 = default(Quaternion);
		((Component)this).transform.GetPositionAndRotation(ref val2, ref val3);
		spherePos = val2;
		radius = (isBox ? (((Vector3)(ref size)).magnitude * 0.5f) : size.x);
		obb = new OBB(val2, val3, new Bounds(Vector3.zero, size));
		r2 = radius * radius;
		BaseEntity.Query.Server.SubscribePlayerChanges(spherePos, radius, Dirty);
		if (tickWakeAIZ == null)
		{
			tickWakeAIZ = TickWakeAIZ;
		}
		if (gridIgnoreFilter == null)
		{
			gridIgnoreFilter = FilterIgnorenNPC;
		}
		if (gridQueryFilter == null)
		{
			gridQueryFilter = FilterNonNPCInTrigger;
		}
		SetTickRate(isFast: true);
	}

	private void Dirty()
	{
		if (!IsInvoking(tickWakeAIZ))
		{
			SetTickRate(isFast: true);
		}
	}

	public PooledList<BasePlayer> GetPooledListOfPlayers()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
		BaseEntity.Query.Server.GetPlayersInSphere(((Component)this).transform.position, radius, (List<BasePlayer>)(object)val);
		Vector3 val2 = default(Vector3);
		Quaternion val3 = default(Quaternion);
		((Component)this).transform.GetPositionAndRotation(ref val2, ref val3);
		OBB val4 = default(OBB);
		((OBB)(ref val4))._002Ector(val2, val3, new Bounds(Vector3.zero, size));
		foreach (BasePlayer item in (List<BasePlayer>)(object)val)
		{
			if (Object.op_Implicit((Object)(object)item) && (!isBox || ((OBB)(ref val4)).Contains(item.TriggerPoint())))
			{
				((List<BasePlayer>)(object)val).Add(item);
			}
		}
		return val;
	}

	private bool BoxCheck(BasePlayer p)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (!((OBB)(ref obb)).Contains(p.TriggerPoint()))
		{
			return false;
		}
		return true;
	}

	private bool SphereCheck(BasePlayer p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.SqrMagnitude(p.TriggerPoint() - spherePos) > r2)
		{
			return false;
		}
		return true;
	}

	private bool FilterIgnorenNPC(BasePlayer p)
	{
		return p.IsNpc;
	}

	private bool FilterNonNPCInTrigger(BasePlayer p)
	{
		if (!isBox)
		{
			return SphereCheck(p);
		}
		return BoxCheck(p);
	}

	private void TickWakeAIZ()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		bool flag = BaseEntity.Query.Server.AnyPlayersInSphereFast(spherePos, radius, out foundEmptyCoarseGrid, gridIgnoreFilter, gridQueryFilter);
		if (!hadContents & flag)
		{
			if (sleepAI == null)
			{
				sleepAI = SleepAI;
			}
			CancelInvoke(sleepAI);
			SetZonesSleeping(flag: false);
			SetTickRate(isFast: false);
		}
		if (hadContents && !flag)
		{
			DelayedSleepAI();
			SetTickRate(isFast: true);
		}
		hadContents = flag;
		if (foundEmptyCoarseGrid && IsInvoking(tickWakeAIZ))
		{
			CancelInvoke(tickWakeAIZ);
		}
	}

	private void SetTickRate(bool isFast)
	{
		if (isFast)
		{
			if (IsInvoking(tickWakeAIZ))
			{
				CancelInvoke(tickWakeAIZ);
			}
			InvokeRandomized(tickWakeAIZ, TimeBetweenTicksInactiveZone, TimeBetweenTicksInactiveZone, TimeBetweenTicksInactiveZone * 0.25f);
		}
		else
		{
			if (IsInvoking(tickWakeAIZ))
			{
				CancelInvoke(tickWakeAIZ);
			}
			InvokeRepeating(tickWakeAIZ, TimeBetweenTicksActiveZone, TimeBetweenTicksActiveZone);
		}
	}

	private void SetZonesSleeping(bool flag)
	{
		if ((Object)(object)aiz != (Object)null)
		{
			if (flag)
			{
				aiz.SleepAI();
			}
			else
			{
				aiz.WakeAI();
			}
		}
		if (zones == null || zones.Count <= 0)
		{
			return;
		}
		foreach (AIInformationZone zone in zones)
		{
			if ((Object)(object)zone != (Object)null)
			{
				if (flag)
				{
					zone.SleepAI();
				}
				else
				{
					zone.WakeAI();
				}
			}
		}
	}

	private void DelayedSleepAI()
	{
		if (sleepAI == null)
		{
			sleepAI = SleepAI;
		}
		CancelInvoke(sleepAI);
		Invoke(sleepAI, sleepDelaySeconds);
	}

	private void SleepAI()
	{
		SetZonesSleeping(flag: true);
	}

	public WakeAIZ()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		sleepDelaySeconds = 30f;
		size = Vector3.one * 30f;
		base._002Ector();
	}
}
