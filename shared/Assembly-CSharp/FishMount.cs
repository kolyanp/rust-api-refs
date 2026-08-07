using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class FishMount : StorageContainer
{
	public Animator[] FishRoots = (Animator[])(object)new Animator[0];

	public GameObjectRef FishInteractSound = new GameObjectRef();

	public float UseCooldown = 3f;

	public const Flags HasFish = Flags.Reserved1;

	private int GetCurrentFishItemIndex
	{
		get
		{
			ItemModFishable itemModFishable = default(ItemModFishable);
			if (base.inventory.GetSlot(0) == null || !((Component)base.inventory.GetSlot(0).info).TryGetComponent<ItemModFishable>(ref itemModFishable))
			{
				return -1;
			}
			return itemModFishable.FishMountIndex;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("FishMount.OnRpcMessage"))
		{
			if (rpc == 3280542489u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UseFish"));
				}
				using (TimeWarning.New("UseFish"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3280542489u, "UseFish", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							UseFish(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in UseFish");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.simpleInt == null)
		{
			info.msg.simpleInt = Pool.Get<SimpleInt>();
		}
		info.msg.simpleInt.value = GetCurrentFishItemIndex;
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		ItemModFishable itemModFishable = default(ItemModFishable);
		if (((Component)item.info).TryGetComponent<ItemModFishable>(ref itemModFishable) && itemModFishable.CanBeMounted)
		{
			return true;
		}
		return false;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		SetFlagLocal(Flags.Reserved1, GetCurrentFishItemIndex >= 0);
		SendNetworkUpdate();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void UseFish(RPCMessage msg)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (HasFlag(Flags.Reserved1) && !IsBusy())
		{
			Effect.server.Run(FishInteractSound.resourcePath, ((Component)this).transform.position);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			Invoke(ClearBusy, UseCooldown);
			ClientRPC(RpcTarget.NetworkGroup("PlayAnimation"));
		}
	}

	private void ClearBusy()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}
}
