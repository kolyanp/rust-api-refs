using System;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class ReactiveTarget : Signage
{
	[Header("Reactive Target")]
	public Animator myAnimator;

	public GameObjectRef bullseyeEffect;

	public GameObjectRef knockdownEffect;

	public float activationPowerTime;

	public int activationPowerAmount;

	public string mainBoneCollider;

	public string bullseyeBoneCollider;

	public bool isPaintableTarget;

	public Transform movableColliderRoot;

	public Vector3 movableColliderKnockedDownAngle;

	public GameObject movablePlayerForceTrigger;

	private float lastToggleTime;

	public const Flags Flag_KnockedDown = Flags.Reserved1;

	public float knockdownHealth;

	private int inputAmountReset;

	private int inputAmountLower;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ReactiveTarget.OnRpcMessage"))
		{
			if (rpc == 1798082523 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Lower"));
				}
				using (TimeWarning.New("RPC_Lower"))
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
							RPC_Lower(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_Lower");
					}
				}
				return true;
			}
			if (rpc == 2169477377u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Reset"));
				}
				using (TimeWarning.New("RPC_Reset"))
				{
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
							RPC_Reset(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_Reset");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool CanUpdateSign(BasePlayer player)
	{
		if (!isPaintableTarget)
		{
			return false;
		}
		return base.CanUpdateSign(player);
	}

	public void SetCollidersUpright()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		movableColliderRoot.localEulerAngles = Vector3.zero;
		if ((Object)(object)movablePlayerForceTrigger != (Object)null)
		{
			movablePlayerForceTrigger.SetActive(false);
		}
	}

	public void SetCollidersKnockedDown()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		movableColliderRoot.localEulerAngles = movableColliderKnockedDownAngle;
		if ((Object)(object)movablePlayerForceTrigger != (Object)null)
		{
			movablePlayerForceTrigger.SetActive(true);
		}
	}

	public void OnHitShared(HitInfo info)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (IsKnockedDown() || IsLowered())
		{
			return;
		}
		bool num = info.HitBone == StringPool.Get(mainBoneCollider);
		bool flag = info.HitBone == StringPool.Get(bullseyeBoneCollider);
		if ((!num && !flag) || !base.isServer)
		{
			return;
		}
		float num2 = info.damageTypes.Total();
		if (flag)
		{
			num2 *= 2f;
			Effect.server.Run(bullseyeEffect.resourcePath, this, StringPool.Get(bullseyeBoneCollider), Vector3.zero, Vector3.zero);
		}
		knockdownHealth -= num2;
		if (knockdownHealth <= 0f)
		{
			Effect.server.Run(knockdownEffect.resourcePath, this, StringPool.Get(bullseyeBoneCollider), Vector3.zero, Vector3.zero);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			QueueReset();
			SendPowerBurst();
			SendNetworkUpdate();
		}
		else
		{
			ClientRPC(RpcTarget.NetworkGroup("HitEffect"), info.Initiator.net.ID);
		}
		Hurt(1f, DamageType.Suicide, info.Initiator, useProtection: false);
	}

	public bool IsKnockedDown()
	{
		if (IsLowered())
		{
			return HasFlag(Flags.Reserved1);
		}
		return false;
	}

	public bool IsLowered()
	{
		return !HasFlag(Flags.On);
	}

	public override void OnAttacked(HitInfo info)
	{
		OnHitShared(info);
		base.OnAttacked(info);
	}

	public bool CanToggle()
	{
		float num = 1f;
		num = ((inputAmountReset > 0) ? 0.25f : 1f);
		return Time.time > lastToggleTime + num;
	}

	public bool CanLower()
	{
		if (inputAmountLower <= inputAmountReset)
		{
			return inputAmountReset == 0;
		}
		return true;
	}

	public bool CanReset()
	{
		if (inputAmountReset <= inputAmountLower)
		{
			return inputAmountLower == 0;
		}
		return true;
	}

	public void QueueReset()
	{
		float time = ((inputAmountReset > 0) ? 0.25f : 6f);
		Invoke(ResetTarget, time);
	}

	public void ResetTarget()
	{
		if (IsLowered() && CanToggle() && CanReset())
		{
			CancelInvoke(ResetTarget);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
			}
			knockdownHealth = 100f;
			SendPowerBurst();
			Interface.CallHook("OnReactiveTargetReset", this);
		}
	}

	private void LowerTarget()
	{
		if (!IsKnockedDown() && CanToggle() && CanLower())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
			SendPowerBurst();
		}
	}

	private void SendPowerBurst()
	{
		lastToggleTime = Time.time;
		MarkDirtyForceUpdateOutputs();
		Invoke(base.MarkDirtyForceUpdateOutputs, activationPowerTime * 1.01f);
	}

	public override int ConsumptionAmount()
	{
		return 1;
	}

	public override bool IsRootEntity()
	{
		return true;
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		switch (inputSlot)
		{
		case 0:
			base.UpdateFromInput(inputAmount, inputSlot);
			break;
		case 1:
			inputAmountReset = inputAmount;
			if (inputAmount > 0)
			{
				ResetTarget();
			}
			break;
		case 2:
			inputAmountLower = inputAmount;
			if (inputAmount > 0)
			{
				LowerTarget();
			}
			break;
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (IsLowered())
		{
			if (IsPowered())
			{
				return base.GetPassthroughAmount();
			}
			if (IsKnockedDown() && Time.time < lastToggleTime + activationPowerTime)
			{
				return activationPowerAmount;
			}
		}
		return 0;
	}

	[RPC_Server]
	public void RPC_Reset(RPCMessage msg)
	{
		ResetTarget();
	}

	[RPC_Server]
	public void RPC_Lower(RPCMessage msg)
	{
		LowerTarget();
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if ((next & Flags.On) != Flags.On)
		{
			SetCollidersKnockedDown();
		}
		else
		{
			SetCollidersUpright();
		}
	}

	public ReactiveTarget()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		activationPowerTime = 0.5f;
		activationPowerAmount = 1;
		mainBoneCollider = "target_collider";
		bullseyeBoneCollider = "target_collider_bullseye";
		movableColliderKnockedDownAngle = new Vector3(-55f, 0f, 0f);
		lastToggleTime = float.NegativeInfinity;
		knockdownHealth = 100f;
		base._002Ector();
	}
}
