using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Rust;
using Spatial;
using UnityEngine;

public class CeilingLight : IOEntity
{
	public float pushScale = 2f;

	public Rigidbody MovingJoint;

	public bool UseLowerSleepLimits;

	public TransformLineRenderer[] lines;

	[Space]
	public bool shouldAffectGrowables = true;

	public int consumptionAmount = 2;

	public const Flags RecentlyHit = Flags.Reserved3;

	private Action resetHitAction;

	public static Grid<CeilingLight> FarmLightGrid = new Grid<CeilingLight>(32, 8096f);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CeilingLight.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return consumptionAmount;
	}

	public override void OnAttacked(HitInfo info)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			float num = 3f * (info.damageTypes.Total() / 50f);
			NetworkableId arg = (NetworkableId)(((Object)(object)info.Initiator != (Object)null && info.Initiator is BasePlayer && !info.IsPredicting) ? info.Initiator.net.ID : default(NetworkableId));
			ClientRPC(RpcTarget.NetworkGroup("ClientPhysPush"), arg, info.attackNormal * num, info.HitPositionWorld);
			MarkRecentlyHit();
		}
		base.OnAttacked(info);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		MarkRecentlyHit();
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		bool num = IsOn();
		bool flag = IsPowered();
		if (num != flag)
		{
			SetFlagLocal(Flags.On, flag);
			SendNetworkUpdate_Flags();
			if (flag)
			{
				LightsOn();
			}
			else
			{
				LightsOff();
			}
		}
	}

	public override void Hurt(HitInfo info)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			if (info.damageTypes.Has(DamageType.Explosion))
			{
				ClientRPC(RpcTarget.NetworkGroup("ClientPhysPush"), default(NetworkableId), info.attackNormal * 3f * (info.damageTypes.Total() / 50f), info.HitPositionWorld);
				MarkRecentlyHit();
			}
			base.Hurt(info);
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		RefreshGrowables();
	}

	private void RefreshGrowables()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		List<GrowableEntity> list = Pool.Get<List<GrowableEntity>>();
		Vis.Entities(((Component)this).transform.position + new Vector3(0f, 0f - ConVar.Server.ceilingLightHeightOffset, 0f), ConVar.Server.ceilingLightGrowableRange, list, 524288, (QueryTriggerInteraction)2);
		List<PlanterBox> list2 = Pool.Get<List<PlanterBox>>();
		foreach (GrowableEntity item in list)
		{
			if (item.isServer)
			{
				PlanterBox planter = item.GetPlanter();
				if ((Object)(object)planter != (Object)null && !list2.Contains(planter))
				{
					list2.Add(planter);
					planter.ForceLightUpdate();
				}
				item.CalculateQualities(firstTime: false, forceArtificialLightUpdates: true);
				item.SendNetworkUpdate();
			}
		}
		Pool.FreeUnmanaged<PlanterBox>(ref list2);
		Pool.FreeUnmanaged<GrowableEntity>(ref list);
	}

	private void LightsOn()
	{
		if (shouldAffectGrowables)
		{
			RefreshGrowables();
		}
	}

	private void LightsOff()
	{
		if (shouldAffectGrowables)
		{
			RefreshGrowables();
		}
	}

	private void MarkRecentlyHit()
	{
		if (resetHitAction == null)
		{
			resetHitAction = ResetRecentlyHit;
		}
		CancelInvoke(resetHitAction);
		Invoke(resetHitAction, 15f);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: true);
	}

	private void ResetRecentlyHit()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
	}

	public override void ServerInit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		Vector3 position = ((Component)this).transform.position;
		FarmLightGrid.Add(this, position.x, position.z);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		FarmLightGrid.Remove(this);
	}
}
