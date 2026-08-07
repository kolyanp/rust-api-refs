using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerHelicopterWithFlares : PlayerHelicopter, ICanFireHelicopterFlares
{
	[SerializeField]
	[Header("Helicopter Flares")]
	private GameObjectRef flareStoragePrefab;

	[SerializeField]
	private Renderer flareLightOff;

	[SerializeField]
	private Renderer flareLightRed;

	[SerializeField]
	private Renderer flareLightGreen;

	public EntityRef<HelicopterFlares> flaresInstance;

	private TimeSince timeSinceFailedFlareRPC;

	public BaseEntity flareEntity => this;

	public HelicopterFlares FlaresInstance => flaresInstance.Get(base.isServer);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PlayerHelicopterWithFlares.OnRpcMessage"))
		{
			if (rpc == 4185921214u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_OpenStorage"));
				}
				using (TimeWarning.New("RPC_OpenStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4185921214u, "RPC_OpenStorage", this, player, 6f))
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
							RPC_OpenStorage(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_OpenStorage");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child.prefabID == flareStoragePrefab.GetEntity().prefabID)
		{
			HelicopterFlares helicopterFlares = (HelicopterFlares)child;
			flaresInstance.Set(helicopterFlares);
			helicopterFlares.owner = this;
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.helicopterFlares != null)
		{
			flaresInstance.uid = info.msg.helicopterFlares.flareStorageID;
		}
	}

	public HelicopterFlares GetFlares()
	{
		HelicopterFlares helicopterFlares = flaresInstance.Get(base.isServer);
		if (helicopterFlares.IsValid())
		{
			return helicopterFlares;
		}
		return null;
	}

	public void FireFlares()
	{
		throw new NotImplementedException();
	}

	public void FlareFireFailed(BasePlayer player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!(TimeSince.op_Implicit(timeSinceFailedFlareRPC) <= 1f))
		{
			ClientRPC(RpcTarget.Player("ClientFlareFireFailed", player));
			timeSinceFailedFlareRPC = TimeSince.op_Implicit(0f);
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.helicopterFlares = Pool.Get<HelicopterFlares>();
		info.msg.helicopterFlares.flareStorageID = flaresInstance.uid;
	}

	public override void PilotInput(InputState inputState, BasePlayer player)
	{
		base.PilotInput(inputState, player);
		if (IsOn() && inputState.WasJustPressed(BUTTON.FIRE_SECONDARY) && !GetFlares().TryFireFlare())
		{
			FlareFireFailed(player);
		}
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot && flaresInstance.IsValid(base.isServer))
		{
			flaresInstance.Get(base.isServer).DropItems();
		}
		base.DoServerDestroy();
	}

	[RPC_Server.IsVisible(6f)]
	[RPC_Server]
	public void RPC_OpenStorage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (IsOn() || !CanBeLooted(player) || player.isMounted || (IsSafe() && (Object)(object)player != (Object)(object)creatorEntity))
		{
			return;
		}
		StorageContainer flares = GetFlares();
		if (!((Object)(object)flares == (Object)null))
		{
			BasePlayer driver = GetDriver();
			if (!((Object)(object)driver != (Object)null) || !((Object)(object)driver != (Object)(object)player))
			{
				flares.PlayerOpenLoot(player);
			}
		}
	}

	public override bool AdminFixUp(int tier)
	{
		if (!base.AdminFixUp(tier))
		{
			return false;
		}
		GetFlares()?.RefillFlares();
		return true;
	}
}
