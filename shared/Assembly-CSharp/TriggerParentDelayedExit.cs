using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerParentDelayedExit : TriggerParent
{
	private struct TrackedEntityData
	{
		public TimeSince SinceTriggerPhysicsExit;
	}

	[ServerVar(Help = "Makes TriggerParentDelayedExit act as a TriggerParent again")]
	public static bool disable_delayed_exit;

	private ListDictionary<BaseEntity, TrackedEntityData> entityContentsWaitingForLeave;

	private Action UpdateTick;

	internal override void OnEntityEnter(BaseEntity ent)
	{
		ListDictionary<BaseEntity, TrackedEntityData> obj = entityContentsWaitingForLeave;
		if (obj != null && obj.Contains(ent))
		{
			entityContentsWaitingForLeave.Remove(ent);
		}
		base.OnEntityEnter(ent);
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (disable_delayed_exit)
		{
			base.OnEntityLeave(ent);
			return;
		}
		if (ent.IsForceUpdatingTriggers)
		{
			base.OnEntityLeave(ent);
			return;
		}
		if (ent is BasePlayer basePlayer && (basePlayer.IsDead() || basePlayer.IsSleeping() || PlayerCanReparent(basePlayer)))
		{
			base.OnEntityLeave(ent);
			return;
		}
		if (entityContentsWaitingForLeave == null)
		{
			entityContentsWaitingForLeave = new ListDictionary<BaseEntity, TrackedEntityData>();
		}
		if (!entityContentsWaitingForLeave.Contains(ent))
		{
			entityContentsWaitingForLeave.Add(ent, new TrackedEntityData
			{
				SinceTriggerPhysicsExit = TimeSince.op_Implicit(0f)
			});
		}
		else
		{
			entityContentsWaitingForLeave[ent] = new TrackedEntityData
			{
				SinceTriggerPhysicsExit = TimeSince.op_Implicit(0f)
			};
		}
		if (UpdateTick == null)
		{
			UpdateTick = TickLeaveContents;
		}
		if (!IsInvoking(UpdateTick))
		{
			InvokeRepeating(UpdateTick, 0f, 0f);
		}
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		if (!base.IsBeingDisabled || entityContentsWaitingForLeave == null)
		{
			return;
		}
		foreach (var (ent, _) in entityContentsWaitingForLeave)
		{
			base.OnEntityLeave(ent);
		}
		entityContentsWaitingForLeave = null;
		CancelInvoke(UpdateTick);
	}

	private bool PlayerCanReparent(BasePlayer p)
	{
		TriggerParent triggerParent = p.FindSuitableParent();
		if (Object.op_Implicit((Object)(object)triggerParent))
		{
			return (Object)(object)triggerParent != (Object)(object)this;
		}
		return false;
	}

	private void TickLeaveContents()
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		BufferList<BaseEntity> keys = entityContentsWaitingForLeave.Keys;
		BufferList<TrackedEntityData> values = entityContentsWaitingForLeave.Values;
		for (int i = 0; i < entityContentsWaitingForLeave.Count; i++)
		{
			BaseEntity baseEntity = keys[i];
			if ((Object)(object)baseEntity == (Object)null)
			{
				entityContentsWaitingForLeave.RemoveAt(i);
				i--;
				continue;
			}
			if (disable_delayed_exit)
			{
				RemoveTickedEnt(baseEntity, ref i);
				continue;
			}
			TrackedEntityData trackedEntityData = values[i];
			if (baseEntity is BasePlayer basePlayer && (basePlayer.IsSleeping() || PlayerCanReparent(basePlayer)))
			{
				RemoveTickedEnt(baseEntity, ref i);
				if (basePlayer.IsSleeping())
				{
					Unparent(basePlayer);
				}
			}
			else if (!ShouldParent(baseEntity))
			{
				RemoveTickedEnt(baseEntity, ref i);
			}
			else if (!doClippingCheck && IsClipping(baseEntity) && !(baseEntity is BaseCorpse))
			{
				RemoveTickedEnt(baseEntity, ref i);
			}
			else if (TimeSince.op_Implicit(trackedEntityData.SinceTriggerPhysicsExit) > 1f)
			{
				RemoveTickedEnt(baseEntity, ref i);
			}
			else
			{
				values[i] = trackedEntityData;
			}
		}
		if (entityContentsWaitingForLeave.Count == 0)
		{
			entityContentsWaitingForLeave = null;
			CancelInvoke(UpdateTick);
		}
		void RemoveTickedEnt(BaseEntity ent, ref int reference)
		{
			base.OnEntityLeave(ent);
			entityContentsWaitingForLeave.RemoveAt(reference);
			reference--;
		}
	}
}
