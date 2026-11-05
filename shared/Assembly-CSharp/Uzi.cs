using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class Uzi : BaseProjectile
{
	public const Flags Flag_StockDeployed = Flags.Reserved1;

	[Header("Uzi")]
	[Tooltip("Recoil used while the stock is deployed, the regular recoil is used while it's collapsed")]
	public RecoilProperties stockDeployedRecoil;

	[Tooltip("Matches the viewmodel's idle_stock_deploy animation. Aiming, attacking and reloading are blocked meanwhile, and holstering cancels it")]
	public float stockDeployDuration = 2.8f;

	[Tooltip("Matches the viewmodel's idle_stock_collapse animation. Aiming, attacking and reloading are blocked meanwhile, and holstering cancels it")]
	public float stockCollapseDuration = 1.67f;

	[Tooltip("Repeat delay while the stock is deployed, where the gun is also semi automatic. The regular repeat delay is used while it's collapsed")]
	public float stockDeployedRepeatDelay = 0.16f;

	[Tooltip("Deploy delay while the stock is deployed, the regular deploy delay is used while it's collapsed")]
	public float stockDeployedDeployDelay = 1.4f;

	private TimeUntil timeUntilStockChanged;

	private Action _actionFinishStockChange;

	public override RecoilProperties recoilProperties
	{
		get
		{
			if ((!IsStockDeployed() && !IsInTurret()) || !((Object)(object)stockDeployedRecoil != (Object)null))
			{
				return base.recoilProperties;
			}
			return stockDeployedRecoil.GetRecoil();
		}
	}

	private Action actionFinishStockChange => FinishStockChange;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Uzi.OnRpcMessage"))
		{
			if (rpc == 2897204502u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ToggleStock"));
				}
				using (TimeWarning.New("ToggleStock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2897204502u, "ToggleStock", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(2897204502u, "ToggleStock", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage _ = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							ToggleStock(_);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in ToggleStock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsStockDeployed()
	{
		return HasFlag(Flags.Reserved1);
	}

	public override bool CanBeUsedWithShield()
	{
		if (base.CanBeUsedWithShield())
		{
			return !IsStockDeployed();
		}
		return false;
	}

	public override RecoilProperties GetRecoilStats()
	{
		if (!((Object)(object)stockDeployedRecoil != (Object)null))
		{
			return base.GetRecoilStats();
		}
		return stockDeployedRecoil.GetRecoil();
	}

	private float StockChangeDuration(bool deploying)
	{
		if (!deploying)
		{
			return stockCollapseDuration;
		}
		return stockDeployDuration;
	}

	private bool IsInTurret()
	{
		BaseEntity baseEntity = parentEntity.Get(isServer);
		return baseEntity is AutoTurret || baseEntity is AttackHelicopterTurret;
	}

	public override bool IsAutomatic()
	{
		if (base.IsAutomatic())
		{
			return !IsStockDeployed();
		}
		return false;
	}

	public override float ScaleRepeatDelay(float delay)
	{
		if (IsStockDeployed() && !IsInTurret())
		{
			delay *= stockDeployedRepeatDelay / repeatDelay;
		}
		return base.ScaleRepeatDelay(delay);
	}

	protected override float GetDeployDelay()
	{
		if (IsStockDeployed())
		{
			return stockDeployedDeployDelay;
		}
		return base.GetDeployDelay();
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.CallsPerSecond(2uL)]
	private void ToggleStock(RPCMessage _)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = !IsStockDeployed();
		SetStockDeployed(flag);
		timeUntilStockChanged = TimeUntil.op_Implicit(StockChangeDuration(flag));
		StartAttackCooldown(StockChangeDuration(flag));
		UpdateShieldState(bHeld: false);
		Invoke(actionFinishStockChange, StockChangeDuration(flag));
	}

	private void FinishStockChange()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		timeUntilStockChanged = TimeUntil.op_Implicit(0f);
		UpdateShieldState(bHeld: true);
	}

	protected override bool CanDeployShield()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (base.CanDeployShield())
		{
			return TimeUntil.op_Implicit(timeUntilStockChanged) <= 0f;
		}
		return false;
	}

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!(command == "unload_ammo") || !(TimeUntil.op_Implicit(timeUntilStockChanged) > 0f))
		{
			base.ServerCommand(item, command, player);
		}
	}

	public override void OnHeldChanged()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		base.OnHeldChanged();
		if (!IsDeployed())
		{
			CancelInvoke(actionFinishStockChange);
			if (TimeUntil.op_Implicit(timeUntilStockChanged) > 0f)
			{
				timeUntilStockChanged = TimeUntil.op_Implicit(0f);
				SetStockDeployed(!IsStockDeployed());
			}
		}
	}

	public void SetStockDeployed(bool deployed)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved1, deployed);
	}
}
