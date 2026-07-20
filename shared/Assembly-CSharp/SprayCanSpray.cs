using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class SprayCanSpray : DecayEntity, ISplashable
{
	private DateTime sprayTimestamp;

	[NonSerialized]
	public ulong sprayedByPlayer;

	public static ListHashSet<SprayCanSpray> AllSprays = new ListHashSet<SprayCanSpray>();

	[NonSerialized]
	public int splashThreshold;

	public override bool BypassInsideDecayMultiplier => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SprayCanSpray.OnRpcMessage"))
		{
			if (rpc == 2774110739u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RequestWaterClear"));
				}
				using (TimeWarning.New("Server_RequestWaterClear"))
				{
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
							Server_RequestWaterClear(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_RequestWaterClear");
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
		if (info.msg.spray == null)
		{
			info.msg.spray = Pool.Get<Spray>();
		}
		info.msg.spray.sprayedBy = sprayedByPlayer;
		info.msg.spray.timestamp = sprayTimestamp.ToBinary();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.spray != null)
		{
			sprayedByPlayer = info.msg.spray.sprayedBy;
			sprayTimestamp = DateTime.FromBinary(info.msg.spray.timestamp);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		sprayTimestamp = DateTime.Now;
		sprayedByPlayer = deployedBy.userID;
		if (Global.MaxSpraysPerPlayer > 0 && sprayedByPlayer != 0L)
		{
			int num = -1;
			DateTime now = DateTime.Now;
			int num2 = 0;
			for (int i = 0; i < AllSprays.Count; i++)
			{
				if (AllSprays[i].sprayedByPlayer == sprayedByPlayer)
				{
					num2++;
					if (num == -1 || AllSprays[i].sprayTimestamp < now)
					{
						num = i;
						now = AllSprays[i].sprayTimestamp;
					}
				}
			}
			if (num2 >= Global.MaxSpraysPerPlayer && num != -1)
			{
				AllSprays[num].Kill();
			}
		}
		if ((Object)(object)deployedBy == (Object)null || !deployedBy.IsBuildingAuthed())
		{
			Invoke(ApplyOutOfAuthConditionPenalty, 1f);
		}
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Sprinkler.SplashableGrid.OnParentChanged(this, oldParent, newParent);
	}

	public void ApplyOutOfAuthConditionPenalty()
	{
		if (!IsFullySpawned())
		{
			Invoke(ApplyOutOfAuthConditionPenalty, 1f);
			return;
		}
		float amount = MaxHealth() * (1f - Global.SprayOutOfAuthMultiplier);
		Hurt(amount, DamageType.Decay);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRandomized(RainCheck, 60f, 180f, 30f);
		if (!AllSprays.Contains(this))
		{
			AllSprays.Add(this);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (AllSprays.Contains(this))
		{
			AllSprays.Remove(this);
		}
		Sprinkler.SplashableGrid.DeregisterEntity(this);
	}

	public void RainCheck()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (Climate.GetRain(((Component)this).transform.position) > 0f && IsOutside())
		{
			Kill();
		}
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		return amount > splashThreshold;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		if (!base.IsDestroyed)
		{
			Kill();
		}
		return 1;
	}

	[RPC_Server]
	private void Server_RequestWaterClear(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && Menu_WaterClear_ShowIf(player) && Interface.CallHook("OnSprayRemove", this, player) == null)
		{
			Kill();
		}
	}

	public bool Menu_WaterClear_ShowIf(BasePlayer player)
	{
		if ((Object)(object)player.GetHeldEntity() != (Object)null && player.GetHeldEntity() is BaseLiquidVessel baseLiquidVessel)
		{
			return baseLiquidVessel.AmountHeld() > 0;
		}
		return false;
	}
}
