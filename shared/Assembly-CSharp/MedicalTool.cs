using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class MedicalTool : AttackEntity
{
	public float healDurationSelf = 4f;

	public float healDurationOther = 4f;

	public float healDurationOtherWounded = 7f;

	public float maxDistanceOther = 2f;

	public bool canUseOnOther = true;

	public bool canRevive = true;

	public static readonly int USE_SELF_ANIMATOR_HASH = Animator.StringToHash("use_self");

	public static readonly int USE_OTHER_ANIMATOR_HASH = Animator.StringToHash("use_other");

	public static readonly int USE_OTHER_WOUNDED_ANIMATOR_HASH = Animator.StringToHash("use_other_wounded");

	private const float SERVER_VALID_OTHER_DISTANCE = 4f;

	private const float SERVER_VALID_OTHER_DISTANCE_SQR = 16f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MedicalTool.OnRpcMessage"))
		{
			if (rpc == 789049461 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UseOther"));
				}
				using (TimeWarning.New("UseOther"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(789049461u, "UseOther", this, player))
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
							UseOther(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in UseOther");
					}
				}
				return true;
			}
			if (rpc == 2918424470u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UseSelf"));
				}
				using (TimeWarning.New("UseSelf"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(2918424470u, "UseSelf", this, player))
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
							UseSelf(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in UseSelf");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void UseOther(RPCMessage msg)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		BaseEntity entity;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else if (player.CanInteract() && HasItemAmount() && canUseOnOther && BaseNetworkable.serverEntities.TryGetEntity(msg.read.EntityID(), out entity) && entity is IMedicalToolTarget medicalToolTarget && Vector3.SqrMagnitude(((Component)entity).transform.position - ((Component)player).transform.position) < 16f && medicalToolTarget.IsValidMedicalToolItem(GetOwnerItemDefinition()))
		{
			ClientRPC(RpcTarget.Player("Reset", player));
			GiveEffectsTo(player, medicalToolTarget);
			UseItemAmount(1);
			StartAttackCooldown(repeatDelay);
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void UseSelf(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else if (player.CanInteract() && HasItemAmount() && ((IMedicalToolTarget)player).IsValidMedicalToolItem(GetOwnerItemDefinition()))
		{
			ClientRPC(RpcTarget.Player("Reset", player));
			GiveEffectsTo(player, player);
			UseItemAmount(1);
			StartAttackCooldown(repeatDelay);
		}
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		if (base.isClient)
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!((Object)(object)ownerPlayer == (Object)null) && ownerPlayer.CanInteract() && HasItemAmount() && ((IMedicalToolTarget)ownerPlayer).IsValidMedicalToolItem(GetOwnerItemDefinition()))
		{
			GiveEffectsTo(ownerPlayer, ownerPlayer);
			UseItemAmount(1);
			StartAttackCooldown(repeatDelay);
			SignalBroadcast(Signal.Attack, string.Empty);
			if (ownerPlayer.IsNpc)
			{
				ownerPlayer.SignalBroadcast(Signal.Attack);
			}
		}
	}

	private void GiveEffectsTo(BasePlayer fromPlayer, IMedicalToolTarget toTarget)
	{
		if ((Object)(object)fromPlayer == (Object)null || (Object)(object)toTarget.GetEntity() == (Object)null)
		{
			return;
		}
		ItemDefinition ownerItemDefinition = GetOwnerItemDefinition();
		if (!((Object)(object)ownerItemDefinition == (Object)null))
		{
			ItemModConsumable component = ((Component)ownerItemDefinition).GetComponent<ItemModConsumable>();
			if ((Object)(object)component == (Object)null)
			{
				Debug.LogWarning((object)("No consumable for medicaltool: " + ((Object)this).name));
			}
			else if (Interface.CallHook("OnHealingItemUse", this, fromPlayer, toTarget) == null)
			{
				Facepunch.Rust.Analytics.Azure.OnMedUsed(ownerItemDefinition.shortname, fromPlayer, toTarget.GetEntity());
				toTarget.OnMedicalToolApplied(fromPlayer, ownerItemDefinition, component, this, canRevive);
			}
		}
	}
}
