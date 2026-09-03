using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class PatternFirework : MortarFirework, IUGCBrowserEntity
{
	public enum FuseLength
	{
		Short = 0,
		Medium = 1,
		Long = 2,
		Max = Long
	}

	public const int CurrentVersion = 1;

	[Header("PatternFirework")]
	public GameObjectRef FireworkDesignerDialog;

	public int MaxStars = 25;

	public float ShellFuseLengthShort = 3f;

	public float ShellFuseLengthMed = 5.5f;

	public float ShellFuseLengthLong = 8f;

	[NonSerialized]
	public Design Design;

	[NonSerialized]
	public FuseLength ShellFuseLength;

	public uint[] GetContentCRCs
	{
		get
		{
			if (Design == null || Design.stars.Count <= 0)
			{
				return Array.Empty<uint>();
			}
			return new uint[1] { 1u };
		}
	}

	public UGCType ContentType => UGCType.PatternBoomer;

	public List<ulong> EditingHistory
	{
		get
		{
			if (Design == null)
			{
				return new List<ulong>();
			}
			return new List<ulong> { Design.editedBy };
		}
	}

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	public override void DestroyShared()
	{
		base.DestroyShared();
		Design design = Design;
		if (design != null)
		{
			design.Dispose();
		}
		Design = null;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ShellFuseLength = FuseLength.Medium;
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void StartOpenDesigner(RPCMessage rpc)
	{
		if (PlayerCanModify(rpc.player))
		{
			ClientRPC(RpcTarget.Player("OpenDesigner", rpc.player));
		}
	}

	[RPC_Server]
	[RPC_Server.InputValidation(new Type[] { typeof(Design) })]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxRepeatedElements(35)]
	[RPC_Server.IsVisible(3f)]
	private void ServerSetFireworkDesign(RPCMessage rpc)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerCanModify(rpc.player))
		{
			return;
		}
		Design val = rpc.read.Proto<Design>((Design)null);
		if (Interface.CallHook("OnFireworkDesignChange", this, val, rpc.player) != null)
		{
			return;
		}
		if (val?.stars != null)
		{
			while (val.stars.Count > MaxStars)
			{
				int index = val.stars.Count - 1;
				val.stars[index].Dispose();
				val.stars.RemoveAt(index);
			}
			foreach (Star star in val.stars)
			{
				star.position = new Vector2(Mathf.Clamp(star.position.x, -1f, 1f), Mathf.Clamp(star.position.y, -1f, 1f));
				star.color = new Color(Mathf.Clamp01(star.color.r), Mathf.Clamp01(star.color.g), Mathf.Clamp01(star.color.b), 1f);
			}
			val.editedBy = rpc.player.userID;
		}
		Design design = Design;
		if (design != null)
		{
			design.Dispose();
		}
		Design = val;
		Interface.CallHook("OnFireworkDesignChanged", this, val, rpc.player);
		SendNetworkUpdateImmediate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void SetShellFuseLength(RPCMessage rpc)
	{
		if (PlayerCanModify(rpc.player))
		{
			ShellFuseLength = (FuseLength)Mathf.Clamp(rpc.read.Int32(), 0, 2);
			SendNetworkUpdateImmediate();
		}
	}

	private bool PlayerCanModify(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null || !player.CanInteract())
		{
			return false;
		}
		object obj = Interface.CallHook("CanDesignFirework", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
		if ((Object)(object)buildingPrivilege != (Object)null && !buildingPrivilege.CanAdministrate(player))
		{
			return false;
		}
		return true;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.patternFirework = Pool.Get<PatternFirework>();
		PatternFirework patternFirework = info.msg.patternFirework;
		Design design = Design;
		patternFirework.design = ((design != null) ? design.Copy() : null);
		info.msg.patternFirework.shellFuseLength = (int)ShellFuseLength;
	}

	public void ClearContent()
	{
		Design design = Design;
		if (design != null)
		{
			design.Dispose();
		}
		Design = null;
		SendNetworkUpdateImmediate();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.patternFirework != null)
		{
			Design design = Design;
			if (design != null)
			{
				design.Dispose();
			}
			Design design2 = info.msg.patternFirework.design;
			Design = ((design2 != null) ? design2.Copy() : null);
			ShellFuseLength = (FuseLength)info.msg.patternFirework.shellFuseLength;
		}
	}

	public float GetShellFuseLength()
	{
		return ShellFuseLength switch
		{
			FuseLength.Short => ShellFuseLengthShort, 
			FuseLength.Medium => ShellFuseLengthMed, 
			FuseLength.Long => ShellFuseLengthLong, 
			_ => ShellFuseLengthMed, 
		};
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("PatternFirework.OnRpcMessage"))
		{
			if (rpc == 3850129568u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ServerSetFireworkDesign"));
				}
				using (TimeWarning.New("ServerSetFireworkDesign"))
				{
					using (msg.read.UseRepeatedElementLimit(35))
					{
						using (TimeWarning.New("Conditions"))
						{
							if (!RPC_Server.CallsPerSecond.Test(3850129568u, "ServerSetFireworkDesign", this, player, 5uL))
							{
								return true;
							}
							long position = msg.read.Position;
							Design val = msg.read.Proto<Design>((Design)null);
							try
							{
								foreach (Star star in val.stars)
								{
									if (!RPC_Server.InputValidation.Test(star.position))
									{
										return true;
									}
								}
								msg.read.Position = position;
								if (!RPC_Server.IsVisible.Test(3850129568u, "ServerSetFireworkDesign", this, player, 3f))
								{
									return true;
								}
							}
							finally
							{
								((IDisposable)val)?.Dispose();
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
								ServerSetFireworkDesign(rpc2);
							}
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
							player.Kick("RPC Error in ServerSetFireworkDesign");
						}
					}
				}
				return true;
			}
			if (rpc == 2132764204 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetShellFuseLength"));
				}
				using (TimeWarning.New("SetShellFuseLength"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2132764204u, "SetShellFuseLength", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2132764204u, "SetShellFuseLength", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage shellFuseLength = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetShellFuseLength(shellFuseLength);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SetShellFuseLength");
					}
				}
				return true;
			}
			if (rpc == 2760408151u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - StartOpenDesigner"));
				}
				using (TimeWarning.New("StartOpenDesigner"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2760408151u, "StartOpenDesigner", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2760408151u, "StartOpenDesigner", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							StartOpenDesigner(rpc3);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in StartOpenDesigner");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}
}
