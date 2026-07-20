using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class SmallRamp : Door, global::IBoatBuildingPiece
{
	[Header("SmallRamp")]
	public TriggerParent ParentTrigger;

	protected override bool IgnoreBlockageDotCheck => true;

	public void OnAddedToBoat(PlayerBoat boat)
	{
		if (Object.op_Implicit((Object)(object)ParentTrigger))
		{
			ParentTrigger.associatedMountable = boat;
		}
	}

	private bool IsBoundsClearOfWorldObstacles()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return !GamePhysics.CheckOBBAndEntity(WorldSpaceBounds(), 65536, (QueryTriggerInteraction)0, this);
	}

	protected override void ReverseDoorAnimation(bool wasOpening, bool reverse)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		AnimatorStateInfo currentAnimatorStateInfo = model.animator.GetCurrentAnimatorStateInfo(0);
		model.animator.Play("small_ramp_raise", 0, 1f - ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime);
	}

	protected override bool CheckOnClose()
	{
		return false;
	}

	protected override bool OnlyCheckForVehicles()
	{
		return false;
	}

	public override void StabilityCheck()
	{
		GroundWatch groundWatch = default(GroundWatch);
		if (((Component)this).TryGetComponent<GroundWatch>(ref groundWatch))
		{
			groundWatch.DirectCallOnPhysicsNeighbourChanged();
		}
	}

	protected override void OnPlayerClosedDoor(BasePlayer player)
	{
		base.OnPlayerClosedDoor(player);
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		foreach (BaseEntity child in children)
		{
			if (child is BasePlayer basePlayer && basePlayer.IsSleeping())
			{
				list.Add(basePlayer);
			}
		}
		foreach (BasePlayer ply in list)
		{
			ply.SetParent(null, worldPositionStays: true, sendImmediate: true);
			Invoke(delegate
			{
				ply.SetServerFall(wantsOn: true);
			}, 1.5f);
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	protected override bool CanDoorBeOpened()
	{
		if (base.CanDoorBeOpened())
		{
			return IsBoundsClearOfWorldObstacles();
		}
		return false;
	}

	public override void Hurt(HitInfo info)
	{
		PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
		if ((Object)(object)parentPlayerBoat != (Object)null && !parentPlayerBoat.IsDestructibleWreck)
		{
			parentPlayerBoat.OnBoatDeployableHurt(this, info);
		}
		else
		{
			base.Hurt(info);
		}
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfInteractablePlayerBoat(this);
		}
		return false;
	}
}
