using System;
using Oxide.Core;
using UnityEngine;

public class WaterCatcher : LiquidContainer, IPowergridEntity, IOilSwitchReceiver
{
	public class WaterCatcherWorkQueue : PersistentObjectWorkQueue<WaterCatcher>
	{
		protected override void RunJob(WaterCatcher entity)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (!(TimeUntil.op_Implicit(entity.nextCollect) > 0f))
			{
				entity.CollectWater();
			}
		}
	}

	public const Flags Flag_CanProduceItem = Flags.Reserved3;

	public const Flags Flag_RequiresPowergridToProduce = Flags.Reserved4;

	public const Flags Flag_Dripping = Flags.Reserved9;

	[Header("Water Catcher")]
	public ItemDefinition itemToCreate;

	public WaterCatcherCollectRate collectionRates;

	public float maxItemToCreate;

	[Header("Outside Test")]
	public bool requireOutside;

	public Vector3 rainTestPosition;

	public float rainTestSize;

	protected const float collectInterval = 60f;

	public bool lockInventory;

	[Tooltip("Only spawn items when flag CanProduceItem is set to true")]
	public bool conditionalSpawning;

	[Tooltip("If there is water in the container, and nothing connected, should a flag be set to show dripping effects?")]
	public bool doDrippingFlags;

	[Tooltip("If the powergrid is enabled then only spawn items if the powergrid stage is at least this (stage 0 is powergrid offline, meaning this would be unaffected by the powergrid). This setting has no effect if conditionalSpawning is disabled.")]
	public int requiredPowergridStage;

	public bool requireOilSwitchActive;

	public float overrideCollectInterval;

	public bool overridePreventCopyFrom;

	public bool overridePreventCopyTo;

	[Header("Conditional Spawning Sounds")]
	public GameObject soundEmitterObject;

	public bool doConditionalSounds;

	public SoundDefinition startProductionSound;

	public SoundDefinition productionLoopSound;

	public SoundDefinition stopProductionSound;

	public Phrase overrideDisplayName;

	public SoundDefinition pumpingSound;

	public const Flags Flag_PumpInProcess = Flags.Reserved10;

	protected TimeUntil nextCollect;

	public static WaterCatcherWorkQueue CollectWorkQueue = new WaterCatcherWorkQueue();

	[ServerVar(Saved = true)]
	public static float WaterCatcherBudgetMs = 0.1f;

	[ServerVar(Help = "Debug flag to force enable conditional spawning for all water catchers, regardless of their individual settings.")]
	public static bool ForceEnableConditionalSpawning = false;

	private float currentOilRigMultiplier;

	private Action clearWaterMovedFlag;

	bool IPowergridEntity.Server_ShouldConnectToPowergrid()
	{
		return requiredPowergridStage > 0;
	}

	void IPowergridEntity.Server_OnPowergridStageChanged(int newStage)
	{
		if (RequiresPowergridToProduce())
		{
			ToggleProducing(newStage >= requiredPowergridStage);
		}
	}

	public override void ServerInit()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		AddResource(1);
		float num = ((overrideCollectInterval > 0f) ? overrideCollectInterval : 60f);
		nextCollect = TimeUntil.op_Implicit(num + Random.Range(0f, num * 0.1f));
		((PersistentObjectWorkQueue<WaterCatcher>)CollectWorkQueue).Add(this);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
		{
			flagsUpdateScope.Set(Flags.Reserved4, requiredPowergridStage > 0);
		}
		if (requireOilSwitchActive)
		{
			OilSwitchBroadcast.RegisterReceiver(this);
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		((PersistentObjectWorkQueue<WaterCatcher>)CollectWorkQueue).Remove(this);
		if (requireOilSwitchActive)
		{
			OilSwitchBroadcast.DeregisterReceiver(this);
		}
	}

	protected virtual void CollectWater()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		if (doDrippingFlags)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
			flagsUpdateScope.Set(Flags.Reserved9, HasFlag(Flags.Reserved3) && hasResource() && (pushTargets == null || pushTargets.Count == 0));
		}
		if (!ForceEnableConditionalSpawning && conditionalSpawning && !HasFlag(Flags.Reserved3))
		{
			return;
		}
		float num = ((overrideCollectInterval > 0f) ? overrideCollectInterval : 60f);
		nextCollect = TimeUntil.op_Implicit(num + Random.Range(0f, num * 0.1f));
		if (base.inventory == null || IsFull() || Interface.CallHook("OnWaterCollect", this) != null)
		{
			return;
		}
		float num2 = collectionRates.baseRate;
		if (requireOutside)
		{
			Vector3 position = ((Component)this).transform.position;
			num2 += Climate.GetFog(position) * collectionRates.fogRate;
			if (TestIsOutside(((Component)this).transform, rainTestPosition, rainTestSize, 256f))
			{
				num2 += Climate.GetRain(position) * collectionRates.rainRate;
				num2 += Climate.GetSnow(position) * collectionRates.snowRate;
			}
		}
		AddResource(Mathf.CeilToInt(maxItemToCreate * num2));
	}

	protected bool IsFull()
	{
		if (base.inventory.itemList.Count == 0)
		{
			return false;
		}
		if (base.inventory.itemList[0].amount < base.inventory.maxStackSize)
		{
			return false;
		}
		return true;
	}

	protected bool hasResource()
	{
		if (base.inventory.itemList.Count == 0)
		{
			return false;
		}
		return base.inventory.itemList[0].amount > 0;
	}

	public static bool TestIsOutside(Transform t, Vector3 testPositionOffset, float testSize, float testDistance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 localToWorldMatrix = t.localToWorldMatrix;
		return !Physics.SphereCast(new Ray(((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint3x4(testPositionOffset), Vector3.up), testSize, testDistance, 161546513);
	}

	protected void AddResource(int iAmount)
	{
		if (!ForceEnableConditionalSpawning && conditionalSpawning && !HasFlag(Flags.Reserved3))
		{
			return;
		}
		if (requireOilSwitchActive)
		{
			iAmount = Mathf.RoundToInt((float)iAmount * currentOilRigMultiplier);
		}
		if (outputs.Length != 0)
		{
			IOEntity iOEntity = CheckPushLiquid(outputs[0].connectedTo.Get(), iAmount, this, IOEntity.backtracking * 2);
			if ((Object)(object)iOEntity != (Object)null && iOEntity is LiquidContainer liquidContainer && (liquidContainer.inventory.GetSlot(0) == null || (Object)(object)liquidContainer.inventory.GetSlot(0).info == (Object)(object)itemToCreate))
			{
				ItemContainer.LimitStack limitStack = ItemContainer.LimitStack.Existing;
				int amount = liquidContainer.inventory.GetAmount(itemToCreate.itemid, onlyUsableAmounts: true);
				int num = iAmount;
				if ((Object)(object)itemToCreate == (Object)(object)LiquidContainerCrude.CrudeItem)
				{
					limitStack = ItemContainer.LimitStack.None;
					if (amount + num > liquidContainer.inventory.maxStackSize)
					{
						num = liquidContainer.inventory.maxStackSize - amount;
					}
				}
				if (num > 0)
				{
					liquidContainer.inventory.AddItem(itemToCreate, num, 0uL, limitStack);
					if (lockInventory)
					{
						base.inventory.GetSlot(0)?.LockUnlock(bNewState: true);
					}
					return;
				}
			}
		}
		ItemContainer.LimitStack limitStack2 = ItemContainer.LimitStack.Existing;
		if ((Object)(object)itemToCreate == (Object)(object)LiquidContainerCrude.CrudeItem)
		{
			Item slot = base.inventory.GetSlot(0);
			if (slot != null && (Object)(object)slot.info == (Object)(object)LiquidContainerCrude.CrudeItem)
			{
				limitStack2 = ItemContainer.LimitStack.None;
				iAmount = Mathf.Min(new int[3]
				{
					iAmount,
					LiquidContainerCrude.MaxStackSizeCrude - slot.amount,
					maxStackSize - slot.amount
				});
			}
			if (iAmount <= 0)
			{
				return;
			}
		}
		base.inventory.AddItem(itemToCreate, iAmount, 0uL, limitStack2);
		if (lockInventory)
		{
			base.inventory.GetSlot(0)?.LockUnlock(bNewState: true);
		}
		UpdateOnFlag();
	}

	protected IOEntity CheckPushLiquid(IOEntity connected, int amount, IOEntity fromSource, int depth)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if (depth <= 0 || (Object)(object)itemToCreate == (Object)null)
		{
			return null;
		}
		if ((Object)(object)connected == (Object)null)
		{
			return null;
		}
		Vector3 worldHandlePosition = Vector3.zero;
		IOEntity iOEntity = connected.FindGravitySource(ref worldHandlePosition, IOEntity.backtracking, ignoreSelf: true);
		if ((Object)(object)iOEntity != (Object)null && !connected.AllowLiquidPassthrough(iOEntity, worldHandlePosition))
		{
			return null;
		}
		if ((Object)(object)connected == (Object)(object)this || ConsiderConnectedTo(connected))
		{
			return null;
		}
		if (connected.prefabID == 2150367216u)
		{
			return null;
		}
		IOSlot[] array = connected.outputs;
		foreach (IOSlot iOSlot in array)
		{
			IOEntity iOEntity2 = iOSlot.connectedTo.Get();
			Vector3 sourceWorldPosition = ((Component)connected).transform.TransformPoint(iOSlot.handlePosition);
			if ((Object)(object)iOEntity2 != (Object)null && (Object)(object)iOEntity2 != (Object)(object)fromSource && iOEntity2.AllowLiquidPassthrough(connected, sourceWorldPosition))
			{
				IOEntity iOEntity3 = CheckPushLiquid(iOEntity2, amount, fromSource, depth - 1);
				if ((Object)(object)iOEntity3 != (Object)null)
				{
					return iOEntity3;
				}
			}
		}
		if (connected is LiquidContainer { inventory: not null } liquidContainer && liquidContainer.inventory.GetAmount(itemToCreate.itemid) < liquidContainer.inventory.maxStackSize)
		{
			return connected;
		}
		return null;
	}

	public virtual void ToggleProducing(bool state)
	{
		if (state && requireOilSwitchActive && currentOilRigMultiplier <= 0f)
		{
			state = false;
		}
		if (ForceEnableConditionalSpawning)
		{
			state = true;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved3, state);
	}

	[ContextMenu("Toggle producing")]
	public void ToggleProducing()
	{
		ToggleProducing(!HasFlag(Flags.Reserved3));
	}

	private bool RequiresPowergridToProduce()
	{
		if (HasFlag(Flags.Reserved4) && Powergrid.enabled)
		{
			return conditionalSpawning;
		}
		return false;
	}

	public void OnOilSwitchToggled(float newValue)
	{
		currentOilRigMultiplier = newValue;
		ToggleProducing(newValue > 0f);
	}

	protected override void OnWaterMoved(int amount)
	{
		base.OnWaterMoved(amount);
		if (!requireOilSwitchActive)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved10, b: true);
		if (clearWaterMovedFlag == null)
		{
			clearWaterMovedFlag = ClearWaterMovedFlag;
		}
		CancelInvoke(clearWaterMovedFlag);
		Invoke(clearWaterMovedFlag, autofillTickRate * 2f + 1f);
	}

	private void ClearWaterMovedFlag()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved10, b: false);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (HasFlag(Flags.Reserved10))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
			{
				flagsUpdateScope.Set(Flags.Reserved10, b: false);
			}
		}
	}

	public WaterCatcher()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		maxItemToCreate = 10f;
		requireOutside = true;
		rainTestPosition = new Vector3(0f, 1f, 0f);
		rainTestSize = 1f;
		doConditionalSounds = true;
		base._002Ector();
	}
}
