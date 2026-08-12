using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseLiquidVessel : AttackEntity
{
	[Header("Liquid Vessel")]
	public GameObjectRef thrownWaterObject;

	public GameObjectRef ThrowEffect3P;

	public SoundDefinition throwSound3P;

	public GameObjectRef fillFromContainer;

	public GameObjectRef fillFromWorld;

	public SoundDefinition fillFromContainerStartSoundDef;

	public SoundDefinition fillFromContainerSoundDef;

	public SoundDefinition fillFromWorldStartSoundDef;

	public SoundDefinition fillFromWorldSoundDef;

	public bool hasLid;

	public float throwScale = 10f;

	public bool canDrinkFrom;

	public bool updateVMWater;

	public float minThrowFrac;

	public bool useThrowAnim;

	public float fillMlPerSec = 500f;

	public static Phrase DifferentLiquidType;

	private float lastFillTime;

	private TimeSince timeSinceLastToast;

	private float nextFreeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseLiquidVessel.OnRpcMessage"))
		{
			if (rpc == 4013436649u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoDrink"));
				}
				using (TimeWarning.New("DoDrink"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4013436649u, "DoDrink", this, player))
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
							DoDrink(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoDrink");
					}
				}
				return true;
			}
			if (rpc == 2781345828u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SendFilling"));
				}
				using (TimeWarning.New("SendFilling"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2781345828u, "SendFilling", this, player))
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
							SendFilling(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SendFilling");
					}
				}
				return true;
			}
			if (rpc == 3038767821u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - ThrowContents"));
				}
				using (TimeWarning.New("ThrowContents"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3038767821u, "ThrowContents", this, player))
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
							ThrowContents(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in ThrowContents");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(FillCheck, 1f, 1f);
	}

	public override void OnHeldChanged()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.OnHeldChanged();
		if (IsDisabled())
		{
			StopFilling();
		}
		if (!hasLid)
		{
			DoThrow(((Component)this).transform.position, Vector3.zero);
			Item item = GetItem();
			if (item != null && item.contents != null)
			{
				item.contents.SetLocked(IsDisabled());
				SendNetworkUpdateImmediate();
			}
		}
	}

	public void SetFilling(bool isFilling)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Open, isFilling);
		}
		if (isFilling)
		{
			StartFilling();
		}
		else
		{
			StopFilling();
		}
		OnSetFilling(isFilling);
	}

	public virtual void OnSetFilling(bool flag)
	{
	}

	public void StartFilling()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.realtimeSinceStartup - lastFillTime;
		StopFilling();
		InvokeRepeating(FillCheck, 0f, 0.3f);
		if (num > 0.2f)
		{
			LiquidContainer facingLiquidContainer = GetFacingLiquidContainer();
			if ((Object)(object)facingLiquidContainer != (Object)null && facingLiquidContainer.GetLiquidItem() != null)
			{
				if (fillFromContainer.isValid)
				{
					Effect.server.Run(fillFromContainer.resourcePath, ((Component)facingLiquidContainer).transform.position, Vector3.up);
				}
				ClientRPC(RpcTarget.NetworkGroup("CLIENT_StartFillingSoundsContainer"));
			}
			else if (CanFillFromWorld())
			{
				if (fillFromWorld.isValid)
				{
					Effect.server.Run(fillFromWorld.resourcePath, GetOwnerPlayer(), 0u, Vector3.zero, Vector3.up);
				}
				ClientRPC(RpcTarget.NetworkGroup("CLIENT_StartFillingSoundsWorld"));
			}
		}
		lastFillTime = Time.realtimeSinceStartup;
	}

	public void StopFilling()
	{
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_StopFillingSounds"));
		CancelInvoke(FillCheck);
	}

	public void FillCheck()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient)
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return;
		}
		float num = (Time.realtimeSinceStartup - lastFillTime) * fillMlPerSec;
		Vector3 pos = ((Component)ownerPlayer).transform.position - new Vector3(0f, 1f, 0f);
		LiquidContainer facingLiquidContainer = GetFacingLiquidContainer();
		if (Interface.CallHook("OnLiquidVesselFill", this, ownerPlayer, facingLiquidContainer) != null)
		{
			return;
		}
		if ((Object)(object)facingLiquidContainer == (Object)null && CanFillFromWorld())
		{
			Item contents = GetContents();
			ItemDefinition itemDefinition = WaterResource.SV_GetAtPoint(pos);
			if (contents != null && contents.info.itemid != itemDefinition.itemid)
			{
				if (TimeSince.op_Implicit(timeSinceLastToast) > 5f)
				{
					timeSinceLastToast = TimeSince.op_Implicit(0f);
					ownerPlayer.ShowToast(GameTip.Styles.Red_Normal, DifferentLiquidType, false);
				}
				return;
			}
			AddLiquid(itemDefinition, Mathf.FloorToInt(num));
		}
		else if ((Object)(object)facingLiquidContainer != (Object)null && facingLiquidContainer.HasLiquidItem())
		{
			int num2 = Mathf.CeilToInt((1f - HeldFraction()) * (float)MaxHoldable());
			if (num2 > 0)
			{
				GetContents();
				Item liquidItem = facingLiquidContainer.GetLiquidItem();
				int num3 = Mathf.Min(Mathf.CeilToInt(num), Mathf.Min(liquidItem.amount, num2));
				AddLiquid(liquidItem.info, num3);
				liquidItem.UseItem(num3);
				facingLiquidContainer.OpenTap(2f);
			}
		}
		lastFillTime = Time.realtimeSinceStartup;
	}

	public void LoseWater(int amount)
	{
		if (!base.UsingInfiniteAmmoCheat)
		{
			Item contents = GetContents();
			if (contents != null)
			{
				contents.UseItem(amount);
				contents.MarkDirty();
				SendNetworkUpdateImmediate();
			}
		}
	}

	public void AddLiquid(ItemDefinition liquidType, int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		Item item = GetItem();
		Item item2 = item.contents.GetSlot(0);
		ItemModContainer component = ((Component)item.info).GetComponent<ItemModContainer>();
		if (item2 == null)
		{
			Item item3 = ItemManager.Create(liquidType, amount, 0uL, isServerSide: true, 0uL);
			item3?.MoveToContainer(item.contents);
			item.contents?.onItemAddedToStack?.Invoke(item3, amount);
			return;
		}
		int num = Mathf.Clamp(item2.amount + amount, 0, component.maxStackSize);
		ItemDefinition itemDefinition = WaterResource.Merge(item2.info, liquidType);
		if ((Object)(object)itemDefinition != (Object)(object)item2.info)
		{
			item2.Remove();
			item2 = ItemManager.Create(itemDefinition, num, 0uL, isServerSide: true, 0uL);
			item2.MoveToContainer(item.contents);
		}
		else
		{
			item2.amount = num;
		}
		item.contents?.onItemAddedToStack?.Invoke(item2, amount);
		item2.MarkDirty();
		SendNetworkUpdateImmediate();
	}

	public Item GetContents()
	{
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return null;
		}
		Item slot = item.contents.GetSlot(0);
		if (slot == null)
		{
			return null;
		}
		return slot;
	}

	public int AmountHeld()
	{
		return GetContents()?.amount ?? 0;
	}

	public float HeldFraction()
	{
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return 0f;
		}
		return (float)AmountHeld() / (float)MaxHoldable();
	}

	public int MaxHoldable()
	{
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return 1;
		}
		return ((Component)GetItem().info).GetComponent<ItemModContainer>().maxStackSize;
	}

	public bool CanDrink()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return false;
		}
		if (!ownerPlayer.metabolism.CanConsume())
		{
			return false;
		}
		if (!canDrinkFrom)
		{
			return false;
		}
		Item item = GetItem();
		if (item == null)
		{
			return false;
		}
		if (item.contents == null)
		{
			return false;
		}
		if (item.contents.itemList == null)
		{
			return false;
		}
		if (item.contents.itemList.Count == 0)
		{
			return false;
		}
		return true;
	}

	private bool IsWeaponBusy()
	{
		return Time.realtimeSinceStartup < nextFreeTime;
	}

	private void SetBusyFor(float dur)
	{
		nextFreeTime = Time.realtimeSinceStartup + dur;
	}

	private void ClearBusy()
	{
		nextFreeTime = Time.realtimeSinceStartup - 1f;
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void DoDrink(RPCMessage msg)
	{
		if (!msg.player.CanInteract())
		{
			return;
		}
		Item item = GetItem();
		if (item == null || item.contents == null || !msg.player.metabolism.CanConsume())
		{
			return;
		}
		foreach (Item item2 in item.contents.itemList)
		{
			ItemModConsume component = ((Component)item2.info).GetComponent<ItemModConsume>();
			if (!((Object)(object)component == (Object)null) && component.CanDoAction(item2, msg.player))
			{
				component.DoAction(item2, msg.player);
				item.contents?.onItemRemovedFromStack?.Invoke(item2, 0);
				break;
			}
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void ThrowContents(RPCMessage msg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!((Object)(object)ownerPlayer == (Object)null))
		{
			DoThrow(ownerPlayer.eyes.position + ownerPlayer.eyes.BodyForward() * 1f, ownerPlayer.estimatedVelocity + ownerPlayer.eyes.BodyForward() * throwScale);
			Effect.server.Run(ThrowEffect3P.resourcePath, ((Component)ownerPlayer).transform.position, ownerPlayer.eyes.BodyForward(), ownerPlayer.net.connection);
		}
	}

	public void DoThrow(Vector3 pos, Vector3 velocity)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer == (Object)null)
		{
			return;
		}
		Item item = GetItem();
		if (item == null || item.contents == null)
		{
			return;
		}
		Item slot = item.contents.GetSlot(0);
		if (slot != null && slot.amount > 0)
		{
			Ray ray = default(Ray);
			((Ray)(ref ray))._002Ector(ownerPlayer.eyes.position, ownerPlayer.eyes.BodyForward());
			float num = 1f;
			if (GamePhysics.Trace(ray, 0f, out var hitInfo, num, 1084293377, (QueryTriggerInteraction)0))
			{
				num = Mathf.Clamp01(((RaycastHit)(ref hitInfo)).distance - 0.1f);
			}
			Vector3 point = ((Ray)(ref ray)).GetPoint(num);
			WaterBall waterBall = GameManager.server.CreateEntity(thrownWaterObject.resourcePath, point, Quaternion.identity) as WaterBall;
			if (Object.op_Implicit((Object)(object)waterBall))
			{
				waterBall.liquidType = slot.info;
				waterBall.waterAmount = slot.amount;
				((Component)waterBall).transform.position = point;
				waterBall.SetVelocity(velocity);
				waterBall.Spawn();
			}
			slot.UseItem(slot.amount);
			item.contents?.onItemAddedRemoved?.Invoke(slot, arg2: false);
			slot.MarkDirty();
			SendNetworkUpdateImmediate();
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void SendFilling(RPCMessage msg)
	{
		bool filling = msg.read.Bit();
		SetFilling(filling);
	}

	public bool CanFillFromWorld()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer))
		{
			return false;
		}
		if (ownerPlayer.IsInWaterVolume(((Component)this).transform.position, out var natural) && !natural)
		{
			return false;
		}
		return ownerPlayer.WaterFactor() >= 0.05f;
	}

	public bool CanThrow()
	{
		return HeldFraction() > minThrowFrac;
	}

	public LiquidContainer GetFacingLiquidContainer()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!Object.op_Implicit((Object)(object)ownerPlayer) || (Object)(object)ownerPlayer.eyes == (Object)null)
		{
			return null;
		}
		RaycastHit hit = default(RaycastHit);
		if (Physics.Raycast(ownerPlayer.eyes.HeadRay(), ref hit, 2f, 1237003025))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if (Object.op_Implicit((Object)(object)entity) && !((Component)((RaycastHit)(ref hit)).collider).gameObject.CompareTag("Not Player Usable") && !((Component)((RaycastHit)(ref hit)).collider).gameObject.CompareTag("Usable Primary"))
			{
				entity = entity.ToServer<BaseEntity>();
				return ((Component)entity).GetComponent<LiquidContainer>();
			}
		}
		return null;
	}

	static BaseLiquidVessel()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		DifferentLiquidType = new Phrase("fill_different_liquid_type", "You can't mix different liquids");
	}
}
