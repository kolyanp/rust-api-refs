using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Assertions;

public class LivestockMountable : NPCMountable
{
	private static readonly Phrase NoRoomToLoadPhrase = new Phrase("livestock_no_room_to_load", "Not enough room around the vehicle for the animal to jump on");

	private static readonly Phrase NoRoomToUnloadPhrase = new Phrase("livestock_no_room_to_unload", "Nowhere for the animal to get down here");

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("LivestockMountable.OnRpcMessage"))
		{
			if (rpc == 2472081994u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_LoadAnimal"));
				}
				using (TimeWarning.New("RPC_LoadAnimal"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!BaseEntity.RPC_Server.CallsPerSecond.Test(2472081994u, "RPC_LoadAnimal", GetBaseEntity(), player, 1uL))
						{
							return true;
						}
						if (!BaseEntity.RPC_Server.MaxDistance.Test(2472081994u, "RPC_LoadAnimal", GetBaseEntity(), player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage msg2 = new BaseEntity.RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_LoadAnimal(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_LoadAnimal");
					}
				}
				return true;
			}
			if (rpc == 395352663 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_UnloadAnimal"));
				}
				using (TimeWarning.New("RPC_UnloadAnimal"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!BaseEntity.RPC_Server.CallsPerSecond.Test(395352663u, "RPC_UnloadAnimal", GetBaseEntity(), player, 1uL))
						{
							return true;
						}
						if (!BaseEntity.RPC_Server.MaxDistance.Test(395352663u, "RPC_UnloadAnimal", GetBaseEntity(), player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage msg3 = new BaseEntity.RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_UnloadAnimal(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_UnloadAnimal");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected override bool CanSeat(BaseNPC2 npc, int index)
	{
		if (npc is LivestockAnimal livestockAnimal)
		{
			return ((uint)anchors[index].acceptedSizes & (uint)livestockAnimal.MountType) != 0;
		}
		return false;
	}

	[BaseEntity.RPC_Server.CallsPerSecond(1uL)]
	[BaseEntity.RPC_Server.MaxDistance(3f)]
	[BaseEntity.RPC_Server]
	private void RPC_LoadAnimal(BaseEntity.RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && Livestock.allowMounting)
		{
			LivestockAnimal livestockAnimal = FindLedAnimal(msg.player);
			bool noClearApproach = default;
			if (((Object)(object)livestockAnimal != (Object)null && !RequestMount(livestockAnimal, out noClearApproach)) & noClearApproach)
			{
				msg.player.ShowToast(GameTip.Styles.Error, NoRoomToLoadPhrase, false);
			}
		}
	}

	[BaseEntity.RPC_Server.MaxDistance(3f)]
	[BaseEntity.RPC_Server.CallsPerSecond(1uL)]
	[BaseEntity.RPC_Server]
	private void RPC_UnloadAnimal(BaseEntity.RPCMessage msg)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)msg.player == (Object)null) && (!TryDismountOne(((Component)msg.player).transform.position, out var noDismountPoint) & noDismountPoint))
		{
			msg.player.ShowToast(GameTip.Styles.Error, NoRoomToUnloadPhrase, false);
		}
	}

	private LivestockAnimal FindLedAnimal(BasePlayer player)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		PooledList<LivestockAnimal> val = Pool.Get<PooledList<LivestockAnimal>>();
		try
		{
			BaseEntity.Query.Server.GetBrainsInSphere(((Component)player).transform.position, 12f, (List<LivestockAnimal>)(object)val);
			foreach (LivestockAnimal item in (List<LivestockAnimal>)(object)val)
			{
				if (item.CanStopLead(player) && !item.IsMounted && HasFreeAnchorFor(item))
				{
					return item;
				}
			}
			return null;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	static LivestockMountable()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
	}
}
