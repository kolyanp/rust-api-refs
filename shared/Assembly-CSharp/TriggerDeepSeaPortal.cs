using System;
using System.Collections.Generic;
using Facepunch;
using LeTai;
using Oxide.Core;
using UnityEngine;

public class TriggerDeepSeaPortal : TriggerBase
{
	public DeepSeaPortal Portal;

	[Tooltip("Set to false if you only want this trigger to show toasts warning players why they can't enter the deep sea")]
	public bool WillTeleport = true;

	public bool ShowToasts = true;

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if (ent.isClient || Portal.isClient)
		{
			return;
		}
		DeepSeaManager serverInstance = PointEntity<DeepSeaManager>.ServerInstance;
		if ((Object)(object)serverInstance == (Object)null || !serverInstance.IsOpen() || (Portal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance && !Portal.HasFlag(BaseEntity.Flags.Open)))
		{
			return;
		}
		if (ent is BoatBuildingBlock boatBuildingBlock && (Object)(object)boatBuildingBlock.GetParentEntity() != (Object)null)
		{
			ent = boatBuildingBlock.GetParentEntity();
		}
		var (flag, val) = CanEntityTeleport(ent);
		if (!flag)
		{
			if (!ShowToasts || val == null)
			{
				return;
			}
			if (ent is BasePlayer basePlayer)
			{
				basePlayer.ShowToast(GameTip.Styles.Blue_Long, val, false);
			}
			else
			{
				if (!(ent is BaseVehicle entity))
				{
					return;
				}
				List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
				BaseVehicle.GetPassengersForVehicle(entity, list);
				foreach (BasePlayer item in list)
				{
					item.ShowToast(GameTip.Styles.Blue_Long, val, false);
				}
				Pool.FreeUnmanaged<BasePlayer>(ref list);
			}
		}
		else
		{
			if (!WillTeleport || Interface.CallHook("OnDeepSeaTeleport", this, ent) != null)
			{
				return;
			}
			if (Portal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance)
			{
				ExtensionMethods.NextFrames((MonoBehaviour)(object)Portal, (Action)delegate
				{
					PointEntity<DeepSeaManager>.ServerInstance.MoveToDeepSea(ent);
				}, 1);
			}
			else
			{
				ExtensionMethods.NextFrames((MonoBehaviour)(object)Portal, (Action)delegate
				{
					PointEntity<DeepSeaManager>.ServerInstance.MoveToMainIsland(ent);
				}, 1);
			}
		}
	}

	private (bool, Phrase) CanEntityTeleport(BaseEntity entity)
	{
		object obj = Interface.CallHook("CanTeleportDeepSea", entity, Portal);
		if (obj is ValueTuple<bool, Phrase>)
		{
			return ((bool, Phrase))obj;
		}
		if (Portal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance)
		{
			return DeepSeaManager.CanTeleportToDeepSea(entity);
		}
		if (Portal.PortalMode == DeepSeaPortal.PortalModeEnum.Exit)
		{
			return DeepSeaManager.CanTeleportToMainIsland(entity);
		}
		return (false, null);
	}
}
