using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class VehicleVendor : NPCTalking
{
	public const float VehicleSpawnerSearchRadius = 40f;

	private EntityRef<VehicleSpawner> spawnerRef;

	public VehicleSpawner GetVehicleSpawner()
	{
		return spawnerRef.Get(serverside: true);
	}

	public override void UpdateFlags()
	{
		base.UpdateFlags();
		VehicleSpawner vehicleSpawner = GetVehicleSpawner();
		bool b = (Object)(object)vehicleSpawner != (Object)null && vehicleSpawner.IsPadOccupied();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b);
	}

	public bool Conversation_IsPadUsable()
	{
		VehicleSpawner vehicleSpawner = GetVehicleSpawner();
		if (!((Object)(object)vehicleSpawner == (Object)null))
		{
			return vehicleSpawner.IsPadUsable();
		}
		return true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		FindVehicleSpawner();
	}

	public void FindVehicleSpawner()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (spawnerRef.IsValid(serverside: true) || conversationResultActions.Length == 0)
		{
			return;
		}
		VehicleSpawner vehicleSpawner = null;
		float num = float.MaxValue;
		List<VehicleSpawner> list = Pool.Get<List<VehicleSpawner>>();
		Vis.Entities(((Component)this).transform.position, 40f, list, 1218652417, (QueryTriggerInteraction)2);
		foreach (VehicleSpawner item in list)
		{
			if (!item.isClient && item.IsValid() && SellsThrough(item))
			{
				float num2 = Vector3.SqrMagnitude(((Component)this).transform.position - ((Component)item).transform.position);
				if (!(num2 >= num))
				{
					vehicleSpawner = item;
					num = num2;
				}
			}
		}
		Pool.FreeUnmanaged<VehicleSpawner>(ref list);
		if ((Object)(object)vehicleSpawner != (Object)null)
		{
			spawnerRef.Set(vehicleSpawner);
		}
	}

	private bool SellsThrough(VehicleSpawner spawner)
	{
		NPCConversationResultAction[] array = conversationResultActions;
		foreach (NPCConversationResultAction nPCConversationResultAction in array)
		{
			if (spawner.SpawnsFor(nPCConversationResultAction.broadcastMessage))
			{
				return true;
			}
		}
		return false;
	}

	public override ConversationData GetConversationFor(BasePlayer player)
	{
		return conversations[0];
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
	}
}
