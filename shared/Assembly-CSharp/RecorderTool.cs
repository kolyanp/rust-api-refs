using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class RecorderTool : ThrownWeapon, ICassettePlayer
{
	[ClientVar(Saved = true, Help = "(Generated) When enabled, logs additional debug info about cassette recording state and audio buffer operations; saved between sessions")]
	public static bool debugRecording;

	public AudioSource RecorderAudioSource;

	public SoundDefinition RecordStartSfx;

	public SoundDefinition RewindSfx;

	public SoundDefinition RecordFinishedSfx;

	public SoundDefinition PlayTapeSfx;

	public SoundDefinition StopTapeSfx;

	public float ThrowScale = 3f;

	public Cassette cachedCassette { get; set; }

	public Sprite LoadedCassetteIcon
	{
		get
		{
			if (!((Object)(object)cachedCassette != (Object)null))
			{
				return null;
			}
			return cachedCassette.HudSprite;
		}
	}

	public BaseEntity ToBaseEntity => this;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RecorderTool.OnRpcMessage"))
		{
			if (rpc == 4278517885u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - OnCassetteRecordingEnded"));
				}
				using (TimeWarning.New("OnCassetteRecordingEnded"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4278517885u, "OnCassetteRecordingEnded", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(4278517885u, "OnCassetteRecordingEnded", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							OnCassetteRecordingEnded(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in OnCassetteRecordingEnded");
					}
				}
				return true;
			}
			if (rpc == 3075830603u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_TogglePlaying"));
				}
				using (TimeWarning.New("Server_TogglePlaying"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3075830603u, "Server_TogglePlaying", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3075830603u, "Server_TogglePlaying", this, player))
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
							Server_TogglePlaying(msg2);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Server_TogglePlaying");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool HasCassette()
	{
		return (Object)(object)cachedCassette != (Object)null;
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(2uL)]
	public void Server_TogglePlaying(RPCMessage msg)
	{
		bool b = msg.read.ReadByte() == 1;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b);
	}

	public void OnCassetteInserted(Cassette c)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		cachedCassette = c;
		ClientRPC(RpcTarget.NetworkGroup("Client_OnCassetteInserted"), c.net.ID);
	}

	public void OnCassetteRemoved(Cassette c)
	{
		cachedCassette = null;
		ClientRPC(RpcTarget.NetworkGroup("Client_OnCassetteRemoved"));
	}

	protected override void SetUpThrownWeapon(BaseEntity ent, Item ownerItem)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer != (Object)null)
		{
			ent.OwnerID = ownerPlayer.userID;
		}
		if (ent is DeployedRecorder deployedRecorder)
		{
			if ((Object)(object)cachedCassette != (Object)null)
			{
				ownerItem.contents.itemList[0].SetParent(deployedRecorder.inventory);
			}
			deployedRecorder.ItemOwnership = ownerItem.TakeOwnershipShare();
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void OnCassetteRecordingEnded(RPCMessage rpc)
	{
		if (GetItem() != null)
		{
			GetItem().contents.itemList[0].SetItemOwnership(rpc.player, ItemOwnershipPhrases.Recorded);
		}
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		if (IsDisabled())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
		}
	}
}
