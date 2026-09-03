using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class HBHFSensor : BaseDetector
{
	public int range = 10;

	public const int MIN_RANGE = 2;

	public const int MAX_RANGE = 10;

	public GameObjectRef detectUpEffectPrefab;

	public GameObjectRef detectDownEffectPrefab;

	public GameObjectRef uiPanelPrefab;

	public const Flags Flag_IncludeOthers = Flags.Reserved4;

	public const Flags Flag_IncludeAuthed = Flags.Reserved3;

	[ServerVar(Help = "When enabled, broadcasts debug drawing for HBHFSensor visibility checks (eye position, forward, range, per-player LOS rays).")]
	public static bool DebugDraw;

	private int detectedPlayers;

	private Action UpdatePassthroughAmountCB;

	public int DetectedPlayers => detectedPlayers;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("HBHFSensor.OnRpcMessage"))
		{
			if (rpc == 4073303808u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetConfig"));
				}
				using (TimeWarning.New("SetConfig"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4073303808u, "SetConfig", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4073303808u, "SetConfig", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage config = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetConfig(config);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SetConfig");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnObjects()
	{
		base.OnObjects();
		UpdatePassthroughAmount();
		if (UpdatePassthroughAmountCB == null)
		{
			UpdatePassthroughAmountCB = UpdatePassthroughAmount;
		}
		InvokeRandomized(UpdatePassthroughAmountCB, 0f, 1f, 0.1f);
	}

	public override void OnEmpty()
	{
		base.OnEmpty();
		UpdatePassthroughAmount();
		if (UpdatePassthroughAmountCB == null)
		{
			UpdatePassthroughAmountCB = UpdatePassthroughAmount;
		}
		CancelInvoke(UpdatePassthroughAmountCB);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return Mathf.Min(detectedPlayers, GetCurrentEnergy());
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		if (inputAmount == 0)
		{
			detectedPlayers = 0;
		}
	}

	public void UpdatePassthroughAmount()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient || !IsPowered())
		{
			return;
		}
		int num = detectedPlayers;
		detectedPlayers = CountDetectedPlayers();
		if (num != detectedPlayers)
		{
			MarkDirty();
			if (detectedPlayers > num)
			{
				Effect.server.Run(detectUpEffectPrefab.resourcePath, ((Component)this).transform.position, Vector3.up);
			}
			else if (detectedPlayers < num)
			{
				Effect.server.Run(detectDownEffectPrefab.resourcePath, ((Component)this).transform.position, Vector3.up);
			}
		}
	}

	private int CountDetectedPlayers()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		if (myTrigger.entityContents == null || myTrigger.entityContents.Count == 0)
		{
			return 0;
		}
		IPrivilege privilege = null;
		bool flag = false;
		int num = 0;
		Vector3 val = ((Component)this).transform.position + ((Component)this).transform.forward * 0.1f;
		if (DebugDraw)
		{
			DebugDrawSensor(val);
		}
		foreach (BaseEntity entityContent in myTrigger.entityContents)
		{
			if (!(entityContent is BasePlayer basePlayer) || Interface.CallHook("OnSensorDetect", this, basePlayer) != null || (Object)(object)basePlayer == (Object)null || basePlayer.IsDead() || basePlayer.IsSleeping() || !basePlayer.isServer)
			{
				continue;
			}
			if (!flag)
			{
				privilege = GetPrivilege();
				flag = true;
			}
			bool flag2 = privilege?.IsAuthed(basePlayer) ?? false;
			if ((!flag2 || ShouldIncludeAuthorized()) && (flag2 || ShouldIncludeOthers()))
			{
				Vector3 val2 = basePlayer.ClosestPoint(val);
				Vector3 val3 = val2 - val;
				Vector3 normalized = ((Vector3)(ref val3)).normalized;
				Vector3 val4 = val2 + normalized * 0.5f;
				bool flag3 = basePlayer.IsVisible(val, val2, range);
				bool flag4 = flag3 && basePlayer.CanSee(val2, val4);
				bool num2 = flag3 & flag4;
				if (DebugDraw)
				{
					DebugDrawPlayer(val, basePlayer, val2, val4, flag3, flag4);
				}
				if (num2)
				{
					num++;
				}
			}
		}
		return num;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void SetConfig(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && CanUse(player))
		{
			bool b = msg.read.Bit();
			bool b2 = msg.read.Bit();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved3, b);
				flagsUpdateScope.Set(Flags.Reserved4, b2);
			}
			int num = msg.read.Int32();
			SetRange(num);
		}
	}

	public void SetRange(int value)
	{
		value = Mathf.Clamp(value, 2, 10);
		range = value;
		SendNetworkUpdate();
	}

	private void DebugDrawSensor(Vector3 eyePos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(position, 2f, Color.white, 0.05f));
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(eyePos, 2f, Color.yellow, 0.05f));
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(position, eyePos, 2f, Color.yellow));
		Vector3 pos = position + ((Component)this).transform.forward * 1f;
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(position, pos, 2f, Color.cyan));
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(eyePos, 2f, new Color(1f, 1f, 0f, 0.25f), range));
	}

	private void DebugDrawPlayer(Vector3 eyePos, BasePlayer player, Vector3 closestPoint, Vector3 probeEnd, bool sensorSeesPoint, bool rayReachesBody)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		OBB val = player.WorldSpaceBounds();
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Box(val.position, 2f, Color.white, val.extents * 2f, val.rotation));
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(eyePos, closestPoint, 2f, sensorSeesPoint ? Color.green : Color.red));
		if (sensorSeesPoint)
		{
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(closestPoint, probeEnd, 2f, rayReachesBody ? Color.green : Color.red));
		}
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(closestPoint, 2f, Color.yellow, 0.08f));
	}

	public bool CanUse(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseHBHFSensor", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return player.CanBuild();
	}

	public bool ShouldIncludeAuthorized()
	{
		return HasFlag(Flags.Reserved3);
	}

	public bool ShouldIncludeOthers()
	{
		return HasFlag(Flags.Reserved4);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity.genericInt1 = range;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			range = info.msg.ioEntity.genericInt1;
		}
	}
}
