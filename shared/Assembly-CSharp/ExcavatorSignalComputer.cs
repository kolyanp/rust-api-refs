using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class ExcavatorSignalComputer : BaseCombatEntity
{
	public float chargePower;

	public const Flags Flag_Ready = Flags.Reserved7;

	public const Flags Flag_HasPower = Flags.Reserved8;

	public const Flags Flag_Transmitting = Flags.Reserved9;

	public GameObjectRef supplyPlanePrefab;

	public Transform[] dropPoints;

	public Text statusText;

	public Text timerText;

	[Tooltip("If true, will auto charge without any power")]
	public bool requiresPowerToCharge = true;

	[Tooltip("How much we start with min note:only if no power required")]
	public float startChargeMin;

	[Tooltip("How much we start with max note:only if no power required")]
	public float startChargeMax;

	private int numSuppliesCalled;

	public int maxNumSuppliesCalled = -1;

	public static readonly Phrase readyphrase;

	public static readonly Phrase chargephrase;

	public static readonly Phrase emptyphrase;

	public static readonly Phrase transmitphrase;

	public static readonly Phrase inboundphrase;

	[ServerVar(Help = "(Generated) Amount of charge (in seconds of operation) the excavator signal computer requires before it can manually call a supply drop")]
	public static float chargeNeededForSupplies;

	[ServerVar(Help = "(Generated) Amount of charge required for the excavator to automatically trigger supply drop delivery without player activation")]
	public static float automaticChargeNeededForSupplies;

	private float lastChargeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ExcavatorSignalComputer.OnRpcMessage"))
		{
			if (rpc == 1824723998 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestSupplies"));
				}
				using (TimeWarning.New("RequestSupplies"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1824723998u, "RequestSupplies", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1824723998u, "RequestSupplies", this, player, 3f))
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
							RequestSupplies(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RequestSupplies");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool HasSuppliesRemaining()
	{
		if (maxNumSuppliesCalled != -1)
		{
			return numSuppliesCalled < maxNumSuppliesCalled;
		}
		return true;
	}

	public float GetChargeNeededForSupplies()
	{
		if (requiresPowerToCharge)
		{
			return chargeNeededForSupplies;
		}
		return automaticChargeNeededForSupplies;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity = Pool.Get<IOEntity>();
		info.msg.ioEntity.genericFloat1 = chargePower;
		info.msg.ioEntity.genericFloat2 = GetChargeNeededForSupplies();
		info.msg.ioEntity.genericInt1 = numSuppliesCalled;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		lastChargeTime = Time.time;
		InvokeRepeating(ChargeThink, 0f, 1f);
		if (!requiresPowerToCharge)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, b: true);
			}
			chargePower = Random.Range(startChargeMin, startChargeMax) * GetChargeNeededForSupplies();
		}
	}

	public override void PostServerLoad()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (requiresPowerToCharge)
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: false);
		}
		else
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: true);
		}
		flagsUpdateScope.Set(Flags.Reserved7, b: false);
	}

	public virtual void ChargeThink()
	{
		float num = chargePower;
		float num2 = Time.time - lastChargeTime;
		lastChargeTime = Time.time;
		if (IsPowered() && HasSuppliesRemaining())
		{
			chargePower += num2;
		}
		chargePower = Mathf.Clamp(chargePower, 0f, GetChargeNeededForSupplies());
		Flags num3 = flags;
		SetFlagLocal(Flags.Reserved7, chargePower >= GetChargeNeededForSupplies());
		if (num3 != flags || num != chargePower)
		{
			SendNetworkUpdate();
		}
	}

	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		base.OnEntityMessage(from, msg);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (!(msg == "DieselEngineOn"))
		{
			if (msg == "DieselEngineOff")
			{
				flagsUpdateScope.Set(Flags.Reserved8, b: false);
			}
		}
		else
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: true);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void RequestSupplies(RPCMessage rpc)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if (HasFlag(Flags.Reserved7) && IsPowered() && chargePower >= GetChargeNeededForSupplies() && HasSuppliesRemaining() && Interface.CallHook("OnExcavatorSuppliesRequest", this, rpc.player) == null)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(supplyPlanePrefab.resourcePath);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				Vector3 position = dropPoints[Random.Range(0, dropPoints.Length)].position;
				Vector3 val = default(Vector3);
				((Vector3)(ref val))._002Ector(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
				((Component)baseEntity).SendMessage("InitDropPosition", (object)(position + val), (SendMessageOptions)1);
				baseEntity.Spawn();
			}
			Interface.CallHook("OnExcavatorSuppliesRequested", this, rpc.player, baseEntity);
			chargePower -= GetChargeNeededForSupplies();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
			{
				flagsUpdateScope.Set(Flags.Reserved7, b: false);
				flagsUpdateScope.Set(Flags.Reserved9, b: true);
			}
			Invoke(StopTransmitting, 5f);
			numSuppliesCalled++;
			SendNetworkUpdate();
		}
	}

	public void StopTransmitting()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved9, b: false);
	}

	public virtual bool IsPowered()
	{
		return HasFlag(Flags.Reserved8);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			chargePower = info.msg.ioEntity.genericFloat1;
			numSuppliesCalled = info.msg.ioEntity.genericInt1;
		}
	}

	static ExcavatorSignalComputer()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		readyphrase = new Phrase("excavator.signal.ready", "READY");
		chargephrase = new Phrase("excavator.signal.charging", "COMSYS CHARGING");
		emptyphrase = new Phrase("excavator.signal.empty", "OFFLINE");
		transmitphrase = new Phrase("excavator.signal.transmit", "TRANSMITTING");
		inboundphrase = new Phrase("excavator.signal.inbound", "CARGO INBOUND");
		chargeNeededForSupplies = 600f;
		automaticChargeNeededForSupplies = 600f;
	}
}
