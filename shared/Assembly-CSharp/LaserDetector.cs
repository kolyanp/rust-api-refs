using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class LaserDetector : BaseDetector
{
	public const Flags Flag_Triggered = Flags.Reserved12;

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!HasFlag(Flags.Reserved12))
		{
			return 0;
		}
		return currentEnergy;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (IsPowered() && (next & Flags.Reserved1) == Flags.Reserved1)
		{
			InvokeRepeating(VisibilityCheck, 0f, 1f);
		}
		else
		{
			CancelInvoke(VisibilityCheck);
		}
	}

	private void VisibilityCheck()
	{
		if (myTrigger.entityContents == null)
		{
			return;
		}
		bool b = false;
		foreach (BaseEntity entityContent in myTrigger.entityContents)
		{
			if (!entityContent.isClient && CanSee(entityContent))
			{
				b = true;
				break;
			}
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, b);
		}
		MarkDirty();
	}

	public override void OnEmpty()
	{
		base.OnEmpty();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
		}
		MarkDirty();
	}

	public bool CanSee(BaseEntity ent)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		PooledList<RaycastHit> val = Pool.Get<PooledList<RaycastHit>>();
		try
		{
			Vector3 val2 = ((Component)this).transform.position + ((Component)this).transform.forward * 0.25f;
			GamePhysics.TraceAll(new Ray(val2, ((Component)this).transform.forward), 0.12f, (List<RaycastHit>)(object)val, 12f, 0x48A12101 | LayerMask.op_Implicit(myTrigger.InterestLayers), (QueryTriggerInteraction)1, this);
			foreach (RaycastHit item in (List<RaycastHit>)(object)val)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(item);
				if (!((Object)(object)entity == (Object)null) && !entity.isClient)
				{
					return (Object)(object)entity == (Object)(object)ent;
				}
			}
			if (!(ent is BaseVehicle))
			{
				return false;
			}
			Vector3 worldVelocity = ent.GetWorldVelocity();
			if (((Vector3)(ref worldVelocity)).magnitude > 5f)
			{
				Vector3 val3 = ((Component)ent).transform.position + worldVelocity * Time.fixedDeltaTime - val2;
				Vector3 normalized = ((Vector3)(ref val3)).normalized;
				((List<RaycastHit>)(object)val).Clear();
				GamePhysics.TraceAll(new Ray(val2, normalized), 0.25f, (List<RaycastHit>)(object)val, 20f, 0x48A12101 | LayerMask.op_Implicit(myTrigger.InterestLayers), (QueryTriggerInteraction)1, this);
				foreach (RaycastHit item2 in (List<RaycastHit>)(object)val)
				{
					BaseEntity entity2 = RaycastHitEx.GetEntity(item2);
					if (!((Object)(object)entity2 == (Object)null) && !entity2.isClient && (Object)(object)entity2 == (Object)(object)ent)
					{
						return true;
					}
				}
			}
			return false;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		if (inputAmount == 0)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved12, b: false);
			}
		}
	}
}
