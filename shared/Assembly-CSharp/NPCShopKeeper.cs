using System;
using System.Collections.Generic;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class NPCShopKeeper : NPCPlayer
{
	public enum ShopkeeperEvent
	{
		SaleSuccess,
		EndConvoBoughtNothing,
		Talk,
		EndConvoBoughtSomething,
		Wave
	}

	public EntityRef invisibleVendingMachineRef;

	public InvisibleVendingMachine machine;

	public bool canBeHurt;

	public ChildAnimatorSubSystem ChildAnimator;

	public int TotalNoAnimations = 2;

	public int TotalYesAnimations = 2;

	public int TotalGreetAnimations = 2;

	public int TotalByeAnimations = 2;

	public int TotalWaveAnimations = 4;

	public float greetDir;

	public Vector3 initialFacingDir;

	public BasePlayer lastWavedAtPlayer;

	protected override string OverrideCorpseName => "Shopkeeper";

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NPCShopKeeper.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public InvisibleVendingMachine GetVendingMachine()
	{
		if (!invisibleVendingMachineRef.IsValid(base.isServer))
		{
			return null;
		}
		return ((Component)invisibleVendingMachineRef.Get(base.isServer)).GetComponent<InvisibleVendingMachine>();
	}

	public override void UpdateProtectionFromClothing()
	{
	}

	protected override bool AllowRagdoll()
	{
		return canBeHurt;
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		if ((Object)(object)invisibleVendingMachineRef.Get(base.isServer) != (Object)null && invisibleVendingMachineRef.Get(base.isServer) is InvisibleVendingMachine invisibleVendingMachine)
		{
			invisibleVendingMachine.SetAttachedNPC(null);
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (canBeHurt)
		{
			base.Hurt(info);
		}
	}

	public override void ServerInit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		Quaternion val = ((Component)this).transform.rotation;
		if ((Object)(object)GetParentEntity() != (Object)null)
		{
			val = ((Component)this).transform.localRotation;
		}
		initialFacingDir = val * Vector3.forward;
		Invoke(DelayedSleepEnd, 3f);
		SetAimDirection(val * Vector3.forward);
		InvokeRandomized(Greeting, Random.Range(5f, 10f), 5f, Random.Range(0f, 2f));
	}

	public override void PostInitShared()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		base.PostInitShared();
		if (base.isServer)
		{
			if ((Object)(object)machine == (Object)null)
			{
				machine = InvisibleVendingMachine.GetMachineAtPosition(1f, ((Component)this).transform.position);
			}
			if (invisibleVendingMachineRef.IsValid(serverside: true) && (Object)(object)machine == (Object)null)
			{
				machine = GetVendingMachine();
				machine.SetAttachedNPC(this);
			}
			else if ((Object)(object)machine != (Object)null && !invisibleVendingMachineRef.IsValid(serverside: true))
			{
				invisibleVendingMachineRef.Set(machine);
				machine.SetAttachedNPC(this);
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.shopKeeper = Pool.Get<ShopKeeper>();
		info.msg.shopKeeper.vendingRef = invisibleVendingMachineRef.uid;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.shopKeeper != null)
		{
			invisibleVendingMachineRef.uid = info.msg.shopKeeper.vendingRef;
		}
	}

	public void DelayedSleepEnd()
	{
		EndSleeping();
	}

	public virtual void Greeting()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)base.eyes == (Object)null)
		{
			return;
		}
		using (TimeWarning.New("WaveCheck"))
		{
			if (!BaseNetworkable.HasCloseConnections(net.group, ((Component)this).transform.position, 10f))
			{
				return;
			}
			PooledList<BasePlayer> val = Pool.Get<PooledList<BasePlayer>>();
			try
			{
				foreach (Connection subscriber in net.group.subscribers)
				{
					if (subscriber.player is BasePlayer basePlayer && basePlayer.Distance((BaseEntity)this) <= 10f)
					{
						((List<BasePlayer>)(object)val).Add(basePlayer);
					}
				}
				BasePlayer basePlayer2 = null;
				foreach (BasePlayer item in (List<BasePlayer>)(object)val)
				{
					if (!item.isClient && !item.IsNpc && !((Object)(object)item == (Object)(object)this) && item.IsVisible(base.eyes.position) && !((Object)(object)item == (Object)(object)lastWavedAtPlayer) && !(Vector3.Dot(Vector3Ex.Direction2D(item.eyes.position, base.eyes.position), initialFacingDir) < 0.2f))
					{
						basePlayer2 = item;
						break;
					}
				}
				if ((Object)(object)basePlayer2 == (Object)null && !((List<BasePlayer>)(object)val).Contains(lastWavedAtPlayer))
				{
					lastWavedAtPlayer = null;
				}
				if ((Object)(object)basePlayer2 != (Object)null)
				{
					ClientRPC(RpcTarget.NetworkGroup("ClientNotifyShopEvent"), 4);
					SetAimDirection(Vector3Ex.Direction2D(basePlayer2.eyes.position, base.eyes.position));
					lastWavedAtPlayer = basePlayer2;
				}
				else
				{
					SetAimDirection(initialFacingDir);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void NotifyEvent(ShopkeeperEvent shopEvent)
	{
		ClientRPC(RpcTarget.NetworkGroup("ClientNotifyShopEvent"), (int)shopEvent);
	}
}
