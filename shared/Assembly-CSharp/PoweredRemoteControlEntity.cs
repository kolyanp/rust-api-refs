using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class PoweredRemoteControlEntity : IOEntity, IRemoteControllable, IAdminUpdatableIdentifier
{
	public string rcIdentifier = "";

	public Transform viewEyes;

	public GameObjectRef IDPanelPrefab;

	public RemoteControllableControls rcControls;

	public bool isStatic;

	public bool appendEntityIDToIdentifier;

	public virtual bool RequiresMouse => false;

	public virtual float MaxRange => 10000f;

	public RemoteControllableControls RequiredControls => rcControls;

	public bool CanPing => EntityCanPing;

	protected virtual bool EntityCanPing => false;

	public virtual bool CanAcceptInput => false;

	public int ViewerCount { get; set; }

	public CameraViewerId? ControllingViewerId { get; set; }

	public bool IsBeingControlled
	{
		get
		{
			if (ViewerCount > 0)
			{
				return ControllingViewerId.HasValue;
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PoweredRemoteControlEntity.OnRpcMessage"))
		{
			if (rpc == 2025588587 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_AdminUpdateIdentifier"));
				}
				using (TimeWarning.New("Server_AdminUpdateIdentifier"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2025588587u, "Server_AdminUpdateIdentifier", this, player, 3f))
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
							Server_AdminUpdateIdentifier(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_AdminUpdateIdentifier");
					}
				}
				return true;
			}
			if (rpc == 1677685895 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RequestOpenRCPanel"));
				}
				using (TimeWarning.New("SERVER_RequestOpenRCPanel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1677685895u, "SERVER_RequestOpenRCPanel", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_RequestOpenRCPanel(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_RequestOpenRCPanel");
					}
				}
				return true;
			}
			if (rpc == 1053317251 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetID"));
				}
				using (TimeWarning.New("Server_SetID"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1053317251u, "Server_SetID", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Server_SetID(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in Server_SetID");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsStatic()
	{
		return isStatic;
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		UpdateRCAccess(IsPowered());
	}

	public void UpdateRCAccess(bool isOnline)
	{
		if (isOnline)
		{
			RemoteControlEntity.InstallControllable(this);
		}
		else
		{
			RemoteControlEntity.RemoveControllable(this);
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		string text = "#ID";
		if (IsStatic() && rcIdentifier.Contains(text))
		{
			int length = rcIdentifier.IndexOf(text);
			_ = text.Length;
			string text2 = rcIdentifier.Substring(0, length);
			text2 += ((object)Unsafe.As<NetworkableId, NetworkableId>(ref net.ID)/*cast due to constrained. prefix*/).ToString();
			UpdateIdentifier(text2);
		}
	}

	public virtual bool InitializeControl(CameraViewerId viewerID)
	{
		ViewerCount++;
		if (CanAcceptInput && !ControllingViewerId.HasValue)
		{
			ControllingViewerId = viewerID;
			return true;
		}
		return !CanAcceptInput;
	}

	public virtual void StopControl(CameraViewerId viewerID)
	{
		ViewerCount--;
		if (ControllingViewerId == viewerID)
		{
			ControllingViewerId = null;
		}
	}

	public Matrix4x4 GetEyesMatrix()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return viewEyes.localToWorldMatrix;
	}

	public virtual void UserInput(InputState inputState, CameraViewerId viewerID)
	{
	}

	public Transform GetEyes()
	{
		return viewEyes;
	}

	public virtual float GetFovScale()
	{
		return 1f;
	}

	public virtual bool CanControl(ulong playerID)
	{
		object obj = Interface.CallHook("OnEntityControl", this, playerID);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!IsPowered())
		{
			return IsStatic();
		}
		return true;
	}

	public BaseEntity GetEnt()
	{
		return this;
	}

	public virtual void RCSetup()
	{
	}

	public virtual void RCShutdown()
	{
		if (base.isServer)
		{
			RemoteControlEntity.RemoveControllable(this);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void Server_SetID(RPCMessage msg)
	{
		string oldID = msg.read.String();
		string newID = msg.read.String();
		SetID(msg.player, oldID, newID);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void Server_AdminUpdateIdentifier(RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null) && (msg.player.IsAdmin || msg.player.IsDeveloper))
		{
			string oldID = msg.read.String();
			string newID = msg.read.String();
			SetID(msg.player, oldID, newID, bypassChecks: true);
		}
	}

	public void SetID(BasePlayer player, string oldID, string newID, bool bypassChecks = false)
	{
		if ((!IsStatic() || bypassChecks) && (CanChangeID(player) || bypassChecks) && (string.IsNullOrEmpty(oldID) || ComputerStation.IsValidIdentifier(oldID)) && ComputerStation.IsValidIdentifier(newID) && oldID == GetIdentifier())
		{
			UpdateIdentifier(newID);
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	public void SERVER_RequestOpenRCPanel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && CanChangeID(player))
		{
			ClientRPC(RpcTarget.Player("CLIENT_OpenRCPanel", player), GetIdentifier());
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk || IsStatic())
		{
			info.msg.rcEntity = Pool.Get<RCEntity>();
			info.msg.rcEntity.identifier = GetIdentifier();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.rcEntity != null && ComputerStation.IsValidIdentifier(info.msg.rcEntity.identifier))
		{
			UpdateIdentifier(info.msg.rcEntity.identifier);
		}
	}

	public void UpdateIdentifier(string newID, bool clientSend = false)
	{
		if (Interface.CallHook("OnRemoteIdentifierUpdate", this, newID) != null)
		{
			return;
		}
		_ = rcIdentifier;
		if (base.isServer)
		{
			if (!RemoteControlEntity.IDInUse(newID))
			{
				rcIdentifier = newID;
			}
			if (!Application.isLoadingSave)
			{
				SendNetworkUpdate();
			}
		}
	}

	public string GetIdentifier()
	{
		return rcIdentifier;
	}

	public override void InitShared()
	{
		base.InitShared();
		RCSetup();
	}

	public override void DestroyShared()
	{
		RCShutdown();
		base.DestroyShared();
	}

	public bool CanChangeID(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			if (player.CanBuild())
			{
				return player.IsBuildingAuthed();
			}
			return false;
		}
		return false;
	}
}
