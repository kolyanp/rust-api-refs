using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

public class Sprinkler : IOEntity
{
	public float SplashFrequency = 1f;

	public Transform Eyes;

	public int WaterPerSplash = 1;

	public float DecayPerSplash = 0.8f;

	public const Flags Flag_Radiation = Flags.Reserved3;

	public TriggerSplashable DynamicObjectsTrigger;

	public static PartialMobileStaticGrid<BaseEntity> SplashableGrid = new PartialMobileStaticGrid<BaseEntity>();

	public ItemDefinition currentFuelType;

	private IOEntity currentFuelSource;

	private HashSet<ISplashable> cachedSplashables = new HashSet<ISplashable>();

	private TimeSince updateSplashableCache;

	private bool forceUpdateSplashables;

	private TimeSince timeSinceFuelTypeRequest;

	private Action DoSplashCB;

	public override bool BlockFluidDraining => (Object)(object)currentFuelSource != (Object)null;

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.sprinkler = Pool.Get<Sprinkler>();
			info.msg.sprinkler.currentFuelType = (((Object)(object)currentFuelType != (Object)null) ? currentFuelType.itemid : 0);
		}
	}

	public override int ConsumptionAmount()
	{
		return 2;
	}

	public override int DesiredPower(int inputIndex = 0)
	{
		if (currentEnergy < ConsumptionAmount())
		{
			return 0;
		}
		return ConsumptionAmount();
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		RefreshSprinklerState(inputAmount);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		return inputAmount;
	}

	public void DoSplash()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)currentFuelType == (Object)null)
		{
			if (TimeSince.op_Implicit(timeSinceFuelTypeRequest) > SplashFrequency * 2f)
			{
				timeSinceFuelTypeRequest = TimeSince.op_Implicit(0f);
				SendChangedToRoot(forceUpdate: true);
			}
			return;
		}
		using (TimeWarning.New("SprinklerSplash"))
		{
			int num = WaterPerSplash;
			PooledList<ISplashable> val = Pool.Get<PooledList<ISplashable>>();
			try
			{
				Vector3 position = Eyes.position;
				if (TimeSince.op_Implicit(updateSplashableCache) > SplashFrequency * 4f || forceUpdateSplashables)
				{
					cachedSplashables.Clear();
					forceUpdateSplashables = false;
					updateSplashableCache = TimeSince.op_Implicit(0f);
					Vector3 up = ((Component)this).transform.up;
					float sprinklerEyeHeightOffset = Server.sprinklerEyeHeightOffset;
					float num2 = Vector3.Angle(up, Vector3.up) / 180f;
					num2 = Mathf.Clamp(num2, 0.2f, 1f);
					sprinklerEyeHeightOffset *= num2;
					Vector3 val2 = position + up * (Server.sprinklerRadius * 0.5f);
					Vector3 val3 = position + up * sprinklerEyeHeightOffset;
					List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
					Vector3 val4 = Vector3.Lerp(val2, val3, 0.5f);
					((Component)DynamicObjectsTrigger).transform.position = val4;
					Transform transform = ((Component)DynamicObjectsTrigger).transform;
					Vector3 val5 = val4 - val2;
					transform.up = ((Vector3)(ref val5)).normalized;
					SplashableGrid.UpdateMobileEntities();
					SplashableGrid.Grid.Query(val4.x, val4.z, Server.sprinklerRadius, list);
					if (list.Count > 0)
					{
						Transform transform2 = ((Component)DynamicObjectsTrigger).transform;
						Vector3 center = DynamicObjectsTrigger.Capsule.center;
						Bounds val6 = ((Collider)DynamicObjectsTrigger.Capsule).bounds;
						OBB val7 = default(OBB);
						((OBB)(ref val7))._002Ector(transform2, new Bounds(center, ((Bounds)(ref val6)).extents * 2f));
						foreach (BaseEntity item in list)
						{
							if ((Object)(object)item != (Object)null && ((OBB)(ref val7)).Intersects(item.WorldSpaceBounds()) && CanEverSplashEntity(item, out var foundSplashable) && item.IsVisible(position))
							{
								cachedSplashables.Add(foundSplashable);
							}
						}
					}
					Pool.FreeUnmanaged<BaseEntity>(ref list);
				}
				foreach (ISplashable cachedSplashable in cachedSplashables)
				{
					if (!ObjectEx.IsUnityNull(cachedSplashable) && cachedSplashable.WantsSplash(currentFuelType, num))
					{
						((List<ISplashable>)(object)val).Add(cachedSplashable);
					}
				}
				using (TimeWarning.New("UpdateDynamicSplashables"))
				{
					if (DynamicObjectsTrigger.entityContents != null)
					{
						foreach (BaseEntity entityContent in DynamicObjectsTrigger.entityContents)
						{
							if (CanEverSplashEntity(entityContent, out var foundSplashable2) && foundSplashable2.WantsSplash(currentFuelType, num))
							{
								if (DynamicObjectsTrigger.ShouldCheckLineOfSight(entityContent))
								{
									DynamicObjectsTrigger.RecordLineOfSight(entityContent, entityContent.IsVisible(position));
								}
								if (DynamicObjectsTrigger.HasLineOfSight(entityContent))
								{
									((List<ISplashable>)(object)val).Add(foundSplashable2);
								}
							}
						}
					}
				}
				if (((List<ISplashable>)(object)val).Count > 0)
				{
					int num3 = num / ((List<ISplashable>)(object)val).Count;
					float num4 = (float)(num % ((List<ISplashable>)(object)val).Count) / (float)((List<ISplashable>)(object)val).Count;
					foreach (ISplashable item2 in (List<ISplashable>)(object)val)
					{
						int amount = num3 + ((Random.value < num4) ? 1 : 0);
						if (!ObjectEx.IsUnityNull(item2) && item2.WantsSplash(currentFuelType, amount))
						{
							int num5 = item2.DoSplash(currentFuelType, amount);
							num -= num5;
							if (num <= 0)
							{
								break;
							}
						}
					}
				}
				if (DecayPerSplash > 0f)
				{
					Hurt(DecayPerSplash);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		Interface.CallHook("OnSprinklerSplashed", this);
		bool CanEverSplashEntity(BaseEntity targetEnt, out ISplashable reference)
		{
			reference = null;
			if (targetEnt.isClient)
			{
				return false;
			}
			if (targetEnt is ISplashable splashable)
			{
				if (targetEnt is IOEntity entity && IsConnectedTo(entity, IOEntity.backtracking))
				{
					return false;
				}
				if (targetEnt is BasePlayer && currentFuelType.baseRadioactivity > 0f)
				{
					return false;
				}
				reference = splashable;
				return true;
			}
			return false;
		}
	}

	public void RefreshSprinklerState()
	{
		RefreshSprinklerState(currentEnergy);
	}

	private void RefreshSprinklerState(int availableFlow)
	{
		bool flag = availableFlow >= ConsumptionAmount();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, flag);
			flagsUpdateScope.Set(Flags.Reserved3, flag && (Object)(object)currentFuelType != (Object)null && currentFuelType.baseRadioactivity > 0f);
		}
		if (DoSplashCB == null)
		{
			DoSplashCB = DoSplash;
		}
		if (flag)
		{
			if (!IsInvoking(DoSplashCB))
			{
				InvokeRandomized(DoSplashCB, SplashFrequency * 0.5f, SplashFrequency, SplashFrequency * 0.2f);
				forceUpdateSplashables = true;
			}
		}
		else
		{
			if (IsInvoking(DoSplashCB))
			{
				CancelInvoke(DoSplashCB);
			}
			currentFuelSource = null;
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		RefreshSprinklerState();
	}

	public override void SetFuelType(ItemDefinition def, IOEntity source)
	{
		base.SetFuelType(def, source);
		if ((Object)(object)currentFuelType != (Object)(object)def)
		{
			forceUpdateSplashables = true;
		}
		currentFuelType = def;
		currentFuelSource = source;
		RefreshSprinklerState();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.sprinkler != null)
		{
			currentFuelType = ItemManager.FindItemDefinition(info.msg.sprinkler.currentFuelType);
		}
	}
}
