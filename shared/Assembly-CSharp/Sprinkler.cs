using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Oxide.Core;
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

	private Action DoSplashCB;

	public override bool BlockFluidDraining => (Object)(object)currentFuelSource != (Object)null;

	public override int ConsumptionAmount()
	{
		return 2;
	}

	public override int DesiredPower(int inputIndex = 0)
	{
		return Mathf.Clamp(currentEnergy, 0, ConsumptionAmount());
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		SetSprinklerState(inputAmount > 0);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		return inputAmount;
	}

	public void DoSplash()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SprinklerSplash"))
		{
			int waterAmount = WaterPerSplash;
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
					float num = Vector3.Angle(up, Vector3.up) / 180f;
					num = Mathf.Clamp(num, 0.2f, 1f);
					sprinklerEyeHeightOffset *= num;
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
							if ((Object)(object)item != (Object)null && ((OBB)(ref val7)).Intersects(item.WorldSpaceBounds()) && ProcessEntity(item, out var foundSplashable) && item.IsVisible(position))
							{
								cachedSplashables.Add(foundSplashable);
							}
						}
					}
					Pool.FreeUnmanaged<BaseEntity>(ref list);
				}
				foreach (ISplashable cachedSplashable in cachedSplashables)
				{
					((List<ISplashable>)(object)val).Add(cachedSplashable);
				}
				using (TimeWarning.New("UpdateDynamicSplashables"))
				{
					if (DynamicObjectsTrigger.entityContents != null)
					{
						foreach (BaseEntity entityContent in DynamicObjectsTrigger.entityContents)
						{
							if (ProcessEntity(entityContent, out var foundSplashable2))
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
					int num2 = waterAmount / ((List<ISplashable>)(object)val).Count;
					float num3 = (float)(waterAmount % ((List<ISplashable>)(object)val).Count) / (float)((List<ISplashable>)(object)val).Count;
					foreach (ISplashable item2 in (List<ISplashable>)(object)val)
					{
						int amount = num2 + ((Random.value < num3) ? 1 : 0);
						if (!ObjectEx.IsUnityNull(item2) && item2.WantsSplash(currentFuelType, amount))
						{
							int num4 = item2.DoSplash(currentFuelType, amount);
							waterAmount -= num4;
							if (waterAmount <= 0)
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
			bool ProcessEntity(BaseEntity targetEnt, out ISplashable reference)
			{
				reference = null;
				if (targetEnt.isClient)
				{
					return false;
				}
				if (targetEnt is ISplashable splashable && splashable.WantsSplash(currentFuelType, waterAmount))
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
		Interface.CallHook("OnSprinklerSplashed", this);
	}

	public void SetSprinklerState(bool wantsOn)
	{
		if (wantsOn)
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}
	}

	public void TurnOn()
	{
		if (IsOn())
		{
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: true);
			if ((Object)(object)currentFuelType != (Object)null)
			{
				flagsUpdateScope.Set(Flags.Reserved3, currentFuelType.baseRadioactivity > 0f);
			}
		}
		forceUpdateSplashables = true;
		if (DoSplashCB == null)
		{
			DoSplashCB = DoSplash;
		}
		if (!IsInvoking(DoSplashCB))
		{
			InvokeRandomized(DoSplashCB, SplashFrequency * 0.5f, SplashFrequency, SplashFrequency * 0.2f);
		}
	}

	public void TurnOff()
	{
		if (IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
				flagsUpdateScope.Set(Flags.Reserved3, b: false);
			}
			if (DoSplashCB == null)
			{
				DoSplashCB = DoSplash;
			}
			if (IsInvoking(DoSplashCB))
			{
				CancelInvoke(DoSplashCB);
			}
			currentFuelSource = null;
			currentFuelType = null;
		}
	}

	public override void SetFuelType(ItemDefinition def, IOEntity source)
	{
		base.SetFuelType(def, source);
		currentFuelType = def;
		currentFuelSource = source;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if ((Object)(object)currentFuelType != (Object)null)
		{
			flagsUpdateScope.Set(Flags.Reserved3, currentFuelType.baseRadioactivity > 0f && IsOn());
		}
		else
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			if (Server.useLegacySprinklerLoadProcess)
			{
				SetFlagLocal(Flags.On, b: false);
			}
			else if (HasFlag(Flags.On) && !IsInvoking(DoSplash))
			{
				InvokeRandomized(DoSplash, SplashFrequency * 0.5f, SplashFrequency, SplashFrequency * 0.2f);
			}
		}
	}
}
